using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Enums;

namespace CampusFlow.Application.Autenticacion;

/// <summary>
/// Registrada como singleton. Se escribe desde el hilo de UI (el servicio no usa ConfigureAwait(false),
/// así que tras cada await vuelve a ese hilo), por eso no necesita sincronización.
/// </summary>
public sealed class SesionUsuario : ISesionUsuario
{
    public UsuarioDto? Actual { get; private set; }

    public bool EstaAutenticado => Actual is not null;

    public bool TieneRol(params RolUsuario[] roles) => Actual is { } u && roles.Contains(u.Rol);

    public void Iniciar(UsuarioDto usuario) => Actual = usuario;

    public void Cerrar() => Actual = null;
}