using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Abstractions;
using CampusFlow.Application.Estudiantes;
using CampusFlow.Domain.Common;
using CampusFlow.Domain.Entities;

namespace CampusFlow.Tests.Aplicacion;

internal sealed class RepositorioEnMemoria : IEstudianteRepository
{
    private readonly List<Estudiante> _datos = [];
    private int _siguienteId = 1;

    public bool SimularConflictoAlAgregar { get; set; }
    public int Cantidad => _datos.Count;

    public Task<Estudiante?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(_datos.FirstOrDefault(e => e.Id == id));

    public Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken ct = default) =>
        Task.FromResult(_datos.Any(e =>
            string.Equals(e.Identificacion, identificacion, StringComparison.OrdinalIgnoreCase)));

    public Task<IReadOnlyList<Estudiante>> ListarAsync(FiltroEstudiantes filtro, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Estudiante>>([.. _datos]);

    public Task AgregarAsync(Estudiante estudiante, CancellationToken ct = default)
    {
        if (SimularConflictoAlAgregar)
            throw new ConflictoDeDatosException("simulado");

        // Id tiene setter protegido; la BD real lo asigna, aquí lo simulamos.
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(estudiante, _siguienteId++);
        _datos.Add(estudiante);
        return Task.CompletedTask;
    }

    public Task ActualizarAsync(Estudiante estudiante, CancellationToken ct = default) =>
        Task.CompletedTask; // misma referencia en memoria: ya está actualizado
}