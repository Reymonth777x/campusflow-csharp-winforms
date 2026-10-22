using CampusFlow.Application.Abstractions;

namespace CampusFlow.Tests.Aplicacion;

/// <summary>Hasher instantáneo para probar el servicio sin pagar el costo de PBKDF2 en cada prueba.</summary>
internal sealed class HasherFalso : IPasswordHasher
{
    public string Hash(string password) => "hash::" + password;

    public bool Verify(string password, string hash) => hash == "hash::" + password;
}