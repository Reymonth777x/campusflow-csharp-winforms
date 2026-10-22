using CampusFlow.Application.Autenticacion;
using CampusFlow.Application.Estudiantes;
using CampusFlow.Application.Estudiantes.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CampusFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        // Singleton: en WinForms no hay scope por petición (ver Fase 3). Registra TODOS los validadores del ensamblado.
        services.AddValidatorsFromAssemblyContaining<CrearEstudianteValidator>(ServiceLifetime.Singleton);

        // La sesión es una sola para toda la aplicación.
        services.AddSingleton<ISesionUsuario, SesionUsuario>();

        services.AddTransient<IEstudianteService, EstudianteService>();
        services.AddTransient<IAuthService, AuthService>();
        return services;
    }
}