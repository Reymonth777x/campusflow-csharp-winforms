using CampusFlow.Application.Autenticacion.Validators;
using FluentValidation;

namespace CampusFlow.Application.Usuarios.Validators;

public sealed class RestablecerPasswordValidator : AbstractValidator<RestablecerPasswordRequest>
{
    public RestablecerPasswordValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El usuario no es válido.");

        // El request no trae el nombre de usuario: "contraseña = usuario" la comprueba el servicio con el usuario ya cargado.
        RuleFor(x => x.NuevaPassword).PasswordSegura(_ => null);

        RuleFor(x => x.ConfirmarPassword).Equal(x => x.NuevaPassword).WithMessage("Las contraseñas no coinciden.");
    }
}
