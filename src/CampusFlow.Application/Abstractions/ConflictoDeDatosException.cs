using System;
using System.Collections.Generic;
using System.Text;

namespace CampusFlow.Application.Abstractions;

/// <summary>La capa de datos rechazó la operación por una restricción (p. ej. identificación duplicada).</summary>
public sealed class ConflictoDeDatosException(string mensaje, Exception? inner = null)
    : Exception(mensaje, inner);