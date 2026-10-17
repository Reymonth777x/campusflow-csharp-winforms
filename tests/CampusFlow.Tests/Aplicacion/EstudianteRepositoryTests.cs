using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Abstractions;
using CampusFlow.Application.Estudiantes;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;
using CampusFlow.Infrastructure.Persistence;
using CampusFlow.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CampusFlow.Tests.Persistence;

public sealed class EstudianteRepositoryTests : IDisposable
{
    private readonly SqliteConnection _conexion = new("Data Source=:memory:");
    private readonly EstudianteRepository _repo;

    private sealed class Fabrica(DbContextOptions<CampusFlowDbContext> opciones)
        : IDbContextFactory<CampusFlowDbContext>
    {
        public CampusFlowDbContext CreateDbContext() => new(opciones);
    }

    public EstudianteRepositoryTests()
    {
        _conexion.Open();
        var opciones = new DbContextOptionsBuilder<CampusFlowDbContext>().UseSqlite(_conexion).Options;

        using (var db = new CampusFlowDbContext(opciones))
            db.Database.Migrate();

        _repo = new EstudianteRepository(new Fabrica(opciones));
    }

    private static Estudiante Crear(string identificacion, string nombre = "Juan", string apellidos = "Pérez") =>
        Estudiante.Crear(identificacion, nombre, apellidos, "juan@example.com", new DateOnly(2026, 2, 1));

    [Fact]
    public async Task Agregar_y_obtener_por_id_conserva_los_datos()
    {
        var estudiante = Crear("1-1234-5678");
        await _repo.AgregarAsync(estudiante);

        var leido = await _repo.ObtenerPorIdAsync(estudiante.Id);

        Assert.NotNull(leido);
        Assert.True(leido.Id > 0);
        Assert.Equal("1-1234-5678", leido.Identificacion);
    }

    [Theory]
    [InlineData("AB-123", "AB-123")]
    [InlineData("AB-123", "ab-123")]
    public async Task Agregar_identificacion_duplicada_lanza_conflicto(string primera, string segunda)
    {
        await _repo.AgregarAsync(Crear(primera));

        await Assert.ThrowsAsync<ConflictoDeDatosException>(() => _repo.AgregarAsync(Crear(segunda)));
    }

    [Fact]
    public async Task ExisteIdentificacion_ignora_mayusculas()
    {
        await _repo.AgregarAsync(Crear("AB-123"));

        Assert.True(await _repo.ExisteIdentificacionAsync("ab-123"));
        Assert.False(await _repo.ExisteIdentificacionAsync("ZZ-999"));
    }

    [Fact]
    public async Task Listar_filtra_por_texto_y_trata_los_comodines_como_texto_literal()
    {
        await _repo.AgregarAsync(Crear("1", "Juan", "Pérez"));
        await _repo.AgregarAsync(Crear("2", "Ana 100%", "Mora"));

        var porApellido = await _repo.ListarAsync(new FiltroEstudiantes(Texto: "pérez"));
        var porPorcentaje = await _repo.ListarAsync(new FiltroEstudiantes(Texto: "%"));

        Assert.Equal("1", Assert.Single(porApellido).Identificacion);
        Assert.Equal("2", Assert.Single(porPorcentaje).Identificacion); // sin escapar devolvería ambos
    }

    [Fact]
    public async Task Listar_filtra_por_estado()
    {
        var activo = Crear("1");
        var inactivo = Crear("2");
        await _repo.AgregarAsync(activo);
        await _repo.AgregarAsync(inactivo);

        inactivo.Desactivar();
        await _repo.ActualizarAsync(inactivo);

        var soloInactivos = await _repo.ListarAsync(new FiltroEstudiantes(Estado: EstadoEstudiante.Inactivo));

        Assert.Equal("2", Assert.Single(soloInactivos).Identificacion);
    }

    [Fact]
    public async Task Actualizar_persiste_los_cambios()
    {
        var estudiante = Crear("1");
        await _repo.AgregarAsync(estudiante);

        var cargado = (await _repo.ObtenerPorIdAsync(estudiante.Id))!;
        cargado.ActualizarDatos("María", "Rojas", "maria@example.com");
        await _repo.ActualizarAsync(cargado);

        var releido = (await _repo.ObtenerPorIdAsync(estudiante.Id))!;
        Assert.Equal("María Rojas", releido.NombreCompleto);
        Assert.Equal("maria@example.com", releido.Correo);
    }

    public void Dispose() => _conexion.Dispose();
}