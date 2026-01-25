using System;
using MySql.Data.MySqlClient;

namespace BatteryMonitorHP.Helpers
{
    /// <summary>
    /// Clase helper para manejar conexiones MySQL
    /// Generada automáticamente por el Generador Inteligente C#
    /// </summary>
    public static class DatabaseHelper
    {
        private static string connectionString = "server=localhost;database=batterymonitorhp_db;uid=root;pwd=;port=3306;";
        
        /// <summary>
        /// Obtiene una conexión MySQL configurada
        /// </summary>
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
        
        /// <summary>
        /// Prueba la conexión a la base de datos
        /// </summary>
        public static bool TestConnection()
        {
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Error de conexión MySQL: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Ejecuta una consulta que no retorna resultados
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
}