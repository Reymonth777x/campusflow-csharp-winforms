using CampusFlow.Application;
using CampusFlow.Infrastructure;
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
        WinFormsApp.Run(host.Services.GetRequiredService<FrmMain>());
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddApplication();
        services.AddInfrastructure();
        services.AddTransient<FrmMain>();
    }

    private static void HandleFatal(Exception? ex)
    {
        // Fase 8: aquí va Serilog.
        MessageBox.Show(
            "Ocurrió un error inesperado. La operación no se completó.",
            "CampusFlow", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}