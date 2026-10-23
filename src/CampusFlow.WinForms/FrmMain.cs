using System.ComponentModel;
using CampusFlow.Application.Autenticacion;
using CampusFlow.WinForms.Acceso;
using CampusFlow.WinForms.Common;
using CampusFlow.WinForms.Estudiantes;
using CampusFlow.WinForms.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace CampusFlow.WinForms;

/// <summary>
/// Ventana principal. Solo existe tras un login válido y con la contraseña ya resuelta (ver Program.cs).
/// Muestra quién tiene la sesión, ofrece los módulos según el rol y permite cambiar la contraseña o cerrar
/// la sesión. La navegación completa llega en la Fase 5.
/// </summary>
[DesignerCategory("Code")] // se abre como código, nunca en el diseñador (evita que Visual Studio cree un .resx)
public sealed partial class FrmMain : Form
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAuthService _auth;
    private readonly ISesionUsuario _sesion;

    /// <summary>
    /// true si el usuario eligió "Cerrar sesión" (hay que volver al login);
    /// false si cerró la ventana con la X (hay que salir de la aplicación).
    /// </summary>
    public bool CerrarSesionSolicitada { get; private set; }

    public FrmMain(IServiceScopeFactory scopeFactory, IAuthService auth, ISesionUsuario sesion)
    {
        _scopeFactory = scopeFactory;
        _auth = auth;
        _sesion = sesion;

        ConstruirInterfaz();
        MostrarSesion();

        // Comodidad de UI: el módulo de usuarios solo se ofrece al Administrador. La autorización real
        // la aplica IUsuarioService; ocultar el botón no es seguridad.
        btnUsuarios.Visible = _sesion.PuedeGestionarUsuarios();

        btnEstudiantes.Click += btnEstudiantes_Click;
        btnUsuarios.Click += btnUsuarios_Click;
        btnCambiarPassword.Click += btnCambiarPassword_Click;
        btnCerrarSesion.Click += btnCerrarSesion_Click;

        // Cualquier cierre (X o "Cerrar sesión") deja la sesión limpia.
        FormClosed += (_, _) => _auth.CerrarSesion();
    }

    private void MostrarSesion()
    {
        lblSesion.Text = _sesion.Actual is { } usuario
            ? $"{usuario.NombreCompleto} – {usuario.Rol}"
            : "Sin sesión";
    }

    private void btnEstudiantes_Click(object? sender, EventArgs e)
    {
        // Scope por pantalla: al terminar el using se desecha el formulario y todo lo que se le inyectó.
        using var scope = _scopeFactory.CreateScope();
        var formulario = scope.ServiceProvider.GetRequiredService<FrmEstudiantes>();
        formulario.ShowDialog(this);
    }

    private void btnUsuarios_Click(object? sender, EventArgs e)
    {
        using var scope = _scopeFactory.CreateScope();
        var formulario = scope.ServiceProvider.GetRequiredService<FrmUsuarios>();
        formulario.ShowDialog(this);

        // El administrador pudo editar su propio nombre: la sesión ya lo refleja, falta repintar la etiqueta.
        MostrarSesion();
    }

    private void btnCambiarPassword_Click(object? sender, EventArgs e)
    {
        using var scope = _scopeFactory.CreateScope();
        var formulario = scope.ServiceProvider.GetRequiredService<FrmCambiarPassword>();
        formulario.Cargar(obligatorio: false); // voluntario: se puede cancelar

        if (formulario.ShowDialog(this) == DialogResult.OK)
            Mensajes.Informacion(this, "Contraseña actualizada correctamente.");
    }

    private void btnCerrarSesion_Click(object? sender, EventArgs e)
    {
        if (!Mensajes.Confirmar(this, "¿Cerrar la sesión actual?")) return;

        CerrarSesionSolicitada = true; // Program.cs lo lee tras cerrarse la ventana para mostrar el login otra vez
        Close();
    }
}