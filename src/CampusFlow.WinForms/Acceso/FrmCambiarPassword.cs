using System.ComponentModel;
using CampusFlow.Application.Autenticacion;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Acceso;

/// <summary>
/// Cambio de contraseña del usuario de la sesión actual. Modo obligatorio: lo exige el sistema tras un alta
/// o restablecimiento (no se puede cerrar ni cancelar, solo cerrar sesión). Modo voluntario: lo abre el
/// usuario desde la ventana principal. DialogResult.OK = la contraseña se cambió.
/// </summary>
[DesignerCategory("Code")] // se abre como código, nunca en el diseñador (evita que Visual Studio cree un .resx)
public sealed partial class FrmCambiarPassword : Form
{
    private readonly IAuthService _auth;
    private bool _ocupado;

    public FrmCambiarPassword(IAuthService auth)
    {
        _auth = auth;

        ConstruirInterfaz();
        ConectarEventos();
    }

    /// <summary>Configura el modo del formulario. Se llama una sola vez, antes de ShowDialog.</summary>
    public void Cargar(bool obligatorio)
    {
        if (obligatorio)
        {
            Text = "Cree su contraseña";
            lblIntroduccion.Text =
                "Su contraseña actual es temporal, la asignó un administrador. " +
                "Debe elegir una contraseña propia para continuar.";

            // Sin X de cierre: la única salida es cambiar la contraseña o cerrar la sesión.
            ControlBox = false;
            btnCancelar.Text = "Cerrar sesión";

            // Sin Esc: cerrar la sesión por accidente con una tecla sería demasiado agresivo.
            CancelButton = null;

            // En el arranque este formulario no tiene ventana padre: se centra en pantalla y se deja
            // en la barra de tareas para que no se pueda perder detrás de otras ventanas.
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = true;
        }
        else
        {
            Text = "Cambiar contraseña";
            lblIntroduccion.Text = "Ingrese su contraseña actual y elija una nueva.";
        }

        ActiveControl = txtPasswordActual;
    }

    private void ConectarEventos()
    {
        btnGuardar.Click += async (_, _) => await GuardarAsync();

        // Bloquea el cierre mientras se guarda: así nunca queda una escritura a medias con la ventana ya cerrada.
        FormClosing += (_, e) => { if (_ocupado) e.Cancel = true; };

        // Al corregir un campo, su error desaparece sin esperar a guardar de nuevo.
        txtPasswordActual.TextChanged += LimpiarError;
        txtNuevaPassword.TextChanged += LimpiarError;
        txtConfirmarPassword.TextChanged += LimpiarError;
    }

    private void LimpiarError(object? sender, EventArgs e)
    {
        if (sender is Control control) errValidacion.SetError(control, string.Empty);
    }

    private async Task GuardarAsync()
    {
        if (_ocupado) return;

        errValidacion.Clear();
        EstablecerOcupado(true);
        try
        {
            // CancellationToken.None es deliberado: una escritura no se cancela a mitad de camino
            // (el cierre ya está bloqueado mientras guarda) y un intento fallido DEBE quedar contado.
            var resultado = await _auth.CambiarPasswordAsync(
                new CambiarPasswordRequest(
                    txtPasswordActual.Text,
                    txtNuevaPassword.Text,
                    txtConfirmarPassword.Text),
                CancellationToken.None);

            if (resultado.EsFalla)
            {
                // Errores con campo junto a su control; los demás (bloqueo, cuenta no disponible) en un mensaje.
                ErroresFormulario.Mostrar(
                    this, errValidacion, resultado.Errores, ControlDe,
                    [txtPasswordActual, txtNuevaPassword, txtConfirmarPassword]);
                return;
            }

            // Se libera ANTES de cerrar: FormClosing rechaza el cierre mientras _ocupado sea true.
            EstablecerOcupado(false);
            DialogResult = DialogResult.OK;
        }
        finally
        {
            EstablecerOcupado(false);
        }
    }

    // nameof mantiene el vínculo con el Request: si se renombra una propiedad, el compilador avisa aquí.
    private Control? ControlDe(string? campo) => campo switch
    {
        nameof(CambiarPasswordRequest.PasswordActual) => txtPasswordActual,
        nameof(CambiarPasswordRequest.NuevaPassword) => txtNuevaPassword,
        nameof(CambiarPasswordRequest.ConfirmarPassword) => txtConfirmarPassword,
        _ => null
    };

    private void EstablecerOcupado(bool ocupado)
    {
        _ocupado = ocupado;
        UseWaitCursor = ocupado;
        tlpCampos.Enabled = !ocupado;
        btnGuardar.Enabled = !ocupado;
        btnCancelar.Enabled = !ocupado;
    }

    // ErrorProvider no está en ningún contenedor: se desecha a mano.
    protected override void Dispose(bool disposing)
    {
        if (disposing) errValidacion.Dispose();

        base.Dispose(disposing);
    }
}