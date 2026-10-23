using CampusFlow.Application;
using CampusFlow.Application.Autenticacion;
using CampusFlow.Infrastructure;
using CampusFlow.WinForms.Acceso;
using CampusFlow.WinForms.Estudiantes;
using CampusFlow.WinForms.Usuarios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WinFormsApp = System.Windows.Forms.Application;

namespace CampusFlow.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        WinFormsApp.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        WinFormsApp.ThreadException += (_, e) => HandleFatal(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => HandleFatal(e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) => { HandleFatal(e.Exception); e.SetObserved(); };

        var builder = Host.CreateApplicationBuilder();
        ConfigureServices(builder.Services);

        using var host = builder.Build();
        host.Services.MigrarBaseDeDatos();

        // Ciclo de vida: login → ventana principal → (si se cerró la sesión) login otra vez.
        while (true)
        {
            if (!Autenticar(host.Services)) return; // canceló el login: la aplicación termina

            bool volverAlLogin;
            using (var scope = host.Services.CreateScope())
            {
                var principal = scope.ServiceProvider.GetRequiredService<FrmMain>();
                WinFormsApp.Run(principal);
                volverAlLogin = principal.CerrarSesionSolicitada;
            }

            if (!volverAlLogin) return; // cerró con la X: la aplicación termina
        }
    }

    /// <summary>
    /// Primer arranque (sin usuarios): crea el administrador. Después pide login; si la cuenta tiene una
    /// contraseña temporal, obliga a cambiarla antes de dejar entrar. Devuelve true solo si hay una sesión
    /// plenamente habilitada; false si la persona canceló el login (la aplicación termina).
    /// </summary>
    private static bool Autenticar(IServiceProvider servicios)
    {
        // Un scope para este ciclo: al terminar se desechan los formularios resueltos.
        using var scope = servicios.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var sesion = scope.ServiceProvider.GetRequiredService<ISesionUsuario>();

        // Task.Run + GetResult: aún no hay ciclo de mensajes. Correr la consulta en el pool (sin contexto de
        // sincronización) evita el interbloqueo que causaría esperar en el hilo de UI una tarea que
        // necesita volver a él. Es una consulta local y breve, aceptable solo en el arranque.
        var hayUsuarios = Task.Run(() => auth.HayUsuariosAsync()).GetAwaiter().GetResult();

        if (!hayUsuarios)
        {
            var alta = scope.ServiceProvider.GetRequiredService<FrmAdministradorInicial>();
            if (alta.ShowDialog() != DialogResult.OK) return false; // sin administrador no se puede usar el sistema
        }

        // Bucle: si la persona cierra sesión en el cambio obligatorio, vuelve al login sin cerrar la aplicación.
        while (true)
        {
            var login = scope.ServiceProvider.GetRequiredService<FrmLogin>();
            if (login.ShowDialog() != DialogResult.OK) return false;

            // Cuenta normal: entra directo.
            if (!sesion.RequiereCambioDePassword) return true;

            // Contraseña temporal: hasta que no elija una propia, la sesión no cuenta como autenticada
            // y ningún servicio la atiende (ver SesionUsuario).
            var cambio = scope.ServiceProvider.GetRequiredService<FrmCambiarPassword>();
            cambio.Cargar(obligatorio: true);
            if (cambio.ShowDialog() == DialogResult.OK) return true;

            auth.CerrarSesion(); // eligió "Cerrar sesión" (o cerró la ventana a la fuerza): de vuelta al login
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddApplication();
        services.AddInfrastructure();

        services.AddTransient<FrmMain>();
        services.AddTransient<FrmLogin>();
        services.AddTransient<FrmAdministradorInicial>();
        services.AddTransient<FrmCambiarPassword>();
        services.AddTransient<FrmEstudiantes>();
        services.AddTransient<FrmEstudianteDetalle>();
        services.AddTransient<FrmUsuarios>();
        services.AddTransient<FrmUsuarioDetalle>();
        services.AddTransient<FrmRestablecerPassword>();
    }

    private static void HandleFatal(Exception? ex)
    {
        // Fase 8: aquí va Serilog. Por ahora, mensaje controlado, no stack trace al usuario.
        MessageBox.Show(
            "Ocurrió un error inesperado. La operación no se completó.",
            "CampusFlow", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}