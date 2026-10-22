using CampusFlow.Domain.Enums;

namespace CampusFlow.Application.Autenticacion;

/// <summary>
/// Fuente única de verdad de "qué rol puede hacer qué". Servicios y UI consultan aquí,
/// así que cambiar una regla es tocar un solo lugar.
/// </summary>
public static class Politicas
{
    /// <summary>Crear, editar y activar/desactivar estudiantes. El rol Consulta queda como solo lectura.</summary>
    public static bool PuedeEscribirEstudiantes(this ISesionUsuario sesion) =>
        sesion.TieneRol(RolUsuario.Administrador, RolUsuario.Docente);

    /// <summary>Gestión de cuentas de usuario: exclusiva del Administrador.</summary>
    public static bool PuedeGestionarUsuarios(this ISesionUsuario sesion) =>
        sesion.TieneRol(RolUsuario.Administrador);
}