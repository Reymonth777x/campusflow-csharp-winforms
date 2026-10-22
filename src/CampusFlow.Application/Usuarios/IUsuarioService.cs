using CampusFlow.Application.Common;

namespace CampusFlow.Application.Usuarios;

/// <summary>Administración de cuentas. Todas las operaciones exigen una sesión de Administrador.</summary>
public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioListadoDto>> ListarAsync(CancellationToken ct = default);
    Task<Result<UsuarioListadoDto>> CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default);
    Task<Result<UsuarioListadoDto>> ActualizarAsync(ActualizarUsuarioRequest request, CancellationToken ct = default);
    Task<Result> DesactivarAsync(int id, CancellationToken ct = default);
    Task<Result> ActivarAsync(int id, CancellationToken ct = default);

    /// <summary>Asigna una contraseña nueva y desbloquea la cuenta.</summary>
    Task<Result> RestablecerPasswordAsync(RestablecerPasswordRequest request, CancellationToken ct = default);
}