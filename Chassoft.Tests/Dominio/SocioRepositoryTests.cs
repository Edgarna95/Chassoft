using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using System.Linq;
using Chassoft.Dominio.Personas;
using Chassoft.Infrastructure.Repositories;

namespace Chassoft.Tests
{
    public class SocioRepositoryTests
    {
        [Test]
        public void Agregar_Deberia_Guardar_Socio()
        {
            var repo = new SocioRepositoryInMemory();

            var socio = new Socio(
                "Juan Perez",
                "12345678"
            );
     
            repo.Agregar(socio);

            var resultado = repo.ObtenerPorDni("12345678");
       
            Assert.IsNotNull(resultado);
            Assert.AreEqual("Juan Perez", resultado.NombreCompleto);
            Assert.AreEqual("12345678", resultado.Dni);
            Assert.IsTrue(resultado.Activo);
        }

        [Test]
        public void ObtenerPorDni_SocioInexistente_RetornaNull()
        {

            var repo = new SocioRepositoryInMemory();

            var socio = new Socio(
                "Juan Perez",
                "12345678"
            );

            repo.Agregar(socio);

            var resultado = repo.ObtenerPorDni("99999999");

            Assert.IsNull(resultado);
        }
    }
}