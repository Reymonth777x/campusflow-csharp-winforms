using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Entities;

namespace CampusFlow.Application.Estudiantes;

internal static class EstudianteMappings
{
    public static EstudianteDto ToDto(this Estudiante e) => new(
        e.Id, e.Identificacion, e.Nombre, e.Apellidos, e.NombreCompleto,
        e.Correo, e.FechaIngreso, e.Estado);
}
