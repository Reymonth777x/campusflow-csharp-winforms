using CampusFlow.Domain.Common;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;
using Xunit;

namespace CampusFlow.Tests.Domain;

public class UsuarioPasswordTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 22, 9, 0, 0, TimeSpan.Zero);

    private static Usuario Crear() =>
        Usuario.Crear("jperez", "Juan Pérez", "hash-original", RolUsuario.Docente, Ahora);

    [Fact]
    public void Un_usuario_nuevo_no_tiene_cambio_de_password_pendiente() =>
        Assert.False(Crear().DebeCambiarPassword);

    [Fact]
    public void ExigirCambioDePassword_activa_la_bandera()
    {
        var usuario = Crear();

        usuario.ExigirCambioDePassword();

        Assert.True(usuario.DebeCambiarPassword);
    }

    [Fact]
    public void AsignarPasswordTemporal_cambia_el_hash_y_exige_el_cambio()
    {
        var usuario = Crear();

        usuario.AsignarPasswordTemporal("hash-temporal");

        Assert.Equal("hash-temporal", usuario.PasswordHash);
        Assert.True(usuario.DebeCambiarPassword);
    }

    [Fact]
    public void EstablecerPasswordPropia_cambia_el_hash_y_levanta_la_exigencia()
    {
        var usuario = Crear();
        usuario.ExigirCambioDePassword();

        usuario.EstablecerPasswordPropia("hash-propio");

        Assert.Equal("hash-propio", usuario.PasswordHash);
        Assert.False(usuario.DebeCambiarPassword);
    }

    [Fact]
    public void Un_hash_vacio_lanza_y_no_modifica_nada()
    {
        var usuario = Crear();

        Assert.Throws<DomainException>(() => usuario.AsignarPasswordTemporal("  "));
        Assert.Throws<DomainException>(() => usuario.EstablecerPasswordPropia(null));

        Assert.Equal("hash-original", usuario.PasswordHash);
        Assert.False(usuario.DebeCambiarPassword);
    }
}