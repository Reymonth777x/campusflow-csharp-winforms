using CampusFlow.Domain.Enums;

namespace CampusFlow.Application.Autenticacion;

/// <summary>Lo único del usuario que viaja hacia la UI: sin hash, sin contadores internos.</summary>
/// <param name="DebeCambiarPassword">
/// true si debe elegir una contraseña propia antes de usar el sistema. Parámetro opcional (false)
/// para no romper el código que ya construye este DTO.
/// </param>
public sealed record UsuarioDto(
    int Id,
    string NombreUsuario,
    string NombreCompleto,
    RolUsuario Rol,
    bool Activo,
    bool DebeCambiarPassword = false);

public sealed record LoginRequest(string NombreUsuario, string Password)
{
    // Un record imprime todas sus propiedades en ToString: sin este override, la contraseña
    // aparecería en logs, el depurador o cualquier mensaje que formatee el objeto.
    public override string ToString() => $"LoginRequest {{ NombreUsuario = {NombreUsuario} }}";
}

public sealed record CrearAdministradorInicialRequest(
    string NombreUsuario,
    string NombreCompleto,
    string Password,
    string ConfirmarPassword)
{
    // Misma razón que en LoginRequest.
    public override string ToString() =>
        $"CrearAdministradorInicialRequest {{ NombreUsuario = {NombreUsuario} }}";
}

public sealed record CambiarPasswordRequest(
    string PasswordActual,
    string NuevaPassword,
    string ConfirmarPassword)
{
    // Tres contraseñas: ninguna debe aparecer jamás en un log.
    public override string ToString() => "CambiarPasswordRequest { }";
}