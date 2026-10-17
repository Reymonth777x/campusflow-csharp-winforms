using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using CampusFlow.Application.Estudiantes;
using CampusFlow.Domain.Enums;
using CampusFlow.WinForms.Common;
using Microsoft.Extensions.DependencyInjection;

namespace CampusFlow.WinForms.Estudiantes;

/// <summary>
/// Pantalla de listado de estudiantes. Solo orquesta la UI: toda la lógica de negocio
/// vive en <see cref="IEstudianteService"/>, y aquí solo se muestran DTOs.
/// </summary>
public sealed partial class FrmEstudiantes : Form
{
    // Pausa tras la última tecla antes de consultar la BD (debounce).
    private const int EsperaBusquedaMs = 300;

    /// <summary>Opción del combo de estado; Valor null significa "sin filtro".</summary>
    private sealed record OpcionEstado(string Texto, EstadoEstudiante? Valor);

    private readonly IEstudianteService _servicio;
    private readonly IServiceScopeFactory _scopeFactory;

    // Se cancela al cerrar el formulario: aborta consultas en vuelo para no tocar una UI que ya no existe.
    private readonly CancellationTokenSource _ctsFormulario = new();

    // Una por búsqueda; cada pulsación cancela la anterior. Enlazada a _ctsFormulario.
    private CancellationTokenSource? _ctsBusqueda;

    // true mientras corre una escritura (cambio de estado): evita dobles clics.
    private bool _ocupado;

    public FrmEstudiantes(IEstudianteService servicio, IServiceScopeFactory scopeFactory)
    {
        _servicio = servicio;
        _scopeFactory = scopeFactory;

        ConstruirInterfaz(); // controles y layout (ver FrmEstudiantes.Layout.cs)
        ConectarEventos();   // se enlazan DESPUÉS de construir: así los DataSource iniciales no disparan búsquedas
        ActualizarBotones();
    }

    private void ConectarEventos()
    {
        Shown += FrmEstudiantes_Shown;
        FormClosed += FrmEstudiantes_FormClosed;

        txtFiltro.TextChanged += txtFiltro_TextChanged;
        txtFiltro.KeyDown += txtFiltro_KeyDown;
        cmbEstado.SelectedIndexChanged += cmbEstado_SelectedIndexChanged;
        btnBuscar.Click += btnBuscar_Click;

        btnNuevo.Click += btnNuevo_Click;
        btnEditar.Click += btnEditar_Click;
        btnCambiarEstado.Click += btnCambiarEstado_Click;

        dgvEstudiantes.SelectionChanged += (_, _) => ActualizarBotones();
        dgvEstudiantes.CellDoubleClick += dgvEstudiantes_CellDoubleClick;
        dgvEstudiantes.CellFormatting += dgvEstudiantes_CellFormatting;
    }

    // ───────────── Ciclo de vida ─────────────

    // Shown (no Load): la ventana ya se pintó, así el usuario ve el formulario y luego los datos.
    private async void FrmEstudiantes_Shown(object? sender, EventArgs e) => await BuscarAhoraAsync();

    private void FrmEstudiantes_FormClosed(object? sender, FormClosedEventArgs e)
    {
        // Solo se cancela aquí; el Dispose de los CTS ocurre en Dispose(bool), cuando ya nadie los usa.
        _ctsBusqueda?.Cancel();
        _ctsFormulario.Cancel();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _ctsBusqueda?.Dispose();
            _ctsFormulario.Dispose();
        }

        base.Dispose(disposing);
    }

    // ───────────── Búsqueda reactiva ─────────────

    // async void es inevitable en un evento; por eso solo delega en un Task con try/catch controlado.
    private async void txtFiltro_TextChanged(object? sender, EventArgs e) => await BuscarConEsperaAsync();

    private async void cmbEstado_SelectedIndexChanged(object? sender, EventArgs e) => await BuscarAhoraAsync();

    private async void btnBuscar_Click(object? sender, EventArgs e) => await BuscarAhoraAsync();

    private async void txtFiltro_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;

        e.SuppressKeyPress = true; // evita el pitido de Windows al pulsar Enter en un TextBox
        await BuscarAhoraAsync();
    }

    /// <summary>
    /// Debounce: cada pulsación cancela la espera anterior y arranca otra de 300 ms.
    /// Solo la última tecla llega a consultar la BD.
    /// </summary>
    private async Task BuscarConEsperaAsync()
    {
        if (IsDisposed) return;

        var ct = ReiniciarBusqueda();
        try
        {
            await Task.Delay(EsperaBusquedaMs, ct);
            await CargarAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // Reemplazada por una búsqueda más nueva o formulario cerrado: no es un error.
        }
    }

    /// <summary>Búsqueda inmediata, sin espera (Enter, botón, cambio de combo, recarga tras guardar).</summary>
    private async Task BuscarAhoraAsync(int? idASeleccionar = null)
    {
        if (IsDisposed) return;

        var ct = ReiniciarBusqueda();
        try
        {
            await CargarAsync(ct, idASeleccionar);
        }
        catch (OperationCanceledException)
        {
            // Ver BuscarConEsperaAsync.
        }
    }

    /// <summary>Cancela la búsqueda en curso (si hay) y entrega el token de la nueva.</summary>
    private CancellationToken ReiniciarBusqueda()
    {
        _ctsBusqueda?.Cancel();
        _ctsBusqueda?.Dispose();
        _ctsBusqueda = CancellationTokenSource.CreateLinkedTokenSource(_ctsFormulario.Token);
        return _ctsBusqueda.Token;
    }

    /// <summary>Consulta al servicio con los filtros actuales y refresca la grilla.</summary>
    private async Task CargarAsync(CancellationToken ct, int? idASeleccionar = null)
    {
        var filtro = new FiltroEstudiantes(
            Texto: string.IsNullOrWhiteSpace(txtFiltro.Text) ? null : txtFiltro.Text.Trim(),
            Estado: (cmbEstado.SelectedItem as OpcionEstado)?.Valor);

        // Se recuerda la fila seleccionada para no perderla al recargar.
        var seleccionPrevia = idASeleccionar ?? ObtenerSeleccionado()?.Id;

        var estudiantes = await _servicio.ListarAsync(filtro, ct);

        // Si mientras la BD respondía el usuario siguió escribiendo, este resultado ya es viejo: se descarta.
        ct.ThrowIfCancellationRequested();

        // BindingList: lista con notificaciones que el DataGridView sabe enlazar correctamente.
        dgvEstudiantes.DataSource = new BindingList<EstudianteDto>(estudiantes.ToList());
        SeleccionarPorId(seleccionPrevia);

        lblResumen.Text = estudiantes.Count == 1 ? "1 estudiante" : $"{estudiantes.Count} estudiantes";
        ActualizarBotones();
    }

    // ───────────── Acciones ─────────────

    private async void btnNuevo_Click(object? sender, EventArgs e) => await AbrirDetalleAsync(null);

    private async void btnEditar_Click(object? sender, EventArgs e) => await EditarSeleccionadoAsync();

    private async void btnCambiarEstado_Click(object? sender, EventArgs e) => await CambiarEstadoAsync();

    private async void dgvEstudiantes_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        // RowIndex -1 = doble clic sobre el encabezado: se ignora.
        if (e.RowIndex >= 0) await EditarSeleccionadoAsync();
    }

    private async Task EditarSeleccionadoAsync()
    {
        if (_ocupado) return;

        if (ObtenerSeleccionado() is { } seleccionado)
            await AbrirDetalleAsync(seleccionado);
    }

    /// <summary>Abre el modal de alta (existente = null) o edición, y recarga si se guardó.</summary>
    private async Task AbrirDetalleAsync(EstudianteDto? existente)
    {
        // Scope propio: al salir del using, el contenedor desecha el formulario de detalle.
        using var scope = _scopeFactory.CreateScope();
        var detalle = scope.ServiceProvider.GetRequiredService<FrmEstudianteDetalle>();
        detalle.Cargar(existente);

        if (detalle.ShowDialog(this) == DialogResult.OK)
        {
            // Recarga y deja seleccionado el registro recién creado/editado.
            await BuscarAhoraAsync(detalle.EstudianteGuardado?.Id);
        }
    }

    /// <summary>Soft-delete: alterna Activo/Inactivo. Nunca se borra físicamente (hay historial académico).</summary>
    private async Task CambiarEstadoAsync()
    {
        if (_ocupado || ObtenerSeleccionado() is not { } seleccionado) return;

        var desactivar = seleccionado.Estado == EstadoEstudiante.Activo;

        // Desactivar pide confirmación; reactivar no, porque es inofensivo y reversible.
        if (desactivar && !Mensajes.Confirmar(this,
                $"¿Desactivar a {seleccionado.NombreCompleto}?{Environment.NewLine}" +
                "No se elimina: conserva su historial y puede reactivarse."))
            return;

        var ct = _ctsFormulario.Token;
        EstablecerOcupado(true);
        try
        {
            var resultado = desactivar
                ? await _servicio.DesactivarAsync(seleccionado.Id, ct)
                : await _servicio.ActivarAsync(seleccionado.Id, ct);

            if (resultado.EsFalla)
                Mensajes.Advertencia(this, resultado.Errores[0].Mensaje);

            // Se recarga siempre: refleja el cambio o la causa del fallo (p. ej. otro usuario ya lo eliminó).
            await BuscarAhoraAsync(seleccionado.Id);
        }
        catch (OperationCanceledException)
        {
            // El formulario se está cerrando: no hay nada que mostrar.
        }
        finally
        {
            if (!IsDisposed) EstablecerOcupado(false);
        }
    }

    // ───────────── Grilla y estado de la UI ─────────────

    /// <summary>DTO de la fila seleccionada, o null si no hay ninguna.</summary>
    private EstudianteDto? ObtenerSeleccionado() =>
        dgvEstudiantes.SelectedRows.Count == 1
            ? dgvEstudiantes.SelectedRows[0].DataBoundItem as EstudianteDto
            : null;

    private void SeleccionarPorId(int? id)
    {
        if (id is null) return;

        foreach (DataGridViewRow fila in dgvEstudiantes.Rows)
        {
            if (fila.DataBoundItem is not EstudianteDto dto || dto.Id != id) continue;

            fila.Selected = true;
            dgvEstudiantes.CurrentCell = fila.Cells[0]; // también desplaza la grilla hasta la fila
            return;
        }
    }

    /// <summary>Atenúa las filas de estudiantes inactivos para distinguirlas de un vistazo.</summary>
    private void dgvEstudiantes_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;

        if (dgvEstudiantes.Rows[e.RowIndex].DataBoundItem is EstudianteDto { Estado: EstadoEstudiante.Inactivo })
            e.CellStyle!.ForeColor = Tema.TextoAtenuado;
    }

    private void EstablecerOcupado(bool ocupado)
    {
        _ocupado = ocupado;
        UseWaitCursor = ocupado;
        ActualizarBotones();
    }

    /// <summary>Habilita los botones según haya selección y el texto alterna entre Activar/Desactivar.</summary>
    private void ActualizarBotones()
    {
        var seleccionado = ObtenerSeleccionado();
        var puedeActuar = seleccionado is not null && !_ocupado;

        btnNuevo.Enabled = !_ocupado;
        btnEditar.Enabled = puedeActuar;
        btnCambiarEstado.Enabled = puedeActuar;
        btnCambiarEstado.Text = seleccionado?.Estado == EstadoEstudiante.Inactivo ? "Activar" : "Desactivar";
    }
}