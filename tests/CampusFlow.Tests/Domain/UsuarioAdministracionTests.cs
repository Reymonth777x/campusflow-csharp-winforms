using CampusFlow.Domain.Common;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;
using Xunit;

namespace CampusFlow.Tests.Domain;

public class UsuarioAdministracionTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 17, 9, 0, 0, TimeSpan.Zero);

    private static Usuario Crear() =>
        Usuario.Crear("jperez", "Juan Pérez", "hash", RolUsuario.Docente, Ahora);

    [Fact]
    public void CambiarNombreCompleto_recorta_y_actualiza()
    {
        var usuario = Crear();

        usuario.CambiarNombreCompleto("  Juan P. Pérez ");

        Assert.Equal("Juan P. Pérez", usuario.NombreCompleto);
    }

    [Fact]
    public void CambiarNombreCompleto_vacio_lanza_y_conserva_el_anterior()
    {
        var usuario = Crear();

        Assert.Throws<DomainException>(() => usuario.CambiarNombreCompleto("  "));

        Assert.Equal("Juan Pérez", usuario.NombreCompleto);
    }

    [Fact]
    public void Desbloquear_limpia_intentos_y_bloqueo()
    {
        var usuario = Crear();
        for (var i = 0; i < Usuario.MaxIntentosFallidos; i++) usuario.RegistrarIntentoFallido(Ahora);
        Assert.True(usuario.EstaBloqueado(Ahora));

        usuario.Desbloquear();

        Assert.False(usuario.EstaBloqueado(Ahora));
        Assert.Equal(0, usuario.IntentosFallidos);
    }
}