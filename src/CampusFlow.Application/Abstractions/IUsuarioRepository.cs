using CampusFlow.Domain.Entities;

namespace CampusFlow.Application.Abstractions;

public interface IUsuarioRepository
{
    /// <summary>Busca sin distinguir mayúsculas (lo garantiza la colación de la columna).</summary>
    Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario, CancellationToken ct = default);

    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Todos los usuarios, ordenados por nombre de usuario.</summary>
    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default);

    /// <summary>Cuántos administradores activos hay: base de la regla "nunca quedarse sin administrador".</summary>
    Task<int> ContarAdministradoresActivosAsync(CancellationToken ct = default);

    /// <summary>Para saber si es el primer arranque (no hay ningún usuario todavía).</summary>
    Task<bool> HayUsuariosAsync(CancellationToken ct = default);

    /// <exception cref="ConflictoDeDatosException">Ya existe ese nombre de usuario.</exception>
    Task AgregarAsync(Usuario usuario, CancellationToken ct = default);

    Task ActualizarAsync(Usuario usuario, CancellationToken ct = default);
}