using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;
using CampusFlow.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CampusFlow.Tests.Persistence;

public sealed class EstudiantePersistenciaTests : IDisposable
{
    private readonly SqliteConnection _conexion = new("Data Source=:memory:");
    private readonly DbContextOptions<CampusFlowDbContext> _opciones;

    public EstudiantePersistenciaTests()
    {
        _conexion.Open(); // la BD en memoria vive mientras la conexión siga abierta

        _opciones = new DbContextOptionsBuilder<CampusFlowDbContext>()
            .UseSqlite(_conexion)
            .Options;

        using var db = new CampusFlowDbContext(_opciones);
        db.Database.Migrate(); // prueba la migración real
    }

    private CampusFlowDbContext NuevoContexto() => new(_opciones);

    private static Estudiante Crear(string identificacion = "1-1234-5678") =>
        Estudiante.Crear(identificacion, "Juan", "Pérez Mora", "juan@example.com", new DateOnly(2026, 2, 1));

    [Fact]
    public void Guardar_y_leer_conserva_todos_los_datos()
    {
        using (var escritura = NuevoContexto())
        {
            escritura.Estudiantes.Add(Crear());
            escritura.SaveChanges();
        }

        // Contexto distinto: lee de la BD, no del caché del ChangeTracker.
        using var lectura = NuevoContexto();
        var estudiante = lectura.Estudiantes.Single();

        Assert.True(estudiante.Id > 0);
        Assert.Equal("1-1234-5678", estudiante.Identificacion);
        Assert.Equal("Juan Pérez Mora", estudiante.NombreCompleto);
        Assert.Equal("juan@example.com", estudiante.Correo);
        Assert.Equal(new DateOnly(2026, 2, 1), estudiante.FechaIngreso);
        Assert.Equal(EstadoEstudiante.Activo, estudiante.Estado);
    }

    [Theory]
    [InlineData("AB-123", "AB-123")]
    [InlineData("AB-123", "ab-123")]
    public void Identificacion_duplicada_es_rechazada_sin_importar_mayusculas(string primera, string segunda)
    {
        using (var db = NuevoContexto())
        {
            db.Estudiantes.Add(Crear(primera));
            db.SaveChanges();
        }

        using var db2 = NuevoContexto();
        db2.Estudiantes.Add(Crear(segunda));

        Assert.Throws<DbUpdateException>(() => db2.SaveChanges());
    }

    [Fact]
    public void Actualizar_y_desactivar_se_persisten()
    {
        using (var db = NuevoContexto())
        {
            db.Estudiantes.Add(Crear());
            db.SaveChanges();
        }

        using (var db = NuevoContexto())
        {
            var estudiante = db.Estudiantes.Single();
            estudiante.ActualizarDatos("María", "Rojas", "maria@example.com");
            estudiante.Desactivar();
            db.SaveChanges();
        }

        using var lectura = NuevoContexto();
        var guardado = lectura.Estudiantes.Single();

        Assert.Equal("María Rojas", guardado.NombreCompleto);
        Assert.Equal(EstadoEstudiante.Inactivo, guardado.Estado);
    }

    public void Dispose() => _conexion.Dispose();
}