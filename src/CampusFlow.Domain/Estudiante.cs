using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using CampusFlow.Domain.Common;
using CampusFlow.Domain.Enums;

namespace CampusFlow.Domain.Entities;

public sealed class Estudiante : Entity
{
    public const int MaxIdentificacion = 20;
    public const int MaxNombre = 100;
    public const int MaxApellidos = 100;
    public const int MaxCorreo = 254;

    public string Identificacion { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public string Apellidos { get; private set; } = null!;
    public string Correo { get; private set; } = null!;
    public DateOnly FechaIngreso { get; private set; }
    public EstadoEstudiante Estado { get; private set; }

    public string NombreCompleto => $"{Nombre} {Apellidos}";

    // Requerido por EF Core para materializar la entidad.
    private Estudiante() { }

    private Estudiante(string identificacion, string nombre, string apellidos,
                       string correo, DateOnly fechaIngreso)
    {
        Identificacion = identificacion;
        Nombre = nombre;
        Apellidos = apellidos;
        Correo = correo;
        FechaIngreso = fechaIngreso;
        Estado = EstadoEstudiante.Activo;
    }

    public static Estudiante Crear(string? identificacion, string? nombre, string? apellidos,
                                   string? correo, DateOnly fechaIngreso)
    {
        return new Estudiante(
            Guard.Requerido(identificacion, "La identificación", MaxIdentificacion),
            Guard.Requerido(nombre, "El nombre", MaxNombre),
            Guard.Requerido(apellidos, "Los apellidos", MaxApellidos),
            NormalizarCorreo(correo),
            fechaIngreso);
    }

    public void ActualizarDatos(string? nombre, string? apellidos, string? correo)
    {
        // Se valida todo ANTES de asignar: si algo falla, la entidad no queda a medias.
        var nuevoNombre = Guard.Requerido(nombre, "El nombre", MaxNombre);
        var nuevosApellidos = Guard.Requerido(apellidos, "Los apellidos", MaxApellidos);
        var nuevoCorreo = NormalizarCorreo(correo);

        Nombre = nuevoNombre;
        Apellidos = nuevosApellidos;
        Correo = nuevoCorreo;
    }

    public void Desactivar() => Estado = EstadoEstudiante.Inactivo;

    public void Activar() => Estado = EstadoEstudiante.Activo;

    private static string NormalizarCorreo(string? correo)
    {
        var limpio = Guard.Requerido(correo, "El correo", MaxCorreo).ToLowerInvariant();

        // Address != limpio descarta formatos como "Juan <juan@x.com>".
        if (!MailAddress.TryCreate(limpio, out var direccion) || direccion.Address != limpio)
            throw new DomainException("El correo no tiene un formato válido.");

        return limpio;
    }
}