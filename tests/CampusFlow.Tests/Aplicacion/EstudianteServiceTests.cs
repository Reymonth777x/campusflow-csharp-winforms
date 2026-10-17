using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Common;
using CampusFlow.Application.Estudiantes;
using CampusFlow.Application.Estudiantes.Validators;
using CampusFlow.Domain.Enums;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace CampusFlow.Tests.Aplicacion;

public class EstudianteServiceTests
{
    private readonly RepositorioEnMemoria _repo = new();
    private readonly EstudianteService _servicio;

    public EstudianteServiceTests()
    {
        var reloj = new FakeTimeProvider(new DateTimeOffset(2026, 10, 16, 12, 0, 0, TimeSpan.Zero));
        _servicio = new EstudianteService(
            _repo, new CrearEstudianteValidator(reloj), new ActualizarEstudianteValidator());
    }

    private static CrearEstudianteRequest Solicitud(string identificacion = "1-1234-5678", string nombre = "Juan") =>
        new(identificacion, nombre, "Pérez Mora", "juan@example.com", new DateOnly(2026, 2, 1));

    [Fact]
    public async Task Crear_valido_devuelve_dto_con_id()
    {
        var resultado = await _servicio.CrearAsync(Solicitud());

        Assert.True(resultado.EsExito);
        Assert.Equal(1, resultado.Valor.Id);
        Assert.Equal("Juan Pérez Mora", resultado.Valor.NombreCompleto);
        Assert.Equal(EstadoEstudiante.Activo, resultado.Valor.Estado);
    }

    [Fact]
    public async Task Crear_con_identificacion_existente_devuelve_conflicto()
    {
        await _servicio.CrearAsync(Solicitud("AB-123"));

        var resultado = await _servicio.CrearAsync(Solicitud("ab-123"));

        var error = Assert.Single(resultado.Errores);
        Assert.Equal(TipoError.Conflicto, error.Tipo);
        Assert.Equal("Identificacion", error.Campo);
        Assert.Equal(1, _repo.Cantidad);
    }

    [Fact]
    public async Task Crear_con_datos_invalidos_devuelve_validacion_y_no_guarda()
    {
        var resultado = await _servicio.CrearAsync(Solicitud(nombre: ""));

        Assert.True(resultado.EsFalla);
        Assert.Contains(resultado.Errores, e => e.Tipo == TipoError.Validacion && e.Campo == "Nombre");
        Assert.Equal(0, _repo.Cantidad);
    }

    [Fact]
    public async Task Crear_si_la_bd_rechaza_por_duplicado_devuelve_conflicto()
    {
        _repo.SimularConflictoAlAgregar = true; // otro usuario ganó la carrera

        var resultado = await _servicio.CrearAsync(Solicitud());

        var error = Assert.Single(resultado.Errores);
        Assert.Equal(TipoError.Conflicto, error.Tipo);
    }

    [Fact]
    public async Task Actualizar_valido_cambia_los_datos()
    {
        var creado = (await _servicio.CrearAsync(Solicitud())).Valor;

        var resultado = await _servicio.ActualizarAsync(
            new ActualizarEstudianteRequest(creado.Id, "María", "Rojas", "maria@example.com"));

        Assert.True(resultado.EsExito);
        Assert.Equal("María Rojas", resultado.Valor.NombreCompleto);
        Assert.Equal("1-1234-5678", resultado.Valor.Identificacion);
    }

    [Fact]
    public async Task Actualizar_inexistente_devuelve_no_encontrado()
    {
        var resultado = await _servicio.ActualizarAsync(
            new ActualizarEstudianteRequest(99, "María", "Rojas", "maria@example.com"));

        Assert.Equal(TipoError.NoEncontrado, Assert.Single(resultado.Errores).Tipo);
    }

    [Fact]
    public async Task Desactivar_cambia_el_estado()
    {
        var creado = (await _servicio.CrearAsync(Solicitud())).Valor;

        var resultado = await _servicio.DesactivarAsync(creado.Id);

        Assert.True(resultado.EsExito);
        Assert.Equal(EstadoEstudiante.Inactivo, (await _servicio.ObtenerAsync(creado.Id)).Valor.Estado);
    }

    [Fact]
    public async Task Desactivar_inexistente_devuelve_no_encontrado()
    {
        var resultado = await _servicio.DesactivarAsync(99);

        Assert.Equal(TipoError.NoEncontrado, Assert.Single(resultado.Errores).Tipo);
    }

    [Fact]
    public async Task Obtener_inexistente_devuelve_no_encontrado()
    {
        var resultado = await _servicio.ObtenerAsync(99);

        Assert.Equal(TipoError.NoEncontrado, Assert.Single(resultado.Errores).Tipo);
    }
}