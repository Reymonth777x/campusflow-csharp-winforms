using CampusFlow.Domain.Common;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;
using Xunit;

namespace CampusFlow.Tests.Domain;

public class UsuarioTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 17, 9, 0, 0, TimeSpan.Zero);

    private static Usuario CrearValido() =>
        Usuario.Crear("jperez", "Juan Pérez", "hash-de-prueba", RolUsuario.Docente, Ahora);

    private static void FallarVeces(Usuario usuario, int veces)
    {
        for (var i = 0; i < veces; i++) usuario.RegistrarIntentoFallido(Ahora);
    }

    [Fact]
    public void Crear_con_datos_validos_asigna_propiedades_y_queda_activo()
    {
        var usuario = CrearValido();

        Assert.Equal("jperez", usuario.NombreUsuario);
        Assert.Equal(RolUsuario.Docente, usuario.Rol);
        Assert.True(usuario.Activo);
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHasta);
        Assert.Null(usuario.UltimoAcceso);
        Assert.Equal(Ahora, usuario.FechaCreacion);
    }

    [Fact]
    public void Crear_recorta_espacios_del_nombre_de_usuario()
    {
        var usuario = Usuario.Crear("  jperez ", "Juan Pérez", "hash", RolUsuario.Docente, Ahora);

        Assert.Equal("jperez", usuario.NombreUsuario);
    }

    [Theory]
    [InlineData("ab")]            // muy corto
    [InlineData("con espacios")]  // espacio no permitido
    [InlineData("juan@mail")]     // arroba no permitida
    [InlineData("")]              // vacío
    public void Crear_con_nombre_de_usuario_invalido_lanza_DomainException(string nombre)
    {
        Assert.Throws<DomainException>(() =>
            Usuario.Crear(nombre, "Juan Pérez", "hash", RolUsuario.Docente, Ahora));
    }

    [Fact]
    public void Crear_con_nombre_de_usuario_demasiado_largo_lanza_DomainException()
    {
        var largo = new string('a', Usuario.MaxNombreUsuario + 1);

        Assert.Throws<DomainException>(() =>
            Usuario.Crear(largo, "Juan Pérez", "hash", RolUsuario.Docente, Ahora));
    }

    [Fact]
    public void Crear_con_nombre_completo_vacio_lanza_DomainException()
    {
        Assert.Throws<DomainException>(() =>
            Usuario.Crear("jperez", "  ", "hash", RolUsuario.Docente, Ahora));
    }

    [Fact]
    public void Crear_con_hash_vacio_lanza_DomainException()
    {
        Assert.Throws<DomainException>(() =>
            Usuario.Crear("jperez", "Juan Pérez", "  ", RolUsuario.Docente, Ahora));
    }

    [Fact]
    public void Crear_con_rol_inexistente_lanza_DomainException()
    {
        Assert.Throws<DomainException>(() =>
            Usuario.Crear("jperez", "Juan Pérez", "hash", (RolUsuario)99, Ahora));
    }

    [Fact]
    public void Cinco_intentos_fallidos_bloquean_la_cuenta_por_el_tiempo_configurado()
    {
        var usuario = CrearValido();

        FallarVeces(usuario, Usuario.MaxIntentosFallidos - 1);
        Assert.False(usuario.EstaBloqueado(Ahora));

        usuario.RegistrarIntentoFallido(Ahora); // el quinto

        Assert.True(usuario.EstaBloqueado(Ahora));
        Assert.Equal(Usuario.DuracionBloqueo, usuario.TiempoRestanteDeBloqueo(Ahora));
    }

    [Fact]
    public void Menos_de_cinco_intentos_no_bloquean()
    {
        var usuario = CrearValido();

        FallarVeces(usuario, 4);

        Assert.False(usuario.EstaBloqueado(Ahora));
        Assert.Equal(4, usuario.IntentosFallidos);
    }

    [Fact]
    public void El_bloqueo_expira_cuando_pasa_el_tiempo()
    {
        var usuario = CrearValido();
        FallarVeces(usuario, Usuario.MaxIntentosFallidos);

        Assert.True(usuario.EstaBloqueado(Ahora + Usuario.DuracionBloqueo - TimeSpan.FromSeconds(1)));
        Assert.False(usuario.EstaBloqueado(Ahora + Usuario.DuracionBloqueo));
    }

    [Fact]
    public void Acceso_exitoso_reinicia_el_contador_y_registra_el_ultimo_acceso()
    {
        var usuario = CrearValido();
        FallarVeces(usuario, 3);
        var despues = Ahora.AddMinutes(1);

        usuario.RegistrarAccesoExitoso(despues);

        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHasta);
        Assert.Equal(despues, usuario.UltimoAcceso);
    }

    [Fact]
    public void Desactivar_y_Activar_cambian_el_estado()
    {
        var usuario = CrearValido();

        usuario.Desactivar();
        Assert.False(usuario.Activo);

        usuario.Activar();
        Assert.True(usuario.Activo);
    }
}