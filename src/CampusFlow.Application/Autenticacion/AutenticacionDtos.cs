using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Enums;

namespace CampusFlow.Application.Autenticacion;

/// <summary>Lo único del usuario que viaja hacia la UI: sin hash, sin contadores internos.</summary>
public sealed record UsuarioDto(
    int Id,
    string NombreUsuario,
    string NombreCompleto,
    RolUsuario Rol,
    bool Activo);

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
