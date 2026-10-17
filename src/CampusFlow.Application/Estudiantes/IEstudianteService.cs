using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Common;

namespace CampusFlow.Application.Estudiantes;

public interface IEstudianteService
{
    Task<Result<EstudianteDto>> CrearAsync(CrearEstudianteRequest request, CancellationToken ct = default);
    Task<Result<EstudianteDto>> ActualizarAsync(ActualizarEstudianteRequest request, CancellationToken ct = default);
    Task<Result<EstudianteDto>> ObtenerAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<EstudianteDto>> ListarAsync(FiltroEstudiantes? filtro = null, CancellationToken ct = default);
    Task<Result> DesactivarAsync(int id, CancellationToken ct = default);
    Task<Result> ActivarAsync(int id, CancellationToken ct = default);
}