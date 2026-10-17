using System;
using System.Collections.Generic;
using System.Text;

namespace CampusFlow.Domain.Common;

internal static class Guard
{
    public static string Requerido(string? valor, string campo, int maxLength)
    {
        var limpio = valor?.Trim();

        if (string.IsNullOrWhiteSpace(limpio))
            throw new DomainException($"{campo} es obligatorio.");

        if (limpio.Length > maxLength)
            throw new DomainException($"{campo} no puede exceder {maxLength} caracteres.");

        return limpio;
    }
}
