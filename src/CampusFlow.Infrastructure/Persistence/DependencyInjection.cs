using System.Text;
using CampusFlow.Application.Abstractions;
using CampusFlow.Infrastructure.Persistence;
using CampusFlow.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampusFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string? rutaBaseDatos = null)
    {
        var ruta = rutaBaseDatos ?? DatabasePaths.Default;
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);

        var connectionString = new SqliteConnectionStringBuilder { DataSource = ruta }.ToString();

        services.AddDbContextFactory<CampusFlowDbContext>(o => o.UseSqlite(connectionString));

        // Aquí registramos nuestro repositorio para que la aplicación lo reconozca
        services.AddSingleton<IEstudianteRepository, EstudianteRepository>();

        return services;
    }

    public static void MigrarBaseDeDatos(this IServiceProvider services)
    {
        var factory = services.GetRequiredService<IDbContextFactory<CampusFlowDbContext>>();
        using var db = factory.CreateDbContext();
        db.Database.Migrate();
    }
}