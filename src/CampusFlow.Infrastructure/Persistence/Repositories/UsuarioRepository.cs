using CampusFlow.Application.Abstractions;
using CampusFlow.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using CampusFlow.Domain.Enums;


namespace CampusFlow.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository(IDbContextFactory<CampusFlowDbContext> fabrica) : IUsuarioRepository
{
    private const int SqliteConstraint = 19; // código de SQLite para violación de restricción (índice único)

    public async Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario, CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);

        // La columna es COLLATE NOCASE: la comparación ya ignora mayúsculas.
        return await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, ct);
    }
    public async Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);
        return await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);

        // La columna es NOCASE: el orden ignora mayúsculas ("admin" y "Beta" quedan juntos en orden natural).
        return await db.Usuarios.AsNoTracking().OrderBy(u => u.NombreUsuario).ToListAsync(ct);
    }

    public async Task<int> ContarAdministradoresActivosAsync(CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);
        return await db.Usuarios.CountAsync(u => u.Rol == RolUsuario.Administrador && u.Activo, ct);
    }

    public async Task<bool> HayUsuariosAsync(CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);
        return await db.Usuarios.AnyAsync(ct);
    }

    public async Task AgregarAsync(Usuario usuario, CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);
        db.Usuarios.Add(usuario);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: SqliteConstraint })
        {
            // Traduce la excepción de EF/SQLite a la del contrato: Application no conoce SQLite.
            throw new ConflictoDeDatosException("Ya existe un usuario con ese nombre.", ex);
        }
    }

    public async Task ActualizarAsync(Usuario usuario, CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);
        db.Usuarios.Update(usuario); // entidad desconectada: se marca como modificada
        await db.SaveChangesAsync(ct);
    }
}