using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Repositorios;
using Chassoft.Dominio.Excepciones;
using System.Collections.Generic;
using System.Linq;
namespace Chassoft.Tests
{
    internal class SocioRepositoryInMemory : ISocioRepository
    {
        private readonly List<Socio> _socios = new();
        private int _siguienteId = 1;
        public Socio Agregar(Socio socio)
        {
            VerificarDni(socio);
            var guardado = new Socio(_siguienteId++, socio.NombreCompleto, socio.Dni, socio.FechaAlta, true, socio.Email);
            _socios.Add(guardado); return guardado;
        }
        public bool Actualizar(Socio socio)
        {
            var indice = _socios.FindIndex(s => s.Id == socio.Id);
            if (indice < 0) return false;
            VerificarDni(socio);
            var anterior = _socios[indice];
            _socios[indice] = new Socio(socio.Id, socio.NombreCompleto, socio.Dni, anterior.FechaAlta, anterior.Activo, socio.Email);
            return true;
        }
        private void VerificarDni(Socio socio)
        {
            if (_socios.Any(s => s.Dni == socio.Dni && s.Id != socio.Id))
                throw new RegistroDuplicadoException("Ya existe un socio registrado con el DNI ingresado");
        }
        public Socio? ObtenerPorDni(string dni) => _socios.FirstOrDefault(s => s.Dni == dni.Trim());
        public Socio? ObtenerPorId(int id) => _socios.FirstOrDefault(s => s.Id == id);
        public IEnumerable<Socio> ObtenerTodos(bool? activo = null) => _socios.Where(s => !activo.HasValue || s.Activo == activo.Value).ToList();
        public bool DarDeBaja(int id) => CambiarEstado(id, false);
        public bool Reactivar(int id) => CambiarEstado(id, true);
        private bool CambiarEstado(int id, bool activo)
        {
            var i = _socios.FindIndex(s => s.Id == id);
            if (i < 0) return false;
            var s = _socios[i]; _socios[i] = new Socio(s.Id, s.NombreCompleto, s.Dni, s.FechaAlta, activo, s.Email);
            return true;
        }
        public IEnumerable<Socio> ObtenerSociosSinPago(int anio, int mes) => _socios.Where(s => s.Activo).ToList();
    }
}
