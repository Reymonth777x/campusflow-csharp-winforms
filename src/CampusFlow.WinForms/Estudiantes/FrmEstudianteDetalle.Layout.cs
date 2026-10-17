using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Estudiantes;
using CampusFlow.Domain.Enums;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Estudiantes;

/// <summary>
/// Definición visual de FrmEstudiantes. Todo usa Dock/Anchor/TableLayoutPanel (nunca coordenadas fijas),
/// por eso la ventana se adapta al redimensionarla y a distintos DPI.
/// </summary>
public sealed partial class FrmEstudiantes
{
    private readonly TableLayoutPanel tlpPrincipal = new();
    private readonly FlowLayoutPanel flpFiltros = new();
    private readonly TextBox txtFiltro = new();
    private readonly Label lblEstado = new();
    private readonly ComboBox cmbEstado = new();
    private readonly Button btnBuscar = new();
    private readonly DataGridView dgvEstudiantes = new();
    private readonly TableLayoutPanel tlpAcciones = new();
    private readonly FlowLayoutPanel flpAcciones = new();
    private readonly Button btnNuevo = new();
    private readonly Button btnEditar = new();
    private readonly Button btnCambiarEstado = new();
    private readonly Label lblResumen = new();

    private void ConstruirInterfaz()
    {
        SuspendLayout(); // evita recalcular el layout con cada control agregado

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = Tema.Fuente;
        BackColor = Tema.Fondo;
        ForeColor = Tema.Texto;
        Text = "CampusFlow – Estudiantes";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 560);
        MinimumSize = new Size(720, 420);

        // Filtros: texto libre + estado + botón buscar.
        txtFiltro.Name = "txtFiltro";
        txtFiltro.Width = 340;
        txtFiltro.PlaceholderText = "Buscar por identificación, nombre, apellidos o correo";
        txtFiltro.Margin = new Padding(0, 4, 16, 4);

        lblEstado.Name = "lblEstado";
        lblEstado.Text = "Estado:";
        lblEstado.AutoSize = true;
        lblEstado.Margin = new Padding(0, 8, 6, 0);

        cmbEstado.Name = "cmbEstado";
        cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbEstado.Width = 120;
        cmbEstado.Margin = new Padding(0, 4, 16, 4);
        cmbEstado.DisplayMember = nameof(OpcionEstado.Texto);
        cmbEstado.DataSource = new List<OpcionEstado>
        {
            new("Todos", null),
            new("Activos", EstadoEstudiante.Activo),
            new("Inactivos", EstadoEstudiante.Inactivo)
        };

        btnBuscar.Name = "btnBuscar";
        btnBuscar.Text = "Buscar";
        Tema.EstilizarBoton(btnBuscar, primario: true);

        flpFiltros.Dock = DockStyle.Fill;
        flpFiltros.AutoSize = true;
        flpFiltros.WrapContents = false;
        flpFiltros.Controls.AddRange([txtFiltro, lblEstado, cmbEstado, btnBuscar]);

        ConfigurarGrid();

        // Acciones sobre la fila seleccionada.
        btnNuevo.Name = "btnNuevo";
        btnNuevo.Text = "Nuevo";
        Tema.EstilizarBoton(btnNuevo, primario: true);

        btnEditar.Name = "btnEditar";
        btnEditar.Text = "Editar";
        Tema.EstilizarBoton(btnEditar, primario: false);

        btnCambiarEstado.Name = "btnCambiarEstado";
        btnCambiarEstado.Text = "Desactivar";
        Tema.EstilizarBoton(btnCambiarEstado, primario: false);

        flpAcciones.AutoSize = true;
        flpAcciones.Dock = DockStyle.Fill;
        flpAcciones.Controls.AddRange([btnNuevo, btnEditar, btnCambiarEstado]);

        lblResumen.Name = "lblResumen";
        lblResumen.AutoSize = true;
        lblResumen.Anchor = AnchorStyles.Right;
        lblResumen.ForeColor = Tema.TextoAtenuado;

        tlpAcciones.Dock = DockStyle.Fill;
        tlpAcciones.AutoSize = true;
        tlpAcciones.ColumnCount = 2;
        tlpAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tlpAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpAcciones.Controls.Add(flpAcciones, 0, 0);
        tlpAcciones.Controls.Add(lblResumen, 1, 0);

        // Contenedor raíz: filtros (alto automático) / grilla (ocupa el resto) / acciones (alto automático).
        tlpPrincipal.Dock = DockStyle.Fill;
        tlpPrincipal.Padding = new Padding(16);
        tlpPrincipal.ColumnCount = 1;
        tlpPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpPrincipal.RowCount = 3;
        tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpPrincipal.Controls.Add(flpFiltros, 0, 0);
        tlpPrincipal.Controls.Add(dgvEstudiantes, 0, 1);
        tlpPrincipal.Controls.Add(tlpAcciones, 0, 2);

        Controls.Add(tlpPrincipal);
        ResumeLayout(performLayout: true);
    }

    private void ConfigurarGrid()
    {
        Tema.EstilizarGrid(dgvEstudiantes);

        dgvEstudiantes.Name = "dgvEstudiantes";
        dgvEstudiantes.Dock = DockStyle.Fill;
        dgvEstudiantes.Margin = new Padding(0, 8, 0, 8);

        // Columnas explícitas: control total del orden, títulos y formato. Sin autogeneración,
        // un campo nuevo en el DTO no aparecería "de sorpresa" en la pantalla.
        dgvEstudiantes.AutoGenerateColumns = false;
        dgvEstudiantes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        // Solo lectura: toda modificación pasa por el formulario de detalle y el servicio.
        dgvEstudiantes.ReadOnly = true;
        dgvEstudiantes.AllowUserToAddRows = false;
        dgvEstudiantes.AllowUserToDeleteRows = false; // la tecla Supr no debe quitar filas de la lista
        dgvEstudiantes.AllowUserToResizeRows = false;
        dgvEstudiantes.MultiSelect = false;
        dgvEstudiantes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        dgvEstudiantes.Columns.AddRange(
            Columna(nameof(EstudianteDto.Identificacion), "Identificación", 16),
            Columna(nameof(EstudianteDto.NombreCompleto), "Nombre completo", 30),
            Columna(nameof(EstudianteDto.Correo), "Correo", 28),
            Columna(nameof(EstudianteDto.FechaIngreso), "Ingreso", 12, "d"),
            Columna(nameof(EstudianteDto.Estado), "Estado", 10));
    }

    /// <summary>Crea una columna de texto enlazada a una propiedad del DTO (peso = ancho relativo).</summary>
    private static DataGridViewTextBoxColumn Columna(
        string propiedad, string encabezado, float peso, string formato = "") =>
        new()
        {
            Name = "col" + propiedad,
            DataPropertyName = propiedad,
            HeaderText = encabezado,
            FillWeight = peso,
            DefaultCellStyle = { Format = formato }
        };
}