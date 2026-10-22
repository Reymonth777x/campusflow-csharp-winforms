using CampusFlow.Application.Usuarios;
using CampusFlow.Application.Usuarios.Validators;
using CampusFlow.Domain.Enums;
using Xunit;

namespace CampusFlow.Tests.Aplicacion;

public class UsuarioValidatorsTests
{
    private readonly CrearUsuarioValidator _crear = new();
    private readonly ActualizarUsuarioValidator _actualizar = new();
    private readonly RestablecerPasswordValidator _restablecer = new();

    private static CrearUsuarioRequest Crear(
        string usuario = "mrojas", string nombre = "María Rojas", RolUsuario rol = RolUsuario.Docente,
        string password = "Segura2026", string? confirmar = null) =>
        new(usuario, nombre, rol, password, confirmar ?? password);

    // ───── Crear ─────

    [Fact]
    public void Crear_valido_pasa() =>
        Assert.True(_crear.Validate(Crear()).IsValid);

    [Theory]
    [InlineData("ab", "María Rojas", "Segura2026", "NombreUsuario")]
    [InlineData("mrojas", "", "Segura2026", "NombreCompleto")]
    [InlineData("mrojas", "María Rojas", "corta1", "Password")]
    [InlineData("mrojas", "María Rojas", "sinnumeros", "Password")]
    public void Crear_con_campo_invalido_reporta_error_en_ese_campo(
        string usuario, string nombre, string password, string campo)
    {
        var resultado = _crear.Validate(Crear(usuario, nombre, password: password));

        Assert.Contains(resultado.Errors, e => e.PropertyName == campo);
    }

    [Fact]
    public void Crear_con_confirmacion_distinta_es_invalido()
    {
        var resultado = _crear.Validate(Crear(confirmar: "OtraClave2026"));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "ConfirmarPassword");
    }

    [Fact]
    public void Crear_con_rol_inexistente_es_invalido()
    {
        var resultado = _crear.Validate(Crear(rol: (RolUsuario)99));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "Rol");
    }

    [Fact]
    public void Crear_con_password_igual_al_usuario_es_invalido()
    {
        var resultado = _crear.Validate(Crear(usuario: "mrojas2026", password: "Mrojas2026"));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "Password");
    }

    // ───── Restablecer ─────

    [Fact]
    public void Restablecer_valido_pasa() =>
        Assert.True(_restablecer.Validate(new RestablecerPasswordRequest(1, "NuevaClave2026", "NuevaClave2026")).IsValid);

    [Theory]
    [InlineData("corta1")]
    [InlineData("sinnumeros")]
    public void Restablecer_con_password_debil_es_invalido(string password)
    {
        var resultado = _restablecer.Validate(new RestablecerPasswordRequest(1, password, password));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "NuevaPassword");
    }

    [Fact]
    public void Restablecer_con_confirmacion_distinta_es_invalido()
    {
        var resultado = _restablecer.Validate(new RestablecerPasswordRequest(1, "NuevaClave2026", "OtraClave2026"));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "ConfirmarPassword");
    }

    [Fact]
    public void Restablecer_con_id_cero_es_invalido()
    {
        var resultado = _restablecer.Validate(new RestablecerPasswordRequest(0, "NuevaClave2026", "NuevaClave2026"));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "Id");
    }

    // ───── Actualizar ─────

    [Fact]
    public void Actualizar_valido_pasa() =>
        Assert.True(_actualizar.Validate(new ActualizarUsuarioRequest(1, "María Rojas", RolUsuario.Docente)).IsValid);

    [Theory]
    [InlineData(0, "María Rojas", 2, "Id")]
    [InlineData(1, "", 2, "NombreCompleto")]
    [InlineData(1, "María Rojas", 99, "Rol")]
    public void Actualizar_con_campo_invalido_reporta_error_en_ese_campo(int id, string nombre, int rol, string campo)
    {
        var resultado = _actualizar.Validate(new ActualizarUsuarioRequest(id, nombre, (RolUsuario)rol));

        Assert.Contains(resultado.Errors, e => e.PropertyName == campo);
    }
}
