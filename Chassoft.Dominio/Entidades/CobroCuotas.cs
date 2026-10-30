using System;
using System.Collections.Generic;
using System.Linq;
using Chassoft.Dominio.Personas;
namespace Chassoft.Dominio.Entidades
{
    public sealed class PeriodoCuota
    {
        public int Anio { get; }
        public int Mes { get; }
        public PeriodoCuota(int anio, int mes)
        {
            if (anio < 2000 || anio > 9999 || mes < 1 || mes > 12)
                throw new ArgumentException("Período de cuota inválido.");
            Anio = anio; Mes = mes;
        }
    }
    public sealed class CobroCuotas
    {
        public Socio Socio { get; }
        public int IdEmpleado { get; }
        public decimal ImporteMensual { get; }
        public IReadOnlyList<PeriodoCuota> Periodos { get; }
        public decimal Total => ImporteMensual * Periodos.Count;
        public CobroCuotas(Socio socio, int idEmpleado, decimal importeMensual, IEnumerable<PeriodoCuota> periodos)
        {
            if (socio == null || socio.Id <= 0 || !socio.Activo)
                throw new ArgumentException("Se requiere un socio activo y guardado.");
            if (idEmpleado <= 0) throw new ArgumentException("El empleado es obligatorio.");
            if (importeMensual <= 0 || importeMensual > 99999999.99m || decimal.Round(importeMensual,2) != importeMensual)
                throw new ArgumentException("Importe mensual inválido.");
            var lista = periodos?.ToList() ?? throw new ArgumentException("Seleccioná los períodos.");
            if (lista.Count == 0 || lista.Any(p => p == null)) throw new ArgumentException("Seleccioná al menos un período.");
            if (lista.Select(p => new { p.Anio, p.Mes }).Distinct().Count() != lista.Count)
                throw new ArgumentException("No se puede cobrar el mismo mes dos veces.");
            Socio = socio; IdEmpleado = idEmpleado; ImporteMensual = importeMensual;
            Periodos = lista.AsReadOnly();
        }
    }
}
