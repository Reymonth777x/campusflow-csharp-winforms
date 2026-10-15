using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AplicaciónEscritorioEjercicio1
{
    public partial class frmEstudiantes : Form
    {
        public frmEstudiantes()
        {
            InitializeComponent();
        }

        private void lblNuevoElemento_Click(object sender, EventArgs e)
        {

        }


        private void btnAgregarNuevo_Click(object sender, EventArgs e)
        {
            ///Permitimos agregar a la lista sin repetir ni espacios extras 
            string nombre = txtNuevoElemento.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("Escribe el nombre del estudiante.");
                txtNuevoElemento.Focus();
                return;
            }

            if (lstListaNombres.Items.Contains(nombre))
            {
                MessageBox.Show(
                    "Este estudiante ya está registrado.",
                    "Nombre duplicado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtNuevoElemento.SelectAll();
                txtNuevoElemento.Focus();
                return;
            }

            foreach (var elemento in lstListaNombres.Items)
            {
                string nombreExistente =
                    elemento?.ToString() ?? string.Empty;

                if (string.Equals(
                    nombreExistente,
                    nombre,
                    StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "Este estudiante ya está registrado.",
                        "Nombre duplicado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    txtNuevoElemento.SelectAll();
                    txtNuevoElemento.Focus();
                    return;
                }
            }

            lstListaNombres.Items.Add(nombre);

            txtNuevoElemento.Clear();
            ActualizarCantidad();
            txtNuevoElemento.Focus();
        }





        private void ActualizarCantidad()
        {
            lblCantidad.Text =
                $"Estudiantes registrados: {lstListaNombres.Items.Count}";
        }

        private void lstListaNombres_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtNuevoElemento_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmEstudiantes_Load(object sender, EventArgs e)
        {

        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {

            int indiceSeleccionado = lstListaNombres.SelectedIndex;

            if (indiceSeleccionado == -1)
            {
                MessageBox.Show("Selecciona un estudiante de la lista.");
                return;
            }

            string nombreSeleccionado =
                lstListaNombres.Items[indiceSeleccionado]?.ToString()
                ?? string.Empty;

            DialogResult respuesta = MessageBox.Show(
                $"¿Deseas eliminar a {nombreSeleccionado}?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta == DialogResult.Yes)
            {
                lstListaNombres.Items.RemoveAt(indiceSeleccionado);
                ActualizarCantidad();
            }


        }

        private void btnLimpiarLista_Click(object sender, EventArgs e)
        {

            if (lstListaNombres.Items.Count == 0)
            {
                MessageBox.Show("La lista ya está vacía.");
                return;
            }

            DialogResult respuesta = MessageBox.Show(
                "¿Seguro que deseas eliminar todos los estudiantes?",
                "Confirmar limpieza",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (respuesta == DialogResult.Yes)
            {
                lstListaNombres.Items.Clear();
                ActualizarCantidad();
            }

        }

        private void lblCantidad_Click(object sender, EventArgs e)
        {

        }

        private void btnEditarSeleccionado_Click(object sender, EventArgs e)
        {

            int indiceSeleccionado = lstListaNombres.SelectedIndex;

            if (indiceSeleccionado == -1)
            {
                MessageBox.Show("Selecciona un estudiante para editar.");
                return;
            }

            string nuevoNombre = txtNuevoElemento.Text.Trim();

            if (string.IsNullOrWhiteSpace(nuevoNombre))
            {
                MessageBox.Show("Escribe el nuevo nombre.");
                txtNuevoElemento.Focus();
                return;
            }

            for (int i = 0; i < lstListaNombres.Items.Count; i++)
            {
                if (i == indiceSeleccionado)
                {
                    continue;
                }

                string nombreExistente =
                    lstListaNombres.Items[i]?.ToString() ?? string.Empty;

                if (string.Equals(
                    nombreExistente,
                    nuevoNombre,
                    StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Ya existe otro estudiante con ese nombre.");
                    return;
                }
            }

            lstListaNombres.Items[indiceSeleccionado] = nuevoNombre;

            txtNuevoElemento.Clear();
            ActualizarCantidad();
            txtNuevoElemento.Focus();

            MessageBox.Show("Estudiante actualizado correctamente.");

        }
    }
}
