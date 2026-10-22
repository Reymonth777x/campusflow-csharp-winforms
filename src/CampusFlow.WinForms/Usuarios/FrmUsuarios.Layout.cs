using CampusFlow.Application.Usuarios;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Usuarios;

/// <summary>Definición visual de FrmUsuarios. Todo con Dock/Anchor/TableLayoutPanel: se adapta al redimensionar.</summary>
public sealed partial class FrmUsuarios
{
    // Nombre de la columna de estado: lo reutiliza CellFormatting para traducir el bool Activo a texto.
    private const string ColEstado = "colActivo";

    private readonly TableLayoutPanel tlpPrincipal = new();
    private readonly TableLayoutPanel tlpAcciones = new();
    private readonly FlowLayoutPanel flpAcciones = new();
    private readonly DataGridView dgvUsuarios = new();
    private readonly Button btnNuevo = new();
    private readonly Button btnEditar = new();
    private readonly Button btnCambiarEstado = new();
    private readonly Button btnRestablecerPassword = new();
    private readonly Button btnCerrar = new();
    private readonly Label lblResumen = new();

    private void ConstruirInterfaz()
    {
        SuspendLayout(); // evita recalcular el layout con cada control agregado

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = Tema.Fuente;
        BackColor = Tema.Fondo;
        ForeColor = Tema.Texto;
        Text = "CampusFlow – Usuarios";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(960, 540);
        MinimumSize = new Size(800, 400);

        ConfigurarGrid();

        btnNuevo.Name = "btnNuevo";
        btnNuevo.Text = "Nuevo";
        Tema.EstilizarBoton(btnNuevo, primario: true);

        btnEditar.Name = "btnEditar";
        btnEditar.Text = "Editar";
        Tema.EstilizarBoton(btnEditar, primario: false);

        btnCambiarEstado.Name = "btnCambiarEstado";
        btnCambiarEstado.Text = "Desactivar";
        Tema.EstilizarBoton(btnCambiarEstado, primario: false);

        btnRestablecerPassword.Name = "btnRestablecerPassword";
        btnRestablecerPassword.Text = "Restablecer contraseña";
        Tema.EstilizarBoton(btnRestablecerPassword, primario: false);

        btnCerrar.Name = "btnCerrar";
        btnCerrar.Text = "Cerrar";
        btnCerrar.DialogResult = DialogResult.Cancel; // cierra el formulario sin código adicional
        Tema.EstilizarBoton(btnCerrar, primario: false);
        CancelButton = btnCerrar; // Esc = Cerrar

        flpAcciones.AutoSize = true;
        flpAcciones.Dock = DockStyle.Fill;
        flpAcciones.Controls.AddRange([btnNuevo, btnEditar, btnCambiarEstado, btnRestablecerPassword]);

        lblResumen.Name = "lblResumen";
        lblResumen.AutoSize = true;
        lblResumen.Anchor = AnchorStyles.Right;
        lblResumen.ForeColor = Tema.TextoAtenuado;
        lblResumen.Margin = new Padding(0, 0, 16, 0);

        // Acciones (izquierda) / resumen (centro, absorbe el espacio) / cerrar (derecha).
        tlpAcciones.Dock = DockStyle.Fill;
        tlpAcciones.AutoSize = true;
        tlpAcciones.ColumnCount = 3;
        tlpAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tlpAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tlpAcciones.Controls.Add(flpAcciones, 0, 0);
        tlpAcciones.Controls.Add(lblResumen, 1, 0);
        tlpAcciones.Controls.Add(btnCerrar, 2, 0);

        // Contenedor raíz: grilla (ocupa el resto) / acciones (alto automático).
        tlpPrincipal.Dock = DockStyle.Fill;
        tlpPrincipal.Padding = new Padding(16);
        tlpPrincipal.ColumnCount = 1;
        tlpPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpPrincipal.RowCount = 2;
        tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpPrincipal.Controls.Add(dgvUsuarios, 0, 0);
        tlpPrincipal.Controls.Add(tlpAcciones, 0, 1);

        Controls.Add(tlpPrincipal);
        ResumeLayout(performLayout: true);
    }

    private void ConfigurarGrid()
    {
        Tema.EstilizarGrid(dgvUsuarios);

        dgvUsuarios.Name = "dgvUsuarios";
        dgvUsuarios.Dock = DockStyle.Fill;
        dgvUsuarios.Margin = new Padding(0, 0, 0, 8);

        // Columnas explícitas: control total del orden, títulos y formato.
        dgvUsuarios.AutoGenerateColumns = false;
        dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        // Solo lectura: toda modificación pasa por los modales y el servicio.
        dgvUsuarios.ReadOnly = true;
        dgvUsuarios.AllowUserToAddRows = false;
        dgvUsuarios.AllowUserToDeleteRows = false; // la tecla Supr no debe quitar filas de la lista
        dgvUsuarios.AllowUserToResizeRows = false;
        dgvUsuarios.MultiSelect = false;
        dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        var ultimoAcceso = Columna(nameof(UsuarioListadoDto.UltimoAcceso), "Último acceso", 22, "g");
        ultimoAcceso.DefaultCellStyle.NullValue = "Nunca"; // null = jamás ha ingresado

        dgvUsuarios.Columns.AddRange(
            Columna(nameof(UsuarioListadoDto.NombreUsuario), "Usuario", 18),
            Columna(nameof(UsuarioListadoDto.NombreCompleto), "Nombre completo", 30),
            Columna(nameof(UsuarioListadoDto.Rol), "Rol", 14),
            Columna(nameof(UsuarioListadoDto.Activo), "Estado", 12), // Name = ColEstado; el texto lo pone CellFormatting
            ultimoAcceso);
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