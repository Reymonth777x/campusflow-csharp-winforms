using CampusFlow.Domain.Entities;

namespace CampusFlow.Application.Usuarios;

internal static class UsuarioListadoMappings
{
    // "ahora" entra como parámetro: Bloqueado depende de la hora y así se prueba con un reloj falso.
    public static UsuarioListadoDto ToListadoDto(this Usuario u, DateTimeOffset ahora) => new(
        u.Id,
        u.NombreUsuario,
        u.NombreCompleto,
        u.Rol,
        u.Activo,
        u.UltimoAcceso?.ToLocalTime().DateTime, // se guarda en UTC, se muestra en hora local
        u.EstaBloqueado(ahora));
}