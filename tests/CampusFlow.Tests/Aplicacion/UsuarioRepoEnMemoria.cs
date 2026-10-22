using CampusFlow.Application.Abstractions;
using CampusFlow.Domain.Common;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;

namespace CampusFlow.Tests.Aplicacion;

internal sealed class UsuarioRepoEnMemoria : IUsuarioRepository
{
    private readonly List<Usuario> _datos = [];
    private int _siguienteId = 1;

    public bool SimularConflictoAlAgregar { get; set; }
    public IReadOnlyList<Usuario> Todos => _datos;

    /// <summary>Inserta de forma síncrona: sirve para preparar datos en el constructor de las pruebas.</summary>
    public void Sembrar(Usuario usuario)
    {
        // Id tiene setter protegido; la BD real lo asigna, aquí lo simulamos.
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(usuario, _siguienteId++);
        _datos.Add(usuario);
    }

    public Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario, CancellationToken ct = default) =>
        Task.FromResult(_datos.FirstOrDefault(u =>
            string.Equals(u.NombreUsuario, nombreUsuario, StringComparison.OrdinalIgnoreCase)));

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(_datos.FirstOrDefault(u => u.Id == id));

    public Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Usuario>>([.. _datos.OrderBy(u => u.NombreUsuario)]);

    public Task<int> ContarAdministradoresActivosAsync(CancellationToken ct = default) =>
        Task.FromResult(_datos.Count(u => u.Rol == RolUsuario.Administrador && u.Activo));

    public Task<bool> HayUsuariosAsync(CancellationToken ct = default) => Task.FromResult(_datos.Count > 0);

    public Task AgregarAsync(Usuario usuario, CancellationToken ct = default)
    {
        if (SimularConflictoAlAgregar)
            throw new ConflictoDeDatosException("simulado");

        Sembrar(usuario);
        return Task.CompletedTask;
    }

    // Misma referencia en memoria: la entidad ya está actualizada.
    public Task ActualizarAsync(Usuario usuario, CancellationToken ct = default) => Task.CompletedTask;
}