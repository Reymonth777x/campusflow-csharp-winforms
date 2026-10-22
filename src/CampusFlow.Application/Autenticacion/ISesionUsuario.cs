using CampusFlow.Domain.Enums;

namespace CampusFlow.Application.Autenticacion;

/// <summary>Quién está usando la aplicación ahora mismo. Una única sesión por proceso.</summary>
public interface ISesionUsuario
{
    UsuarioDto? Actual { get; }

    /// <summary>
    /// true solo si hay sesión Y el usuario no tiene un cambio de contraseña pendiente.
    /// Una sesión pendiente NO está autenticada: los servicios la rechazan por este mismo camino.
    /// </summary>
    bool EstaAutenticado { get; }

    /// <summary>true si hay sesión abierta pero la persona debe elegir una contraseña propia antes de continuar.</summary>
    bool RequiereCambioDePassword { get; }

    /// <summary>true si hay sesión autenticada y el rol del usuario es uno de los indicados.</summary>
    bool TieneRol(params RolUsuario[] roles);

    void Iniciar(UsuarioDto usuario);
    void Cerrar();
}