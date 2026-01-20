using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SistemaProductosApp.Examples
{
    /// <summary>
    /// Ejemplos de uso de MySQL en Windows Forms
    /// </summary>
    public static class MySqlExamples
    {
        /// <summary>
        /// Ejemplo 1: Cargar datos en un DataGridView
        /// </summary>
        public static void LoadDataIntoGrid(DataGridView dataGrid, string tableName)
        {
            try
            {
                string query = $"SELECT * FROM {tableName}";
                DataTable dt = Helpers.DatabaseHelper.ExecuteQuery(query);
                dataGrid.DataSource = dt;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Ejemplo 2: Insertar registro con parámetros
        /// </summary>
        public static bool InsertUser(string nombre, string email, int edad)
        {
            try
            {
                string query = "INSERT INTO usuarios (nombre, email, edad) VALUES (@nombre, @email, @edad)";
                
                int result = Helpers.DatabaseHelper.ExecuteNonQuery(query,
                    new MySqlParameter("@nombre", nombre),
                    new MySqlParameter("@email", email),
                    new MySqlParameter("@edad", edad)
                );
                
                return result > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al insertar: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Ejemplo 3: Buscar usuario por nombre
        /// </summary>
        public static DataTable SearchUserByName(string searchTerm)
        {
            try
            {
                string query = "SELECT * FROM usuarios WHERE nombre LIKE @search";
                return Helpers.DatabaseHelper.ExecuteQuery(query,
                    new MySqlParameter("@search", $"%{searchTerm}%")
                );
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }
        
        /// <summary>
        /// Ejemplo 4: Transacción con múltiples operaciones
        /// </summary>
        public static bool ProcessOrder(int userId, decimal amount)
        {
            using (var transaction = new Helpers.MySqlTransactionWrapper())
            {
                try
                {
                    // Actualizar saldo del usuario
                    var cmd1 = transaction.CreateCommand(
                        "UPDATE usuarios SET saldo = saldo - @monto WHERE id = @id"
                    );
                    cmd1.Parameters.AddWithValue("@monto", amount);
                    cmd1.Parameters.AddWithValue("@id", userId);
                    cmd1.ExecuteNonQuery();
                    
                    // Registrar la transacción
                    var cmd2 = transaction.CreateCommand(
                        "INSERT INTO transacciones (usuario_id, monto, fecha) VALUES (@uid, @monto, NOW())"
                    );
                    cmd2.Parameters.AddWithValue("@uid", userId);
                    cmd2.Parameters.AddWithValue("@monto", amount);
                    cmd2.ExecuteNonQuery();
                    
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Ejemplo 5: Obtener valor único
        /// </summary>
        public static int GetUserCount()
        {
            try
            {
                object result = Helpers.DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM usuarios");
                return Convert.ToInt32(result);
            }
            catch (Exception)
            {
                return 0;
            }
        }
        
        /// <summary>
        /// Ejemplo 6: Validar credenciales de usuario
        /// </summary>
        public static bool ValidateUserCredentials(string username, string password)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM usuarios WHERE username = @user AND password = @pass";
                
                object result = Helpers.DatabaseHelper.ExecuteScalar(query,
                    new MySqlParameter("@user", username),
                    new MySqlParameter("@pass", password) // ¡En producción usar hash!
                );
                
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Ejemplo 7: Cargar ComboBox desde base de datos
        /// </summary>
        public static void LoadComboBoxFromQuery(ComboBox comboBox, string query, string displayMember, string valueMember)
        {
            try
            {
                DataTable dt = Helpers.DatabaseHelper.ExecuteQuery(query);
                comboBox.DataSource = dt;
                comboBox.DisplayMember = displayMember;
                comboBox.ValueMember = valueMember;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ComboBox: {ex.Message}");
            }
        }
    }
}