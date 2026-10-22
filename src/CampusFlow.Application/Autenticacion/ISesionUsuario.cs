using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Enums;

namespace CampusFlow.Application.Autenticacion;

/// <summary>Quién está usando la aplicación ahora mismo. Una única sesión por proceso.</summary>
public interface ISesionUsuario
{
    UsuarioDto? Actual { get; }
    bool EstaAutenticado { get; }

    /// <summary>true si hay sesión y el rol del usuario es uno de los indicados.</summary>
    bool TieneRol(params RolUsuario[] roles);

    void Iniciar(UsuarioDto usuario);
    void Cerrar();
}