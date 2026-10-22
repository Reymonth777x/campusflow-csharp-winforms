using CampusFlow.Application.Autenticacion;
using CampusFlow.Application.Autenticacion.Validators;
using Xunit;

namespace CampusFlow.Tests.Aplicacion;

public class AutenticacionValidatorsTests
{
    private readonly LoginValidator _login = new();
    private readonly CrearAdministradorInicialValidator _admin = new();

    private static CrearAdministradorInicialRequest Admin(
        string usuario = "admin", string nombre = "Administrador General",
        string password = "Segura2026", string? confirmar = null) =>
        new(usuario, nombre, password, confirmar ?? password);

    [Fact]
    public void Login_valido_pasa() =>
        Assert.True(_login.Validate(new LoginRequest("admin", "Segura2026")).IsValid);

    [Theory]
    [InlineData("", "clave", "NombreUsuario")]
    [InlineData("admin", "", "Password")]
    public void Login_con_campo_vacio_reporta_error_en_ese_campo(string usuario, string password, string campo)
    {
        var resultado = _login.Validate(new LoginRequest(usuario, password));

        Assert.Contains(resultado.Errors, e => e.PropertyName == campo);
    }

    [Fact]
    public void Administrador_valido_pasa() =>
        Assert.True(_admin.Validate(Admin()).IsValid);

    [Theory]
    [InlineData("corta1")]       // menos de 8 caracteres
    [InlineData("sinnumeros")]   // sin dígitos
    [InlineData("12345678")]     // sin letras
    public void Password_que_incumple_la_politica_es_invalida(string password)
    {
        var resultado = _admin.Validate(Admin(password: password));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void Password_igual_al_usuario_es_invalida()
    {
        var resultado = _admin.Validate(Admin(usuario: "juan1234", password: "Juan1234"));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void Confirmacion_distinta_es_invalida()
    {
        var resultado = _admin.Validate(Admin(confirmar: "OtraClave2026"));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "ConfirmarPassword");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("con espacios")]
    public void Nombre_de_usuario_invalido_reporta_error(string usuario)
    {
        var resultado = _admin.Validate(Admin(usuario: usuario));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "NombreUsuario");
    }
}