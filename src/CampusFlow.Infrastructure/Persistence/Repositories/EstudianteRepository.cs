using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Abstractions;
using CampusFlow.Application.Estudiantes;
using CampusFlow.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CampusFlow.Infrastructure.Persistence.Repositories;

public sealed class EstudianteRepository(IDbContextFactory<CampusFlowDbContext> fabrica)
    : IEstudianteRepository
{
    private const int SqliteConstraint = 19;

    public async Task<Estudiante?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);
        return await db.Estudiantes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<bool> ExisteIdentificacionAsync(string identificacion, CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);
        // La columna es COLLATE NOCASE: la comparación ya ignora mayúsculas.
        return await db.Estudiantes.AnyAsync(e => e.Identificacion == identificacion, ct);
    }

    public async Task<IReadOnlyList<Estudiante>> ListarAsync(FiltroEstudiantes filtro, CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);
        IQueryable<Estudiante> consulta = db.Estudiantes.AsNoTracking();

        if (filtro.Estado is { } estado)
            consulta = consulta.Where(e => e.Estado == estado);

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var patron = $"%{EscaparLike(filtro.Texto.Trim())}%";
            consulta = consulta.Where(e =>
                EF.Functions.Like(e.Identificacion, patron, "\\") ||
                EF.Functions.Like(e.Nombre, patron, "\\") ||
                EF.Functions.Like(e.Apellidos, patron, "\\") ||
                EF.Functions.Like(e.Correo, patron, "\\"));
        }

        return await consulta
            .OrderBy(e => e.Apellidos).ThenBy(e => e.Nombre)
            .ToListAsync(ct);
    }

    public async Task AgregarAsync(Estudiante estudiante, CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);
        db.Estudiantes.Add(estudiante);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: SqliteConstraint })
        {
            throw new ConflictoDeDatosException("Ya existe un estudiante con esa identificación.", ex);
        }
    }

    public async Task ActualizarAsync(Estudiante estudiante, CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);
        db.Estudiantes.Update(estudiante); // entidad desconectada: se marca como modificada
        await db.SaveChangesAsync(ct);
    }

    // Sin escapar, buscar "%" o "_" devolvería toda la tabla.
    private static string EscaparLike(string texto) =>
        texto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}