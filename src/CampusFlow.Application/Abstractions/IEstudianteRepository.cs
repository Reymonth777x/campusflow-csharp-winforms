using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Estudiantes;
using CampusFlow.Domain.Entities;

namespace CampusFlow.Application.Abstractions;

public interface IEstudianteRepository
{
    Task<Estudiante?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken ct = default);
    Task<IReadOnlyList<Estudiante>> ListarAsync(FiltroEstudiantes filtro, CancellationToken ct = default);

    /// <exception cref="ConflictoDeDatosException">Ya existe una identificación igual.</exception>
    Task AgregarAsync(Estudiante estudiante, CancellationToken ct = default);
    Task ActualizarAsync(Estudiante estudiante, CancellationToken ct = default);
}