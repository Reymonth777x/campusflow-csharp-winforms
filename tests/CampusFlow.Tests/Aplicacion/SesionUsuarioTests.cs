using CampusFlow.Application.Autenticacion;
using CampusFlow.Domain.Enums;
using Xunit;

namespace CampusFlow.Tests.Aplicacion;

public class SesionUsuarioTests
{
    private static UsuarioDto Con(RolUsuario rol) => new(1, "usuario", "Usuario de Prueba", rol, true);

    [Fact]
    public void Sin_sesion_no_esta_autenticado_y_ningun_rol_coincide()
    {
        var sesion = new SesionUsuario();

        Assert.False(sesion.EstaAutenticado);
        Assert.Null(sesion.Actual);
        Assert.False(sesion.TieneRol(RolUsuario.Docente));
    }

    [Fact]
    public void Iniciar_abre_la_sesion_y_TieneRol_evalua_el_rol_del_usuario()
    {
        var sesion = new SesionUsuario();

        sesion.Iniciar(Con(RolUsuario.Docente));

        Assert.True(sesion.EstaAutenticado);
        Assert.True(sesion.TieneRol(RolUsuario.Administrador, RolUsuario.Docente));
        Assert.False(sesion.TieneRol(RolUsuario.Administrador));
    }

    [Fact]
    public void Cerrar_limpia_la_sesion()
    {
        var sesion = new SesionUsuario();
        sesion.Iniciar(Con(RolUsuario.Docente));

        sesion.Cerrar();

        Assert.False(sesion.EstaAutenticado);
        Assert.Null(sesion.Actual);
    }

    [Theory]
    [InlineData(RolUsuario.Administrador, true)]
    [InlineData(RolUsuario.Docente, true)]
    [InlineData(RolUsuario.Consulta, false)]
    public void Politica_de_escritura_de_estudiantes_segun_rol(RolUsuario rol, bool esperado)
    {
        var sesion = new SesionUsuario();
        sesion.Iniciar(Con(rol));

        Assert.Equal(esperado, sesion.PuedeEscribirEstudiantes());
    }

    [Fact]
    public void Sin_sesion_nadie_puede_escribir_estudiantes() =>
        Assert.False(new SesionUsuario().PuedeEscribirEstudiantes());
}