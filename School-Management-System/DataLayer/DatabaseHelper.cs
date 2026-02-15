using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using School_Management_System.Common;
using School_Management_System.DataLayer.Logging;

namespace School_Management_System.DataLayer
{
    public sealed class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            Guard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            _connectionString = connectionString;
        }

        public static DatabaseHelper FromConfig()
        {
            var cs = ConfigurationManager.ConnectionStrings[AppConstants.ConnectionStringName];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
            {
                throw new InvalidOperationException("Missing connection string '" + AppConstants.ConnectionStringName + "' in App.config.");
            }

            return new DatabaseHelper(cs.ConnectionString);
        }

        public bool TestConnection(out string errorMessage)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    errorMessage = null;
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                FileLogger.LogError("DatabaseHelper.TestConnection", ex);
                return false;
            }
        }

        public int ExecuteNonQuery(string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("DatabaseHelper.ExecuteNonQuery", ex);
                throw;
            }
        }

        public object ExecuteScalar(string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                {
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("DatabaseHelper.ExecuteScalar", ex);
                throw;
            }
        }

        public long ExecuteInsert(string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return cmd.LastInsertedId;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("DatabaseHelper.ExecuteInsert", ex);
                throw;
            }
        }

        public DataTable ExecuteDataTable(string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                using (var da = new MySqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("DatabaseHelper.ExecuteDataTable", ex);
                throw;
            }
        }

        public DataSet ExecuteDataSet(string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                using (var da = new MySqlDataAdapter(cmd))
                {
                    var ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("DatabaseHelper.ExecuteDataSet", ex);
                throw;
            }
        }

        public void ExecuteInTransaction(Action<MySqlConnection, MySqlTransaction> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        action(conn, tx);
                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        FileLogger.LogError("DatabaseHelper.ExecuteInTransaction", ex);
                        throw;
                    }
                }
            }
        }

        private static MySqlCommand CreateCommand(MySqlConnection connection, string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            var cmd = new MySqlCommand(sql, connection);
            cmd.CommandType = commandType;
            cmd.CommandTimeout = 30;

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (p != null)
                    {
                        cmd.Parameters.Add(p);
                    }
                }
            }

            return cmd;
        }
    }
}
