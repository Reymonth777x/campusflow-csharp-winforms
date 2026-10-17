using CampusFlow.Domain.Common;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;
using Xunit;

namespace CampusFlow.Tests.Domain;

public class EstudianteTests
{
    private static readonly DateOnly Fecha = new(2026, 2, 1);

    private static Estudiante CrearValido() =>
        Estudiante.Crear("1-1234-5678", "Juan", "Pérez Mora", "juan.perez@example.com", Fecha);

    [Fact]
    public void Crear_ConDatosValidos_AsignaPropiedadesYQuedaActivo()
    {
        var estudiante = CrearValido();

        Assert.Equal("1-1234-5678", estudiante.Identificacion);
        Assert.Equal("Juan Pérez Mora", estudiante.NombreCompleto);
        Assert.Equal(Fecha, estudiante.FechaIngreso);
        Assert.Equal(EstadoEstudiante.Activo, estudiante.Estado);
    }

    [Fact]
    public void Crear_NormalizaEspaciosYMinusculasEnCorreo()
    {
        var estudiante = Estudiante.Crear("  1-1234-5678 ", "  Juan ", " Pérez ", "  JUAN@Example.COM ", Fecha);

        Assert.Equal("1-1234-5678", estudiante.Identificacion);
        Assert.Equal("Juan", estudiante.Nombre);
        Assert.Equal("Pérez", estudiante.Apellidos);
        Assert.Equal("juan@example.com", estudiante.Correo);
    }

    [Theory]
    [InlineData("", "Juan", "Pérez", "juan@example.com")]
    [InlineData("1-1", "   ", "Pérez", "juan@example.com")]
    [InlineData("1-1", "Juan", null, "juan@example.com")]
    [InlineData("1-1", "Juan", "Pérez", "")]
    public void Crear_ConCampoObligatorioVacio_LanzaDomainException(
        string? identificacion, string? nombre, string? apellidos, string? correo)
    {
        Assert.Throws<DomainException>(() =>
            Estudiante.Crear(identificacion, nombre, apellidos, correo, Fecha));
    }

    [Theory]
    [InlineData("sinarroba")]
    [InlineData("a@")]
    [InlineData("Juan <juan@example.com>")]
    public void Crear_ConCorreoInvalido_LanzaDomainException(string correo)
    {
        Assert.Throws<DomainException>(() =>
            Estudiante.Crear("1-1", "Juan", "Pérez", correo, Fecha));
    }

    [Fact]
    public void Crear_ConNombreMasLargoQueElMaximo_LanzaDomainException()
    {
        var nombreLargo = new string('a', Estudiante.MaxNombre + 1);

        Assert.Throws<DomainException>(() =>
            Estudiante.Crear("1-1", nombreLargo, "Pérez", "juan@example.com", Fecha));
    }

    [Fact]
    public void ActualizarDatos_ConDatosValidos_CambiaDatosPeroNoLaIdentificacion()
    {
        var estudiante = CrearValido();

        estudiante.ActualizarDatos("María", "Rojas", "maria@example.com");

        Assert.Equal("María Rojas", estudiante.NombreCompleto);
        Assert.Equal("maria@example.com", estudiante.Correo);
        Assert.Equal("1-1234-5678", estudiante.Identificacion);
    }

    [Fact]
    public void ActualizarDatos_ConCorreoInvalido_NoModificaNada()
    {
        var estudiante = CrearValido();

        Assert.Throws<DomainException>(() =>
            estudiante.ActualizarDatos("María", "Rojas", "correo-malo"));

        Assert.Equal("Juan", estudiante.Nombre);
        Assert.Equal("juan.perez@example.com", estudiante.Correo);
    }

    [Fact]
    public void Desactivar_Y_Activar_CambianElEstado()
    {
        var estudiante = CrearValido();

        estudiante.Desactivar();
        Assert.Equal(EstadoEstudiante.Inactivo, estudiante.Estado);

        estudiante.Activar();
        Assert.Equal(EstadoEstudiante.Activo, estudiante.Estado);
    }
}