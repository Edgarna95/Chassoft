using Chassoft.Dominio.Entidades;
using Chassoft.Dominio.Entidades.Enums;
using Chassoft.Dominio.Personas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chassoft.Tests.Dominio
{
    public class IngresoTests
    {
        [Test]
        public void CrearCuota_SinSocio_LanzaExcepcion()
        {
            decimal monto = 1000;
            int idEmpleado = 1;

            Assert.Throws<ArgumentException>(() =>
            {
                var ingreso = new Ingreso(
                    monto,
                    idEmpleado,
                    TipoIngreso.Cuota,
                    null
                );
            });
        }

        [Test]
        public void CrearDonacion_SinSocio_NoLanzaExcepcion()
        {
            Assert.DoesNotThrow(() =>
                new Ingreso(
                    1000,
                    1,
                    TipoIngreso.Donacion,
                    null
                )
            );
        }

        [Test]
        public void CrearOperacion_MontoNegativo_LanzaExcepcion()
        {      
            decimal monto = -100;
            int idEmpleado = 1;

            Assert.Throws<ArgumentException>(() =>
            {
                var ingreso = new Ingreso(
                    monto,
                    idEmpleado,
                    TipoIngreso.Donacion
                );
            });
        }

        [Test]
        public void CrearCuota_SocioInactivo_LanzaExcepcion()
        {
            var socio = new Socio("Carlos López", "27111222");
            socio.DarDeBaja();

            decimal monto = 1500;
            int idEmpleado = 1;

            Assert.Throws<ArgumentException>(() =>
                new Ingreso(
                    monto,
                    idEmpleado,
                    TipoIngreso.Cuota,
                    socio
                )
            );
        }
    }
}
