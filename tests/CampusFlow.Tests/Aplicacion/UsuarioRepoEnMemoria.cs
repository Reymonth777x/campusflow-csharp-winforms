using CampusFlow.Application.Abstractions;
using CampusFlow.Domain.Common;
using CampusFlow.Domain.Entities;

namespace CampusFlow.Tests.Aplicacion;

internal sealed class UsuarioRepoEnMemoria : IUsuarioRepository
{
    private readonly List<Usuario> _datos = [];
    private int _siguienteId = 1;

    public bool SimularConflictoAlAgregar { get; set; }
    public IReadOnlyList<Usuario> Todos => _datos;

    public Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario, CancellationToken ct = default) =>
        Task.FromResult(_datos.FirstOrDefault(u =>
            string.Equals(u.NombreUsuario, nombreUsuario, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> HayUsuariosAsync(CancellationToken ct = default) => Task.FromResult(_datos.Count > 0);

    public Task AgregarAsync(Usuario usuario, CancellationToken ct = default)
    {
        if (SimularConflictoAlAgregar)
            throw new ConflictoDeDatosException("simulado");

        // Id tiene setter protegido; la BD real lo asigna, aquí lo simulamos.
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(usuario, _siguienteId++);
        _datos.Add(usuario);
        return Task.CompletedTask;
    }

    // Misma referencia en memoria: la entidad ya está actualizada.
    public Task ActualizarAsync(Usuario usuario, CancellationToken ct = default) => Task.CompletedTask;
}