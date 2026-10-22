using System;
using System.Collections.Generic;
using System.Text;
namespace CampusFlow.Application.Abstractions;

/// <summary>
/// Contrato de hashing de contraseñas. La implementación vive en Infrastructure: Application
/// no sabe qué algoritmo se usa, y se puede cambiar sin tocar el servicio de login.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Devuelve un hash autocontenido (algoritmo, iteraciones, sal y resultado).</summary>
    string Hash(string password);

    /// <summary>true solo si la contraseña corresponde al hash. Un hash con formato inválido devuelve false, nunca lanza.</summary>
    bool Verify(string password, string hash);
}