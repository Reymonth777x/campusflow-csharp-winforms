using CampusFlow.Application.Autenticacion.Validators;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Acceso;

/// <summary>Definición visual del cambio de contraseña.</summary>
public sealed partial class FrmCambiarPassword
{
    private readonly ErrorProvider errValidacion = new();
    private readonly TableLayoutPanel tlpPrincipal = new();
    private readonly TableLayoutPanel tlpCampos = new();
    private readonly FlowLayoutPanel flpBotones = new();

    private readonly Label lblIntroduccion = new();
    private readonly Label lblPasswordActual = new();
    private readonly Label lblNuevaPassword = new();
    private readonly Label lblConfirmarPassword = new();
    private readonly Label lblAyuda = new();

    private readonly TextBox txtPasswordActual = new();
    private readonly TextBox txtNuevaPassword = new();
    private readonly TextBox txtConfirmarPassword = new();

    private readonly Button btnGuardar = new();
    private readonly Button btnCancelar = new();

    private void ConstruirInterfaz()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = Tema.Fuente;
        BackColor = Tema.Fondo;
        ForeColor = Tema.Texto;
        FormBorderStyle = FormBorderStyle.FixedDialog; // un modal no se redimensiona
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false; // el modo obligatorio lo vuelve a activar (ver Cargar)
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(520, 400);

        // Enter = Guardar, Esc = Cancelar (el modo obligatorio quita este último).
        AcceptButton = btnGuardar;
        CancelButton = btnCancelar;

        // El icono de error se dibuja a la derecha del control: la 3.ª columna de la tabla le reserva ese espacio.
        errValidacion.ContainerControl = this;
        errValidacion.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        lblIntroduccion.Name = "lblIntroduccion";
        lblIntroduccion.AutoSize = true;
        lblIntroduccion.MaximumSize = new Size(440, 0); // permite salto de línea
        lblIntroduccion.Margin = new Padding(0, 0, 0, 12);

        lblAyuda.Name = "lblAyuda";
        lblAyuda.Text =
            $"La nueva contraseña debe tener al menos {ReglasUsuario.MinPassword} caracteres, " +
            "con letras y números, y ser distinta de la actual.";
        lblAyuda.AutoSize = true;
        lblAyuda.MaximumSize = new Size(440, 0);
        lblAyuda.ForeColor = Tema.TextoAtenuado;
        lblAyuda.Margin = new Padding(0, 8, 0, 0);

        // UseSystemPasswordChar oculta lo que se escribe; MaxLength sale de la política (el mismo tope que el validador).
        foreach (var campo in new[] { txtPasswordActual, txtNuevaPassword, txtConfirmarPassword })
        {
            campo.MaxLength = ReglasUsuario.MaxPassword;
            campo.UseSystemPasswordChar = true;
        }

        txtPasswordActual.Name = "txtPasswordActual";
        txtNuevaPassword.Name = "txtNuevaPassword";
        txtConfirmarPassword.Name = "txtConfirmarPassword";

        tlpCampos.Dock = DockStyle.Fill;
        tlpCampos.AutoSize = true;
        tlpCampos.ColumnCount = 3;
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // etiquetas
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // campos
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));  // icono de error
        tlpCampos.RowCount = 5;
        for (var i = 0; i < 5; i++) tlpCampos.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        tlpCampos.Controls.Add(lblIntroduccion, 0, 0);
        tlpCampos.SetColumnSpan(lblIntroduccion, 3);

        AgregarCampo(1, lblPasswordActual, "Contraseña actual", txtPasswordActual);
        AgregarCampo(2, lblNuevaPassword, "Nueva contraseña", txtNuevaPassword);
        AgregarCampo(3, lblConfirmarPassword, "Confirmar contraseña", txtConfirmarPassword);

        tlpCampos.Controls.Add(lblAyuda, 0, 4);
        tlpCampos.SetColumnSpan(lblAyuda, 3);

        btnGuardar.Name = "btnGuardar";
        btnGuardar.Text = "Cambiar contraseña";
        Tema.EstilizarBoton(btnGuardar, primario: true);

        btnCancelar.Name = "btnCancelar";
        btnCancelar.Text = "Cancelar";
        btnCancelar.DialogResult = DialogResult.Cancel; // cierra el modal sin código adicional
        Tema.EstilizarBoton(btnCancelar, primario: false);

        // RightToLeft: el primer botón agregado queda más a la derecha.
        flpBotones.Dock = DockStyle.Fill;
        flpBotones.AutoSize = true;
        flpBotones.FlowDirection = FlowDirection.RightToLeft;
        flpBotones.Controls.Add(btnGuardar);
        flpBotones.Controls.Add(btnCancelar);

        // Raíz: campos (ocupan el espacio) / botones (alto automático).
        tlpPrincipal.Dock = DockStyle.Fill;
        tlpPrincipal.Padding = new Padding(24);
        tlpPrincipal.ColumnCount = 1;
        tlpPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpPrincipal.RowCount = 2;
        tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpPrincipal.Controls.Add(tlpCampos, 0, 0);
        tlpPrincipal.Controls.Add(flpBotones, 0, 1);

        Controls.Add(tlpPrincipal);
        ResumeLayout(performLayout: true);
    }

    /// <summary>Coloca etiqueta + control en una fila de la tabla.</summary>
    private void AgregarCampo(int fila, Label etiqueta, string texto, Control control)
    {
        etiqueta.Name = "lbl" + control.Name[3..]; // txtPasswordActual → lblPasswordActual
        etiqueta.Text = texto;
        etiqueta.AutoSize = true;
        etiqueta.Anchor = AnchorStyles.Left;
        etiqueta.Margin = new Padding(0, 0, 16, 0);

        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        control.Margin = new Padding(0, 8, 0, 8);

        tlpCampos.Controls.Add(etiqueta, 0, fila);
        tlpCampos.Controls.Add(control, 1, fila);
    }
}