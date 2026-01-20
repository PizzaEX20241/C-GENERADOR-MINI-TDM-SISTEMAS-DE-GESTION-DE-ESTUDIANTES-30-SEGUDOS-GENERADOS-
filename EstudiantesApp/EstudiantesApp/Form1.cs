using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
namespace EstudiantesApp
{
    public class Form1 : Form
    {
        private MySqlConnection connection;
        private DataGridView dataGridView1;
        private TextBox txtNombre;
        private TextBox txtApellido;
        private TextBox txtMatricula;
        private Button btnAgregar;
        private Button btnActualizar;
        private Button btnEliminar;
        private Button btnLimpiar;
        private Label label1;
        private Label label2;
        private Label label3;
        public Form1()
        {
            InitializeComponent();
            InitializeDatabase();
            LoadEstudiantes();
        }
        private void InitializeComponent()
        {
            // Configuración del formulario
            this.ClientSize = new Size(800, 500);
            this.Text = "Sistema de Gestión de Estudiantes";
            this.BackColor = Color.FromArgb(0, 0, 128); // Azul marino
            this.StartPosition = FormStartPosition.CenterScreen;
            // DataGridView
            this.dataGridView1 = new DataGridView();
            this.dataGridView1.Location = new Point(20, 20);
            this.dataGridView1.Size = new Size(760, 250);
            this.dataGridView1.BackgroundColor = Color.FromArgb(0, 0, 128);
            this.dataGridView1.ForeColor = Color.White;
            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.CellClick += new DataGridViewCellEventHandler(dataGridView1_CellClick);
            // Labels
            this.label1 = new Label();
            this.label1.Text = "Nombre:";
            this.label1.Location = new Point(20, 290);
            this.label1.Size = new Size(100, 20);
            this.label1.ForeColor = Color.White;
            this.label2 = new Label();
            this.label2.Text = "Apellido:";
            this.label2.Location = new Point(20, 320);
            this.label2.Size = new Size(100, 20);
            this.label2.ForeColor = Color.White;
            this.label3 = new Label();
            this.label3.Text = "Matrícula:";
            this.label3.Location = new Point(20, 350);
            this.label3.Size = new Size(100, 20);
            this.label3.ForeColor = Color.White;
            // TextBoxes
            this.txtNombre = new TextBox();
            this.txtNombre.Location = new Point(120, 290);
            this.txtNombre.Size = new Size(200, 20);
            this.txtApellido = new TextBox();
            this.txtApellido.Location = new Point(120, 320);
            this.txtApellido.Size = new Size(200, 20);
            this.txtMatricula = new TextBox();
            this.txtMatricula.Location = new Point(120, 350);
            this.txtMatricula.Size = new Size(200, 20);
            this.txtMatricula.ReadOnly = true;
            // Botones
            this.btnAgregar = new Button();
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.Location = new Point(350, 290);
            this.btnAgregar.Size = new Size(100, 30);
            this.btnAgregar.BackColor = Color.Green;
            this.btnAgregar.ForeColor = Color.White;
            this.btnAgregar.FlatStyle = FlatStyle.Flat;
            this.btnAgregar.FlatAppearance.BorderSize = 0;
            this.btnAgregar.Click += new EventHandler(btnAgregar_Click);
            this.btnActualizar = new Button();
            this.btnActualizar.Text = "Actualizar";
            this.btnActualizar.Location = new Point(350, 330);
            this.btnActualizar.Size = new Size(100, 30);
            this.btnActualizar.BackColor = Color.Orange;
            this.btnActualizar.ForeColor = Color.White;
            this.btnActualizar.FlatStyle = FlatStyle.Flat;
            this.btnActualizar.FlatAppearance.BorderSize = 0;
            this.btnActualizar.Click += new EventHandler(btnActualizar_Click);
            this.btnEliminar = new Button();
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.Location = new Point(470, 290);
            this.btnEliminar.Size = new Size(100, 30);
            this.btnEliminar.BackColor = Color.Red;
            this.btnEliminar.ForeColor = Color.White;
            this.btnEliminar.FlatStyle = FlatStyle.Flat;
            this.btnEliminar.FlatAppearance.BorderSize = 0;
            this.btnEliminar.Click += new EventHandler(btnEliminar_Click);
            this.btnLimpiar = new Button();
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.Location = new Point(470, 330);
            this.btnLimpiar.Size = new Size(100, 30);
            this.btnLimpiar.BackColor = Color.Gray;
            this.btnLimpiar.ForeColor = Color.White;
            this.btnLimpiar.FlatStyle = FlatStyle.Flat;
            this.btnLimpiar.FlatAppearance.BorderSize = 0;
            this.btnLimpiar.Click += new EventHandler(btnLimpiar_Click);
            // Agregar controles al formulario
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.txtApellido);
            this.Controls.Add(this.txtMatricula);
            this.Controls.Add(this.btnAgregar);
            this.Controls.Add(this.btnActualizar);
            this.Controls.Add(this.btnEliminar);
            this.Controls.Add(this.btnLimpiar);
        }
        private void InitializeDatabase()
        {
            try
            {
                // Conectar al servidor MySQL sin especificar base de datos
                string server = "localhost";
                string userId = "root";
                string password = ""; // Cambia si tienes contraseña
                string connectionString = $"Server={server};Uid={userId};Pwd={password};";
                connection = new MySqlConnection(connectionString);
                connection.Open();
                // Verificar si existe la base de datos
                string dbName = "estudiantes_db";
                string checkDbQuery = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{dbName}'";
                using (MySqlCommand cmd = new MySqlCommand(checkDbQuery, connection))
                {
                    object result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        // Crear la base de datos si no existe
                        string createDbQuery = $"CREATE DATABASE {dbName}";
                        using (MySqlCommand createCmd = new MySqlCommand(createDbQuery, connection))
                        {
                            createCmd.ExecuteNonQuery();
                        }
                        // Crear la tabla
                        string useDbQuery = $"USE {dbName}";
                        using (MySqlCommand useCmd = new MySqlCommand(useDbQuery, connection))
                        {
                            useCmd.ExecuteNonQuery();
                        }
                        string createTableQuery = @"
                            CREATE TABLE estudiantes (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                nombre VARCHAR(100) NOT NULL,
                                apellido VARCHAR(100) NOT NULL,
                                matricula VARCHAR(20) NOT NULL UNIQUE
                            )";
                        using (MySqlCommand tableCmd = new MySqlCommand(createTableQuery, connection))
                        {
                            tableCmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Base de datos y tabla creadas exitosamente.");
                    }
                    else
                    {
                        // Usar la base de datos existente
                        string useDbQuery = $"USE {dbName}";
                        using (MySqlCommand useCmd = new MySqlCommand(useDbQuery, connection))
                        {
                            useCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con la base de datos: {ex.Message}");
                Application.Exit();
            }
        }
        private void LoadEstudiantes()
        {
            try
            {
                string query = "SELECT id, nombre, apellido, matricula FROM estudiantes ORDER BY id";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
                // Configurar las columnas
                if (dataGridView1.Columns.Count > 0)
                {
                    dataGridView1.Columns["id"].Visible = false;
                    dataGridView1.Columns["nombre"].HeaderText = "Nombre";
                    dataGridView1.Columns["apellido"].HeaderText = "Apellido";
                    dataGridView1.Columns["matricula"].HeaderText = "Matrícula";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estudiantes: {ex.Message}");
            }
        }
        private string GenerarMatricula()
        {
            // Generar un número aleatorio y concatenarlo con "2026"
            Random rand = new Random();
            int numero = rand.Next(1000, 9999);
            return $"2026{numero}";
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                txtNombre.Text = row.Cells["nombre"].Value.ToString();
                txtApellido.Text = row.Cells["apellido"].Value.ToString();
                txtMatricula.Text = row.Cells["matricula"].Value.ToString();
            }
        }
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("Por favor, complete nombre y apellido.");
                return;
            }
            try
            {
                string matricula = GenerarMatricula();
                string query = "INSERT INTO estudiantes (nombre, apellido, matricula) VALUES (@nombre, @apellido, @matricula)";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text.Trim());
                    cmd.Parameters.AddWithValue("@apellido", txtApellido.Text.Trim());
                    cmd.Parameters.AddWithValue("@matricula", matricula);
                    cmd.ExecuteNonQuery();
                    txtMatricula.Text = matricula;
                    LoadEstudiantes();
                    MessageBox.Show("Estudiante agregado exitosamente.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar estudiante: {ex.Message}");
            }
        }
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMatricula.Text))
            {
                MessageBox.Show("Seleccione un estudiante para actualizar.");
                return;
            }
            try
            {
                string query = "UPDATE estudiantes SET nombre = @nombre, apellido = @apellido WHERE matricula = @matricula";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text.Trim());
                    cmd.Parameters.AddWithValue("@apellido", txtApellido.Text.Trim());
                    cmd.Parameters.AddWithValue("@matricula", txtMatricula.Text.Trim());
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        LoadEstudiantes();
                        MessageBox.Show("Estudiante actualizado exitosamente.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar estudiante: {ex.Message}");
            }
        }
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMatricula.Text))
            {
                MessageBox.Show("Seleccione un estudiante para eliminar.");
                return;
            }
            DialogResult result = MessageBox.Show("¿Está seguro de eliminar este estudiante?",
                "Confirmar eliminación", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM estudiantes WHERE matricula = @matricula";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@matricula", txtMatricula.Text.Trim());
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            LoadEstudiantes();
                            btnLimpiar_Click(sender, e);
                            MessageBox.Show("Estudiante eliminado exitosamente.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar estudiante: {ex.Message}");
                }
            }
        }
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtNombre.Clear();
            txtApellido.Clear();
            txtMatricula.Clear();
            dataGridView1.ClearSelection();
        }
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
