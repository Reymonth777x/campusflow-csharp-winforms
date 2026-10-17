using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Entities;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Estudiantes;

/// <summary>Definición visual del modal de alta/edición.</summary>
public sealed partial class FrmEstudianteDetalle
{
    private readonly ErrorProvider errValidacion = new();
    private readonly TableLayoutPanel tlpPrincipal = new();
    private readonly TableLayoutPanel tlpCampos = new();
    private readonly FlowLayoutPanel flpBotones = new();

    private readonly Label lblIdentificacion = new();
    private readonly Label lblNombre = new();
    private readonly Label lblApellidos = new();
    private readonly Label lblCorreo = new();
    private readonly Label lblFechaIngreso = new();
    private readonly Label lblNota = new();

    private readonly TextBox txtIdentificacion = new();
    private readonly TextBox txtNombre = new();
    private readonly TextBox txtApellidos = new();
    private readonly TextBox txtCorreo = new();
    private readonly DateTimePicker dtpFechaIngreso = new();

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
        ClientSize = new Size(520, 360);

        // Enter = Guardar, Esc = Cancelar (comportamiento estándar de un diálogo).
        AcceptButton = btnGuardar;
        CancelButton = btnCancelar;

        // El icono de error se dibuja a la derecha del control: la 3.ª columna de la tabla le reserva ese espacio.
        errValidacion.ContainerControl = this;
        errValidacion.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        // Los límites salen del dominio: la UI no deja escribir más de lo que la entidad aceptaría.
        txtIdentificacion.Name = "txtIdentificacion";
        txtIdentificacion.MaxLength = Estudiante.MaxIdentificacion;
        txtNombre.Name = "txtNombre";
        txtNombre.MaxLength = Estudiante.MaxNombre;
        txtApellidos.Name = "txtApellidos";
        txtApellidos.MaxLength = Estudiante.MaxApellidos;
        txtCorreo.Name = "txtCorreo";
        txtCorreo.MaxLength = Estudiante.MaxCorreo;

        dtpFechaIngreso.Name = "dtpFechaIngreso";
        dtpFechaIngreso.Format = DateTimePickerFormat.Short;
        dtpFechaIngreso.MinDate = new DateTime(1900, 1, 1); // mismo mínimo que CrearEstudianteValidator

        tlpCampos.Dock = DockStyle.Fill;
        tlpCampos.AutoSize = true;
        tlpCampos.ColumnCount = 3;
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));          // etiquetas
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));     // campos
        tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));     // icono de error
        tlpCampos.RowCount = 6;
        for (var i = 0; i < 6; i++) tlpCampos.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AgregarCampo(0, lblIdentificacion, "Identificación", txtIdentificacion);
        AgregarCampo(1, lblNombre, "Nombre", txtNombre);
        AgregarCampo(2, lblApellidos, "Apellidos", txtApellidos);
        AgregarCampo(3, lblCorreo, "Correo", txtCorreo);
        AgregarCampo(4, lblFechaIngreso, "Fecha de ingreso", dtpFechaIngreso);

        // Aviso visible solo en edición (ver Cargar).
        lblNota.Name = "lblNota";
        lblNota.Text = "La identificación y la fecha de ingreso no se pueden modificar.";
        lblNota.AutoSize = true;
        lblNota.ForeColor = Tema.TextoAtenuado;
        lblNota.Margin = new Padding(0, 8, 0, 0);
        lblNota.Visible = false;
        tlpCampos.Controls.Add(lblNota, 0, 5);
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
        etiqueta.Name = "lbl" + control.Name[3..]; // txtNombre → lblNombre (nombre sin su prefijo de 3 letras)
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