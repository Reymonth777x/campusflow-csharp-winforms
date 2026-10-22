using CampusFlow.Application.Common;

namespace CampusFlow.WinForms.Common;

/// <summary>
/// Muestra los errores de un Result en un formulario: los que traen campo van junto a su control
/// (ErrorProvider) y los demás en un cuadro de mensaje.
/// </summary>
internal static class ErroresFormulario
{
    /// <param name="controlDe">Traduce Error.Campo (nombre de propiedad del Request) al control que lo captura; null si no hay.</param>
    /// <param name="camposEnOrden">Orden visual de los campos: el foco va al primero que tenga error.</param>
    public static void Mostrar(
        IWin32Window dueno,
        ErrorProvider proveedor,
        IReadOnlyList<Error> errores,
        Func<string?, Control?> controlDe,
        IReadOnlyList<Control> camposEnOrden)
    {
        var generales = new List<string>();

        foreach (var error in errores)
        {
            var control = controlDe(error.Campo);
            if (control is null)
            {
                generales.Add(error.Mensaje);
                continue;
            }

            // Un campo puede traer varios errores: se muestra solo el primero para no abrumar.
            if (proveedor.GetError(control).Length == 0)
                proveedor.SetError(control, error.Mensaje);
        }

        camposEnOrden.FirstOrDefault(c => proveedor.GetError(c).Length > 0)?.Focus();

        if (generales.Count > 0)
            Mensajes.Advertencia(dueno, string.Join(Environment.NewLine, generales));
    }
}