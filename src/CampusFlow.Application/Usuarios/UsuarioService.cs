using CampusFlow.Application.Abstractions;
using CampusFlow.Application.Autenticacion;
using CampusFlow.Application.Common;
using CampusFlow.Domain.Common;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;
using FluentValidation;

namespace CampusFlow.Application.Usuarios;

/// <summary>
/// Administración de cuentas. Defensas, en este orden: (1) autorización: solo Administrador,
/// (2) validación de entrada, (3) reglas de negocio: nada de autolesionarse ni de quedarse sin
/// administradores, (4) el dominio como último seguro.
/// </summary>
public sealed class UsuarioService(
    IUsuarioRepository repositorio,
    IPasswordHasher hasher,
    ISesionUsuario sesion,
    TimeProvider reloj,
    IValidator<CrearUsuarioRequest> validadorCrear,
    IValidator<ActualizarUsuarioRequest> validadorActualizar,
    IValidator<RestablecerPasswordRequest> validadorPassword) : IUsuarioService
{
    private static readonly Error SinSesion =
        new(TipoError.NoAutorizado, "Debe iniciar sesión para continuar.");

    private static readonly Error SinPermiso =
        new(TipoError.NoAutorizado, "No tiene permiso para realizar esta operación.");

    private static readonly Error NoEncontrado =
        new(TipoError.NoEncontrado, "El usuario no existe.");

    private static readonly Error NombreDuplicado = new(
        TipoError.Conflicto, "Ya existe un usuario con ese nombre.", nameof(CrearUsuarioRequest.NombreUsuario));

    private static readonly Error UltimoAdministrador = new(
        TipoError.Conflicto, "No se puede dejar el sistema sin un administrador activo.");

    /// <summary>Autorización. Va ANTES que cualquier otra cosa: a quien no tiene permiso no se le revela nada. Null = permitido.</summary>
    private Error? Denegar()
    {
        if (!sesion.EstaAutenticado) return SinSesion;
        return sesion.PuedeGestionarUsuarios() ? null : SinPermiso;
    }

    private bool EsLaSesionActual(Usuario usuario) => sesion.Actual?.Id == usuario.Id;

    /// <summary>true si el usuario es un administrador activo y es el único: tocarlo dejaría el sistema sin administrador.</summary>
    private async Task<bool> EsUltimoAdministradorActivoAsync(Usuario usuario, CancellationToken ct) =>
        usuario is { Rol: RolUsuario.Administrador, Activo: true }
        && await repositorio.ContarAdministradoresActivosAsync(ct) <= 1;

    public async Task<IReadOnlyList<UsuarioListadoDto>> ListarAsync(CancellationToken ct = default)
    {
        // Sin Result: llegar aquí sin ser administrador es un error de programación (la UI no ofrece
        // la pantalla a otros roles), no un error de negocio. Por eso lanza excepción.
        if (!sesion.PuedeGestionarUsuarios())
            throw new UnauthorizedAccessException("Se intentó listar usuarios sin ser administrador.");

        var ahora = reloj.GetUtcNow();
        var usuarios = await repositorio.ListarAsync(ct);
        return [.. usuarios.Select(u => u.ToListadoDto(ahora))];
    }

    public async Task<Result<UsuarioListadoDto>> CrearAsync(
        CrearUsuarioRequest request, CancellationToken ct = default)
    {
        if (Denegar() is { } denegado)
            return Result<UsuarioListadoDto>.Falla(denegado);

        // Se normaliza ANTES de validar, para que validador y entidad vean exactamente el mismo texto.
        request = request with
        {
            NombreUsuario = request.NombreUsuario.Trim(),
            NombreCompleto = request.NombreCompleto.Trim()
        };

        var validacion = await validadorCrear.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return Result<UsuarioListadoDto>.Falla(validacion.ToErrores());

        if (await repositorio.ObtenerPorNombreAsync(request.NombreUsuario, ct) is not null)
            return Result<UsuarioListadoDto>.Falla(NombreDuplicado);

        // Task.Run: PBKDF2 es CPU intensivo (cientos de ms) y no debe correr en el hilo de la UI.
        var hash = await Task.Run(() => hasher.Hash(request.Password), ct);

        try
        {
            var ahora = reloj.GetUtcNow();
            var usuario = Usuario.Crear(request.NombreUsuario, request.NombreCompleto, hash, request.Rol, ahora);
            usuario.ExigirCambioDePassword(); // la contraseña la eligió el administrador: deberá cambiarla al primer ingreso

            await repositorio.AgregarAsync(usuario, ct);
            return Result<UsuarioListadoDto>.Exito(usuario.ToListadoDto(ahora));
        }
        catch (DomainException ex)
        {
            // Segunda línea de defensa: el dominio rechazó algo que el validador dejó pasar.
            return Result<UsuarioListadoDto>.Falla(new Error(TipoError.Validacion, ex.Message));
        }
        catch (ConflictoDeDatosException)
        {
            // Otro proceso creó el mismo nombre entre la consulta y el guardado: lo atrapa el índice único.
            return Result<UsuarioListadoDto>.Falla(NombreDuplicado);
        }
    }

    public async Task<Result<UsuarioListadoDto>> ActualizarAsync(
        ActualizarUsuarioRequest request, CancellationToken ct = default)
    {
        if (Denegar() is { } denegado)
            return Result<UsuarioListadoDto>.Falla(denegado);

        request = request with { NombreCompleto = request.NombreCompleto.Trim() };

        var validacion = await validadorActualizar.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return Result<UsuarioListadoDto>.Falla(validacion.ToErrores());

        var usuario = await repositorio.ObtenerPorIdAsync(request.Id, ct);
        if (usuario is null)
            return Result<UsuarioListadoDto>.Falla(NoEncontrado);

        // Las reglas de rol solo aplican si el rol realmente cambia.
        if (request.Rol != usuario.Rol)
        {
            // Sin esta regla, un administrador podría degradarse y seguir con la sesión abierta como Administrador.
            if (EsLaSesionActual(usuario))
                return Result<UsuarioListadoDto>.Falla(new Error(
                    TipoError.Conflicto, "No puede cambiar su propio rol.", nameof(ActualizarUsuarioRequest.Rol)));

            if (await EsUltimoAdministradorActivoAsync(usuario, ct))
                return Result<UsuarioListadoDto>.Falla(
                    UltimoAdministrador with { Campo = nameof(ActualizarUsuarioRequest.Rol) });
        }

        try
        {
            usuario.CambiarNombreCompleto(request.NombreCompleto);
            usuario.CambiarRol(request.Rol);
            await repositorio.ActualizarAsync(usuario, ct);
        }
        catch (DomainException ex)
        {
            return Result<UsuarioListadoDto>.Falla(new Error(TipoError.Validacion, ex.Message));
        }

        // Si el administrador editó su propia cuenta, la sesión refleja el nombre nuevo.
        if (EsLaSesionActual(usuario))
            sesion.Iniciar(usuario.ToDto());

        return Result<UsuarioListadoDto>.Exito(usuario.ToListadoDto(reloj.GetUtcNow()));
    }

    public async Task<Result> DesactivarAsync(int id, CancellationToken ct = default)
    {
        if (Denegar() is { } denegado)
            return Result.Falla(denegado);

        var usuario = await repositorio.ObtenerPorIdAsync(id, ct);
        if (usuario is null)
            return Result.Falla(NoEncontrado);

        if (EsLaSesionActual(usuario))
            return Result.Falla(new Error(TipoError.Conflicto, "No puede desactivar su propia cuenta."));

        // Red de seguridad del invariante "siempre hay un administrador activo", independiente de la UI y de la sesión.
        if (await EsUltimoAdministradorActivoAsync(usuario, ct))
            return Result.Falla(UltimoAdministrador);

        usuario.Desactivar();
        await repositorio.ActualizarAsync(usuario, ct);
        return Result.Exito();
    }

    public async Task<Result> ActivarAsync(int id, CancellationToken ct = default)
    {
        if (Denegar() is { } denegado)
            return Result.Falla(denegado);

        var usuario = await repositorio.ObtenerPorIdAsync(id, ct);
        if (usuario is null)
            return Result.Falla(NoEncontrado);

        usuario.Activar();
        await repositorio.ActualizarAsync(usuario, ct);
        return Result.Exito();
    }

    public async Task<Result> RestablecerPasswordAsync(
        RestablecerPasswordRequest request, CancellationToken ct = default)
    {
        if (Denegar() is { } denegado)
            return Result.Falla(denegado);

        var validacion = await validadorPassword.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return Result.Falla(validacion.ToErrores());

        var usuario = await repositorio.ObtenerPorIdAsync(request.Id, ct);
        if (usuario is null)
            return Result.Falla(NoEncontrado);

        // Regla que el validador no puede comprobar porque el request no trae el nombre de usuario.
        if (string.Equals(request.NuevaPassword, usuario.NombreUsuario, StringComparison.OrdinalIgnoreCase))
            return Result.Falla(new Error(
                TipoError.Validacion,
                "La contraseña no puede ser igual al usuario.",
                nameof(RestablecerPasswordRequest.NuevaPassword)));

        var hash = await Task.Run(() => hasher.Hash(request.NuevaPassword), ct);

        usuario.AsignarPasswordTemporal(hash); // temporal: la persona deberá cambiarla al ingresar
        usuario.Desbloquear(); // si no, un usuario bloqueado seguiría sin poder entrar aunque tenga clave nueva
        await repositorio.ActualizarAsync(usuario, ct);
        return Result.Exito();
    }
}