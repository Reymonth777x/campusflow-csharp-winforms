using System;
using System.Collections.Generic;
using System.Text;
using CampusFlow.Application.Common;

namespace CampusFlow.Application.Autenticacion;

public interface IAuthService
{
    /// <summary>Valida credenciales y, si son correctas, abre la sesión. Aplica el bloqueo por intentos fallidos.</summary>
    Task<Result<UsuarioDto>> IniciarSesionAsync(LoginRequest request, CancellationToken ct = default);

    void CerrarSesion();

    /// <summary>false en el primer arranque: la UI debe ofrecer crear el administrador inicial en vez de iniciar sesión.</summary>
    Task<bool> HayUsuariosAsync(CancellationToken ct = default);

    /// <summary>Crea el primer administrador. Solo funciona mientras no exista ningún usuario.</summary>
    Task<Result<UsuarioDto>> RegistrarAdministradorInicialAsync(
        CrearAdministradorInicialRequest request, CancellationToken ct = default);
}