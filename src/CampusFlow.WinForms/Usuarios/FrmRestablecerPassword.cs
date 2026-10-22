using System.ComponentModel;
using CampusFlow.Application.Usuarios;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Usuarios;

/// <summary>
/// Modal para asignar una contraseña nueva a un usuario (la ve el administrador al escribirla).
/// Al guardar, el servicio también desbloquea la cuenta.
/// </summary>
[DesignerCategory("Code")]
public sealed partial class FrmRestablecerPassword : Form
{
    private readonly IUsuarioService _servicio;
    private UsuarioListadoDto? _usuario;
    private bool _ocupado;

    public FrmRestablecerPassword(IUsuarioService servicio)
    {
        _servicio = servicio;

        ConstruirInterfaz();
        ConectarEventos();
    }

    /// <summary>Indica a qué usuario se le cambia la contraseña. Se llama una sola vez, antes de ShowDialog.</summary>
    public void Cargar(UsuarioListadoDto usuario)
    {
        _usuario = usuario;
        lblDestino.Text = $"Usuario: {usuario.NombreUsuario} ({usuario.NombreCompleto})";
        ActiveControl = txtNuevaPassword;
    }

    private void ConectarEventos()
    {
        btnRestablecer.Click += async (_, _) => await RestablecerAsync();
        FormClosing += (_, e) => { if (_ocupado) e.Cancel = true; }; // no se cierra a mitad de una escritura

        txtNuevaPassword.TextChanged += LimpiarError;
        txtConfirmarPassword.TextChanged += LimpiarError;
    }

    private void LimpiarError(object? sender, EventArgs e)
    {
        if (sender is Control control) errValidacion.SetError(control, string.Empty);
    }

    private async Task RestablecerAsync()
    {
        if (_ocupado || _usuario is null) return;

        errValidacion.Clear();
        EstablecerOcupado(true);
        try
        {
            // CancellationToken.None: una escritura no se cancela a medias (el cierre ya está bloqueado).
            var resultado = await _servicio.RestablecerPasswordAsync(
                new RestablecerPasswordRequest(_usuario.Id, txtNuevaPassword.Text, txtConfirmarPassword.Text),
                CancellationToken.None);

            if (resultado.EsFalla)
            {
                ErroresFormulario.Mostrar(
                    this, errValidacion, resultado.Errores, ControlDe,
                    [txtNuevaPassword, txtConfirmarPassword]);
                return;
            }

            // Se libera ANTES de cerrar: el FormClosing rechaza el cierre mientras _ocupado sea true.
            EstablecerOcupado(false);
            DialogResult = DialogResult.OK;
        }
        finally
        {
            EstablecerOcupado(false);
        }
    }

    private Control? ControlDe(string? campo) => campo switch
    {
        nameof(RestablecerPasswordRequest.NuevaPassword) => txtNuevaPassword,
        nameof(RestablecerPasswordRequest.ConfirmarPassword) => txtConfirmarPassword,
        _ => null
    };

    private void EstablecerOcupado(bool ocupado)
    {
        _ocupado = ocupado;
        UseWaitCursor = ocupado;
        tlpCampos.Enabled = !ocupado;
        btnRestablecer.Enabled = !ocupado;
        btnCancelar.Enabled = !ocupado;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) errValidacion.Dispose();

        base.Dispose(disposing);
    }
}