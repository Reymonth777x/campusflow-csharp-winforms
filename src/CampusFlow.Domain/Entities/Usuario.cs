using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CampusFlow.Domain.Common;
using CampusFlow.Domain.Enums;

namespace CampusFlow.Domain.Entities;

/// <summary>
/// Cuenta de acceso al sistema. Guarda solo el HASH de la contraseña (jamás la contraseña) y
/// encapsula la política de bloqueo por intentos fallidos. Todas las fechas son UTC.
/// </summary>
public sealed class Usuario : Entity
{
    public const int MinNombreUsuario = 3;
    public const int MaxNombreUsuario = 30;
    public const int MaxNombreCompleto = 100;
    public const int MaxPasswordHash = 256;

    public const int MaxIntentosFallidos = 5;
    public static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    // \z (y no $): $ aceptaría un salto de línea final, p. ej. "juan\n".
    public const string PatronNombreUsuario = @"^[A-Za-z0-9._-]{3,30}\z";

    public string NombreUsuario { get; private set; } = null!;
    public string NombreCompleto { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public RolUsuario Rol { get; private set; }
    public bool Activo { get; private set; }
    public int IntentosFallidos { get; private set; }
    public DateTimeOffset? BloqueadoHasta { get; private set; }
    public DateTimeOffset? UltimoAcceso { get; private set; }
    public DateTimeOffset FechaCreacion { get; private set; }

    // Requerido por EF Core para materializar la entidad.
    private Usuario() { }

    private Usuario(string nombreUsuario, string nombreCompleto, string passwordHash,
                    RolUsuario rol, DateTimeOffset ahora)
    {
        NombreUsuario = nombreUsuario;
        NombreCompleto = nombreCompleto;
        PasswordHash = passwordHash;
        Rol = rol;
        Activo = true;
        FechaCreacion = ahora;
    }

    /// <summary>
    /// Recibe el HASH ya calculado: el dominio no conoce la contraseña en claro ni cómo se hashea.
    /// La fortaleza de la contraseña se valida en Application, que es la capa que la recibe en claro.
    /// </summary>
    public static Usuario Crear(string? nombreUsuario, string? nombreCompleto, string? passwordHash,
                                RolUsuario rol, DateTimeOffset ahora)
    {
        var usuario = Guard.Requerido(nombreUsuario, "El usuario", MaxNombreUsuario);
        if (!Regex.IsMatch(usuario, PatronNombreUsuario))
            throw new DomainException(
                $"El usuario debe tener entre {MinNombreUsuario} y {MaxNombreUsuario} caracteres: " +
                "letras, números, punto, guion o guion bajo.");

        if (!Enum.IsDefined(rol))
            throw new DomainException("El rol no es válido.");

        return new Usuario(
            usuario,
            Guard.Requerido(nombreCompleto, "El nombre completo", MaxNombreCompleto),
            Guard.Requerido(passwordHash, "El hash de la contraseña", MaxPasswordHash),
            rol,
            ahora);
    }

    // La hora entra como parámetro (no DateTimeOffset.UtcNow): así el bloqueo se prueba sin esperar 15 minutos.
    public bool EstaBloqueado(DateTimeOffset ahora) => BloqueadoHasta is { } hasta && hasta > ahora;

    public TimeSpan TiempoRestanteDeBloqueo(DateTimeOffset ahora) =>
        BloqueadoHasta is { } hasta && hasta > ahora ? hasta - ahora : TimeSpan.Zero;

    /// <summary>Un fallo suma al contador; al llegar al máximo bloquea la cuenta y reinicia el contador.</summary>
    public void RegistrarIntentoFallido(DateTimeOffset ahora)
    {
        IntentosFallidos++;
        if (IntentosFallidos < MaxIntentosFallidos) return;

        BloqueadoHasta = ahora + DuracionBloqueo;
        IntentosFallidos = 0; // al expirar el bloqueo se empieza de cero
    }

    /// <summary>Un acceso correcto borra cualquier rastro de intentos fallidos.</summary>
    public void RegistrarAccesoExitoso(DateTimeOffset ahora)
    {
        IntentosFallidos = 0;
        BloqueadoHasta = null;
        UltimoAcceso = ahora;
    }

    public void CambiarPasswordHash(string? nuevoHash) =>
        PasswordHash = Guard.Requerido(nuevoHash, "El hash de la contraseña", MaxPasswordHash);

    public void CambiarRol(RolUsuario rol)
    {
        if (!Enum.IsDefined(rol)) throw new DomainException("El rol no es válido.");
        Rol = rol;
    }

    public void CambiarNombreCompleto(string? nombreCompleto) =>
        NombreCompleto = Guard.Requerido(nombreCompleto, "El nombre completo", MaxNombreCompleto);

    /// <summary>Quita el bloqueo y reinicia el contador. Lo usa el restablecimiento de contraseña.</summary>
    public void Desbloquear()
    {
        IntentosFallidos = 0;
        BloqueadoHasta = null;
    }

    public void Desactivar() => Activo = false;

    public void Activar() => Activo = true;
}