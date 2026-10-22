using CampusFlow.Application.Autenticacion.Validators;
using FluentValidation;

namespace CampusFlow.Application.Usuarios.Validators;

public sealed class ActualizarUsuarioValidator : AbstractValidator<ActualizarUsuarioRequest>
{
    public ActualizarUsuarioValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El usuario no es válido.");
        RuleFor(x => x.NombreCompleto).NombreCompletoValido();
        RuleFor(x => x.Rol).IsInEnum().WithMessage("El rol no es válido.");
    }
}