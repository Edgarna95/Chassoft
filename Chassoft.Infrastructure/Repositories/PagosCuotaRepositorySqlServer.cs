using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Chassoft.Dominio.Entidades;
using Chassoft.Dominio.Repositories;

namespace Chassoft.Infrastructure.Repositories
{
    public class PagosCuotaRepositorySqlServer : IPagosCuotaRepository
    {
        private readonly string _connectionString;

        public PagosCuotaRepositorySqlServer(string connectionString)
            => _connectionString = connectionString;

        public bool ExistePago(int idSocio, int anio, int mes)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM PagosCuota WHERE IdSocio=@s AND Anio=@a AND Mes=@m", conn);
            cmd.Parameters.Add("@s", SqlDbType.Int).Value      = idSocio;
            cmd.Parameters.Add("@a", SqlDbType.SmallInt).Value = anio;
            cmd.Parameters.Add("@m", SqlDbType.TinyInt).Value  = mes;
            return (int)cmd.ExecuteScalar()! > 0;
        }

        public IEnumerable<PagoCuota> ObtenerPorSocio(int idSocio)
        {
            var lista = new List<PagoCuota>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(
                @"SELECT Id, IdSocio, Anio, Mes, MontoAbonado, FechaPago, IdEmpleado
                  FROM PagosCuota WHERE IdSocio = @s ORDER BY Anio DESC, Mes DESC", conn);
            cmd.Parameters.Add("@s", SqlDbType.Int).Value = idSocio;
            using var r = cmd.ExecuteReader();
            while (r.Read())
                lista.Add(new PagoCuota(r.GetInt32(0), r.GetInt32(1), r.GetInt16(2),
                                        r.GetByte(3), r.GetDecimal(4), r.GetDateTime(5), r.GetInt32(6)));
            return lista;
        }

        public IEnumerable<int> ObtenerIdsSociosSinPago(int anio, int mes)
        {
            var lista = new List<int>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(
                @"SELECT s.Id FROM Socios s
                  WHERE s.Activo = 1
                    AND NOT EXISTS(SELECT 1 FROM PagosCuota p
                                   WHERE p.IdSocio=s.Id AND p.Anio=@a AND p.Mes=@m)", conn);
            cmd.Parameters.Add("@a", SqlDbType.SmallInt).Value = anio;
            cmd.Parameters.Add("@m", SqlDbType.TinyInt).Value  = mes;
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(r.GetInt32(0));
            return lista;
        }
    }
}
