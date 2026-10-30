using NUnit.Framework;
using System;
using Chassoft.Dominio.Entidades;
using Chassoft.Dominio.Entidades.Enums;
using Chassoft.Dominio.Personas;

namespace Chassoft.Tests.Dominio
{
    public class EgresoTests
    {
        [Test]
        public void CrearEgresoConProveedorValidoNoLanzaExcepcion()
        {
            var proveedor = new Proveedor(
                nombre: "Proveedor Test",
                cuit: "30-12345678-9"
            );
            Assert.DoesNotThrow(() =>
                new Egreso(
                    1000,
                    1,
                    proveedor,
                    TipoEgreso.Proveedor
                )
            );
        }

        [Test]
        public void CrearEgreso_MontoNegativo_LanzaExcepcion()
        {           
            var proveedor = new Proveedor(
                nombre: "Proveedor Test",
                cuit: "30-12345678-9"
            );
          
            Assert.Throws<ArgumentException>(() =>
                new Egreso(
                    -100,
                    1,
                    proveedor,
                    TipoEgreso.Servicios
                )
            );
        }

        [Test]
        public void CrearEgreso_SinProveedor_LanzaExcepcion()
        {
            decimal monto = 1000;
            int idEmpleado = 1;
            Proveedor proveedor = null!;

            Assert.Throws<ArgumentException>(() =>
                new Egreso(
                    monto,
                    idEmpleado,
                    proveedor,
                    TipoEgreso.Servicios
                )
            );
        }
    }
}

