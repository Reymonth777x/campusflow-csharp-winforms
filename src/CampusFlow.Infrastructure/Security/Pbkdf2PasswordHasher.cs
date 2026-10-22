using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using CampusFlow.Application.Abstractions;

namespace CampusFlow.Infrastructure.Security;

/// <summary>
/// PBKDF2-HMAC-SHA256 con sal aleatoria. Formato: v1.pbkdf2-sha256.{iteraciones}.{sal}.{hash} (Base64).
/// Las iteraciones viajan dentro del hash: se pueden subir en el futuro y los hashes viejos siguen verificando.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    public const int IteracionesPorDefecto = 600_000;

    private const string Prefijo = "v1.pbkdf2-sha256";
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;

    // Tope sobre las iteraciones LEÍDAS del hash: si alguien manipulara la BD con un número enorme,
    // verificar bloquearía la aplicación.
    private const int MaxIteracionesAceptadas = 5_000_000;

    private readonly int _iteraciones;

    // El parámetro permite usar pocas iteraciones en las pruebas, para que no tarden.
    public Pbkdf2PasswordHasher(int iteraciones = IteracionesPorDefecto)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(iteraciones, 1);
        _iteraciones = iteraciones;
    }

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        // Sal nueva por cada hash: dos usuarios con la misma contraseña obtienen hashes distintos.
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, sal, _iteraciones, HashAlgorithmName.SHA256, TamanoHash);

        return $"{Prefijo}.{_iteraciones}.{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        ArgumentNullException.ThrowIfNull(password);

        // Base64 no contiene '.', así que Split es seguro. Cualquier formato extraño es simplemente "no coincide".
        var partes = (hash ?? string.Empty).Split('.');
        if (partes.Length != 5 || $"{partes[0]}.{partes[1]}" != Prefijo) return false;

        if (!int.TryParse(partes[2], out var iteraciones) || iteraciones < 1 || iteraciones > MaxIteracionesAceptadas)
            return false;

        byte[] sal, esperado;
        try
        {
            sal = Convert.FromBase64String(partes[3]);
            esperado = Convert.FromBase64String(partes[4]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (sal.Length < 8 || esperado.Length is < 16 or > 64) return false;

        var calculado = Rfc2898DeriveBytes.Pbkdf2(password, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

        // FixedTimeEquals: tarda lo mismo acierte o no, para no filtrar información por tiempo de respuesta.
        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }
}