using Chassoft.Dominio.Entidades;
using Chassoft.Dominio.Entidades.Enums;
using Chassoft.Dominio.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Chassoft.Dominio.Personas;

namespace Chassoft.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio SQL Server para operaciones economicas.
    /// Las operaciones se persisten en tablas separadas (Ingresos / Egresos)
    /// con una fila coordinadora en Operaciones para datos comunes.
    /// </summary>
    public class OperacionRepositorySqlServer : IOperacionRepository
    {
        private readonly string _connectionString;

        public OperacionRepositorySqlServer(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Persiste una operacion. Detecta si es Ingreso o Egreso
        /// e inserta en la tabla correspondiente ademas de la tabla Operaciones.
        /// </summary>
        public void Agregar(Operacion operacion)
        {
            if (operacion is Ingreso ingreso && ingreso.Tipo == TipoIngreso.Cuota)
                throw new ArgumentException("Para cuotas debe indicarse el detalle de períodos mediante CobrarCuotas.");
            Guardar(operacion, null);
        }

        public int CobrarCuotas(CobroCuotas cobro)
        {
            if (cobro == null) throw new ArgumentNullException(nameof(cobro));
            var ingreso = new Ingreso(cobro.Total, cobro.IdEmpleado, TipoIngreso.Cuota, cobro.Socio);
            return Guardar(ingreso, cobro);
        }

        private int Guardar(Operacion operacion, CobroCuotas? cobro)
        {
            if (operacion == null) throw new ArgumentNullException(nameof(operacion));
            if (operacion.Id != 0) throw new ArgumentException("La operación ya fue registrada.");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using var empleado = new SqlCommand("SELECT COUNT(1) FROM Empleados WITH (UPDLOCK,HOLDLOCK) WHERE Id=@id AND Activo=1", connection, transaction);
                        empleado.Parameters.Add("@id", SqlDbType.Int).Value = operacion.IdEmpleado;
                        if ((int)empleado.ExecuteScalar() != 1) throw new ArgumentException("El empleado no existe o está inactivo.");
                        if (cobro != null)
                        {
                            using var socio = new SqlCommand("SELECT COUNT(1) FROM Socios WITH (UPDLOCK,HOLDLOCK) WHERE Id=@id AND Activo=1", connection, transaction);
                            socio.Parameters.Add("@id", SqlDbType.Int).Value = cobro.Socio.Id;
                            if ((int)socio.ExecuteScalar() != 1) throw new ArgumentException("El socio no existe o está inactivo.");
                        }
                        // 1. Insertar en tabla base y recuperar el Id generado
                        var cmdBase = new SqlCommand(
                            @"INSERT INTO Operaciones (Fecha, Monto, IdEmpleado, Tipo)
                              OUTPUT INSERTED.Id
                              VALUES (@fecha, @monto, @idEmpleado, @tipoOperacion)",
                            connection, transaction);

                        cmdBase.Parameters.Add("@fecha", SqlDbType.DateTime2).Value = operacion.Fecha;
                        var monto = cmdBase.Parameters.Add("@monto", SqlDbType.Decimal);
                        monto.Precision = 18; monto.Scale = 2; monto.Value = operacion.Monto;
                        cmdBase.Parameters.Add("@tipoOperacion", SqlDbType.NVarChar, 10).Value = operacion is Ingreso ? "Ingreso" : "Egreso";
                        cmdBase.Parameters.Add("@idEmpleado", SqlDbType.Int).Value = operacion.IdEmpleado;

                        var idOperacion = (int)cmdBase.ExecuteScalar();

                        // 2. Insertar en tabla especifica segun el tipo concreto
                        switch (operacion)
                        {
                            case Ingreso ingreso:
                                InsertarIngreso(connection, transaction, idOperacion, ingreso);
                                break;
                            case Egreso egreso:
                                InsertarEgreso(connection, transaction, idOperacion, egreso);
                                break;
                            default:
                                throw new InvalidOperationException(
                                    $"Tipo de operacion no soportado: {operacion.GetType().Name}");
                        }

                        if (cobro != null)
                        {
                            foreach (var periodo in cobro.Periodos)
                            {
                                using var pago = new SqlCommand(
                                    @"INSERT INTO PagosCuota (IdSocio, Anio, Mes, MontoAbonado, FechaPago, IdEmpleado, IdOperacion)
                                      VALUES (@socio, @anio, @mes, @monto, @fecha, @empleado, @operacion)", connection, transaction);
                                pago.Parameters.Add("@socio", SqlDbType.Int).Value = cobro.Socio.Id;
                                pago.Parameters.Add("@anio", SqlDbType.SmallInt).Value = periodo.Anio;
                                pago.Parameters.Add("@mes", SqlDbType.TinyInt).Value = periodo.Mes;
                                var importe = pago.Parameters.Add("@monto", SqlDbType.Decimal);
                                importe.Precision = 10; importe.Scale = 2; importe.Value = cobro.ImporteMensual;
                                pago.Parameters.Add("@fecha", SqlDbType.DateTime2).Value = operacion.Fecha;
                                pago.Parameters.Add("@empleado", SqlDbType.Int).Value = cobro.IdEmpleado;
                                pago.Parameters.Add("@operacion", SqlDbType.Int).Value = idOperacion;
                                pago.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                        return idOperacion;
                    }
                    catch (SqlException ex) when (cobro != null && (ex.Number == 2601 || ex.Number == 2627))
                    {
                        transaction.Rollback();
                        throw new CuotasYaPagadasException("Uno de los meses ya está pagado. No se registró ningún cobro.", ex);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Retorna todas las operaciones (ingresos y egresos) ordenadas por fecha descendente.
        /// Para detalles especificos de tipo usar los metodos ObtenerIngresos / ObtenerEgresos.
        /// </summary>
        public IEnumerable<Operacion> ObtenerTodas()
        {
            var lista = new List<Operacion>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                lista.AddRange(ObtenerIngresosInterno(connection));
                lista.AddRange(ObtenerEgresosInterno(connection));
            }

            return lista.OrderByDescending(o => o.Fecha).ThenByDescending(o => o.Id).ToList();
        }

        // ── Metodos privados ──────────────────────────────────────────────────

        private static void InsertarIngreso(SqlConnection conn, SqlTransaction tx,
            int idOperacion, Ingreso ingreso)
        {
            var cmd = new SqlCommand(
                @"INSERT INTO Ingresos (IdOperacion, Tipo, IdSocio)
                  VALUES (@idOperacion, @tipo, @idSocio)",
                conn, tx);

            cmd.Parameters.Add("@idOperacion", SqlDbType.Int).Value = idOperacion;
            cmd.Parameters.Add("@tipo", SqlDbType.Int).Value = (int)ingreso.Tipo;
            cmd.Parameters.Add("@idSocio", SqlDbType.Int).Value =
                (ingreso.Socio != null && ingreso.Socio.Id > 0)
                    ? (object)ingreso.Socio.Id
                    : DBNull.Value;

            cmd.ExecuteNonQuery();
        }

        private static void InsertarEgreso(SqlConnection conn, SqlTransaction tx,
            int idOperacion, Egreso egreso)
        {
            var cmd = new SqlCommand(
                @"INSERT INTO Egresos (IdOperacion, Tipo, IdProveedor)
                  VALUES (@idOperacion, @tipo, @idProveedor)",
                conn, tx);

            cmd.Parameters.Add("@idOperacion", SqlDbType.Int).Value = idOperacion;
            cmd.Parameters.Add("@tipo", SqlDbType.Int).Value = (int)egreso.Tipo;
            cmd.Parameters.Add("@idProveedor", SqlDbType.Int).Value = egreso.Proveedor.Id;

            cmd.ExecuteNonQuery();
        }

        private static IEnumerable<Ingreso> ObtenerIngresosInterno(SqlConnection connection)
        {
            var lista = new List<Ingreso>();
            using var cmd = new SqlCommand(
                @"SELECT o.Id, o.Fecha, o.Monto, o.IdEmpleado, i.Tipo,
                         s.Id, s.NombreCompleto, s.Dni, s.FechaAlta, s.Activo, s.Email
                  FROM Operaciones o
                  INNER JOIN Ingresos i ON i.IdOperacion = o.Id
                  LEFT JOIN Socios s ON s.Id = i.IdSocio", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(LeerIngreso(reader));
            return lista;
        }

        internal static Ingreso LeerIngreso(IDataRecord r)
        {
            Socio? socio = r.IsDBNull(5) ? null : new Socio(r.GetInt32(5), r.GetString(6), r.GetString(7),
                r.GetDateTime(8), r.GetBoolean(9), r.IsDBNull(10) ? null : r.GetString(10));
            return Ingreso.Recuperar(r.GetInt32(0), r.GetDateTime(1), r.GetDecimal(2), r.GetInt32(3),
                (TipoIngreso)r.GetInt32(4), socio);
        }

        private static IEnumerable<Egreso> ObtenerEgresosInterno(SqlConnection connection)
        {
            var lista = new List<Egreso>();
            using var cmd = new SqlCommand(
                @"SELECT o.Id, o.Fecha, o.Monto, o.IdEmpleado, e.Tipo,
                         p.Id, p.Nombre, p.Cuit, p.RazonSocial, p.TelefonoDeContacto,
                         p.CorreoDeContacto, p.CondicionIVA, p.CbuAsociado
                  FROM Operaciones o
                  INNER JOIN Egresos e ON e.IdOperacion = o.Id
                  INNER JOIN Proveedores p ON p.Id = e.IdProveedor", connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(LeerEgreso(reader));
            return lista;
        }

        internal static Egreso LeerEgreso(IDataRecord r)
        {
            string? Opcional(int i) => r.IsDBNull(i) ? null : r.GetString(i);
            var proveedor = new Proveedor(r.GetInt32(5), r.GetString(6), r.GetString(7),
                Opcional(8), Opcional(9), Opcional(10), Opcional(11), Opcional(12));
            return Egreso.Recuperar(r.GetInt32(0), r.GetDateTime(1), r.GetDecimal(2), r.GetInt32(3),
                proveedor, (TipoEgreso)r.GetInt32(4));
        }
    }
}
