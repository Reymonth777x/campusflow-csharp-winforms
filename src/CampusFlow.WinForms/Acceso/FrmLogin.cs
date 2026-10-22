using CampusFlow.Application.Autenticacion;
using CampusFlow.Application.Common;
using System.ComponentModel;

namespace CampusFlow.WinForms.Acceso;

/// <summary>
/// Pantalla de inicio de sesión. Al autenticar, <see cref="IAuthService"/> abre la sesión por sí mismo:
/// este formulario solo recoge credenciales y muestra el resultado (DialogResult.OK = ingresó).
/// </summary>
/// 
[DesignerCategory("Code")]
public sealed partial class FrmLogin : Form
{
    private readonly IAuthService _auth;
    private bool _ocupado;

    public FrmLogin(IAuthService auth)
    {
        _auth = auth;

        ConstruirInterfaz();
        ConectarEventos();
    }

    private void ConectarEventos()
    {
        btnIngresar.Click += btnIngresar_Click;
        FormClosing += FrmLogin_FormClosing;

        // Al volver a escribir, el error anterior desaparece.
        txtUsuario.TextChanged += LimpiarError;
        txtPassword.TextChanged += LimpiarError;
    }

    // Bloquea el cierre mientras se verifica: así no se puede abortar un intento a mitad (ver IngresarAsync).
    private void FrmLogin_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_ocupado) e.Cancel = true;
    }

    private void LimpiarError(object? sender, EventArgs e)
    {
        lblError.Text = string.Empty;
        if (sender is Control control) errValidacion.SetError(control, string.Empty);
    }

    private async void btnIngresar_Click(object? sender, EventArgs e) => await IngresarAsync();

    private async Task IngresarAsync()
    {
        if (_ocupado) return;

        lblError.Text = string.Empty;
        errValidacion.Clear();
        EstablecerOcupado(true);
        try
        {
            // CancellationToken.None es deliberado: el cierre está bloqueado mientras corre, y un intento
            // fallido DEBE quedar contado (el servicio lo guarda para el bloqueo por intentos).
            var resultado = await _auth.IniciarSesionAsync(
                new LoginRequest(txtUsuario.Text, txtPassword.Text), CancellationToken.None);

            if (resultado.EsExito)
            {
                // Se libera ANTES de cerrar: FormClosing rechaza el cierre mientras _ocupado sea true.
                EstablecerOcupado(false);
                DialogResult = DialogResult.OK;
                return;
            }

            // Tras un fallo la contraseña se borra. Va ANTES de mostrar errores: borrar dispara TextChanged,
            // que limpia mensajes.
            txtPassword.Clear();
            MostrarErrores(resultado.Errores);
        }
        finally
        {
            EstablecerOcupado(false);
        }
    }

    /// <summary>
    /// Errores con campo (p. ej. "Ingrese su usuario") van junto al control. Los demás (credenciales
    /// incorrectas, cuenta bloqueada o desactivada) no pertenecen a un campo y van en la etiqueta roja.
    /// </summary>
    private void MostrarErrores(IReadOnlyList<Error> errores)
    {
        var generales = new List<string>();

        foreach (var error in errores)
        {
            var control = ControlDe(error.Campo);
            if (control is null)
            {
                generales.Add(error.Mensaje);
                continue;
            }

            if (errValidacion.GetError(control).Length == 0)
                errValidacion.SetError(control, error.Mensaje);
        }

        lblError.Text = string.Join(Environment.NewLine, generales);

        // Foco al primer campo con error; si el error es general, a la contraseña (ya vacía) para reintentar.
        Control[] campos = [txtUsuario, txtPassword];
        var destino = campos.FirstOrDefault(c => errValidacion.GetError(c).Length > 0);
        if (destino is null && generales.Count > 0) destino = txtPassword;
        destino?.Focus();
    }

    // Traduce Error.Campo (nombre de propiedad de LoginRequest) al control correspondiente.
    private Control? ControlDe(string? campo) => campo switch
    {
        nameof(LoginRequest.NombreUsuario) => txtUsuario,
        nameof(LoginRequest.Password) => txtPassword,
        _ => null
    };

    private void EstablecerOcupado(bool ocupado)
    {
        _ocupado = ocupado;
        UseWaitCursor = ocupado;
        txtUsuario.Enabled = !ocupado;
        txtPassword.Enabled = !ocupado;
        btnIngresar.Enabled = !ocupado;
        btnSalir.Enabled = !ocupado;
    }

    // ErrorProvider no está en ningún contenedor: se desecha a mano.
    protected override void Dispose(bool disposing)
    {
        if (disposing) errValidacion.Dispose();

        base.Dispose(disposing);
    }
}