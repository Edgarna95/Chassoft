using System;

namespace Chassoft.Dominio.Personas
{
    public class Socio
    {
        public int Id { get; private set; }
        public string NombreCompleto { get; private set; }
        public string Dni { get; private set; }
        public DateTime FechaAlta { get; private set; }
        public bool Activo { get; private set; }
        public string? Email { get; private set; }

        /// <summary>Constructor para crear un nuevo socio.</summary>
        public Socio(string nombreCompleto, string dni, string? email = null)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                throw new ArgumentException("El nombre del socio es obligatorio");
            if (string.IsNullOrWhiteSpace(dni))
                throw new ArgumentException("El DNI del socio es obligatorio");

            if (nombreCompleto.Trim().Length > 200 || dni.Trim().Length > 20 || (email?.Trim().Length ?? 0) > 254)
                throw new ArgumentException("Los datos superan la longitud permitida.");
            NombreCompleto = nombreCompleto.Trim();
            Dni = dni.Trim();
            FechaAlta = DateTime.Today;
            Activo = true;
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        }

        /// <summary>Constructor de reconstitución desde base de datos.</summary>
        public Socio(int id, string nombreCompleto, string dni,
                     DateTime fechaAlta, bool activo, string? email)
            : this(nombreCompleto, dni, email)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El Id debe ser mayor a cero");
            Id = id;
            FechaAlta = fechaAlta;
            Activo = activo;
        }

        public void DarDeBaja()
        {
            if (!Activo)
                throw new InvalidOperationException("El socio ya está dado de baja");
            Activo = false;
        }
    }
}
