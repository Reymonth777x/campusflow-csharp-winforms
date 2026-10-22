using CampusFlow.Domain.Enums;

namespace CampusFlow.Application.Usuarios;

/// <summary>Lo que la pantalla de administración ve de un usuario: nunca el hash ni los contadores internos.</summary>
/// <param name="UltimoAcceso">En hora local, listo para mostrar; null si nunca ha ingresado.</param>
/// <param name="Bloqueado">true si la cuenta está bloqueada por intentos fallidos en este momento.</param>
public sealed record UsuarioListadoDto(
    int Id,
    string NombreUsuario,
    string NombreCompleto,
    RolUsuario Rol,
    bool Activo,
    DateTime? UltimoAcceso,
    bool Bloqueado);

public sealed record CrearUsuarioRequest(
    string NombreUsuario,
    string NombreCompleto,
    RolUsuario Rol,
    string Password,
    string ConfirmarPassword)
{
    // Sin este override, un record imprimiría la contraseña en logs o en el depurador.
    public override string ToString() =>
        $"CrearUsuarioRequest {{ NombreUsuario = {NombreUsuario}, Rol = {Rol} }}";
}

// El nombre de usuario no se puede cambiar: es la identidad de la cuenta. No está aquí a propósito.
public sealed record ActualizarUsuarioRequest(int Id, string NombreCompleto, RolUsuario Rol);

public sealed record RestablecerPasswordRequest(int Id, string NuevaPassword, string ConfirmarPassword)
{
    // Misma razón que en CrearUsuarioRequest.
    public override string ToString() => $"RestablecerPasswordRequest {{ Id = {Id} }}";
}