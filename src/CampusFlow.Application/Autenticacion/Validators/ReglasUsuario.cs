using CampusFlow.Domain.Entities;
using FluentValidation;

namespace CampusFlow.Application.Autenticacion.Validators;

/// <summary>
/// Reglas de validación compartidas por todos los validadores que tocan cuentas de usuario.
/// La política de contraseña vive SOLO aquí: cambiarla es editar este archivo.
/// </summary>
public static class ReglasUsuario
{
    public const int MinPassword = 8;
    public const int MaxPassword = 128; // tope: PBKDF2 procesa toda la entrada, sin límite sería un vector de abuso

    public static IRuleBuilderOptions<T, string> NombreUsuarioValido<T>(this IRuleBuilderInitial<T, string> regla) =>
        regla.Cascade(CascadeMode.Stop)
             .NotEmpty().WithMessage("El usuario es obligatorio.")
             .Matches(Usuario.PatronNombreUsuario)
             .WithMessage($"El usuario debe tener entre {Usuario.MinNombreUsuario} y {Usuario.MaxNombreUsuario} " +
                          "caracteres: letras, números, punto, guion o guion bajo.");

    public static IRuleBuilderOptions<T, string> NombreCompletoValido<T>(this IRuleBuilderInitial<T, string> regla) =>
        regla.Cascade(CascadeMode.Stop)
             .NotEmpty().WithMessage("El nombre completo es obligatorio.")
             .MaximumLength(Usuario.MaxNombreCompleto)
             .WithMessage($"El nombre completo no puede exceder {Usuario.MaxNombreCompleto} caracteres.");

    /// <param name="nombreUsuario">
    /// Cómo obtener el usuario del request para prohibir "contraseña = usuario". Si el request no lo trae
    /// (restablecer), se pasa <c>_ => null</c> y esa comprobación la hace el servicio con el usuario cargado.
    /// </param>
    public static IRuleBuilderOptions<T, string> PasswordSegura<T>(
        this IRuleBuilderInitial<T, string> regla, Func<T, string?> nombreUsuario) =>
        // Stop: si falla una regla, las siguientes no corren (evita evaluar un Must sobre un texto vacío).
        regla.Cascade(CascadeMode.Stop)
             .NotEmpty().WithMessage("La contraseña es obligatoria.")
             .MinimumLength(MinPassword).WithMessage($"La contraseña debe tener al menos {MinPassword} caracteres.")
             .MaximumLength(MaxPassword).WithMessage($"La contraseña no puede exceder {MaxPassword} caracteres.")
             .Must(p => p.Any(char.IsLetter) && p.Any(char.IsDigit))
             .WithMessage("La contraseña debe incluir al menos una letra y un número.")
             .Must((x, p) => !string.Equals(p, nombreUsuario(x), StringComparison.OrdinalIgnoreCase))
             .WithMessage("La contraseña no puede ser igual al usuario.");
}