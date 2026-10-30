using Chassoft.Dominio.Entidades;
using Chassoft.Dominio.Entidades.Enums;
using Chassoft.Infrastructure.Repositories;
using System.Linq;

namespace Chassoft.Tests
{
    public class OperacionRepositoryTests
    {
        [Test]
        public void Agregar_VariasOperaciones_Deberia_Devolver_Todas()
        {
            var repo = new OperacionRepositoryInMemory();

            var ingreso1 = new Ingreso(1000m, 1, TipoIngreso.Donacion);
            var ingreso2 = new Ingreso(2000m, 2, TipoIngreso.Donacion);

            repo.Agregar(ingreso1);
            repo.Agregar(ingreso2);

            var operaciones = repo.ObtenerTodas().ToList();

            Assert.AreEqual(2, operaciones.Count);
            Assert.AreEqual(1000m, operaciones[0].Monto);
            Assert.AreEqual(2000m, operaciones[1].Monto);
        }
    }
}
