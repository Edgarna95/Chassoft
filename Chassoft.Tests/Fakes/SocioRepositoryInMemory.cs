using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chassoft.Tests
{
    /// <summary>
    /// Implementacion en memoria de ISocioRepository para tests unitarios.
    /// No toca la base de datos.
    /// </summary>
    internal class SocioRepositoryInMemory : ISocioRepository
    {
        private readonly List<Socio> _socios = new();

        public void Agregar(Socio socio)
        {
            if (_socios.Any(s => s.Dni == socio.Dni))
                throw new ArgumentException("Ya existe un socio registrado con el DNI ingresado");
            _socios.Add(new Socio(_socios.Count + 1, socio.NombreCompleto, socio.Dni, socio.FechaAlta, socio.Activo, socio.Email));
        }

        public Socio? ObtenerPorDni(string dni)
            => _socios.FirstOrDefault(s => s.Dni == dni);

        public IEnumerable<Socio> ObtenerTodos()
            => _socios.AsReadOnly();

        public void DarDeBaja(int id)
        {
            var socio = _socios.FirstOrDefault(s => s.Id == id);
            if (socio is null) throw new ArgumentException($"No existe un socio con Id {id}");
            // Socio is a domain object; mark via reflection or replace — use workaround:
            _socios.Remove(socio);
            _socios.Add(new Socio(socio.Id, socio.NombreCompleto, socio.Dni, socio.FechaAlta, false, socio.Email));
        }

        public Socio? ObtenerPorId(int id)
            => _socios.FirstOrDefault(s => s.Id == id);

        public IEnumerable<Socio> ObtenerSociosSinPago(int anio, int mes)
            => _socios.Where(s => s.Activo).ToList();
    }
}
