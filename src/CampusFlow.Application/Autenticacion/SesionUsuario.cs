using CampusFlow.Domain.Enums;

namespace CampusFlow.Application.Autenticacion;

/// <summary>
/// Registrada como singleton. Se escribe desde el hilo de UI (el servicio no usa ConfigureAwait(false),
/// así que tras cada await vuelve a ese hilo), por eso no necesita sincronización.
/// </summary>
public sealed class SesionUsuario : ISesionUsuario
{
    public UsuarioDto? Actual { get; private set; }

    // Defensa en profundidad: mientras el cambio de contraseña esté pendiente, ningún servicio
    // (que consulta EstaAutenticado o TieneRol) atenderá a este usuario, aunque se salte la UI.
    public bool EstaAutenticado => Actual is { DebeCambiarPassword: false };

    public bool RequiereCambioDePassword => Actual is { DebeCambiarPassword: true };

    public bool TieneRol(params RolUsuario[] roles) =>
        Actual is { DebeCambiarPassword: false } usuario && roles.Contains(usuario.Rol);

    public void Iniciar(UsuarioDto usuario) => Actual = usuario;

    public void Cerrar() => Actual = null;
}