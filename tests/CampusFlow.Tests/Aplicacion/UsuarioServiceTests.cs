using CampusFlow.Application.Autenticacion;
using CampusFlow.Application.Common;
using CampusFlow.Application.Usuarios;
using CampusFlow.Application.Usuarios.Validators;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace CampusFlow.Tests.Aplicacion;

public class UsuarioServiceTests
{
    private const string Clave = "Segura2026";

    private readonly UsuarioRepoEnMemoria _repo = new();
    private readonly HasherFalso _hasher = new();
    private readonly SesionUsuario _sesion = new();
    private readonly FakeTimeProvider _reloj = new(new DateTimeOffset(2026, 10, 17, 9, 0, 0, TimeSpan.Zero));
    private readonly UsuarioService _servicio;
    private readonly Usuario _admin; // Id 1: es quien tiene la sesión por defecto

    public UsuarioServiceTests()
    {
        _servicio = new UsuarioService(
            _repo, _hasher, _sesion, _reloj,
            new CrearUsuarioValidator(), new ActualizarUsuarioValidator(), new RestablecerPasswordValidator());

        _admin = Sembrar("admin", RolUsuario.Administrador);
        _sesion.Iniciar(new UsuarioDto(_admin.Id, _admin.NombreUsuario, _admin.NombreCompleto, _admin.Rol, true));
    }

    /// <summary>Crea un usuario directo en el repositorio, sin pasar por el servicio.</summary>
    private Usuario Sembrar(string nombre, RolUsuario rol)
    {
        var usuario = Usuario.Crear(nombre, "Persona " + nombre, _hasher.Hash(Clave), rol, _reloj.GetUtcNow());
        _repo.Sembrar(usuario);
        return usuario;
    }

    /// <summary>Cambia quién tiene la sesión. Con el id 99 (que no existe en el repositorio) simula una sesión desactualizada.</summary>
    private void IniciarSesionComo(RolUsuario rol, int id = 99) =>
        _sesion.Iniciar(new UsuarioDto(id, "otro", "Otro Usuario", rol, true));

    private static CrearUsuarioRequest SolicitudCrear(
        string nombre = "mrojas", string password = Clave, RolUsuario rol = RolUsuario.Docente) =>
        new(nombre, "María Rojas", rol, password, password);

    // ───── Crear ─────

    [Fact]
    public async Task Crear_valido_guarda_el_usuario_con_su_rol_y_la_contrasena_hasheada()
    {
        var resultado = await _servicio.CrearAsync(SolicitudCrear());

        Assert.True(resultado.EsExito);
        Assert.Equal(RolUsuario.Docente, resultado.Valor.Rol);
        Assert.True(resultado.Valor.Activo);

        var guardado = _repo.Todos.Single(u => u.NombreUsuario == "mrojas");
        Assert.NotEqual(Clave, guardado.PasswordHash); // nunca se guarda la contraseña en claro
        Assert.True(_hasher.Verify(Clave, guardado.PasswordHash));
    }

    [Fact]
    public async Task Crear_con_nombre_existente_devuelve_conflicto_sin_importar_mayusculas()
    {
        Sembrar("mrojas", RolUsuario.Docente);

        var resultado = await _servicio.CrearAsync(SolicitudCrear("MROJAS"));

        var error = Assert.Single(resultado.Errores);
        Assert.Equal(TipoError.Conflicto, error.Tipo);
        Assert.Equal("NombreUsuario", error.Campo);
        Assert.Equal(2, _repo.Todos.Count); // admin + el existente: no se agregó nada
    }

    [Fact]
    public async Task Crear_con_datos_invalidos_devuelve_validacion_y_no_guarda()
    {
        var resultado = await _servicio.CrearAsync(SolicitudCrear(password: "corta"));

        Assert.Contains(resultado.Errores, e => e.Tipo == TipoError.Validacion && e.Campo == "Password");
        Assert.Single(_repo.Todos);
    }

    [Fact]
    public async Task Crear_sin_sesion_devuelve_NoAutorizado_y_no_guarda()
    {
        _sesion.Cerrar();

        var resultado = await _servicio.CrearAsync(SolicitudCrear());

        Assert.Equal(TipoError.NoAutorizado, Assert.Single(resultado.Errores).Tipo);
        Assert.Single(_repo.Todos);
    }

    [Theory]
    [InlineData(RolUsuario.Docente)]
    [InlineData(RolUsuario.Consulta)]
    public async Task Crear_sin_ser_administrador_devuelve_NoAutorizado_y_no_guarda(RolUsuario rol)
    {
        IniciarSesionComo(rol);

        var resultado = await _servicio.CrearAsync(SolicitudCrear());

        Assert.Equal(TipoError.NoAutorizado, Assert.Single(resultado.Errores).Tipo);
        Assert.Single(_repo.Todos);
    }

    [Fact]
    public async Task Crear_si_la_bd_rechaza_por_duplicado_devuelve_conflicto()
    {
        _repo.SimularConflictoAlAgregar = true; // otro proceso ganó la carrera

        var resultado = await _servicio.CrearAsync(SolicitudCrear());

        var error = Assert.Single(resultado.Errores);
        Assert.Equal(TipoError.Conflicto, error.Tipo);
        Assert.Equal("NombreUsuario", error.Campo);
    }

    // ───── Actualizar ─────

    [Fact]
    public async Task Actualizar_cambia_nombre_y_rol()
    {
        var docente = Sembrar("jperez", RolUsuario.Docente);

        var resultado = await _servicio.ActualizarAsync(
            new ActualizarUsuarioRequest(docente.Id, "Juan P. Pérez", RolUsuario.Consulta));

        Assert.True(resultado.EsExito);
        Assert.Equal("Juan P. Pérez", docente.NombreCompleto);
        Assert.Equal(RolUsuario.Consulta, docente.Rol);
        Assert.Equal(RolUsuario.Consulta, resultado.Valor.Rol);
    }

    [Fact]
    public async Task Actualizar_inexistente_devuelve_NoEncontrado()
    {
        var resultado = await _servicio.ActualizarAsync(
            new ActualizarUsuarioRequest(99, "Alguien", RolUsuario.Docente));

        Assert.Equal(TipoError.NoEncontrado, Assert.Single(resultado.Errores).Tipo);
    }

    [Fact]
    public async Task Actualizar_el_propio_rol_se_rechaza_y_no_modifica()
    {
        var resultado = await _servicio.ActualizarAsync(
            new ActualizarUsuarioRequest(_admin.Id, "Admin", RolUsuario.Docente));

        var error = Assert.Single(resultado.Errores);
        Assert.Equal(TipoError.Conflicto, error.Tipo);
        Assert.Equal("Rol", error.Campo);
        Assert.Equal(RolUsuario.Administrador, _admin.Rol);
    }

    [Fact]
    public async Task Actualizar_el_propio_nombre_se_permite_y_refresca_la_sesion()
    {
        var resultado = await _servicio.ActualizarAsync(
            new ActualizarUsuarioRequest(_admin.Id, "Nombre Nuevo", RolUsuario.Administrador));

        Assert.True(resultado.EsExito);
        Assert.Equal("Nombre Nuevo", _sesion.Actual!.NombreCompleto);
    }

    [Fact]
    public async Task Degradar_al_ultimo_administrador_activo_se_rechaza()
    {
        // Sesión desactualizada (id 99): la regla de "propia cuenta" no aplica, la de "último administrador" sí.
        IniciarSesionComo(RolUsuario.Administrador);

        var resultado = await _servicio.ActualizarAsync(
            new ActualizarUsuarioRequest(_admin.Id, "Admin", RolUsuario.Docente));

        Assert.Equal(TipoError.Conflicto, Assert.Single(resultado.Errores).Tipo);
        Assert.Equal(RolUsuario.Administrador, _admin.Rol);
    }

    [Fact]
    public async Task Degradar_a_un_administrador_cuando_hay_otro_activo_se_permite()
    {
        var otro = Sembrar("admin2", RolUsuario.Administrador);

        var resultado = await _servicio.ActualizarAsync(
            new ActualizarUsuarioRequest(otro.Id, "Admin Dos", RolUsuario.Docente));

        Assert.True(resultado.EsExito);
        Assert.Equal(RolUsuario.Docente, otro.Rol);
    }

    // ───── Desactivar / Activar ─────

    [Fact]
    public async Task Desactivar_cambia_el_estado()
    {
        var docente = Sembrar("jperez", RolUsuario.Docente);

        var resultado = await _servicio.DesactivarAsync(docente.Id);

        Assert.True(resultado.EsExito);
        Assert.False(docente.Activo);
    }

    [Fact]
    public async Task Desactivar_la_propia_cuenta_se_rechaza()
    {
        var resultado = await _servicio.DesactivarAsync(_admin.Id);

        Assert.Equal(TipoError.Conflicto, Assert.Single(resultado.Errores).Tipo);
        Assert.True(_admin.Activo);
    }

    [Fact]
    public async Task Desactivar_al_ultimo_administrador_activo_se_rechaza()
    {
        IniciarSesionComo(RolUsuario.Administrador); // sesión desactualizada: ver prueba de degradar

        var resultado = await _servicio.DesactivarAsync(_admin.Id);

        Assert.Equal(TipoError.Conflicto, Assert.Single(resultado.Errores).Tipo);
        Assert.True(_admin.Activo);
    }

    [Fact]
    public async Task Desactivar_inexistente_devuelve_NoEncontrado()
    {
        var resultado = await _servicio.DesactivarAsync(99);

        Assert.Equal(TipoError.NoEncontrado, Assert.Single(resultado.Errores).Tipo);
    }

    [Theory]
    [InlineData(RolUsuario.Docente)]
    [InlineData(RolUsuario.Consulta)]
    public async Task Desactivar_sin_ser_administrador_devuelve_NoAutorizado_y_no_cambia_nada(RolUsuario rol)
    {
        var docente = Sembrar("jperez", RolUsuario.Docente);
        IniciarSesionComo(rol);

        var resultado = await _servicio.DesactivarAsync(docente.Id);

        Assert.Equal(TipoError.NoAutorizado, Assert.Single(resultado.Errores).Tipo);
        Assert.True(docente.Activo);
    }

    [Fact]
    public async Task Activar_reactiva_la_cuenta()
    {
        var docente = Sembrar("jperez", RolUsuario.Docente);
        docente.Desactivar();

        var resultado = await _servicio.ActivarAsync(docente.Id);

        Assert.True(resultado.EsExito);
        Assert.True(docente.Activo);
    }

    // ───── Restablecer contraseña ─────

    [Fact]
    public async Task Restablecer_cambia_la_contrasena_y_desbloquea_la_cuenta()
    {
        var docente = Sembrar("jperez", RolUsuario.Docente);
        for (var i = 0; i < Usuario.MaxIntentosFallidos; i++) docente.RegistrarIntentoFallido(_reloj.GetUtcNow());
        Assert.True(docente.EstaBloqueado(_reloj.GetUtcNow()));

        var resultado = await _servicio.RestablecerPasswordAsync(
            new RestablecerPasswordRequest(docente.Id, "NuevaClave2026", "NuevaClave2026"));

        Assert.True(resultado.EsExito);
        Assert.True(_hasher.Verify("NuevaClave2026", docente.PasswordHash));
        Assert.False(docente.EstaBloqueado(_reloj.GetUtcNow()));
        Assert.Equal(0, docente.IntentosFallidos);
    }

    [Fact]
    public async Task Restablecer_con_password_debil_devuelve_validacion_y_no_cambia_nada()
    {
        var docente = Sembrar("jperez", RolUsuario.Docente);

        var resultado = await _servicio.RestablecerPasswordAsync(
            new RestablecerPasswordRequest(docente.Id, "corta1", "corta1"));

        Assert.Contains(resultado.Errores, e => e.Tipo == TipoError.Validacion && e.Campo == "NuevaPassword");
        Assert.True(_hasher.Verify(Clave, docente.PasswordHash)); // sigue la contraseña anterior
    }

    [Fact]
    public async Task Restablecer_con_password_igual_al_usuario_se_rechaza()
    {
        var docente = Sembrar("jperez2026", RolUsuario.Docente);

        var resultado = await _servicio.RestablecerPasswordAsync(
            new RestablecerPasswordRequest(docente.Id, "jperez2026", "jperez2026"));

        var error = Assert.Single(resultado.Errores);
        Assert.Equal(TipoError.Validacion, error.Tipo);
        Assert.Equal("NuevaPassword", error.Campo);
        Assert.True(_hasher.Verify(Clave, docente.PasswordHash));
    }

    [Fact]
    public async Task Restablecer_inexistente_devuelve_NoEncontrado()
    {
        var resultado = await _servicio.RestablecerPasswordAsync(
            new RestablecerPasswordRequest(99, "NuevaClave2026", "NuevaClave2026"));

        Assert.Equal(TipoError.NoEncontrado, Assert.Single(resultado.Errores).Tipo);
    }

    [Fact]
    public async Task Restablecer_sin_ser_administrador_devuelve_NoAutorizado_y_no_cambia_nada()
    {
        var docente = Sembrar("jperez", RolUsuario.Docente);
        IniciarSesionComo(RolUsuario.Docente);

        var resultado = await _servicio.RestablecerPasswordAsync(
            new RestablecerPasswordRequest(docente.Id, "NuevaClave2026", "NuevaClave2026"));

        Assert.Equal(TipoError.NoAutorizado, Assert.Single(resultado.Errores).Tipo);
        Assert.True(_hasher.Verify(Clave, docente.PasswordHash));
    }

    // ───── Listar ─────

    [Fact]
    public async Task Listar_sin_ser_administrador_lanza_UnauthorizedAccessException()
    {
        IniciarSesionComo(RolUsuario.Docente);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _servicio.ListarAsync());
    }

    [Fact]
    public async Task Listar_devuelve_todos_ordenados_y_marca_los_bloqueados()
    {
        var zeta = Sembrar("zeta", RolUsuario.Docente);
        Sembrar("beta", RolUsuario.Consulta);
        for (var i = 0; i < Usuario.MaxIntentosFallidos; i++) zeta.RegistrarIntentoFallido(_reloj.GetUtcNow());

        var lista = await _servicio.ListarAsync();

        Assert.Equal(new[] { "admin", "beta", "zeta" }, lista.Select(u => u.NombreUsuario).ToArray());
        Assert.True(lista.Single(u => u.NombreUsuario == "zeta").Bloqueado);
        Assert.False(lista.Single(u => u.NombreUsuario == "beta").Bloqueado);
    }

    // ───── Contraseña temporal ─────

    [Fact]
    public async Task Crear_deja_al_usuario_obligado_a_cambiar_su_contrasena_al_primer_ingreso()
    {
        await _servicio.CrearAsync(SolicitudCrear());

        Assert.True(_repo.Todos.Single(u => u.NombreUsuario == "mrojas").DebeCambiarPassword);
    }

    [Fact]
    public async Task Restablecer_deja_al_usuario_obligado_a_cambiar_su_contrasena()
    {
        var docente = Sembrar("jperez", RolUsuario.Docente);
        Assert.False(docente.DebeCambiarPassword);

        await _servicio.RestablecerPasswordAsync(
            new RestablecerPasswordRequest(docente.Id, "NuevaClave2026", "NuevaClave2026"));

        Assert.True(docente.DebeCambiarPassword);
    }
}