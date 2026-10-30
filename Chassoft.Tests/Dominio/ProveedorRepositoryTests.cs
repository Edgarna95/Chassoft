using Chassoft.Dominio.Personas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chassoft.Infrastructure.Repositories;

namespace Chassoft.Tests.Dominio
{
    internal class ProveedorRepositoryTests
    {

        [Test]
        public void Agregar_Deberia_Guardar_Proveedor()
        {
            // Arrange
            var repo = new ProveedorRepositoryInMemory();

            var proveedor = new Proveedor(
                "Proveedor Test",
                "30-12345678-9"
            );

            // Act
            repo.Agregar(proveedor);

            var resultado = repo.ObtenerPorCuit("30-12345678-9");

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual("Proveedor Test", resultado.Nombre);
            Assert.AreEqual("30-12345678-9", resultado.Cuit);
        }

        [Test]
        public void Agregar_ProveedorConCuitDuplicado_LanzaExcepcion()
        {      
            var repo = new ProveedorRepositoryInMemory();

            var proveedor1 = new Proveedor(
                nombre: "Proveedor Uno",
                cuit: "30-12345678-9"
            );

            var proveedor2 = new Proveedor(
                nombre: "Proveedor Dos",
                cuit: "30-12345678-9" //mismo CUIT
            );

            repo.Agregar(proveedor1);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                repo.Agregar(proveedor2)
            );

            Assert.That(
                ex.Message,
                Is.EqualTo("Ya existe un proveedor registrado con el CUIT ingresado")
            );
        }

    }
}
