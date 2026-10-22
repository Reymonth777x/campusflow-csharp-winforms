using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Entities;

namespace CampusFlow.Application.Autenticacion;

internal static class UsuarioMappings
{
    public static UsuarioDto ToDto(this Usuario u) =>
        new(u.Id, u.NombreUsuario, u.NombreCompleto, u.Rol, u.Activo);
}
