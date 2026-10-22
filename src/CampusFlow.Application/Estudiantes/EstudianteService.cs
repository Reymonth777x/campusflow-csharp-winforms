using CampusFlow.Application.Abstractions;
using CampusFlow.Application.Autenticacion;
using CampusFlow.Application.Common;
using CampusFlow.Domain.Common;
using CampusFlow.Domain.Entities;
using FluentValidation;

namespace CampusFlow.Application.Estudiantes;

public sealed class EstudianteService(
    IEstudianteRepository repositorio,
    IValidator<CrearEstudianteRequest> validadorCrear,
    IValidator<ActualizarEstudianteRequest> validadorActualizar,
    ISesionUsuario sesion) : IEstudianteService
{
    private static Error Duplicado => new(
        TipoError.Conflicto,
        "Ya existe un estudiante con esa identificación.",
        nameof(CrearEstudianteRequest.Identificacion));

    private static Error NoEncontrado => new(TipoError.NoEncontrado, "El estudiante no existe.");

    private static readonly Error SinSesion =
        new(TipoError.NoAutorizado, "Debe iniciar sesión para continuar.");

    private static readonly Error SinPermiso =
        new(TipoError.NoAutorizado, "No tiene permiso para realizar esta operación.");

    /// <summary>
    /// Autorización de ESCRITURA. Es la defensa real: ocultar botones en la UI es solo comodidad,
    /// cualquier otra pantalla o futuro cliente que llame al servicio pasa por aquí. Devuelve null si está permitido.
    /// </summary>
    private Error? DenegarEscritura()
    {
        if (!sesion.EstaAutenticado) return SinSesion;
        return sesion.PuedeEscribirEstudiantes() ? null : SinPermiso;
    }

    public async Task<Result<EstudianteDto>> CrearAsync(
        CrearEstudianteRequest request, CancellationToken ct = default)
    {
        // La autorización va ANTES que la validación: a quien no tiene permiso no se le revela nada más.
        if (DenegarEscritura() is { } denegado)
            return Result<EstudianteDto>.Falla(denegado);

        var validacion = await validadorCrear.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return Result<EstudianteDto>.Falla(validacion.ToErrores());

        var identificacion = request.Identificacion.Trim();

        if (await repositorio.ExisteIdentificacionAsync(identificacion, ct))
            return Result<EstudianteDto>.Falla(Duplicado);

        try
        {
            var estudiante = Estudiante.Crear(
                identificacion, request.Nombre, request.Apellidos, request.Correo, request.FechaIngreso);

            await repositorio.AgregarAsync(estudiante, ct);
            return Result<EstudianteDto>.Exito(estudiante.ToDto());
        }
        catch (DomainException ex)
        {
            return Result<EstudianteDto>.Falla(new Error(TipoError.Validacion, ex.Message));
        }
        catch (ConflictoDeDatosException)
        {
            // Otro usuario insertó la misma identificación entre la consulta y el guardado.
            return Result<EstudianteDto>.Falla(Duplicado);
        }
    }

    public async Task<Result<EstudianteDto>> ActualizarAsync(
        ActualizarEstudianteRequest request, CancellationToken ct = default)
    {
        if (DenegarEscritura() is { } denegado)
            return Result<EstudianteDto>.Falla(denegado);

        var validacion = await validadorActualizar.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return Result<EstudianteDto>.Falla(validacion.ToErrores());

        var estudiante = await repositorio.ObtenerPorIdAsync(request.Id, ct);
        if (estudiante is null)
            return Result<EstudianteDto>.Falla(NoEncontrado);

        try
        {
            estudiante.ActualizarDatos(request.Nombre, request.Apellidos, request.Correo);
            await repositorio.ActualizarAsync(estudiante, ct);
            return Result<EstudianteDto>.Exito(estudiante.ToDto());
        }
        catch (DomainException ex)
        {
            return Result<EstudianteDto>.Falla(new Error(TipoError.Validacion, ex.Message));
        }
    }

    public async Task<Result<EstudianteDto>> ObtenerAsync(int id, CancellationToken ct = default)
    {
        // Lectura: basta con estar autenticado, cualquier rol puede consultar.
        if (!sesion.EstaAutenticado)
            return Result<EstudianteDto>.Falla(SinSesion);

        var estudiante = await repositorio.ObtenerPorIdAsync(id, ct);

        return estudiante is null
            ? Result<EstudianteDto>.Falla(NoEncontrado)
            : Result<EstudianteDto>.Exito(estudiante.ToDto());
    }

    public async Task<IReadOnlyList<EstudianteDto>> ListarAsync(
        FiltroEstudiantes? filtro = null, CancellationToken ct = default)
    {
        // Este método no devuelve Result, y llegar aquí sin sesión es un error de programación
        // (la UI no debe ser alcanzable sin login), no un error de negocio: por eso lanza excepción.
        if (!sesion.EstaAutenticado)
            throw new UnauthorizedAccessException("Se intentó consultar estudiantes sin una sesión iniciada.");

        var estudiantes = await repositorio.ListarAsync(filtro ?? new FiltroEstudiantes(), ct);
        return [.. estudiantes.Select(e => e.ToDto())];
    }

    public Task<Result> DesactivarAsync(int id, CancellationToken ct = default) =>
        CambiarEstadoAsync(id, e => e.Desactivar(), ct);

    public Task<Result> ActivarAsync(int id, CancellationToken ct = default) =>
        CambiarEstadoAsync(id, e => e.Activar(), ct);

    private async Task<Result> CambiarEstadoAsync(int id, Action<Estudiante> cambio, CancellationToken ct)
    {
        if (DenegarEscritura() is { } denegado)
            return Result.Falla(denegado);

        var estudiante = await repositorio.ObtenerPorIdAsync(id, ct);
        if (estudiante is null)
            return Result.Falla(NoEncontrado);

        cambio(estudiante);
        await repositorio.ActualizarAsync(estudiante, ct);
        return Result.Exito();
    }
}