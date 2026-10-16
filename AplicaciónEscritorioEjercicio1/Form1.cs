namespace AplicaciónEscritorioEjercicio1
{
    public partial class frmPrincipal : Form
    {
        public frmPrincipal()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            DialogResult respuesta = MessageBox.Show(
                "¿Está seguro de que desea salir de la aplicación?",
                "Confirmar salida",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta == DialogResult.Yes)
            {
                //Cierra el formulario donde está ubicado el control
                //this.Close();

                //Cierra toda la aplicación
                Application.Exit();
            }
            }

        private void btnAceptar_Click_1(object sender, EventArgs e)
        {
            frmEstudiantes formularioEstudiantes = new frmEstudiantes();
            formularioEstudiantes.ShowDialog();
            formularioEstudiantes.Dispose();

        }
    }
}
