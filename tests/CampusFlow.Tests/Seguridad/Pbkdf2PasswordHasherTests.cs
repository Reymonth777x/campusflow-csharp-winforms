using CampusFlow.Infrastructure.Security;
using Xunit;

namespace CampusFlow.Tests.Seguridad;

public class Pbkdf2PasswordHasherTests
{
    // Pocas iteraciones: estas pruebas verifican la lógica, no la dureza del algoritmo, y no deben tardar.
    private readonly Pbkdf2PasswordHasher _hasher = new(iteraciones: 1_000);

    [Fact]
    public void Hash_y_Verify_con_la_misma_contrasena_coinciden()
    {
        var hash = _hasher.Hash("Segura2026");

        Assert.True(_hasher.Verify("Segura2026", hash));
    }

    [Fact]
    public void Verify_con_contrasena_incorrecta_devuelve_false()
    {
        var hash = _hasher.Hash("Segura2026");

        Assert.False(_hasher.Verify("segura2026", hash)); // distinta solo en una mayúscula
    }

    [Fact]
    public void Dos_hashes_de_la_misma_contrasena_son_distintos_por_la_sal()
    {
        Assert.NotEqual(_hasher.Hash("Segura2026"), _hasher.Hash("Segura2026"));
    }

    [Fact]
    public void Por_defecto_usa_600000_iteraciones()
    {
        var hash = new Pbkdf2PasswordHasher().Hash("Segura2026");

        Assert.Contains(".600000.", hash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("v1.pbkdf2-sha256.x.y.z")]            // iteraciones no numéricas
    [InlineData("v1.pbkdf2-sha256.1000.@@@.@@@")]     // Base64 inválido
    public void Verify_con_formato_invalido_devuelve_false_sin_lanzar(string hashInvalido)
    {
        Assert.False(_hasher.Verify("Segura2026", hashInvalido));
    }

    [Fact]
    public void Verify_usa_las_iteraciones_guardadas_en_el_hash_no_las_propias()
    {
        var hashViejo = new Pbkdf2PasswordHasher(1_000).Hash("Segura2026");

        // Un hasher configurado con otras iteraciones debe poder verificar hashes antiguos.
        Assert.True(new Pbkdf2PasswordHasher(2_000).Verify("Segura2026", hashViejo));
    }
}