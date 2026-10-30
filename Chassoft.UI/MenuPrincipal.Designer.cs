namespace Chassoft.UI
{
    partial class MenuPrincipal
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
            this.bAccesoModuloSocios = new System.Windows.Forms.Button();
            this.bAccesoMenuProveedores = new System.Windows.Forms.Button();
            this.bAccesoMenuOperaciones = new System.Windows.Forms.Button();
            this.bReportes = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // bAccesoModuloSocios
            //
            this.bAccesoModuloSocios.Location = new System.Drawing.Point(12, 12);
            this.bAccesoModuloSocios.Name = "bAccesoModuloSocios";
            this.bAccesoModuloSocios.Size = new System.Drawing.Size(279, 126);
            this.bAccesoModuloSocios.TabIndex = 0;
            this.bAccesoModuloSocios.Text = "Menú de Socios";
            this.bAccesoModuloSocios.UseVisualStyleBackColor = true;
            //
            // bAccesoMenuProveedores
            //
            this.bAccesoMenuProveedores.Location = new System.Drawing.Point(399, 12);
            this.bAccesoMenuProveedores.Name = "bAccesoMenuProveedores";
            this.bAccesoMenuProveedores.Size = new System.Drawing.Size(279, 126);
            this.bAccesoMenuProveedores.TabIndex = 1;
            this.bAccesoMenuProveedores.Text = "Menú de Proveedores";
            this.bAccesoMenuProveedores.UseVisualStyleBackColor = true;
            //
            // bAccesoMenuOperaciones
            //
            this.bAccesoMenuOperaciones.Location = new System.Drawing.Point(12, 163);
            this.bAccesoMenuOperaciones.Name = "bAccesoMenuOperaciones";
            this.bAccesoMenuOperaciones.Size = new System.Drawing.Size(279, 126);
            this.bAccesoMenuOperaciones.TabIndex = 2;
            this.bAccesoMenuOperaciones.Text = "Cobros y Pagos del club";
            this.bAccesoMenuOperaciones.UseVisualStyleBackColor = true;
            //
            // bReportes
            //
            this.bReportes.Location = new System.Drawing.Point(399, 163);
            this.bReportes.Name = "bReportes";
            this.bReportes.Size = new System.Drawing.Size(279, 126);
            this.bReportes.TabIndex = 3;
            this.bReportes.Text = "Reportes";
            this.bReportes.UseVisualStyleBackColor = true;
            //
            // MenuPrincipal
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 327);
            this.Controls.Add(this.bReportes);
            this.Controls.Add(this.bAccesoMenuOperaciones);
            this.Controls.Add(this.bAccesoMenuProveedores);
            this.Controls.Add(this.bAccesoModuloSocios);
            this.Name = "MenuPrincipal";
            this.Text = "MenuPrincipal";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bAccesoModuloSocios;
        private System.Windows.Forms.Button bAccesoMenuProveedores;
        private System.Windows.Forms.Button bAccesoMenuOperaciones;
        private System.Windows.Forms.Button bReportes;
    }
}
