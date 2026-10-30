using System;

namespace Chassoft.Dominio.Entidades
{
    /// <summary>Registra el pago de una cuota mensual por parte de un socio.</summary>
    public class PagoCuota
    {
        public int Id { get; private set; }
        public int IdSocio { get; private set; }
        public int Anio { get; private set; }
        public int Mes { get; private set; }
        public decimal MontoAbonado { get; private set; }
        public DateTime FechaPago { get; private set; }
        public int IdEmpleado { get; private set; }

        public PagoCuota(int idSocio, int anio, int mes, decimal montoAbonado, int idEmpleado)
        {
            if (idSocio <= 0)  throw new ArgumentException("IdSocio inválido");
            if (anio < 2000 || anio > 9999)   throw new ArgumentException("Año inválido");
            if (mes < 1 || mes > 12) throw new ArgumentException("Mes debe estar entre 1 y 12");
            if (montoAbonado <= 0 || montoAbonado > 99999999.99m || decimal.Round(montoAbonado, 2) != montoAbonado)   throw new ArgumentException("El monto debe ser mayor a cero");
            if (idEmpleado <= 0)     throw new ArgumentException("IdEmpleado inválido");

            IdSocio = idSocio;
            Anio = anio;
            Mes = mes;
            MontoAbonado = montoAbonado;
            FechaPago = DateTime.UtcNow;
            IdEmpleado = idEmpleado;
        }

        /// <summary>Constructor de reconstitución desde DB.</summary>
        public PagoCuota(int id, int idSocio, int anio, int mes,
                         decimal montoAbonado, DateTime fechaPago, int idEmpleado)
            : this(idSocio, anio, mes, montoAbonado, idEmpleado)
        {
            Id = id;
            FechaPago = fechaPago;
        }
    }
}
