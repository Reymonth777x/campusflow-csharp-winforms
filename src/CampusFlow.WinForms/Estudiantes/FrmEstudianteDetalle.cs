using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Common;
using CampusFlow.Application.Estudiantes;
using CampusFlow.WinForms.Common;

namespace CampusFlow.WinForms.Estudiantes;

/// <summary>
/// Modal de alta y edición. Traduce los controles a <see cref="CrearEstudianteRequest"/> o
/// <see cref="ActualizarEstudianteRequest"/> y muestra los errores del <c>Result</c> junto al
/// campo correspondiente mediante ErrorProvider.
/// </summary>
public sealed partial class FrmEstudianteDetalle : Form
{
    private readonly IEstudianteService _servicio;
    private readonly TimeProvider _reloj;

    // null = alta; con valor = edición de ese estudiante.
    private EstudianteDto? _original;
    private bool _ocupado;

    /// <summary>Estudiante tal como quedó guardado; el listado lo usa para seleccionarlo al volver.</summary>
    public EstudianteDto? EstudianteGuardado { get; private set; }

    public FrmEstudianteDetalle(IEstudianteService servicio, TimeProvider reloj)
    {
        _servicio = servicio;
        _reloj = reloj;

        ConstruirInterfaz();
        ConectarEventos();
    }

    /// <summary>Prepara el formulario. Se llama una sola vez, antes de ShowDialog.</summary>
    public void Cargar(EstudianteDto? estudiante)
    {
        _original = estudiante;
        var hoy = DateOnly.FromDateTime(_reloj.GetLocalNow().DateTime);

        if (estudiante is null)
        {
            Text = "Nuevo estudiante";

            // El selector no deja elegir fechas futuras (misma regla que el validador, pero evitando el error).
            // MaxDate se fija ANTES de Value: asignar un Value fuera de rango lanza excepción.
            dtpFechaIngreso.MaxDate = hoy.ToDateTime(TimeOnly.MaxValue);
            dtpFechaIngreso.Value = hoy.ToDateTime(TimeOnly.MinValue);
            ActiveControl = txtIdentificacion;
            return;
        }

        Text = "Editar estudiante";
        txtIdentificacion.Text = estudiante.Identificacion;
        txtNombre.Text = estudiante.Nombre;
        txtApellidos.Text = estudiante.Apellidos;
        txtCorreo.Text = estudiante.Correo;
        dtpFechaIngreso.Value = estudiante.FechaIngreso.ToDateTime(TimeOnly.MinValue);

        // ActualizarEstudianteRequest no incluye estos dos datos: se muestran pero no se pueden tocar.
        txtIdentificacion.ReadOnly = true;
        dtpFechaIngreso.Enabled = false;
        lblNota.Visible = true;
        ActiveControl = txtNombre;
    }

    private void ConectarEventos()
    {
        btnGuardar.Click += btnGuardar_Click;
        FormClosing += FrmEstudianteDetalle_FormClosing;

        // Al corregir un campo, su error desaparece sin esperar a guardar de nuevo.
        txtIdentificacion.TextChanged += LimpiarError;
        txtNombre.TextChanged += LimpiarError;
        txtApellidos.TextChanged += LimpiarError;
        txtCorreo.TextChanged += LimpiarError;
        dtpFechaIngreso.ValueChanged += LimpiarError;
    }

    // Bloquea el cierre mientras se guarda: así nunca queda una escritura a medias con la ventana ya cerrada.
    private void FrmEstudianteDetalle_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_ocupado) e.Cancel = true;
    }

    private void LimpiarError(object? sender, EventArgs e)
    {
        if (sender is Control control) errValidacion.SetError(control, string.Empty);
    }

    private async void btnGuardar_Click(object? sender, EventArgs e) => await GuardarAsync();

    private async Task GuardarAsync()
    {
        if (_ocupado) return;

        errValidacion.Clear();
        EstablecerOcupado(true);
        try
        {
            // CancellationToken.None es deliberado: una escritura no se cancela a mitad de camino
            // (el cierre ya está bloqueado mientras guarda). Ver FormClosing.
            var resultado = _original is null
                ? await _servicio.CrearAsync(
                    new CrearEstudianteRequest(
                        txtIdentificacion.Text,
                        txtNombre.Text,
                        txtApellidos.Text,
                        txtCorreo.Text,
                        DateOnly.FromDateTime(dtpFechaIngreso.Value)),
                    CancellationToken.None)
                : await _servicio.ActualizarAsync(
                    new ActualizarEstudianteRequest(
                        _original.Id,
                        txtNombre.Text,
                        txtApellidos.Text,
                        txtCorreo.Text),
                    CancellationToken.None);

            if (resultado.EsFalla)
            {
                MostrarErrores(resultado.Errores);
                return;
            }

            EstudianteGuardado = resultado.Valor;

            // Se libera ANTES de cerrar: FormClosing rechaza el cierre mientras _ocupado sea true.
            EstablecerOcupado(false);
            DialogResult = DialogResult.OK;
        }
        finally
        {
            EstablecerOcupado(false);
        }
    }

    /// <summary>
    /// Asigna cada error de negocio al control de su campo. Los errores sin campo asociado
    /// (p. ej. "el estudiante no existe") no tienen dónde pintarse y van a un cuadro de mensaje.
    /// </summary>
    private void MostrarErrores(IReadOnlyList<Error> errores)
    {
        var generales = new List<string>();

        foreach (var error in errores)
        {
            var control = ControlDe(error.Campo);
            if (control is null)
            {
                generales.Add(error.Mensaje);
                continue;
            }

            // Un campo puede traer varios errores ("obligatorio" + "formato inválido"):
            // se muestra solo el primero para no abrumar.
            if (errValidacion.GetError(control).Length == 0)
                errValidacion.SetError(control, error.Mensaje);
        }

        // El foco va al primer campo con error en el orden visual del formulario, no en el orden del Result.
        CamposEnOrden.FirstOrDefault(c => errValidacion.GetError(c).Length > 0)?.Focus();

        if (generales.Count > 0)
            Mensajes.Advertencia(this, string.Join(Environment.NewLine, generales));
    }

    /// <summary>
    /// Traduce el nombre de propiedad del Request (Error.Campo) al control que lo captura.
    /// nameof(...) mantiene el vínculo: si se renombra una propiedad, el compilador avisa aquí.
    /// </summary>
    private Control? ControlDe(string? campo) => campo switch
    {
        nameof(CrearEstudianteRequest.Identificacion) => txtIdentificacion,
        nameof(CrearEstudianteRequest.Nombre) => txtNombre,
        nameof(CrearEstudianteRequest.Apellidos) => txtApellidos,
        nameof(CrearEstudianteRequest.Correo) => txtCorreo,
        nameof(CrearEstudianteRequest.FechaIngreso) => dtpFechaIngreso,
        _ => null
    };

    private Control[] CamposEnOrden =>
        [txtIdentificacion, txtNombre, txtApellidos, txtCorreo, dtpFechaIngreso];

    private void EstablecerOcupado(bool ocupado)
    {
        _ocupado = ocupado;
        UseWaitCursor = ocupado;
        tlpCampos.Enabled = !ocupado; // los controles ya deshabilitados (modo edición) conservan su propio estado
        btnGuardar.Enabled = !ocupado;
        btnCancelar.Enabled = !ocupado;
    }

    // ErrorProvider es un componente que no está en ningún contenedor: se desecha a mano.
    protected override void Dispose(bool disposing)
    {
        if (disposing) errValidacion.Dispose();

        base.Dispose(disposing);
    }
}