using CampusFlow.Application.Autenticacion;
using CampusFlow.Application.Common;
using CampusFlow.WinForms.Common;
using System.ComponentModel;

namespace CampusFlow.WinForms.Acceso;

/// <summary>
/// Se muestra una sola vez, en el primer arranque (cuando no existe ningún usuario), para crear
/// la cuenta de administrador. El servicio rechaza esta operación si ya hay usuarios registrados.
/// </summary>
/// 
[DesignerCategory("Code")]
public sealed partial class FrmAdministradorInicial : Form
{
    private readonly IAuthService _auth;
    private bool _ocupado;

    public FrmAdministradorInicial(IAuthService auth)
    {
        _auth = auth;

        ConstruirInterfaz();
        ConectarEventos();
    }

    private void ConectarEventos()
    {
        btnCrear.Click += btnCrear_Click;
        FormClosing += FrmAdministradorInicial_FormClosing;

        txtNombreCompleto.TextChanged += LimpiarError;
        txtNombreUsuario.TextChanged += LimpiarError;
        txtPassword.TextChanged += LimpiarError;
        txtConfirmarPassword.TextChanged += LimpiarError;
    }

    // No se puede cerrar a mitad de una creación (hashear tarda y escribe en la BD).
    private void FrmAdministradorInicial_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_ocupado) e.Cancel = true;
    }

    private void LimpiarError(object? sender, EventArgs e)
    {
        if (sender is Control control) errValidacion.SetError(control, string.Empty);
    }

    private async void btnCrear_Click(object? sender, EventArgs e) => await CrearAsync();

    private async Task CrearAsync()
    {
        if (_ocupado) return;

        errValidacion.Clear();
        EstablecerOcupado(true);
        try
        {
            // CancellationToken.None: una escritura no se cancela a medias (el cierre ya está bloqueado).
            var resultado = await _auth.RegistrarAdministradorInicialAsync(
                new CrearAdministradorInicialRequest(
                    txtNombreUsuario.Text,
                    txtNombreCompleto.Text,
                    txtPassword.Text,
                    txtConfirmarPassword.Text),
                CancellationToken.None);

            if (resultado.EsFalla)
            {
                MostrarErrores(resultado.Errores);
                return;
            }

            Mensajes.Informacion(this, "Administrador creado correctamente. Ahora puede iniciar sesión.");

            // Se libera ANTES de cerrar: FormClosing rechaza el cierre mientras _ocupado sea true.
            EstablecerOcupado(false);
            DialogResult = DialogResult.OK;
        }
        finally
        {
            EstablecerOcupado(false);
        }
    }

    /// <summary>Errores con campo junto a su control; los demás (p. ej. "ya hay usuarios") en un cuadro de mensaje.</summary>
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

        // Foco al primer campo con error, en el orden visual del formulario.
        CamposEnOrden.FirstOrDefault(c => errValidacion.GetError(c).Length > 0)?.Focus();

        if (generales.Count > 0)
            Mensajes.Advertencia(this, string.Join(Environment.NewLine, generales));
    }

    // nameof mantiene el vínculo con el Request: si se renombra una propiedad, el compilador avisa aquí.
    private Control? ControlDe(string? campo) => campo switch
    {
        nameof(CrearAdministradorInicialRequest.NombreCompleto) => txtNombreCompleto,
        nameof(CrearAdministradorInicialRequest.NombreUsuario) => txtNombreUsuario,
        nameof(CrearAdministradorInicialRequest.Password) => txtPassword,
        nameof(CrearAdministradorInicialRequest.ConfirmarPassword) => txtConfirmarPassword,
        _ => null
    };

    private Control[] CamposEnOrden =>
        [txtNombreCompleto, txtNombreUsuario, txtPassword, txtConfirmarPassword];

    private void EstablecerOcupado(bool ocupado)
    {
        _ocupado = ocupado;
        UseWaitCursor = ocupado;
        tlpCampos.Enabled = !ocupado;
        btnCrear.Enabled = !ocupado;
        btnSalir.Enabled = !ocupado;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) errValidacion.Dispose();

        base.Dispose(disposing);
    }
}