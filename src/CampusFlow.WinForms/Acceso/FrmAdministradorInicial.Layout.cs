using CampusFlow.Application.Autenticacion.Validators;
using CampusFlow.Domain.Entities;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Acceso;

/// <summary>Definición visual del alta del administrador inicial.</summary>
public sealed partial class FrmAdministradorInicial
{
    private readonly ErrorProvider errValidacion = new();
    private readonly TableLayoutPanel tlpPrincipal = new();
    private readonly TableLayoutPanel tlpCampos = new();
    private readonly FlowLayoutPanel flpBotones = new();

    private readonly Label lblIntroduccion = new();
    private readonly Label lblNombreCompleto = new();
    private readonly Label lblNombreUsuario = new();
    private readonly Label lblPassword = new();
    private readonly Label lblConfirmarPassword = new();
    private readonly Label lblAyudaPassword = new();

    private readonly TextBox txtNombreCompleto = new();
    private readonly TextBox txtNombreUsuario = new();
    private readonly TextBox txtPassword = new();
    private readonly TextBox txtConfirmarPassword = new();

    private readonly Button btnCrear = new();
    private readonly Button btnSalir = new();

    private void ConstruirInterfaz()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = Tema.Fuente;
        BackColor = Tema.Fondo;
        ForeColor = Tema.Texto;
        Text = "CampusFlow – Configuración inicial";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 470);

        AcceptButton = btnCrear;
        CancelButton = btnSalir;

        errValidacion.ContainerControl = this;
        errValidacion.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        // Los límites salen del dominio y del validador: la UI no deja escribir más de lo que se aceptaría.
        txtNombreCompleto.Name = "txtNombreCompleto";
        txtNombreCompleto.MaxLength = Usuario.MaxNombreCompleto;
        txtNombreUsuario.Name = "txtNombreUsuario";
        txtNombreUsuario.MaxLength = Usuario.MaxNombreUsuario;
        txtPassword.Name = "txtPassword";
        txtPassword.MaxLength = CrearAdministradorInicialValidator.MaxPassword;
        txtPassword.UseSystemPasswordChar = true;
        txtConfirmarPassword.Name = "txtConfirmarPassword";
        txtConfirmarPassword.MaxLength = CrearAdministradorInicialValidator.MaxPassword;
        txtConfirmarPassword.UseSystemPasswordChar = true;

        lblIntroduccion.Name = "lblIntroduccion";
        lblIntroduccion.Text = "Primer arranque: cree la cuenta del administrador. Tendrá acceso total al sistema.";
        lblIntroduccion.AutoSize = true;
        lblIntroduccion.MaximumSize = new Size(440, 0); // permite salto de línea
        lblIntroduccion.Margin = new Padding(0, 0, 0, 12);

        lblAyudaPassword.Name = "lblAyudaPassword";
        lblAyudaPassword.Text =
            $"La contraseña debe tener al menos {CrearAdministradorInicialValidator.MinPassword} caracteres, con letras y números.";
        lblAyudaPassword.AutoSize = true;
        lblAyudaPassword.MaximumSize = new Size(440, 0);
        lblAyudaPassword.ForeColor = Tema.TextoAtenuado;
        lblAyudaPassword.Margin = new Padding(0, 8, 0, 0);

        tlpCampos.Dock = DockStyle.Fill;
        tlpCampos.AutoSize = true;
        tlpCampos.ColumnCount = 3;
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // etiquetas
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // campos
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));  // icono de error
        tlpCampos.RowCount = 6;
        for (var i = 0; i < 6; i++) tlpCampos.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        tlpCampos.Controls.Add(lblIntroduccion, 0, 0);
        tlpCampos.SetColumnSpan(lblIntroduccion, 3);

        AgregarCampo(1, lblNombreCompleto, "Nombre completo", txtNombreCompleto);
        AgregarCampo(2, lblNombreUsuario, "Usuario", txtNombreUsuario);
        AgregarCampo(3, lblPassword, "Contraseña", txtPassword);
        AgregarCampo(4, lblConfirmarPassword, "Confirmar contraseña", txtConfirmarPassword);

        tlpCampos.Controls.Add(lblAyudaPassword, 0, 5);
        tlpCampos.SetColumnSpan(lblAyudaPassword, 3);

        btnCrear.Name = "btnCrear";
        btnCrear.Text = "Crear administrador";
        Tema.EstilizarBoton(btnCrear, primario: true);

        btnSalir.Name = "btnSalir";
        btnSalir.Text = "Salir";
        btnSalir.DialogResult = DialogResult.Cancel;
        Tema.EstilizarBoton(btnSalir, primario: false);

        flpBotones.Dock = DockStyle.Fill;
        flpBotones.AutoSize = true;
        flpBotones.FlowDirection = FlowDirection.RightToLeft;
        flpBotones.Controls.Add(btnCrear);
        flpBotones.Controls.Add(btnSalir);

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
        ActiveControl = txtNombreCompleto;
        ResumeLayout(performLayout: true);
    }

    /// <summary>Coloca etiqueta + control en una fila de la tabla.</summary>
    private void AgregarCampo(int fila, Label etiqueta, string texto, Control control)
    {
        etiqueta.Name = "lbl" + control.Name[3..]; // txtPassword → lblPassword
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