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
    IValidator<CrearAdministradorInicialRequest> validadorAdmin) : IAuthService
{
    // El MISMO error para "usuario inexistente" y "contraseña incorrecta": no se puede averiguar qué usuarios existen.
    private static readonly Error CredencialesInvalidas =
        new(TipoError.NoAutorizado, "Usuario o contraseña incorrectos.");

    private static readonly Error CuentaDesactivada =
        new(TipoError.NoAutorizado, "La cuenta está desactivada. Contacte al administrador.");

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