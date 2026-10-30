using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Chassoft.Dominio.Repositories;
namespace Chassoft.Infrastructure.Repositories
{
    public class EmpleadoRepositorySqlServer : IEmpleadoRepository
    {
        private readonly string _cs;
        public EmpleadoRepositorySqlServer(string cs) => _cs = cs;
        public IEnumerable<EmpleadoDisponible> ObtenerActivos()
        {
            var lista = new List<EmpleadoDisponible>();
            using var conn = new SqlConnection(_cs); conn.Open();
            using var cmd = new SqlCommand("SELECT Id, NombreCompleto FROM Empleados WHERE Activo=1 ORDER BY NombreCompleto", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(new EmpleadoDisponible(r.GetInt32(0), r.GetString(1)));
            return lista;
        }
    }
}
