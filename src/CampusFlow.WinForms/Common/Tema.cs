using System;
using System.Collections.Generic;
using System.Text;
namespace CampusFlow.WinForms.Common;

/// <summary>
/// Paleta y estilos compartidos. Centralizar colores y fuentes aquí evita "números mágicos"
/// repartidos por los formularios y permite cambiar la identidad visual en un solo lugar.
/// </summary>
internal static class Tema
{
    public static readonly Color Fondo = Color.FromArgb(245, 247, 250);
    public static readonly Color Superficie = Color.White;
    public static readonly Color Primario = Color.FromArgb(37, 99, 235);
    public static readonly Color PrimarioHover = Color.FromArgb(29, 78, 216);
    public static readonly Color Texto = Color.FromArgb(17, 24, 39);
    public static readonly Color TextoAtenuado = Color.FromArgb(107, 114, 128);
    public static readonly Color Borde = Color.FromArgb(209, 213, 219);
    public static readonly Color FilaAlterna = Color.FromArgb(249, 250, 251);
    public static readonly Color Seleccion = Color.FromArgb(219, 234, 254);

    // Segoe UI viene con todo Windows: a diferencia de fuentes como Yu Gothic, no hay riesgo de que falte.
    public static readonly Font Fuente = new("Segoe UI", 9.5F);
    public static readonly Font FuenteNegrita = new("Segoe UI", 9.5F, FontStyle.Bold);

    /// <summary>Aplica estilo "plano" a un botón (primario = relleno de color de acento).</summary>
    public static void EstilizarBoton(Button boton, bool primario)
    {
        boton.FlatStyle = FlatStyle.Flat;
        boton.Cursor = Cursors.Hand;
        boton.AutoSize = true;
        boton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        boton.MinimumSize = new Size(104, 34);
        boton.Padding = new Padding(12, 0, 12, 0);
        boton.FlatAppearance.BorderSize = 1;

        // Se repinta al habilitar/deshabilitar: un botón plano con fondo azul y texto gris
        // deshabilitado sería casi ilegible.
        void Pintar()
        {
            if (!boton.Enabled)
            {
                boton.BackColor = Fondo;
                boton.ForeColor = TextoAtenuado;
                boton.FlatAppearance.BorderColor = Borde;
                return;
            }

            boton.BackColor = primario ? Primario : Superficie;
            boton.ForeColor = primario ? Color.White : Texto;
            boton.FlatAppearance.BorderColor = primario ? Primario : Borde;
            boton.FlatAppearance.MouseOverBackColor = primario ? PrimarioHover : Seleccion;
        }

        Pintar();
        boton.EnabledChanged += (_, _) => Pintar();
    }

    /// <summary>Estilo de solo lectura, limpio y con filas alternas para un DataGridView.</summary>
    public static void EstilizarGrid(DataGridView grid)
    {
        grid.BackgroundColor = Superficie;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Borde;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.RowHeadersVisible = false;
        grid.RowTemplate.Height = 32;

        // Sin esto Windows ignora los colores del encabezado y usa el tema del sistema.
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 36;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Fondo;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Texto;
        grid.ColumnHeadersDefaultCellStyle.Font = FuenteNegrita;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Fondo;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

        grid.DefaultCellStyle.BackColor = Superficie;
        grid.DefaultCellStyle.ForeColor = Texto;
        grid.DefaultCellStyle.SelectionBackColor = Seleccion;
        grid.DefaultCellStyle.SelectionForeColor = Texto;
        grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        grid.AlternatingRowsDefaultCellStyle.BackColor = FilaAlterna;
    }
}
