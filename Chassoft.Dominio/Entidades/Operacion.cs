using System;
namespace Chassoft.Dominio.Entidades
{
    public abstract class Operacion
    {
        public int Id { get; private set; }
        public DateTime Fecha { get; private set; }
        public decimal Monto { get; private set; }
        public int IdEmpleado { get; private set; }
        protected Operacion(decimal monto, int idEmpleado) : this(0, DateTime.UtcNow, monto, idEmpleado) { }
        protected Operacion(int id, DateTime fecha, decimal monto, int idEmpleado)
        {
            if (monto <= 0 || decimal.Round(monto, 2) != monto)
                throw new ArgumentException("El monto debe ser positivo y tener hasta dos decimales.");
            if (idEmpleado <= 0) throw new ArgumentException("El empleado es obligatorio.");
            if (id < 0) throw new ArgumentOutOfRangeException(nameof(id));
            Id = id; Fecha = fecha; Monto = monto; IdEmpleado = idEmpleado;
        }
    }
}
