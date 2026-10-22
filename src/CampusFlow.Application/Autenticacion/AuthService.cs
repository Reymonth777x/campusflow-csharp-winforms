using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Abstractions;
using CampusFlow.Application.Common;
using CampusFlow.Domain.Common;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;
using FluentValidation;

namespace CampusFlow.Application.Autenticacion;

public sealed class AuthService(
    IUsuarioRepository repositorio,
    IPasswordHasher hasher,
    ISesionUsuario sesion,
    TimeProvider reloj,
    IValidator<LoginRequest> validadorLogin,
    IValidator<CrearAdministradorInicialRequest> validadorAdmin,
    IValidator<CambiarPasswordRequest> validadorCambio) : IAuthService
{
    // El MISMO error para "usuario inexistente" y "contraseña incorrecta": no se puede averiguar qué usuarios existen.
    private static readonly Error CredencialesInvalidas =
        new(TipoError.NoAutorizado, "Usuario o contraseña incorrectos.");

    private static readonly Error CuentaDesactivada =
        new(TipoError.NoAutorizado, "La cuenta está desactivada. Contacte al administrador.");
    private static readonly Error SinSesion =
    new(TipoError.NoAutorizado, "Debe iniciar sesión para continuar.");

    private static readonly Error CuentaNoDisponible =
        new(TipoError.NoAutorizado, "La cuenta ya no está disponible. Contacte al administrador.");
    // Hash de relleno: se calcula solo si hace falta, y sirve para que "usuario inexistente"
    // tarde lo mismo que "contraseña incorrecta" (si no, la diferencia de tiempo delataría qué usuarios existen).
    private readonly Lazy<string> _hashFalso = new(() => hasher.Hash(Guid.NewGuid().ToString("N")));

    public async Task<Result<UsuarioDto>> IniciarSesionAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validacion = await validadorLogin.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return Result<UsuarioDto>.Falla(validacion.ToErrores());

        var usuario = await repositorio.ObtenerPorNombreAsync(request.NombreUsuario.Trim(), ct);
        var ahora = reloj.GetUtcNow();

        if (usuario is null)
        {
            // Se verifica contra el hash de relleno solo para gastar el mismo tiempo; el resultado se descarta.
            await Task.Run(() => hasher.Verify(request.Password, _hashFalso.Value), ct);
            return Result<UsuarioDto>.Falla(CredencialesInvalidas);
        }

        if (usuario.EstaBloqueado(ahora))
            return Result<UsuarioDto>.Falla(Bloqueada(usuario.TiempoRestanteDeBloqueo(ahora)));

        // Task.Run: PBKDF2 es CPU intensivo (cientos de ms) y no debe correr en el hilo de la UI.
        var correcta = await Task.Run(() => hasher.Verify(request.Password, usuario.PasswordHash), ct);

        if (!correcta)
        {
            usuario.RegistrarIntentoFallido(ahora);

            // CancellationToken.None a propósito: cerrar la ventana a mitad del intento no debe permitir saltarse el contador.
            await repositorio.ActualizarAsync(usuario, CancellationToken.None);

            return Result<UsuarioDto>.Falla(usuario.EstaBloqueado(ahora)
                ? Bloqueada(usuario.TiempoRestanteDeBloqueo(ahora))
                : CredencialesInvalidas);
        }

        // La cuenta desactivada se revela solo con la contraseña correcta: con una incorrecta, mensaje genérico.
        if (!usuario.Activo)
            return Result<UsuarioDto>.Falla(CuentaDesactivada);

        usuario.RegistrarAccesoExitoso(ahora);
        await repositorio.ActualizarAsync(usuario, CancellationToken.None);

        var dto = usuario.ToDto();
        sesion.Iniciar(dto);
        return Result<UsuarioDto>.Exito(dto);
    }

    public void CerrarSesion() => sesion.Cerrar();
    public async Task<Result> CambiarPasswordAsync(CambiarPasswordRequest request, CancellationToken ct = default)
    {
        // Se usa Actual (y no EstaAutenticado) a propósito: una sesión pendiente de cambio de contraseña
        // no cuenta como autenticada para el resto del sistema, pero SÍ debe poder cambiarla.
        if (sesion.Actual is not { } actual)
            return Result.Falla(SinSesion);

        var validacion = await validadorCambio.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return Result.Falla(validacion.ToErrores());

        // Se recarga de la BD: la sesión guarda un DTO sin hash ni contadores, y la cuenta pudo cambiar desde el login.
        var usuario = await repositorio.ObtenerPorIdAsync(actual.Id, ct);
        if (usuario is null || !usuario.Activo)
            return Result.Falla(CuentaNoDisponible);

        var ahora = reloj.GetUtcNow();
        if (usuario.EstaBloqueado(ahora))
            return Result.Falla(Bloqueada(usuario.TiempoRestanteDeBloqueo(ahora)));

        // Task.Run: PBKDF2 es CPU intensivo (cientos de ms) y no debe correr en el hilo de la UI.
        var correcta = await Task.Run(() => hasher.Verify(request.PasswordActual, usuario.PasswordHash), ct);

        if (!correcta)
        {
            // Misma política que el login: sin contador, esta pantalla serviría para adivinar contraseñas sin límite.
            usuario.RegistrarIntentoFallido(ahora);

            // CancellationToken.None a propósito: cerrar la ventana a mitad del intento no debe saltarse el contador.
            await repositorio.ActualizarAsync(usuario, CancellationToken.None);

            return Result.Falla(usuario.EstaBloqueado(ahora)
                ? Bloqueada(usuario.TiempoRestanteDeBloqueo(ahora))
                : new Error(TipoError.Validacion, "La contraseña actual no es correcta.",
                    nameof(CambiarPasswordRequest.PasswordActual)));
        }

        // Regla que el validador no puede comprobar porque el request no trae el nombre de usuario.
        if (string.Equals(request.NuevaPassword, usuario.NombreUsuario, StringComparison.OrdinalIgnoreCase))
            return Result.Falla(new Error(
                TipoError.Validacion,
                "La contraseña no puede ser igual al usuario.",
                nameof(CambiarPasswordRequest.NuevaPassword)));

        var hash = await Task.Run(() => hasher.Hash(request.NuevaPassword), ct);

        usuario.EstablecerPasswordPropia(hash);
        usuario.Desbloquear(); // la contraseña actual era correcta: se borran los intentos fallidos acumulados
        await repositorio.ActualizarAsync(usuario, ct);

        // Refresca la sesión: la bandera pasa a false y desde este momento el usuario queda autenticado de verdad.
        sesion.Iniciar(usuario.ToDto());
        return Result.Exito();
    }

    public Task<bool> HayUsuariosAsync(CancellationToken ct = default) => repositorio.HayUsuariosAsync(ct);

    public async Task<Result<UsuarioDto>> RegistrarAdministradorInicialAsync(
        CrearAdministradorInicialRequest request, CancellationToken ct = default)
    {
        // Se normaliza ANTES de validar, para que validador y entidad vean exactamente el mismo texto.
        request = request with
        {
            NombreUsuario = request.NombreUsuario.Trim(),
            NombreCompleto = request.NombreCompleto.Trim()
        };

        var validacion = await validadorAdmin.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return Result<UsuarioDto>.Falla(validacion.ToErrores());

        // Esta operación es solo de primer arranque. Sin esta guarda, cualquiera podría crear administradores.
        if (await repositorio.HayUsuariosAsync(ct))
            return Result<UsuarioDto>.Falla(
                new Error(TipoError.Conflicto, "El sistema ya tiene usuarios registrados."));

        var hash = await Task.Run(() => hasher.Hash(request.Password), ct);

        try
        {
            var usuario = Usuario.Crear(
                request.NombreUsuario, request.NombreCompleto, hash, RolUsuario.Administrador, reloj.GetUtcNow());

            await repositorio.AgregarAsync(usuario, ct);
            return Result<UsuarioDto>.Exito(usuario.ToDto());
        }
        catch (DomainException ex)
        {
            // Segunda línea de defensa: el dominio rechazó algo que el validador dejó pasar.
            return Result<UsuarioDto>.Falla(new Error(TipoError.Validacion, ex.Message));
        }
        catch (ConflictoDeDatosException)
        {
            return Result<UsuarioDto>.Falla(new Error(
                TipoError.Conflicto, "Ya existe un usuario con ese nombre.",
                nameof(CrearAdministradorInicialRequest.NombreUsuario)));
        }
    }

    private static Error Bloqueada(TimeSpan restante)
    {
        // Math.Ceiling: nunca mostrar "0 minutos" mientras la cuenta siga bloqueada.
        var minutos = Math.Max(1, (int)Math.Ceiling(restante.TotalMinutes));
        return new Error(TipoError.NoAutorizado,
            $"Cuenta bloqueada por demasiados intentos. Intente de nuevo en {minutos} minuto(s).");
    }
}