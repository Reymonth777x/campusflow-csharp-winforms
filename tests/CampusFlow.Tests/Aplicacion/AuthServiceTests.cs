using CampusFlow.Application.Autenticacion;
using CampusFlow.Application.Autenticacion.Validators;
using CampusFlow.Application.Common;
using CampusFlow.Domain.Entities;
using CampusFlow.Domain.Enums;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace CampusFlow.Tests.Aplicacion;

public class AuthServiceTests
{
    private const string Clave = "Segura2026";

    private readonly UsuarioRepoEnMemoria _repo = new();
    private readonly HasherFalso _hasher = new();
    private readonly SesionUsuario _sesion = new();
    private readonly FakeTimeProvider _reloj = new(new DateTimeOffset(2026, 10, 17, 9, 0, 0, TimeSpan.Zero));
    private readonly AuthService _servicio;

    public AuthServiceTests()
    {
        _servicio = new AuthService(
            _repo, _hasher, _sesion, _reloj,
            new LoginValidator(), new CrearAdministradorInicialValidator(), new CambiarPasswordValidator());
    }

    /// <summary>Crea un usuario directamente en el repositorio (sin pasar por el servicio) y devuelve la entidad.</summary>
    private async Task<Usuario> SembrarAsync(string nombre = "jperez", RolUsuario rol = RolUsuario.Docente)
    {
        var usuario = Usuario.Crear(nombre, "Juan Pérez", _hasher.Hash(Clave), rol, _reloj.GetUtcNow());
        await _repo.AgregarAsync(usuario);
        return usuario;
    }

    private Task<Result<UsuarioDto>> Login(string usuario = "jperez", string password = Clave) =>
        _servicio.IniciarSesionAsync(new LoginRequest(usuario, password));

    private async Task FallarLoginAsync(int veces)
    {
        for (var i = 0; i < veces; i++) await Login(password: "incorrecta1");
    }

    private static CrearAdministradorInicialRequest SolicitudAdmin(string password = Clave) =>
        new("admin", "Administrador General", password, password);

    [Fact]
    public async Task Login_correcto_inicia_sesion_con_el_rol_del_usuario()
    {
        await SembrarAsync(rol: RolUsuario.Docente);

        var resultado = await Login();

        Assert.True(resultado.EsExito);
        Assert.Equal("jperez", resultado.Valor.NombreUsuario);
        Assert.True(_sesion.EstaAutenticado);
        Assert.True(_sesion.TieneRol(RolUsuario.Docente));
        Assert.False(_sesion.TieneRol(RolUsuario.Administrador));
    }

    [Fact]
    public async Task Login_ignora_mayusculas_en_el_nombre_de_usuario()
    {
        await SembrarAsync();

        Assert.True((await Login("JPEREZ")).EsExito);
    }

    [Fact]
    public async Task Usuario_inexistente_devuelve_credenciales_invalidas_y_no_abre_sesion()
    {
        var resultado = await Login("fantasma");

        Assert.Equal(TipoError.NoAutorizado, Assert.Single(resultado.Errores).Tipo);
        Assert.False(_sesion.EstaAutenticado);
    }

    [Fact]
    public async Task Password_incorrecta_da_el_mismo_mensaje_que_usuario_inexistente()
    {
        await SembrarAsync();

        var inexistente = await Login("fantasma");
        var incorrecta = await Login(password: "incorrecta1");

        // Mensajes idénticos: no se puede deducir qué usuarios existen.
        Assert.Equal(inexistente.Errores[0].Mensaje, incorrecta.Errores[0].Mensaje);
        Assert.False(_sesion.EstaAutenticado);
    }

    [Fact]
    public async Task Cinco_fallos_bloquean_la_cuenta_incluso_con_la_contrasena_correcta()
    {
        await SembrarAsync();

        await FallarLoginAsync(Usuario.MaxIntentosFallidos);
        var resultado = await Login(); // ahora con la clave correcta

        Assert.True(resultado.EsFalla);
        Assert.Contains("bloqueada", resultado.Errores[0].Mensaje);
        Assert.False(_sesion.EstaAutenticado);
    }

    [Fact]
    public async Task Pasado_el_tiempo_de_bloqueo_se_puede_entrar_de_nuevo()
    {
        await SembrarAsync();
        await FallarLoginAsync(Usuario.MaxIntentosFallidos);

        _reloj.Advance(Usuario.DuracionBloqueo + TimeSpan.FromSeconds(1));

        Assert.True((await Login()).EsExito);
    }

    [Fact]
    public async Task Un_acceso_exitoso_reinicia_el_contador_de_intentos_fallidos()
    {
        await SembrarAsync();

        await FallarLoginAsync(4);
        Assert.True((await Login()).EsExito);
        _sesion.Cerrar();

        // Si el contador no se hubiera reiniciado, estos 4 fallos sumarían 8 y la cuenta ya estaría bloqueada.
        await FallarLoginAsync(4);

        Assert.True((await Login()).EsExito);
    }

    [Fact]
    public async Task Cuenta_desactivada_con_contrasena_correcta_informa_que_esta_desactivada()
    {
        var usuario = await SembrarAsync();
        usuario.Desactivar();

        var resultado = await Login();

        Assert.Contains("desactivada", resultado.Errores[0].Mensaje);
        Assert.False(_sesion.EstaAutenticado);
    }

    [Fact]
    public async Task Cuenta_desactivada_con_contrasena_incorrecta_no_revela_su_estado()
    {
        var usuario = await SembrarAsync();
        usuario.Desactivar();

        var generico = (await Login("fantasma")).Errores[0].Mensaje;
        var resultado = await Login(password: "incorrecta1");

        Assert.Equal(generico, resultado.Errores[0].Mensaje);
    }

    [Fact]
    public async Task Login_con_campos_vacios_devuelve_errores_de_validacion_por_campo()
    {
        var resultado = await Login("", "");

        Assert.Contains(resultado.Errores, e => e.Tipo == TipoError.Validacion && e.Campo == "NombreUsuario");
        Assert.Contains(resultado.Errores, e => e.Tipo == TipoError.Validacion && e.Campo == "Password");
    }

    [Fact]
    public async Task CerrarSesion_limpia_la_sesion()
    {
        await SembrarAsync();
        await Login();

        _servicio.CerrarSesion();

        Assert.False(_sesion.EstaAutenticado);
        Assert.Null(_sesion.Actual);
    }

    [Fact]
    public async Task HayUsuarios_refleja_si_el_repositorio_tiene_registros()
    {
        Assert.False(await _servicio.HayUsuariosAsync());

        await SembrarAsync();

        Assert.True(await _servicio.HayUsuariosAsync());
    }

    [Fact]
    public async Task Administrador_inicial_se_crea_con_rol_administrador_y_contrasena_hasheada()
    {
        var resultado = await _servicio.RegistrarAdministradorInicialAsync(SolicitudAdmin());

        Assert.True(resultado.EsExito);
        Assert.Equal(RolUsuario.Administrador, resultado.Valor.Rol);

        var guardado = Assert.Single(_repo.Todos);
        Assert.NotEqual(Clave, guardado.PasswordHash); // nunca se guarda la contraseña en claro
        Assert.True(_hasher.Verify(Clave, guardado.PasswordHash));
    }

    [Fact]
    public async Task Administrador_inicial_se_rechaza_si_ya_existen_usuarios()
    {
        await SembrarAsync();

        var resultado = await _servicio.RegistrarAdministradorInicialAsync(SolicitudAdmin());

        Assert.Equal(TipoError.Conflicto, Assert.Single(resultado.Errores).Tipo);
        Assert.Single(_repo.Todos); // no se agregó nada
    }

    [Fact]
    public async Task Administrador_inicial_con_datos_invalidos_devuelve_validacion_y_no_guarda()
    {
        var resultado = await _servicio.RegistrarAdministradorInicialAsync(SolicitudAdmin("corta"));

        Assert.Contains(resultado.Errores, e => e.Tipo == TipoError.Validacion && e.Campo == "Password");
        Assert.Empty(_repo.Todos);
    }

    [Fact]
    public async Task Administrador_inicial_si_la_bd_rechaza_por_duplicado_devuelve_conflicto()
    {
        _repo.SimularConflictoAlAgregar = true;

        var resultado = await _servicio.RegistrarAdministradorInicialAsync(SolicitudAdmin());

        var error = Assert.Single(resultado.Errores);
        Assert.Equal(TipoError.Conflicto, error.Tipo);
        Assert.Equal("NombreUsuario", error.Campo);
    }

    // ───── Cambiar contraseña ─────

    private Task<Result> Cambiar(string actual = Clave, string nueva = "NuevaClave2026", string? confirmar = null) =>
        _servicio.CambiarPasswordAsync(new CambiarPasswordRequest(actual, nueva, confirmar ?? nueva));

    [Fact]
    public async Task Login_de_usuario_con_password_temporal_abre_una_sesion_pendiente_de_cambio()
    {
        var usuario = await SembrarAsync();
        usuario.ExigirCambioDePassword();

        var resultado = await Login();

        Assert.True(resultado.EsExito);
        Assert.True(resultado.Valor.DebeCambiarPassword);
        Assert.True(_sesion.RequiereCambioDePassword);
        Assert.False(_sesion.EstaAutenticado); // todavía no puede usar el sistema
    }

    [Fact]
    public async Task Cambiar_password_valido_guarda_el_hash_levanta_la_exigencia_y_habilita_la_sesion()
    {
        var usuario = await SembrarAsync();
        usuario.ExigirCambioDePassword();
        await Login();

        var resultado = await Cambiar();

        Assert.True(resultado.EsExito);
        Assert.True(_hasher.Verify("NuevaClave2026", usuario.PasswordHash));
        Assert.False(usuario.DebeCambiarPassword);
        Assert.True(_sesion.EstaAutenticado);
        Assert.False(_sesion.RequiereCambioDePassword);
    }

    [Fact]
    public async Task Cambiar_con_la_contrasena_actual_incorrecta_falla_en_ese_campo_y_cuenta_el_intento()
    {
        var usuario = await SembrarAsync();
        await Login();

        var resultado = await Cambiar(actual: "incorrecta1");

        var error = Assert.Single(resultado.Errores);
        Assert.Equal(TipoError.Validacion, error.Tipo);
        Assert.Equal("PasswordActual", error.Campo);
        Assert.Equal(1, usuario.IntentosFallidos);
        Assert.True(_hasher.Verify(Clave, usuario.PasswordHash)); // sigue la contraseña anterior
    }

    [Fact]
    public async Task Cinco_fallos_al_cambiar_bloquean_la_cuenta_incluso_con_la_contrasena_correcta()
    {
        var usuario = await SembrarAsync();
        await Login();

        Result ultimo = Result.Exito();
        for (var i = 0; i < Usuario.MaxIntentosFallidos; i++)
            ultimo = await Cambiar(actual: "incorrecta1");

        Assert.Contains("bloqueada", ultimo.Errores[0].Mensaje);
        Assert.True(usuario.EstaBloqueado(_reloj.GetUtcNow()));

        var conLaCorrecta = await Cambiar(); // ahora con la contraseña actual correcta

        Assert.Contains("bloqueada", conLaCorrecta.Errores[0].Mensaje);
        Assert.True(_hasher.Verify(Clave, usuario.PasswordHash));
    }

    [Fact]
    public async Task Cambiar_sin_sesion_devuelve_NoAutorizado()
    {
        var resultado = await Cambiar();

        Assert.Equal(TipoError.NoAutorizado, Assert.Single(resultado.Errores).Tipo);
    }

    [Fact]
    public async Task Cambiar_con_nueva_contrasena_debil_devuelve_validacion_y_no_cambia_nada()
    {
        var usuario = await SembrarAsync();
        await Login();

        var resultado = await Cambiar(nueva: "corta1");

        Assert.Contains(resultado.Errores, e => e.Tipo == TipoError.Validacion && e.Campo == "NuevaPassword");
        Assert.True(_hasher.Verify(Clave, usuario.PasswordHash));
    }

    [Fact]
    public async Task Cambiar_a_una_contrasena_igual_al_usuario_se_rechaza()
    {
        var usuario = await SembrarAsync("jperez2026");
        await Login("jperez2026");

        var resultado = await Cambiar(nueva: "jperez2026");

        var error = Assert.Single(resultado.Errores);
        Assert.Equal(TipoError.Validacion, error.Tipo);
        Assert.Equal("NuevaPassword", error.Campo);
        Assert.True(_hasher.Verify(Clave, usuario.PasswordHash));
    }

    [Fact]
    public async Task Cambiar_si_la_cuenta_se_desactivo_a_mitad_de_sesion_se_rechaza()
    {
        var usuario = await SembrarAsync();
        await Login();
        usuario.Desactivar();

        var resultado = await Cambiar();

        Assert.Equal(TipoError.NoAutorizado, Assert.Single(resultado.Errores).Tipo);
        Assert.True(_hasher.Verify(Clave, usuario.PasswordHash));
    }

    [Fact]
    public async Task Administrador_inicial_no_queda_obligado_a_cambiar_su_contrasena()
    {
        await _servicio.RegistrarAdministradorInicialAsync(SolicitudAdmin());

        Assert.False(Assert.Single(_repo.Todos).DebeCambiarPassword); // eligió su propia contraseña
    }
}