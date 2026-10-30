using System;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Entidades.Enums;
namespace Chassoft.Dominio.Entidades
{
    public class Ingreso : Operacion
    {
        public Socio? Socio { get; private set; }
        public TipoIngreso Tipo { get; private set; }
        public Ingreso(decimal monto, int idEmpleado, TipoIngreso tipo, Socio? socio = null)
            : this(0, DateTime.UtcNow, monto, idEmpleado, tipo, socio, true) { }
        private Ingreso(int id, DateTime fecha, decimal monto, int idEmpleado, TipoIngreso tipo, Socio? socio, bool alta)
            : base(id, fecha, monto, idEmpleado)
        {
            if (!Enum.IsDefined(typeof(TipoIngreso), tipo)) throw new ArgumentException("Tipo de ingreso inválido.");
            if (tipo == TipoIngreso.Cuota && socio == null) throw new ArgumentException("La cuota debe estar asociada a un socio.");
            // Una baja posterior no invalida el cobro histórico.
            if (alta && tipo == TipoIngreso.Cuota && !socio!.Activo) throw new ArgumentException("El socio está inactivo y no puede pagar cuota.");
            Socio = socio; Tipo = tipo;
        }
        public static Ingreso Recuperar(int id, DateTime fecha, decimal monto, int idEmpleado, TipoIngreso tipo, Socio? socio)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            return new Ingreso(id, fecha, monto, idEmpleado, tipo, socio, false);
        }
    }
}
