using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace CampusFlow.Application.Estudiantes.Validators;

public sealed class ActualizarEstudianteValidator : AbstractValidator<ActualizarEstudianteRequest>
{
    public ActualizarEstudianteValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El estudiante no es válido.");
        RuleFor(x => x.Nombre).NombreValido();
        RuleFor(x => x.Apellidos).ApellidosValidos();
        RuleFor(x => x.Correo).CorreoValido();
    }
}