using CampusFlow.Application.Abstractions;
using CampusFlow.Infrastructure.Persistence;
using CampusFlow.Infrastructure.Persistence.Repositories;
using CampusFlow.Infrastructure.Security;
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

        services.AddSingleton<IEstudianteRepository, EstudianteRepository>();
        services.AddSingleton<IUsuarioRepository, UsuarioRepository>();

        // Con factory explícita: el hasher recibe sus iteraciones por defecto sin depender de cómo el contenedor trata parámetros opcionales.
        services.AddSingleton<IPasswordHasher>(_ => new Pbkdf2PasswordHasher());

        return services;
    }

    public static void MigrarBaseDeDatos(this IServiceProvider services)
    {
        var factory = services.GetRequiredService<IDbContextFactory<CampusFlowDbContext>>();
        using var db = factory.CreateDbContext();
        db.Database.Migrate();
    }
}