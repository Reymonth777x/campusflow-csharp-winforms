using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace CampusFlow.Application.Autenticacion.Validators;

/// <summary>
/// Solo verifica que haya algo escrito. Las reglas de formato NO van aquí a propósito:
/// un login no debe revelar cómo son los usuarios ni las contraseñas válidas.
/// </summary>
public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.NombreUsuario).NotEmpty().WithMessage("Ingrese su usuario.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Ingrese su contraseña.");
    }
}
