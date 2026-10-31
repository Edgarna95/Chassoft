using System;
using Chassoft.Dominio.Personas;
using Chassoft.Dominio.Entidades.Enums;
namespace Chassoft.Dominio.Entidades
{
    public class Egreso : Operacion
    {
        public Proveedor Proveedor { get; private set; }
        public TipoEgreso Tipo { get; private set; }
        public Egreso(decimal monto, int idEmpleado, Proveedor proveedor, TipoEgreso tipo)
            : this(0, DateTime.UtcNow, monto, idEmpleado, proveedor, tipo) { }
        private Egreso(int id, DateTime fecha, decimal monto, int idEmpleado, Proveedor proveedor, TipoEgreso tipo)
            : base(id, fecha, monto, idEmpleado)
        {
            if (proveedor == null) throw new ArgumentException("El egreso debe tener un proveedor.");
            if (id == 0 && !proveedor.Activo) throw new ArgumentException("El proveedor está inactivo. Reactivalo antes de registrar un pago.");
            if (!Enum.IsDefined(typeof(TipoEgreso), tipo)) throw new ArgumentException("Tipo de egreso inválido.");
            Proveedor = proveedor; Tipo = tipo;
        }
        public static Egreso Recuperar(int id, DateTime fecha, decimal monto, int idEmpleado, Proveedor proveedor, TipoEgreso tipo)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            return new Egreso(id, fecha, monto, idEmpleado, proveedor, tipo);
        }
    }
}
