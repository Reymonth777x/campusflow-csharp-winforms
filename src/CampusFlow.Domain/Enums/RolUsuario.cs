using System;
using System.Collections.Generic;
using System.Text;

namespace CampusFlow.Domain.Enums;

/// <summary>
/// Roles del sistema. Se persisten por nombre; los números fijos evitan sorpresas si alguien reordena el enum.
/// </summary>
public enum RolUsuario
{
    Administrador = 1, // acceso total, incluida la gestión de usuarios
    Docente = 2,       // gestión académica
    Consulta = 3       // solo lectura
}