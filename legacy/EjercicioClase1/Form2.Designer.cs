namespace AplicaciónEscritorioEjercicio1
{
    partial class frmEstudiantes
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEstudiantes));
            lblNuevoElemento = new Label();
            txtNuevoElemento = new TextBox();
            btnAgregarNuevo = new Button();
            lstListaNombres = new ListBox();
            btnEliminar = new Button();
            lblCantidad = new Label();
            btnLimpiarLista = new Button();
            btnEditarSeleccionado = new Button();
            btnAtras = new Button();
            SuspendLayout();
            // 
            // lblNuevoElemento
            // 
            lblNuevoElemento.AutoSize = true;
            lblNuevoElemento.Location = new Point(86, 134);
            lblNuevoElemento.Name = "lblNuevoElemento";
            lblNuevoElemento.Size = new Size(100, 15);
            lblNuevoElemento.TabIndex = 0;
            lblNuevoElemento.Text = "Nuevo Estudiante";
            lblNuevoElemento.Click += lblNuevoElemento_Click;
            // 
            // txtNuevoElemento
            // 
            txtNuevoElemento.Location = new Point(192, 134);
            txtNuevoElemento.MaxLength = 100;
            txtNuevoElemento.Name = "txtNuevoElemento";
            txtNuevoElemento.Size = new Size(100, 23);
            txtNuevoElemento.TabIndex = 1;
            txtNuevoElemento.TextChanged += txtNuevoElemento_TextChanged;
            // 
            // btnAgregarNuevo
            // 
            btnAgregarNuevo.BackgroundImage = (Image)resources.GetObject("btnAgregarNuevo.BackgroundImage");
            btnAgregarNuevo.BackgroundImageLayout = ImageLayout.Stretch;
            btnAgregarNuevo.Location = new Point(298, 134);
            btnAgregarNuevo.Name = "btnAgregarNuevo";
            btnAgregarNuevo.Size = new Size(29, 21);
            btnAgregarNuevo.TabIndex = 2;
            btnAgregarNuevo.UseVisualStyleBackColor = true;
            btnAgregarNuevo.Click += btnAgregarNuevo_Click;
            // 
            // lstListaNombres
            // 
            lstListaNombres.BackColor = Color.DarkCyan;
            lstListaNombres.FormattingEnabled = true;
            lstListaNombres.Location = new Point(161, 163);
            lstListaNombres.Name = "lstListaNombres";
            lstListaNombres.Size = new Size(156, 139);
            lstListaNombres.TabIndex = 3;
            lstListaNombres.SelectedIndexChanged += lstListaNombres_SelectedIndexChanged;
            // 
            // btnEliminar
            // 
            btnEliminar.BackgroundImage = (Image)resources.GetObject("btnEliminar.BackgroundImage");
            btnEliminar.BackgroundImageLayout = ImageLayout.Stretch;
            btnEliminar.Location = new Point(282, 396);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(35, 28);
            btnEliminar.TabIndex = 4;
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // lblCantidad
            // 
            lblCantidad.AutoSize = true;
            lblCantidad.Location = new Point(173, 305);
            lblCantidad.Name = "lblCantidad";
            lblCantidad.Size = new Size(134, 15);
            lblCantidad.TabIndex = 6;
            lblCantidad.Text = "Estudiantes registrados: ";
            lblCantidad.Click += lblCantidad_Click;
            // 
            // btnLimpiarLista
            // 
            btnLimpiarLista.Location = new Point(340, 396);
            btnLimpiarLista.Name = "btnLimpiarLista";
            btnLimpiarLista.Size = new Size(75, 23);
            btnLimpiarLista.TabIndex = 7;
            btnLimpiarLista.Text = "Vaciar Lista";
            btnLimpiarLista.UseVisualStyleBackColor = true;
            btnLimpiarLista.Click += btnLimpiarLista_Click;
            // 
            // btnEditarSeleccionado
            // 
            btnEditarSeleccionado.Location = new Point(275, 367);
            btnEditarSeleccionado.Name = "btnEditarSeleccionado";
            btnEditarSeleccionado.Size = new Size(140, 23);
            btnEditarSeleccionado.TabIndex = 8;
            btnEditarSeleccionado.Text = "Editar Seleccionado";
            btnEditarSeleccionado.UseVisualStyleBackColor = true;
            btnEditarSeleccionado.Click += btnEditarSeleccionado_Click;
            // 
            // btnAtras
            // 
            btnAtras.BackgroundImage = (Image)resources.GetObject("btnAtras.BackgroundImage");
            btnAtras.BackgroundImageLayout = ImageLayout.Stretch;
            btnAtras.Location = new Point(55, 401);
            btnAtras.Name = "btnAtras";
            btnAtras.Size = new Size(26, 23);
            btnAtras.TabIndex = 9;
            btnAtras.UseVisualStyleBackColor = true;
            btnAtras.Click += btnAtras_Click;
            // 
            // frmEstudiantes
            // 
            AcceptButton = btnAgregarNuevo;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(471, 480);
            Controls.Add(btnAtras);
            Controls.Add(btnEditarSeleccionado);
            Controls.Add(btnLimpiarLista);
            Controls.Add(lblCantidad);
            Controls.Add(btnEliminar);
            Controls.Add(lstListaNombres);
            Controls.Add(btnAgregarNuevo);
            Controls.Add(txtNuevoElemento);
            Controls.Add(lblNuevoElemento);
            Name = "frmEstudiantes";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Form2";
            Load += frmEstudiantes_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblNuevoElemento;
        private TextBox txtNuevoElemento;
        private Button btnAgregarNuevo;
        private ListBox lstListaNombres;
        private Button btnEliminar;
        private Label lblCantidad;
        private Button btnLimpiarLista;
        private Button btnEditarSeleccionado;
        private Button btnAtras;
    }
}