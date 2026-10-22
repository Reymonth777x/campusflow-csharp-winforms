using CampusFlow.Application.Autenticacion.Validators;
using CampusFlow.Domain.Entities;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Usuarios;

/// <summary>Definición visual del modal de alta/edición de usuarios.</summary>
public sealed partial class FrmUsuarioDetalle
{
    private readonly ErrorProvider errValidacion = new();
    private readonly TableLayoutPanel tlpPrincipal = new();
    private readonly TableLayoutPanel tlpCampos = new();
    private readonly FlowLayoutPanel flpBotones = new();

    private readonly Label lblNombreUsuario = new();
    private readonly Label lblNombreCompleto = new();
    private readonly Label lblRol = new();
    private readonly Label lblPassword = new();
    private readonly Label lblConfirmarPassword = new();
    private readonly Label lblAyudaPassword = new();
    private readonly Label lblNota = new();

    private readonly TextBox txtNombreUsuario = new();
    private readonly TextBox txtNombreCompleto = new();
    private readonly ComboBox cmbRol = new();
    private readonly TextBox txtPassword = new();
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
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(540, 440);

        // Enter = Guardar, Esc = Cancelar (comportamiento estándar de un diálogo).
        AcceptButton = btnGuardar;
        CancelButton = btnCancelar;

        // El icono de error se dibuja a la derecha del control: la 3.ª columna de la tabla le reserva ese espacio.
        errValidacion.ContainerControl = this;
        errValidacion.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        // Los límites salen del dominio y de la política: la UI no deja escribir más de lo que se aceptaría.
        txtNombreUsuario.Name = "txtNombreUsuario";
        txtNombreUsuario.MaxLength = Usuario.MaxNombreUsuario;
        txtNombreCompleto.Name = "txtNombreCompleto";
        txtNombreCompleto.MaxLength = Usuario.MaxNombreCompleto;

        cmbRol.Name = "cmbRol";
        cmbRol.DropDownStyle = ComboBoxStyle.DropDownList; // solo valores de la lista, nunca texto libre
        cmbRol.DataSource = Roles;

        txtPassword.Name = "txtPassword";
        txtPassword.MaxLength = ReglasUsuario.MaxPassword;
        txtPassword.UseSystemPasswordChar = true; // oculta lo que se escribe
        txtConfirmarPassword.Name = "txtConfirmarPassword";
        txtConfirmarPassword.MaxLength = ReglasUsuario.MaxPassword;
        txtConfirmarPassword.UseSystemPasswordChar = true;

        lblAyudaPassword.Name = "lblAyudaPassword";
        lblAyudaPassword.Text =
            $"La contraseña debe tener al menos {ReglasUsuario.MinPassword} caracteres, con letras y números.";
        lblAyudaPassword.AutoSize = true;
        lblAyudaPassword.MaximumSize = new Size(460, 0); // permite salto de línea
        lblAyudaPassword.ForeColor = Tema.TextoAtenuado;
        lblAyudaPassword.Margin = new Padding(0, 8, 0, 0);

        // Aviso visible solo al editar la propia cuenta (ver Cargar).
        lblNota.Name = "lblNota";
        lblNota.Text = "No puede cambiar su propio rol.";
        lblNota.AutoSize = true;
        lblNota.ForeColor = Tema.TextoAtenuado;
        lblNota.Margin = new Padding(0, 8, 0, 0);
        lblNota.Visible = false;

        tlpCampos.Dock = DockStyle.Fill;
        tlpCampos.AutoSize = true;
        tlpCampos.ColumnCount = 3;
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // etiquetas
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // campos
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));  // icono de error
        tlpCampos.RowCount = 7;
        for (var i = 0; i < 7; i++) tlpCampos.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AgregarCampo(0, lblNombreUsuario, "Usuario", txtNombreUsuario);
        AgregarCampo(1, lblNombreCompleto, "Nombre completo", txtNombreCompleto);
        AgregarCampo(2, lblRol, "Rol", cmbRol);
        AgregarCampo(3, lblPassword, "Contraseña", txtPassword);
        AgregarCampo(4, lblConfirmarPassword, "Confirmar contraseña", txtConfirmarPassword);

        tlpCampos.Controls.Add(lblAyudaPassword, 0, 5);
        tlpCampos.SetColumnSpan(lblAyudaPassword, 3);
        tlpCampos.Controls.Add(lblNota, 0, 6);
        tlpCampos.SetColumnSpan(lblNota, 3);

        btnGuardar.Name = "btnGuardar";
        btnGuardar.Text = "Guardar";
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
        etiqueta.Name = "lbl" + control.Name[3..]; // txtPassword → lblPassword (nombre sin su prefijo de 3 letras)
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