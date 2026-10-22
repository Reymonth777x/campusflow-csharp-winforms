using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Entities;
using FluentValidation;

namespace CampusFlow.Application.Autenticacion.Validators;

public sealed class CrearAdministradorInicialValidator : AbstractValidator<CrearAdministradorInicialRequest>
{
    public const int MinPassword = 8;
    public const int MaxPassword = 128; // tope: PBKDF2 procesa toda la entrada, sin límite sería un vector de abuso

    public CrearAdministradorInicialValidator()
    {
        RuleFor(x => x.NombreUsuario)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El usuario es obligatorio.")
            .Matches(Usuario.PatronNombreUsuario)
            .WithMessage($"El usuario debe tener entre {Usuario.MinNombreUsuario} y {Usuario.MaxNombreUsuario} " +
                         "caracteres: letras, números, punto, guion o guion bajo.");

        RuleFor(x => x.NombreCompleto)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(Usuario.MaxNombreCompleto)
            .WithMessage($"El nombre completo no puede exceder {Usuario.MaxNombreCompleto} caracteres.");

        // Stop: si falla una regla, las siguientes no corren (evita evaluar un Must sobre un texto vacío).
        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(MinPassword).WithMessage($"La contraseña debe tener al menos {MinPassword} caracteres.")
            .MaximumLength(MaxPassword).WithMessage($"La contraseña no puede exceder {MaxPassword} caracteres.")
            .Must(p => p.Any(char.IsLetter) && p.Any(char.IsDigit))
            .WithMessage("La contraseña debe incluir al menos una letra y un número.")
            .Must((x, p) => !string.Equals(p, x.NombreUsuario, StringComparison.OrdinalIgnoreCase))
            .WithMessage("La contraseña no puede ser igual al usuario.");

        RuleFor(x => x.ConfirmarPassword)
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");
    }
}