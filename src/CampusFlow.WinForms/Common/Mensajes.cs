using System;
using System.Collections.Generic;
using System.Text;
namespace CampusFlow.WinForms.Common;

/// <summary>
/// Cuadros de mensaje con título y formato uniformes. Siempre reciben el formulario "dueño"
/// para que el diálogo salga centrado sobre él y bloquee solo esa ventana.
/// </summary>
internal static class Mensajes
{
    private const string Titulo = "CampusFlow";

    public static void Advertencia(IWin32Window? dueno, string texto) =>
        MessageBox.Show(dueno, texto, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    /// <summary>Pregunta Sí/No. "No" es el botón por defecto: Enter por accidente no confirma nada.</summary>
    public static bool Confirmar(IWin32Window? dueno, string texto) =>
        MessageBox.Show(dueno, texto, Titulo, MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
}