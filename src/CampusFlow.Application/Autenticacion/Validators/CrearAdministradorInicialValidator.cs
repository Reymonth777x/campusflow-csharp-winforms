using FluentValidation;

namespace CampusFlow.Application.Autenticacion.Validators;

public sealed class CrearAdministradorInicialValidator : AbstractValidator<CrearAdministradorInicialRequest>
{
    // Alias: la política vive en ReglasUsuario; las constantes se conservan porque los formularios ya las usan.
    public const int MinPassword = ReglasUsuario.MinPassword;
    public const int MaxPassword = ReglasUsuario.MaxPassword;

    public CrearAdministradorInicialValidator()
    {
        RuleFor(x => x.NombreUsuario).NombreUsuarioValido();
        RuleFor(x => x.NombreCompleto).NombreCompletoValido();
        RuleFor(x => x.Password).PasswordSegura(x => x.NombreUsuario);
        RuleFor(x => x.ConfirmarPassword).Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");
    }
}