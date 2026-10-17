using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Entities;
using FluentValidation;

namespace CampusFlow.Application.Estudiantes.Validators;

public sealed class CrearEstudianteValidator : AbstractValidator<CrearEstudianteRequest>
{
    private static readonly DateOnly FechaMinima = new(1900, 1, 1);

    public CrearEstudianteValidator(TimeProvider reloj)
    {
        RuleFor(x => x.Identificacion)
            .NotEmpty().WithMessage("La identificación es obligatoria.")
            .MaximumLength(Estudiante.MaxIdentificacion)
            .WithMessage($"La identificación no puede exceder {Estudiante.MaxIdentificacion} caracteres.");

        RuleFor(x => x.Nombre).NombreValido();
        RuleFor(x => x.Apellidos).ApellidosValidos();
        RuleFor(x => x.Correo).CorreoValido();

        // Atrapa también el default(DateOnly) de un campo sin llenar.
        RuleFor(x => x.FechaIngreso)
            .Must(f => f >= FechaMinima).WithMessage("La fecha de ingreso no es válida.")
            .Must(f => f <= Hoy(reloj)).WithMessage("La fecha de ingreso no puede ser futura.");
    }

    private static DateOnly Hoy(TimeProvider reloj) =>
        DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
}