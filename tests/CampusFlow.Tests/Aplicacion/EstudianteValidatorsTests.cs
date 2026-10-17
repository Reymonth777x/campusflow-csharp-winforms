using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Estudiantes;
using CampusFlow.Application.Estudiantes.Validators;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace CampusFlow.Tests.Aplicacion;

public class EstudianteValidatorsTests
{
    // Hoy = 16/10/2026 (reloj fijo: las pruebas no dependen de la fecha real).
    private readonly CrearEstudianteValidator _validador =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 10, 16, 12, 0, 0, TimeSpan.Zero)));

    private static CrearEstudianteRequest Solicitud(
        string identificacion = "1-1234-5678", string nombre = "Juan", string apellidos = "Pérez Mora",
        string correo = "juan@example.com", DateOnly? fecha = null) =>
        new(identificacion, nombre, apellidos, correo, fecha ?? new DateOnly(2026, 2, 1));

    [Fact]
    public void Solicitud_valida_pasa()
    {
        Assert.True(_validador.Validate(Solicitud()).IsValid);
    }

    [Theory]
    [InlineData("", "Juan", "Pérez", "juan@example.com", "Identificacion")]
    [InlineData("1-1", "  ", "Pérez", "juan@example.com", "Nombre")]
    [InlineData("1-1", "Juan", "", "juan@example.com", "Apellidos")]
    [InlineData("1-1", "Juan", "Pérez", "", "Correo")]
    [InlineData("1-1", "Juan", "Pérez", "no-es-correo", "Correo")]
    public void Campo_invalido_reporta_error_en_ese_campo(
        string identificacion, string nombre, string apellidos, string correo, string campo)
    {
        var resultado = _validador.Validate(Solicitud(identificacion, nombre, apellidos, correo));

        Assert.Contains(resultado.Errors, e => e.PropertyName == campo);
    }

    [Fact]
    public void Fecha_de_hoy_es_valida() =>
        Assert.True(_validador.Validate(Solicitud(fecha: new DateOnly(2026, 10, 16))).IsValid);

    [Fact]
    public void Fecha_futura_es_invalida()
    {
        var resultado = _validador.Validate(Solicitud(fecha: new DateOnly(2026, 10, 17)));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "FechaIngreso");
    }

    [Fact]
    public void Fecha_sin_llenar_es_invalida()
    {
        var resultado = _validador.Validate(Solicitud(fecha: default(DateOnly)));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "FechaIngreso");
    }

    [Fact]
    public void Actualizar_con_id_cero_es_invalido()
    {
        var validador = new ActualizarEstudianteValidator();

        var resultado = validador.Validate(new ActualizarEstudianteRequest(0, "Juan", "Pérez", "juan@example.com"));

        Assert.Contains(resultado.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public void Actualizar_valido_pasa()
    {
        var validador = new ActualizarEstudianteValidator();

        Assert.True(validador.Validate(new ActualizarEstudianteRequest(1, "Juan", "Pérez", "juan@example.com")).IsValid);
    }
}