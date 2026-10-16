namespace AplicaciónEscritorioEjercicio1
{
    partial class frmPrincipal
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPrincipal));
            btnAceptar = new Button();
            btnSalir = new Button();
            SuspendLayout();
            // 
            // btnAceptar
            // 
            btnAceptar.BackColor = Color.Navy;
            btnAceptar.Font = new Font("Yu Gothic", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnAceptar.ForeColor = Color.DeepSkyBlue;
            btnAceptar.Location = new Point(228, 332);
            btnAceptar.Name = "btnAceptar";
            btnAceptar.Size = new Size(202, 52);
            btnAceptar.TabIndex = 0;
            btnAceptar.Text = "Lista Estudiantes";
            btnAceptar.UseVisualStyleBackColor = false;
            btnAceptar.Click += btnAceptar_Click_1;
            // 
            // btnSalir
            // 
            btnSalir.BackColor = Color.DarkRed;
            btnSalir.Location = new Point(266, 390);
            btnSalir.Name = "btnSalir";
            btnSalir.Size = new Size(106, 34);
            btnSalir.TabIndex = 1;
            btnSalir.Text = "Salir";
            btnSalir.UseVisualStyleBackColor = false;
            btnSalir.Click += btnSalir_Click;
            // 
            // frmPrincipal
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(675, 581);
            Controls.Add(btnSalir);
            Controls.Add(btnAceptar);
            Name = "frmPrincipal";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Aplicación de Escritorio 1";
            Load += Form1_Load_1;
            ResumeLayout(false);
        }

        #endregion

        private Button btnAceptar;
        private Button btnSalir;
    }
}
