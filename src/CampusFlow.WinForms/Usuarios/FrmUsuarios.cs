using System.ComponentModel;
using CampusFlow.Application.Autenticacion;
using CampusFlow.Application.Usuarios;
using CampusFlow.WinForms.Common;
using Microsoft.Extensions.DependencyInjection;

namespace CampusFlow.WinForms.Usuarios;

/// <summary>
/// Listado y administración de cuentas (solo Administrador). Solo orquesta la UI: las reglas de
/// negocio y la autorización viven en <see cref="IUsuarioService"/>.
/// </summary>
[DesignerCategory("Code")] // se abre como código, nunca en el diseñador (evita que Visual Studio cree un .resx)
public sealed partial class FrmUsuarios : Form
{
    private readonly IUsuarioService _servicio;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISesionUsuario _sesion;

    // Se cancela al cerrar el formulario: aborta consultas en vuelo para no tocar una UI que ya no existe.
    private readonly CancellationTokenSource _cts = new();

    // true mientras corre una escritura: evita dobles clics.
    private bool _ocupado;

    public FrmUsuarios(IUsuarioService servicio, IServiceScopeFactory scopeFactory, ISesionUsuario sesion)
    {
        _servicio = servicio;
        _scopeFactory = scopeFactory;
        _sesion = sesion;

        ConstruirInterfaz();
        ConectarEventos(); // después de construir: así los DataSource iniciales no disparan nada
        ActualizarBotones();
    }

    private void ConectarEventos()
    {
        // Shown (no Load): la ventana ya se pintó, así el usuario ve el formulario y luego los datos.
        Shown += async (_, _) => await CargarAsync();
        FormClosed += (_, _) => _cts.Cancel();

        btnNuevo.Click += async (_, _) => await AbrirDetalleAsync(null);
        btnEditar.Click += async (_, _) => await EditarSeleccionadoAsync();
        btnCambiarEstado.Click += async (_, _) => await CambiarEstadoAsync();
        btnRestablecerPassword.Click += async (_, _) => await RestablecerPasswordAsync();

        dgvUsuarios.SelectionChanged += (_, _) => ActualizarBotones();
        dgvUsuarios.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0) await EditarSeleccionadoAsync(); // -1 = doble clic en el encabezado
        };
        dgvUsuarios.CellFormatting += dgvUsuarios_CellFormatting;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _cts.Dispose();

        base.Dispose(disposing);
    }

    // ───── Carga ─────

    /// <summary>Recarga la grilla y conserva (o fija) la fila seleccionada.</summary>
    private async Task CargarAsync(int? idASeleccionar = null)
    {
        if (IsDisposed) return;

        var seleccionPrevia = idASeleccionar ?? ObtenerSeleccionado()?.Id;
        try
        {
            var usuarios = await _servicio.ListarAsync(_cts.Token);

            // Si el formulario se cerró mientras la BD respondía, el resultado ya no sirve.
            _cts.Token.ThrowIfCancellationRequested();

            dgvUsuarios.DataSource = new BindingList<UsuarioListadoDto>(usuarios.ToList());
            SeleccionarPorId(seleccionPrevia);

            lblResumen.Text = usuarios.Count == 1 ? "1 usuario" : $"{usuarios.Count} usuarios";
            ActualizarBotones();
        }
        catch (OperationCanceledException)
        {
            // Formulario cerrado: no es un error.
        }
    }

    // ───── Acciones ─────

    private async Task EditarSeleccionadoAsync()
    {
        if (_ocupado) return;

        if (ObtenerSeleccionado() is { } seleccionado)
            await AbrirDetalleAsync(seleccionado);
    }

    /// <summary>Abre el modal de alta (existente = null) o edición, y recarga si se guardó.</summary>
    private async Task AbrirDetalleAsync(UsuarioListadoDto? existente)
    {
        // Scope propio: al salir del using, el contenedor desecha el formulario de detalle.
        using var scope = _scopeFactory.CreateScope();
        var detalle = scope.ServiceProvider.GetRequiredService<FrmUsuarioDetalle>();
        detalle.Cargar(existente);

        if (detalle.ShowDialog(this) == DialogResult.OK)
            await CargarAsync(detalle.UsuarioGuardado?.Id);
    }

    private async Task RestablecerPasswordAsync()
    {
        if (_ocupado || ObtenerSeleccionado() is not { } seleccionado) return;

        using var scope = _scopeFactory.CreateScope();
        var formulario = scope.ServiceProvider.GetRequiredService<FrmRestablecerPassword>();
        formulario.Cargar(seleccionado);

        if (formulario.ShowDialog(this) != DialogResult.OK) return;

        Mensajes.Informacion(this,
            $"Contraseña restablecida. {seleccionado.NombreUsuario} ya puede ingresar con la nueva contraseña.");

        // Recarga: restablecer también desbloquea la cuenta, y el estado de la fila cambia.
        await CargarAsync(seleccionado.Id);
    }

    /// <summary>Activa o desactiva la cuenta seleccionada. Nunca se borra un usuario: se conserva su historial.</summary>
    private async Task CambiarEstadoAsync()
    {
        if (_ocupado || ObtenerSeleccionado() is not { } seleccionado) return;

        var desactivar = seleccionado.Activo;

        // Desactivar pide confirmación; reactivar no, porque es inofensivo y reversible.
        if (desactivar && !Mensajes.Confirmar(this,
                $"¿Desactivar la cuenta de {seleccionado.NombreUsuario}?{Environment.NewLine}" +
                "No podrá iniciar sesión hasta que se reactive."))
            return;

        EstablecerOcupado(true);
        try
        {
            var resultado = desactivar
                ? await _servicio.DesactivarAsync(seleccionado.Id, _cts.Token)
                : await _servicio.ActivarAsync(seleccionado.Id, _cts.Token);

            if (resultado.EsFalla)
                Mensajes.Advertencia(this, resultado.Errores[0].Mensaje);

            // Se recarga siempre: refleja el cambio o la causa del fallo.
            await CargarAsync(seleccionado.Id);
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

    // ───── Grilla y estado de la UI ─────

    private UsuarioListadoDto? ObtenerSeleccionado() =>
        dgvUsuarios.SelectedRows.Count == 1
            ? dgvUsuarios.SelectedRows[0].DataBoundItem as UsuarioListadoDto
            : null;

    private void SeleccionarPorId(int? id)
    {
        if (id is null) return;

        foreach (DataGridViewRow fila in dgvUsuarios.Rows)
        {
            if (fila.DataBoundItem is not UsuarioListadoDto dto || dto.Id != id) continue;

            fila.Selected = true;
            dgvUsuarios.CurrentCell = fila.Cells[0]; // también desplaza la grilla hasta la fila
            return;
        }
    }

    /// <summary>Traduce Activo a texto, atenúa las cuentas inactivas y resalta las bloqueadas.</summary>
    private void dgvUsuarios_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || dgvUsuarios.Rows[e.RowIndex].DataBoundItem is not UsuarioListadoDto usuario) return;

        var esColumnaEstado = dgvUsuarios.Columns[e.ColumnIndex].Name == ColEstado;
        if (esColumnaEstado)
        {
            e.Value = !usuario.Activo ? "Inactivo" : usuario.Bloqueado ? "Bloqueado" : "Activo";
            e.FormattingApplied = true;
        }

        if (!usuario.Activo)
            e.CellStyle!.ForeColor = Tema.TextoAtenuado;
        else if (usuario.Bloqueado && esColumnaEstado)
            e.CellStyle!.ForeColor = Color.Firebrick;
    }

    private void EstablecerOcupado(bool ocupado)
    {
        _ocupado = ocupado;
        UseWaitCursor = ocupado;
        ActualizarBotones();
    }

    /// <summary>
    /// Habilita los botones según la selección. Sobre la propia cuenta no se puede activar/desactivar:
    /// es comodidad de UI, la regla real la aplica el servicio.
    /// </summary>
    private void ActualizarBotones()
    {
        var seleccionado = ObtenerSeleccionado();
        var puedeActuar = seleccionado is not null && !_ocupado;
        var esUnoMismo = seleccionado is not null && seleccionado.Id == _sesion.Actual?.Id;

        btnNuevo.Enabled = !_ocupado;
        btnEditar.Enabled = puedeActuar;
        btnRestablecerPassword.Enabled = puedeActuar;
        btnCambiarEstado.Enabled = puedeActuar && !esUnoMismo;
        btnCambiarEstado.Text = seleccionado is { Activo: false } ? "Activar" : "Desactivar";
    }
}