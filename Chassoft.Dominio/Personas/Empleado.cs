using System;
using System.Collections.Generic;
using System.Text;

namespace Chassoft.Dominio.Personas
{
    public class Empleado
    {
        public enum CategoriaEmpleado
        {
            Administrativo,
            Profesor,
            Maestranza,
            Externo
        }
        public int Id { get; private set; }
        public string NombreCompleto { get; private set; }
        public string Dni { get; private set; }
        public CategoriaEmpleado Categoria { get; private set; }
        public DateTime FechaIngreso { get; private set; }
        public string? Email { get; private set; }
        public string? Telefono { get; private set; }

        public Empleado(
            string nombreCompleto,
            string dni,
            CategoriaEmpleado categoria,
            DateTime fechaIngreso,
            string? email = null,
            string? telefono = null)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                throw new ArgumentException("El nombre del empleado es obligatorio");

            if (string.IsNullOrWhiteSpace(dni))
                throw new ArgumentException("El DNI es obligatorio");

            NombreCompleto = nombreCompleto;
            Dni = dni;
            Categoria = categoria;
            FechaIngreso = fechaIngreso;
            Email = email;
            Telefono = telefono;
        }
    }

}
