using System;
using MySql.Data.MySqlClient;

namespace SistemaProductosApp.Helpers
{
    /// <summary>
    /// Clase helper para manejar conexiones MySQL
    /// Generada automáticamente por el Generador Inteligente C#
    /// </summary>
    public static class DatabaseHelper
    {
        private static string connectionString = "server=localhost;database=sistemaproductosapp_db;uid=root;pwd=;port=3306;";
        
        /// <summary>
        /// Obtiene una conexión MySQL configurada
        /// </summary>
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
        
        /// <summary>
        /// Obtiene una conexión MySQL con parámetros personalizados
        /// </summary>
        public static MySqlConnection GetConnection(string server, string database, string username, string password, int port = 3306)
        {
            string customConnectionString = $"server={server};database={database};uid={username};pwd={password};port={port};";
            return new MySqlConnection(customConnectionString);
        }
        
        /// <summary>
        /// Prueba la conexión a la base de datos
        /// </summary>
        public static bool TestConnection(MySqlConnection connection = null)
        {
            bool closeConnection = false;
            
            try
            {
                if (connection == null)
                {
                    connection = GetConnection();
                    closeConnection = true;
                }
                
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                // Puedes registrar el error aquí
                Console.WriteLine($"Error de conexión MySQL: {ex.Message}");
                return false;
            }
            finally
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open && closeConnection)
                {
                    connection.Close();
                }
            }
        }
        
        /// <summary>
        /// Ejecuta una consulta que no retorna resultados (INSERT, UPDATE, DELETE)
        /// </summary>
        public static int ExecuteNonQuery(string query, params MySqlParameter[] parameters)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Ejecuta una consulta y retorna un DataTable
        /// </summary>
        public static System.Data.DataTable ExecuteQuery(string query, params MySqlParameter[] parameters)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            
            return dt;
        }
        
        /// <summary>
        /// Ejecuta una consulta y retorna un solo valor
        /// </summary>
        public static object ExecuteScalar(string query, params MySqlParameter[] parameters)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    
                    return cmd.ExecuteScalar();
                }
            }
        }
        
        /// <summary>
        /// Establece una cadena de conexión personalizada
        /// </summary>
        public static void SetConnectionString(string newConnectionString)
        {
            connectionString = newConnectionString;
        }
        
        /// <summary>
        /// Obtiene la cadena de conexión actual
        /// </summary>
        public static string GetConnectionString()
        {
            return connectionString;
        }
    }
    
    /// <summary>
    /// Clase para manejar transacciones MySQL
    /// </summary>
    public class MySqlTransactionWrapper : IDisposable
    {
        private MySqlConnection connection;
        private MySqlTransaction transaction;
        
        public MySqlTransactionWrapper()
        {
            connection = DatabaseHelper.GetConnection();
            connection.Open();
            transaction = connection.BeginTransaction();
        }
        
        public MySqlCommand CreateCommand(string query)
        {
            MySqlCommand cmd = new MySqlCommand(query, connection, transaction);
            return cmd;
        }
        
        public void Commit()
        {
            transaction?.Commit();
        }
        
        public void Rollback()
        {
            transaction?.Rollback();
        }
        
        public void Dispose()
        {
            transaction?.Dispose();
            connection?.Close();
            connection?.Dispose();
        }
    }
}