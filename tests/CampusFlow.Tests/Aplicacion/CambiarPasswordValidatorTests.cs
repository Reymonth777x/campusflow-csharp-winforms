using CampusFlow.Application.Autenticacion;
using CampusFlow.Application.Autenticacion.Validators;
using Xunit;

namespace CampusFlow.Tests.Aplicacion;

public class CambiarPasswordValidatorTests
{
    private readonly CambiarPasswordValidator _validador = new();

    [Fact]
    public void Solicitud_valida_pasa() =>
        Assert.True(_validador.Validate(new CambiarPasswordRequest("Segura2026", "NuevaClave2026", "NuevaClave2026")).IsValid);

    [Theory]
    [InlineData("", "NuevaClave2026", "NuevaClave2026", "PasswordActual")]
    [InlineData("Segura2026", "corta1", "corta1", "NuevaPassword")]
    [InlineData("Segura2026", "sinnumeros", "sinnumeros", "NuevaPassword")]
    [InlineData("Segura2026", "NuevaClave2026", "OtraClave2026", "ConfirmarPassword")]
    public void Campo_invalido_reporta_error_en_ese_campo(string actual, string nueva, string confirmar, string campo)
    {
        var resultado = _validador.Validate(new CambiarPasswordRequest(actual, nueva, confirmar));

        Assert.Contains(resultado.Errors, e => e.PropertyName == campo);
    }

    [Fact]
    public void Nueva_igual_a_la_actual_es_invalida()
    {
        var resultado = _validador.Validate(new CambiarPasswordRequest("Segura2026", "Segura2026", "Segura2026"));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "NuevaPassword");
    }
}