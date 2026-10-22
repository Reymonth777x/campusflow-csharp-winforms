using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms;

/// <summary>Definición visual de la ventana principal.</summary>
public sealed partial class FrmMain
{
    private readonly TableLayoutPanel tlpPrincipal = new();
    private readonly TableLayoutPanel tlpEncabezado = new();
    private readonly FlowLayoutPanel flpModulos = new();
    private readonly Label lblSesion = new();
    private readonly Button btnCerrarSesion = new();
    private readonly Button btnEstudiantes = new();
    private readonly Button btnUsuarios = new();

    private void ConstruirInterfaz()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = Tema.Fuente;
        BackColor = Tema.Fondo;
        ForeColor = Tema.Texto;
        Text = "CampusFlow";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(900, 560);
        MinimumSize = new Size(640, 400);

        lblSesion.Name = "lblSesion";
        lblSesion.AutoSize = true;
        lblSesion.Anchor = AnchorStyles.Left;
        lblSesion.Font = Tema.FuenteNegrita;

        btnCerrarSesion.Name = "btnCerrarSesion";
        btnCerrarSesion.Text = "Cerrar sesión";
        Tema.EstilizarBoton(btnCerrarSesion, primario: false);

        // Encabezado: quién tiene la sesión (izquierda) y el botón de cerrar sesión (derecha).
        tlpEncabezado.Dock = DockStyle.Fill;
        tlpEncabezado.AutoSize = true;
        tlpEncabezado.ColumnCount = 2;
        tlpEncabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpEncabezado.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tlpEncabezado.Controls.Add(lblSesion, 0, 0);
        tlpEncabezado.Controls.Add(btnCerrarSesion, 1, 0);

        btnEstudiantes.Name = "btnEstudiantes";
        btnEstudiantes.Text = "Estudiantes";
        Tema.EstilizarBoton(btnEstudiantes, primario: true);

        btnUsuarios.Name = "btnUsuarios";
        btnUsuarios.Text = "Usuarios";
        Tema.EstilizarBoton(btnUsuarios, primario: true);

        // Un FlowLayoutPanel salta los controles con Visible = false: sin permiso, el botón no ocupa espacio.
        flpModulos.Dock = DockStyle.Fill;
        flpModulos.Padding = new Padding(0, 16, 0, 0);
        flpModulos.Controls.Add(btnEstudiantes);
        flpModulos.Controls.Add(btnUsuarios);

        tlpPrincipal.Dock = DockStyle.Fill;
        tlpPrincipal.Padding = new Padding(16);
        tlpPrincipal.ColumnCount = 1;
        tlpPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpPrincipal.RowCount = 2;
        tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpPrincipal.Controls.Add(tlpEncabezado, 0, 0);
        tlpPrincipal.Controls.Add(flpModulos, 0, 1);

        Controls.Add(tlpPrincipal);
        ResumeLayout(performLayout: true);
    }
}