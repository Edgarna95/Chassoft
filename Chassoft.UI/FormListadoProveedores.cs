using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chassoft.Dominio.Repositories;
using Microsoft.Data.SqlClient;


namespace Chassoft.UI
{
    public partial class FormListadoProveedores : Form
    {
        private readonly IProveedorRepository _repo;

        public FormListadoProveedores(IProveedorRepository repo)
        {
            InitializeComponent();
            _repo = repo;

        }



        private void FormListadoProveedores_Load(object sender, EventArgs e)
        {
            IntentarCargarProveedores();
        }

        private void IntentarCargarProveedores()
        {
            try { CargarProveedores(); }
            catch (SqlException ex)
            {
                MessageBox.Show(this, "No se pudo actualizar el listado. Revisá la conexión. Código SQL: " + ex.Number,
                    "Proveedores", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CargarProveedores()
        {

            dgvProveedores.DataSource = _repo.ObtenerTodos()
                                             .Select(p => new
                                             {
                                                 p.Nombre,
                                                 p.Cuit,
                                                 p.RazonSocial, p.TelefonoDeContacto, p.CorreoDeContacto, p.CondicionIVA, p.CbuAsociado
                                             })
                                             .ToList();
            dgvProveedores.Columns["Nombre"].HeaderText = "Nombre comercial";
            dgvProveedores.Columns["RazonSocial"].HeaderText = "Razón social";
            dgvProveedores.Columns["TelefonoDeContacto"].HeaderText = "Teléfono";
            dgvProveedores.Columns["CorreoDeContacto"].HeaderText = "Correo";
            dgvProveedores.Columns["CondicionIVA"].HeaderText = "Condición IVA";
            dgvProveedores.Columns["CbuAsociado"].HeaderText = "CBU";
        }

        private void dgvProveedores_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            using (var form = new FormProveedor(_repo))
            {
                form.ShowDialog();
            }

            IntentarCargarProveedores();
        }


    }
}
