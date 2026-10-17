using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Abstractions;
using CampusFlow.Application.Common;
using CampusFlow.Domain.Common;
using CampusFlow.Domain.Entities;
using FluentValidation;

namespace CampusFlow.Application.Estudiantes;

public sealed class EstudianteService(
    IEstudianteRepository repositorio,
    IValidator<CrearEstudianteRequest> validadorCrear,
    IValidator<ActualizarEstudianteRequest> validadorActualizar) : IEstudianteService
{
    private static Error Duplicado => new(
        TipoError.Conflicto,
        "Ya existe un estudiante con esa identificación.",
        nameof(CrearEstudianteRequest.Identificacion));

    private static Error NoEncontrado => new(
        TipoError.NoEncontrado,
        "El estudiante no existe.");

    public async Task<Result<EstudianteDto>> CrearAsync(
        CrearEstudianteRequest request, CancellationToken ct = default)
    {
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
        var estudiante = await repositorio.ObtenerPorIdAsync(id, ct);

        return estudiante is null
            ? Result<EstudianteDto>.Falla(NoEncontrado)
            : Result<EstudianteDto>.Exito(estudiante.ToDto());
    }

    public async Task<IReadOnlyList<EstudianteDto>> ListarAsync(
        FiltroEstudiantes? filtro = null, CancellationToken ct = default)
    {
        var estudiantes = await repositorio.ListarAsync(filtro ?? new FiltroEstudiantes(), ct);
        return [.. estudiantes.Select(e => e.ToDto())];
    }

    public Task<Result> DesactivarAsync(int id, CancellationToken ct = default) =>
        CambiarEstadoAsync(id, e => e.Desactivar(), ct);

    public Task<Result> ActivarAsync(int id, CancellationToken ct = default) =>
        CambiarEstadoAsync(id, e => e.Activar(), ct);

    private async Task<Result> CambiarEstadoAsync(int id, Action<Estudiante> cambio, CancellationToken ct)
    {
        var estudiante = await repositorio.ObtenerPorIdAsync(id, ct);
        if (estudiante is null)
            return Result.Falla(NoEncontrado);

        cambio(estudiante);
        await repositorio.ActualizarAsync(estudiante, ct);
        return Result.Exito();
    }
}