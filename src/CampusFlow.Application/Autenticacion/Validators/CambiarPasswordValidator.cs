using FluentValidation;

namespace CampusFlow.Application.Autenticacion.Validators;

public sealed class CambiarPasswordValidator : AbstractValidator<CambiarPasswordRequest>
{
	public CambiarPasswordValidator()
	{
		RuleFor(x => x.PasswordActual)
			.NotEmpty()
			.WithMessage("Ingrese su contraseña actual.");

		RuleFor(x => x.NuevaPassword)
			.PasswordSegura(_ => null)
			.Must((x, nueva) => nueva != x.PasswordActual)
			.WithMessage("La nueva contraseña debe ser distinta de la actual.");

		RuleFor(x => x.ConfirmarPassword)
			.Equal(x => x.NuevaPassword)
			.WithMessage("Las contraseñas no coinciden.");
	}
}