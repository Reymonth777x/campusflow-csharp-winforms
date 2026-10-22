using CampusFlow.Application.Abstractions;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;
using CampusFlow.Infrastructure.Persistence;
using CampusFlow.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CampusFlow.Tests.Persistence;

public sealed class UsuarioRepositoryTests : IDisposable
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 17, 9, 0, 0, TimeSpan.Zero);

    private readonly SqliteConnection _conexion = new("Data Source=:memory:");
    private readonly UsuarioRepository _repo;

    private sealed class Fabrica(DbContextOptions<CampusFlowDbContext> opciones)
        : IDbContextFactory<CampusFlowDbContext>
    {
        public CampusFlowDbContext CreateDbContext() => new(opciones);
    }

    public UsuarioRepositoryTests()
    {
        _conexion.Open(); // la BD en memoria vive mientras la conexión siga abierta
        var opciones = new DbContextOptionsBuilder<CampusFlowDbContext>().UseSqlite(_conexion).Options;

        using (var db = new CampusFlowDbContext(opciones))
            db.Database.Migrate(); // prueba la migración real, incluida AgregarUsuarios

        _repo = new UsuarioRepository(new Fabrica(opciones));
    }

    private static Usuario Crear(string nombre = "jperez") =>
        Usuario.Crear(nombre, "Juan Pérez", "hash-de-prueba", RolUsuario.Docente, Ahora);

    [Fact]
    public async Task Agregar_y_obtener_por_nombre_conserva_los_datos()
    {
        await _repo.AgregarAsync(Crear());

        var leido = await _repo.ObtenerPorNombreAsync("jperez");

        Assert.NotNull(leido);
        Assert.True(leido.Id > 0);
        Assert.Equal("Juan Pérez", leido.NombreCompleto);
        Assert.Equal("hash-de-prueba", leido.PasswordHash);
        Assert.Equal(RolUsuario.Docente, leido.Rol);
        Assert.True(leido.Activo);
        Assert.Equal(Ahora, leido.FechaCreacion);
    }

    [Fact]
    public async Task Obtener_por_nombre_ignora_mayusculas()
    {
        await _repo.AgregarAsync(Crear("JPerez"));

        Assert.NotNull(await _repo.ObtenerPorNombreAsync("jperez"));
        Assert.Null(await _repo.ObtenerPorNombreAsync("otro"));
    }

    [Theory]
    [InlineData("jperez")]
    [InlineData("JPEREZ")]
    public async Task Agregar_nombre_duplicado_lanza_conflicto_sin_importar_mayusculas(string segundo)
    {
        await _repo.AgregarAsync(Crear("jperez"));

        await Assert.ThrowsAsync<ConflictoDeDatosException>(() => _repo.AgregarAsync(Crear(segundo)));
    }

    [Fact]
    public async Task HayUsuarios_es_false_en_BD_vacia_y_true_despues_de_agregar()
    {
        Assert.False(await _repo.HayUsuariosAsync());

        await _repo.AgregarAsync(Crear());

        Assert.True(await _repo.HayUsuariosAsync());
    }

    [Fact]
    public async Task Actualizar_persiste_el_bloqueo_y_el_ultimo_acceso()
    {
        await _repo.AgregarAsync(Crear());

        var cargado = (await _repo.ObtenerPorNombreAsync("jperez"))!;
        for (var i = 0; i < Usuario.MaxIntentosFallidos; i++) cargado.RegistrarIntentoFallido(Ahora);
        await _repo.ActualizarAsync(cargado);

        var releido = (await _repo.ObtenerPorNombreAsync("jperez"))!;
        Assert.True(releido.EstaBloqueado(Ahora));
        Assert.Equal(Ahora + Usuario.DuracionBloqueo, releido.BloqueadoHasta!.Value);
    }

    public void Dispose() => _conexion.Dispose();
}