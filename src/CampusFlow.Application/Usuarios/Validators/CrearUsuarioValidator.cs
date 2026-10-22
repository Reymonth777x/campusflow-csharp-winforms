using CampusFlow.Application.Autenticacion.Validators;
using FluentValidation;

namespace CampusFlow.Application.Usuarios.Validators;

public sealed class CrearUsuarioValidator : AbstractValidator<CrearUsuarioRequest>
{
    public CrearUsuarioValidator()
    {
        RuleFor(x => x.NombreUsuario).NombreUsuarioValido();
        RuleFor(x => x.NombreCompleto).NombreCompletoValido();

        // IsInEnum rechaza valores como (RolUsuario)99 que un enum permite en C# aunque no existan.
        RuleFor(x => x.Rol).IsInEnum().WithMessage("El rol no es válido.");

        RuleFor(x => x.Password).PasswordSegura(x => x.NombreUsuario);
        RuleFor(x => x.ConfirmarPassword).Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");
    }
}