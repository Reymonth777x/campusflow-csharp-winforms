using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Entities;
using FluentValidation;

namespace CampusFlow.Application.Estudiantes.Validators;

internal static class EstudianteReglas
{
    public static IRuleBuilderOptions<T, string> NombreValido<T>(this IRuleBuilder<T, string> regla) =>
        regla.NotEmpty().WithMessage("El nombre es obligatorio.")
             .MaximumLength(Estudiante.MaxNombre)
             .WithMessage($"El nombre no puede exceder {Estudiante.MaxNombre} caracteres.");

    public static IRuleBuilderOptions<T, string> ApellidosValidos<T>(this IRuleBuilder<T, string> regla) =>
        regla.NotEmpty().WithMessage("Los apellidos son obligatorios.")
             .MaximumLength(Estudiante.MaxApellidos)
             .WithMessage($"Los apellidos no pueden exceder {Estudiante.MaxApellidos} caracteres.");

    public static IRuleBuilderOptions<T, string> CorreoValido<T>(this IRuleBuilder<T, string> regla) =>
        regla.NotEmpty().WithMessage("El correo es obligatorio.")
             .MaximumLength(Estudiante.MaxCorreo)
             .WithMessage($"El correo no puede exceder {Estudiante.MaxCorreo} caracteres.")
             .EmailAddress().WithMessage("El correo no tiene un formato válido.");
}