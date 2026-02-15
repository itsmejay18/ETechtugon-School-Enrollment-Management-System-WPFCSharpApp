using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
                using (var conn = new SqlConnection(_connectionString))
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

        public int ExecuteNonQuery(string sql, CommandType commandType, IEnumerable<SqlParameter> parameters)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
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

        public object ExecuteScalar(string sql, CommandType commandType, IEnumerable<SqlParameter> parameters)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
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

        public DataTable ExecuteDataTable(string sql, CommandType commandType, IEnumerable<SqlParameter> parameters)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                using (var da = new SqlDataAdapter(cmd))
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

        public DataSet ExecuteDataSet(string sql, CommandType commandType, IEnumerable<SqlParameter> parameters)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                using (var da = new SqlDataAdapter(cmd))
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

        private static SqlCommand CreateCommand(SqlConnection connection, string sql, CommandType commandType, IEnumerable<SqlParameter> parameters)
        {
            var cmd = new SqlCommand(sql, connection);
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

