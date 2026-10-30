using Chassoft.Dominio.Personas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chassoft.Tests.Dominio
{
    public class SocioTests
    {
        [Test]
        public void CrearSocio_Valido_NoLanzaExcepcion()
        {
            Assert.DoesNotThrow(() =>
                new Socio("Carlos López", "27111222")
            );
        }

        [Test]
        public void CrearSocio_SinNombre_LanzaExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Socio("", "27111222")
            );
        }

        [Test]
        public void CrearSocio_SinDni_LanzaExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Socio("Carlos López", "")
            );
        }

        [Test]
        public void Socio_AlCrearse_EstaActivo()
        {
            var socio = new Socio("Carlos López", "27111222");

            Assert.IsTrue(socio.Activo);
        }

        [Test]
        public void DarDeBaja_Socio_QuedaInactivo()
        {
            var socio = new Socio("Carlos López", "27111222");

            socio.DarDeBaja();

            Assert.IsFalse(socio.Activo);
        }
    }
}
