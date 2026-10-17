using CampusFlow.WinForms.Common;
using CampusFlow.WinForms.Estudiantes;
using Microsoft.Extensions.DependencyInjection;

namespace CampusFlow.WinForms;


public partial class FrmMain : Form
{
    private readonly IServiceScopeFactory _scopeFactory;

    // TEMPORAL (hasta la Fase 5, navegación): acceso directo a Estudiantes.
    private readonly Button btnEstudiantes = new();

    public FrmMain(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory; // singleton registrado por el host: sirve para crear scopes por pantalla
        InitializeComponent();

        btnEstudiantes.Name = "btnEstudiantes";
        btnEstudiantes.Text = "Estudiantes";
        btnEstudiantes.Location = new Point(24, 24);
        Tema.EstilizarBoton(btnEstudiantes, primario: true);
        btnEstudiantes.Click += btnEstudiantes_Click;
        Controls.Add(btnEstudiantes);
    }

    private void btnEstudiantes_Click(object? sender, EventArgs e)
    {
        // Scope por pantalla: al terminar el using se desecha el formulario y todo lo que se le inyectó.
        using var scope = _scopeFactory.CreateScope();
        var formulario = scope.ServiceProvider.GetRequiredService<FrmEstudiantes>();
        formulario.ShowDialog(this);
    }
}
