using System.ComponentModel;
using CampusFlow.Application.Autenticacion;
using CampusFlow.Application.Common;
using CampusFlow.Application.Usuarios;
using CampusFlow.Domain.Enums;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Usuarios;

/// <summary>
/// Modal de alta y edición de usuarios. En edición no se tocan ni el nombre de usuario (es la identidad
/// de la cuenta) ni la contraseña (para eso existe "Restablecer contraseña").
/// </summary>
[DesignerCategory("Code")]
public sealed partial class FrmUsuarioDetalle : Form
{
    // Los valores del enum, en el orden del combo. Se reutilizan para fijar y leer la selección.
    private static readonly RolUsuario[] Roles = Enum.GetValues<RolUsuario>();

    private readonly IUsuarioService _servicio;
    private readonly ISesionUsuario _sesion;

    // null = alta; con valor = edición de ese usuario.
    private UsuarioListadoDto? _original;
    private bool _ocupado;

    /// <summary>Usuario tal como quedó guardado; el listado lo usa para seleccionarlo al volver.</summary>
    public UsuarioListadoDto? UsuarioGuardado { get; private set; }

    public FrmUsuarioDetalle(IUsuarioService servicio, ISesionUsuario sesion)
    {
        _servicio = servicio;
        _sesion = sesion;

        ConstruirInterfaz();
        ConectarEventos();
    }

    /// <summary>Prepara el formulario. Se llama una sola vez, antes de ShowDialog.</summary>
    public void Cargar(UsuarioListadoDto? usuario)
    {
        _original = usuario;

        if (usuario is null)
        {
            Text = "Nuevo usuario";
            cmbRol.SelectedIndex = Array.IndexOf(Roles, RolUsuario.Docente); // mínimo privilegio por defecto
            ActiveControl = txtNombreUsuario;
            return;
        }

        Text = "Editar usuario";
        txtNombreUsuario.Text = usuario.NombreUsuario;
        txtNombreUsuario.ReadOnly = true;
        txtNombreCompleto.Text = usuario.NombreCompleto;
        cmbRol.SelectedIndex = Array.IndexOf(Roles, usuario.Rol);

        // ActualizarUsuarioRequest no incluye contraseñas: esas filas no tienen sentido en edición.
        foreach (var control in new Control[]
                 { lblPassword, txtPassword, lblConfirmarPassword, txtConfirmarPassword, lblAyudaPassword })
            control.Visible = false;

        // Un administrador no puede cambiarse su propio rol (el servicio también lo rechaza).
        if (usuario.Id == _sesion.Actual?.Id)
        {
            cmbRol.Enabled = false;
            lblNota.Visible = true;
        }

        ActiveControl = txtNombreCompleto;
    }

    private void ConectarEventos()
    {
        btnGuardar.Click += async (_, _) => await GuardarAsync();
        FormClosing += FrmUsuarioDetalle_FormClosing;

        // Al corregir un campo, su error desaparece sin esperar a guardar de nuevo.
        txtNombreUsuario.TextChanged += LimpiarError;
        txtNombreCompleto.TextChanged += LimpiarError;
        cmbRol.SelectedIndexChanged += LimpiarError;
        txtPassword.TextChanged += LimpiarError;
        txtConfirmarPassword.TextChanged += LimpiarError;
    }

    // Bloquea el cierre mientras se guarda: así nunca queda una escritura a medias con la ventana ya cerrada.
    private void FrmUsuarioDetalle_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_ocupado) e.Cancel = true;
    }

    private void LimpiarError(object? sender, EventArgs e)
    {
        if (sender is Control control) errValidacion.SetError(control, string.Empty);
    }

    // Si por algún motivo no hay selección, se cae al rol de menor privilegio.
    private RolUsuario RolSeleccionado => cmbRol.SelectedItem is RolUsuario rol ? rol : RolUsuario.Consulta;

    private async Task GuardarAsync()
    {
        if (_ocupado) return;

        errValidacion.Clear();
        EstablecerOcupado(true);
        try
        {
            // CancellationToken.None es deliberado: una escritura no se cancela a mitad de camino
            // (el cierre ya está bloqueado mientras guarda). Ver FormClosing.
            var resultado = _original is null
                ? await _servicio.CrearAsync(
                    new CrearUsuarioRequest(
                        txtNombreUsuario.Text,
                        txtNombreCompleto.Text,
                        RolSeleccionado,
                        txtPassword.Text,
                        txtConfirmarPassword.Text),
                    CancellationToken.None)
                : await _servicio.ActualizarAsync(
                    new ActualizarUsuarioRequest(_original.Id, txtNombreCompleto.Text, RolSeleccionado),
                    CancellationToken.None);

            if (resultado.EsFalla)
            {
                ErroresFormulario.Mostrar(
                    this, errValidacion, resultado.Errores, ControlDe,
                    [txtNombreUsuario, txtNombreCompleto, cmbRol, txtPassword, txtConfirmarPassword]);
                return;
            }

            UsuarioGuardado = resultado.Valor;

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
    // Rol y NombreCompleto se llaman igual en Crear y Actualizar, así que un solo mapeo sirve para ambos.
    private Control? ControlDe(string? campo) => campo switch
    {
        nameof(CrearUsuarioRequest.NombreUsuario) => txtNombreUsuario,
        nameof(CrearUsuarioRequest.NombreCompleto) => txtNombreCompleto,
        nameof(CrearUsuarioRequest.Rol) => cmbRol,
        nameof(CrearUsuarioRequest.Password) => txtPassword,
        nameof(CrearUsuarioRequest.ConfirmarPassword) => txtConfirmarPassword,
        _ => null
    };

    private void EstablecerOcupado(bool ocupado)
    {
        _ocupado = ocupado;
        UseWaitCursor = ocupado;
        tlpCampos.Enabled = !ocupado; // los controles ya deshabilitados (rol propio) conservan su propio estado
        btnGuardar.Enabled = !ocupado;
        btnCancelar.Enabled = !ocupado;
    }

    // ErrorProvider es un componente que no está en ningún contenedor: se desecha a mano.
    protected override void Dispose(bool disposing)
    {
        if (disposing) errValidacion.Dispose();

        base.Dispose(disposing);
    }
}