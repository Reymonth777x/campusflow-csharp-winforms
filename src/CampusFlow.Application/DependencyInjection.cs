using System;
using System.Collections.Generic;
using System.Text;
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

        // Singleton: ver "Decisiones de diseño" (validación de scopes en WinForms).
        services.AddValidatorsFromAssemblyContaining<CrearEstudianteValidator>(ServiceLifetime.Singleton);

        services.AddTransient<IEstudianteService, EstudianteService>();
        return services;
    }
}