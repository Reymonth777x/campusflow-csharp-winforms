using CampusFlow.Application.Autenticacion.Validators;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Usuarios;

/// <summary>Definición visual del modal de restablecer contraseña.</summary>
public sealed partial class FrmRestablecerPassword
{
    private readonly ErrorProvider errValidacion = new();
    private readonly TableLayoutPanel tlpPrincipal = new();
    private readonly TableLayoutPanel tlpCampos = new();
    private readonly FlowLayoutPanel flpBotones = new();

    private readonly Label lblDestino = new();
    private readonly Label lblNuevaPassword = new();
    private readonly Label lblConfirmarPassword = new();
    private readonly Label lblAyuda = new();

    private readonly TextBox txtNuevaPassword = new();
    private readonly TextBox txtConfirmarPassword = new();

    private readonly Button btnRestablecer = new();
    private readonly Button btnCancelar = new();

    private void ConstruirInterfaz()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = Tema.Fuente;
        BackColor = Tema.Fondo;
        ForeColor = Tema.Texto;
        Text = "Restablecer contraseña";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(520, 330);

        AcceptButton = btnRestablecer;
        CancelButton = btnCancelar;

        errValidacion.ContainerControl = this;
        errValidacion.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        lblDestino.Name = "lblDestino";
        lblDestino.AutoSize = true;
        lblDestino.Font = Tema.FuenteNegrita;
        lblDestino.MaximumSize = new Size(440, 0);
        lblDestino.Margin = new Padding(0, 0, 0, 12);

        lblAyuda.Name = "lblAyuda";
        lblAyuda.Text =
            $"La contraseña debe tener al menos {ReglasUsuario.MinPassword} caracteres, con letras y números. " +
            "Al guardar, la cuenta también se desbloquea.";
        lblAyuda.AutoSize = true;
        lblAyuda.MaximumSize = new Size(440, 0);
        lblAyuda.ForeColor = Tema.TextoAtenuado;
        lblAyuda.Margin = new Padding(0, 8, 0, 0);

        txtNuevaPassword.Name = "txtNuevaPassword";
        txtNuevaPassword.MaxLength = ReglasUsuario.MaxPassword;
        txtNuevaPassword.UseSystemPasswordChar = true;
        txtConfirmarPassword.Name = "txtConfirmarPassword";
        txtConfirmarPassword.MaxLength = ReglasUsuario.MaxPassword;
        txtConfirmarPassword.UseSystemPasswordChar = true;

        tlpCampos.Dock = DockStyle.Fill;
        tlpCampos.AutoSize = true;
        tlpCampos.ColumnCount = 3;
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // etiquetas
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // campos
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));  // icono de error
        tlpCampos.RowCount = 4;
        for (var i = 0; i < 4; i++) tlpCampos.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        tlpCampos.Controls.Add(lblDestino, 0, 0);
        tlpCampos.SetColumnSpan(lblDestino, 3);

        AgregarCampo(1, lblNuevaPassword, "Nueva contraseña", txtNuevaPassword);
        AgregarCampo(2, lblConfirmarPassword, "Confirmar contraseña", txtConfirmarPassword);

        tlpCampos.Controls.Add(lblAyuda, 0, 3);
        tlpCampos.SetColumnSpan(lblAyuda, 3);

        btnRestablecer.Name = "btnRestablecer";
        btnRestablecer.Text = "Restablecer";
        Tema.EstilizarBoton(btnRestablecer, primario: true);

        btnCancelar.Name = "btnCancelar";
        btnCancelar.Text = "Cancelar";
        btnCancelar.DialogResult = DialogResult.Cancel;
        Tema.EstilizarBoton(btnCancelar, primario: false);

        flpBotones.Dock = DockStyle.Fill;
        flpBotones.AutoSize = true;
        flpBotones.FlowDirection = FlowDirection.RightToLeft;
        flpBotones.Controls.Add(btnRestablecer);
        flpBotones.Controls.Add(btnCancelar);

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
        etiqueta.Name = "lbl" + control.Name[3..];
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