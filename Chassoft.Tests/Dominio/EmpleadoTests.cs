using Chassoft.Dominio.Personas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chassoft.Dominio.Personas.Empleado;

namespace Chassoft.Tests.Dominio
{
    public class EmpleadoTests
    {
        [Test]
        public void CrearEmpleado_Valido_NoLanzaExcepcion()
        {
            Assert.DoesNotThrow(() =>
                new Empleado(
                    "Juan Pérez",
                    "30123456",
                    CategoriaEmpleado.Administrativo,
                    DateTime.Today
                )
            );
        }

        [Test]
        public void CrearEmpleado_SinNombre_LanzaExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Empleado(
                    "",
                    "30123456",
                    CategoriaEmpleado.Administrativo,
                    DateTime.Today
                )
            );
        }

        [Test]
        public void CrearEmpleado_SinDni_LanzaExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Empleado(
                    "Juan Pérez",
                    "",
                    CategoriaEmpleado.Administrativo,
                    DateTime.Today
                )
            );
        }

        [Test]
        public void CrearEmpleado_CategoriaValida_NoLanzaExcepcion()
        {
            Assert.DoesNotThrow(() =>
                new Empleado(
                    "Ana Gómez",
                    "28999888",
                    CategoriaEmpleado.Profesor,
                    DateTime.Today
                )
            );
        }
    }
}
