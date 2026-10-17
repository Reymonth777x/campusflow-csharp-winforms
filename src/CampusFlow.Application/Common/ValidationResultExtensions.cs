using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.Results;

namespace CampusFlow.Application.Common;

internal static class ValidationResultExtensions
{
    public static Error[] ToErrores(this ValidationResult resultado) =>
        [.. resultado.Errors.Select(e => new Error(TipoError.Validacion, e.ErrorMessage, e.PropertyName))];
}