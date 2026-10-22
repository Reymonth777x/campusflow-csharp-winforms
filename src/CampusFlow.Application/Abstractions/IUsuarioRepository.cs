using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Domain.Entities;

namespace CampusFlow.Application.Abstractions;

public interface IUsuarioRepository
{
    /// <summary>Busca sin distinguir mayúsculas (lo garantiza la colación de la columna).</summary>
    Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario, CancellationToken ct = default);

    /// <summary>Para saber si es el primer arranque (no hay ningún usuario todavía).</summary>
    Task<bool> HayUsuariosAsync(CancellationToken ct = default);

    /// <exception cref="ConflictoDeDatosException">Ya existe ese nombre de usuario.</exception>
    Task AgregarAsync(Usuario usuario, CancellationToken ct = default);

    Task ActualizarAsync(Usuario usuario, CancellationToken ct = default);
}
