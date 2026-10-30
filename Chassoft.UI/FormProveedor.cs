using System;
using System.Linq;
using System.Net.Mail;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositories;

namespace Chassoft.UI
{
    public partial class FormProveedor : Form
    {
        private readonly IProveedorRepository _repo;
        public FormProveedor(IProveedorRepository repo)
        {
            InitializeComponent();
            _repo = repo;
        }

        private bool ValidarCampos()
        {
            errores.Clear();
            Control primero = null;
            if (string.IsNullOrWhiteSpace(tbNombre.Text))
            {
                errores.SetError(tbNombre, "Ingresá el nombre comercial.");
                primero = tbNombre;
            }
            if (string.IsNullOrWhiteSpace(tbCUIT.Text))
            {
                errores.SetError(tbCUIT, "Ingresá el CUIT.");
                primero = primero ?? tbCUIT;
            }
            var correo = tbCorreo.Text.Trim();
            if (correo.Length > 0)
            {
                try
                {
                    if (new MailAddress(correo).Address != correo) throw new FormatException();
                }
                catch (FormatException)
                {
                    errores.SetError(tbCorreo, "Ingresá un correo como nombre@ejemplo.com.");
                    primero = primero ?? tbCorreo;
                }
            }
            var cbu = tbCBU.Text.Trim();
            if (cbu.Length > 0 && (cbu.Length != 22 || cbu.Any(c => c < '0' || c > '9')))
            {
                errores.SetError(tbCBU, "El CBU debe contener 22 dígitos, sin espacios ni guiones.");
                primero = primero ?? tbCBU;
            }
            if (primero == null) return true;
            primero.Focus();
            return false;
        }

        private void bGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos()) return;
            bGuardar.Enabled = false;
            try
            {
                var proveedor = new Proveedor(tbNombre.Text, tbCUIT.Text, tbRazonSocial.Text,
                    tbTelefono.Text, tbCorreo.Text, tbCondicionIVA.Text, tbCBU.Text);
                _repo.Agregar(proveedor);
                MessageBox.Show(this, "Proveedor guardado correctamente.", "Proveedores",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "Revisá los datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException ex)
            {
                var mensaje = ex.Number == 2601 || ex.Number == 2627
                    ? "Ya existe un registro con esos datos únicos. Revisá el CUIT."
                    : "No se pudo confirmar el guardado. Revisá la conexión y el listado antes de reintentar. Código SQL: " + ex.Number;
                MessageBox.Show(this, mensaje, "No se pudo guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { bGuardar.Enabled = true; }
        }
    }
}
