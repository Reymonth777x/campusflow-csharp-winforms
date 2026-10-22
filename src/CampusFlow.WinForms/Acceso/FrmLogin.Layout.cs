using CampusFlow.Application.Autenticacion.Validators;
using CampusFlow.Domain.Entities;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Acceso;

/// <summary>Definición visual del login.</summary>
public sealed partial class FrmLogin
{
    private static readonly Font FuenteTitulo = new("Segoe UI", 20F, FontStyle.Bold);

    private readonly ErrorProvider errValidacion = new();
    private readonly TableLayoutPanel tlpPrincipal = new();
    private readonly TableLayoutPanel tlpCampos = new();
    private readonly FlowLayoutPanel flpBotones = new();
    private readonly Label lblTitulo = new();
    private readonly Label lblSubtitulo = new();
    private readonly Label lblUsuario = new();
    private readonly Label lblPassword = new();
    private readonly Label lblError = new();
    private readonly TextBox txtUsuario = new();
    private readonly TextBox txtPassword = new();
    private readonly Button btnIngresar = new();
    private readonly Button btnSalir = new();

    private void ConstruirInterfaz()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = Tema.Fuente;
        BackColor = Tema.Fondo;
        ForeColor = Tema.Texto;
        Text = "CampusFlow – Iniciar sesión";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(420, 430);

        // Enter = Ingresar, Esc = Salir.
        AcceptButton = btnIngresar;
        CancelButton = btnSalir;

        errValidacion.ContainerControl = this;
        errValidacion.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        lblTitulo.Name = "lblTitulo";
        lblTitulo.Text = "CampusFlow";
        lblTitulo.Font = FuenteTitulo;
        lblTitulo.ForeColor = Tema.Primario;
        lblTitulo.AutoSize = true;

        lblSubtitulo.Name = "lblSubtitulo";
        lblSubtitulo.Text = "Gestión académica";
        lblSubtitulo.ForeColor = Tema.TextoAtenuado;
        lblSubtitulo.AutoSize = true;
        lblSubtitulo.Margin = new Padding(3, 0, 3, 16);

        lblUsuario.Name = "lblUsuario";
        lblUsuario.Text = "Usuario";
        lblUsuario.AutoSize = true;

        lblPassword.Name = "lblPassword";
        lblPassword.Text = "Contraseña";
        lblPassword.AutoSize = true;

        txtUsuario.Name = "txtUsuario";
        txtUsuario.MaxLength = Usuario.MaxNombreUsuario;

        txtPassword.Name = "txtPassword";
        txtPassword.UseSystemPasswordChar = true; // oculta lo que se escribe
        txtPassword.MaxLength = CrearAdministradorInicialValidator.MaxPassword;

        // Errores generales (credenciales, bloqueo). MaximumSize hace que el texto largo salte de línea.
        lblError.Name = "lblError";
        lblError.AutoSize = true;
        lblError.MaximumSize = new Size(350, 0);
        lblError.ForeColor = Color.Firebrick;
        lblError.Margin = new Padding(0, 12, 0, 0);

        foreach (var campo in new Control[] { txtUsuario, txtPassword })
        {
            campo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            campo.Margin = new Padding(0, 4, 0, 12);
        }

        // 2.ª columna fija: reserva el espacio del icono de error a la derecha de cada campo.
        tlpCampos.Dock = DockStyle.Fill;
        tlpCampos.AutoSize = true;
        tlpCampos.ColumnCount = 2;
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));
        tlpCampos.RowCount = 7;
        for (var i = 0; i < 7; i++) tlpCampos.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AgregarFila(lblTitulo, 0);
        AgregarFila(lblSubtitulo, 1);
        AgregarFila(lblUsuario, 2);
        tlpCampos.Controls.Add(txtUsuario, 0, 3);
        AgregarFila(lblPassword, 4);
        tlpCampos.Controls.Add(txtPassword, 0, 5);
        AgregarFila(lblError, 6);

        btnIngresar.Name = "btnIngresar";
        btnIngresar.Text = "Ingresar";
        Tema.EstilizarBoton(btnIngresar, primario: true);

        btnSalir.Name = "btnSalir";
        btnSalir.Text = "Salir";
        btnSalir.DialogResult = DialogResult.Cancel;
        Tema.EstilizarBoton(btnSalir, primario: false);

        // RightToLeft: el primer botón agregado queda más a la derecha.
        flpBotones.Dock = DockStyle.Fill;
        flpBotones.AutoSize = true;
        flpBotones.FlowDirection = FlowDirection.RightToLeft;
        flpBotones.Controls.Add(btnIngresar);
        flpBotones.Controls.Add(btnSalir);

        tlpPrincipal.Dock = DockStyle.Fill;
        tlpPrincipal.Padding = new Padding(32, 28, 32, 24);
        tlpPrincipal.ColumnCount = 1;
        tlpPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpPrincipal.RowCount = 2;
        tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpPrincipal.Controls.Add(tlpCampos, 0, 0);
        tlpPrincipal.Controls.Add(flpBotones, 0, 1);

        Controls.Add(tlpPrincipal);
        ActiveControl = txtUsuario;
        ResumeLayout(performLayout: true);
    }

    /// <summary>Coloca un control que ocupa el ancho completo (ambas columnas) de la fila indicada.</summary>
    private void AgregarFila(Control control, int fila)
    {
        tlpCampos.Controls.Add(control, 0, fila);
        tlpCampos.SetColumnSpan(control, 2);
    }
}