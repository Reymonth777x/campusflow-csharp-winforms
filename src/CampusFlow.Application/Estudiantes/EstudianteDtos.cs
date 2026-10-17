using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Enums;

namespace CampusFlow.Application.Estudiantes;

public sealed record EstudianteDto(
    int Id,
    string Identificacion,
    string Nombre,
    string Apellidos,
    string NombreCompleto,
    string Correo,
    DateOnly FechaIngreso,
    EstadoEstudiante Estado);

public sealed record CrearEstudianteRequest(
    string Identificacion,
    string Nombre,
    string Apellidos,
    string Correo,
    DateOnly FechaIngreso);

// La identificación no se puede modificar: no está aquí a propósito.
public sealed record ActualizarEstudianteRequest(
    int Id,
    string Nombre,
    string Apellidos,
    string Correo);

public sealed record FiltroEstudiantes(string? Texto = null, EstadoEstudiante? Estado = null);