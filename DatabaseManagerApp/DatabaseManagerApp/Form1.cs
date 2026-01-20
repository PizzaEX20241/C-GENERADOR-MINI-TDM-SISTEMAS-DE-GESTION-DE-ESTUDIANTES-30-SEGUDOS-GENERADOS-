using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
namespace DatabaseManagerApp
{
    // Clase personalizada para botones redondeados
    public class RoundedButton : Button
    {
        private int _borderRadius = 10;
        private Color _buttonColor = Color.FromArgb(52, 152, 219);
        public int BorderRadius
        {
            get { return _borderRadius; }
            set { _borderRadius = value; Invalidate(); }
        }
        public Color ButtonColor
        {
            get { return _buttonColor; }
            set { _buttonColor = value; Invalidate(); }
        }
        public RoundedButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = _buttonColor;
            this.ForeColor = Color.White;
            this.Cursor = Cursors.Hand;
            this.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.Size = new Size(120, 40);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Crear path redondeado
            GraphicsPath path = new GraphicsPath();
            int radius = _borderRadius;
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            this.Region = new Region(path);
            // Rellenar el botón
            using (SolidBrush brush = new SolidBrush(_buttonColor))
            {
                e.Graphics.FillPath(brush, path);
            }
            // Dibujar texto
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                e.Graphics.DrawString(this.Text, this.Font, textBrush,
                    new RectangleF(0, 0, this.Width, this.Height), sf);
            }
        }
    }
    public partial class Form1 : Form
    {
        // Configuración de colores
        private Color mainFormColor = Color.FromArgb(86, 164, 181); // Verde azulado
        private Color dataGridViewBgColor = Color.FromArgb(0, 51, 102); // Azul marino
        private Color columnColor = Color.FromArgb(173, 216, 230); // Azul claro
        // Variables para base de datos
        private MySqlConnection connection;
        private string connectionString = "server=localhost;database=databasemanager;uid=root;pwd=;";
        // Controles
        private Panel loginPanel;
        private Panel mainPanel;
        private Panel configPanel;
        private DataGridView dataGridView;
        private List<TextBox> textBoxes = new List<TextBox>();
        private List<Label> labels = new List<Label>();
        private ComboBox estadoComboBox;
        // Configuración
        private string adminUser = "admin";
        private string adminPass = "admin";
        private string configFile = "config.dat";
        // Colores para botones
        private Color[] buttonColors = new Color[]
        {
            Color.FromArgb(231, 76, 60),    // Rojo
            Color.FromArgb(46, 204, 113),   // Verde
            Color.FromArgb(52, 152, 219),   // Azul
            Color.FromArgb(155, 89, 182),   // Morado
            Color.FromArgb(241, 196, 15),   // Amarillo
            Color.FromArgb(230, 126, 34),   // Naranja
            Color.FromArgb(149, 165, 166),  // Gris
            Color.FromArgb(22, 160, 133)    // Turquesa
        };
        public Form1()
        {
            InitializeComponent();
            InitializeDatabase();
            LoadConfiguration();
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // Configuración del formulario principal
            this.Text = "Database";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = mainFormColor;
            this.FormBorderStyle = FormBorderStyle.None;
            // Crear paneles
            CreateLoginPanel();
            CreateMainPanel();
            CreateConfigPanel();
            // Mostrar login inicialmente
            ShowLoginPanel();
            this.ResumeLayout(false);
        }
        private void CreateLoginPanel()
        {
            loginPanel = new Panel();
            loginPanel.Size = new Size(400, 450);
            loginPanel.BackColor = Color.FromArgb(44, 62, 80);
            loginPanel.Location = new Point(
                (this.ClientSize.Width - 400) / 2,
                (this.ClientSize.Height - 450) / 2
            );
            // Título
            Label titleLabel = new Label();
            titleLabel.Text = "DATABASE MANAGER";
            titleLabel.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Size = new Size(380, 50);
            titleLabel.Location = new Point(10, 30);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // Usuario
            Label userLabel = new Label();
            userLabel.Text = "Usuario:";
            userLabel.Font = new Font("Segoe UI", 12);
            userLabel.ForeColor = Color.White;
            userLabel.Size = new Size(100, 30);
            userLabel.Location = new Point(50, 120);
            TextBox userText = new TextBox();
            userText.Name = "txtUser";
            userText.Font = new Font("Segoe UI", 12);
            userText.Size = new Size(300, 35);
            userText.Location = new Point(50, 150);
            userText.BackColor = Color.FromArgb(52, 73, 94);
            userText.ForeColor = Color.White;
            userText.BorderStyle = BorderStyle.FixedSingle;
            // Contraseña
            Label passLabel = new Label();
            passLabel.Text = "Contraseña:";
            passLabel.Font = new Font("Segoe UI", 12);
            passLabel.ForeColor = Color.White;
            passLabel.Size = new Size(100, 30);
            passLabel.Location = new Point(50, 200);
            TextBox passText = new TextBox();
            passText.Name = "txtPass";
            passText.Font = new Font("Segoe UI", 12);
            passText.Size = new Size(300, 35);
            passText.Location = new Point(50, 230);
            passText.BackColor = Color.FromArgb(52, 73, 94);
            passText.ForeColor = Color.White;
            passText.BorderStyle = BorderStyle.FixedSingle;
            passText.UseSystemPasswordChar = true;
            // Label de información
            Label infoLabel = new Label();
            infoLabel.Text = "Usuario: admin | Contraseña: admin";
            infoLabel.Font = new Font("Segoe UI", 9);
            infoLabel.ForeColor = Color.FromArgb(236, 240, 241);
            infoLabel.Size = new Size(300, 30);
            infoLabel.Location = new Point(50, 280);
            infoLabel.TextAlign = ContentAlignment.MiddleCenter;
            // Botón Login
            RoundedButton loginBtn = new RoundedButton();
            loginBtn.Text = "INICIAR SESIÓN";
            loginBtn.ButtonColor = Color.FromArgb(46, 204, 113);
            loginBtn.Size = new Size(200, 45);
            loginBtn.Location = new Point(100, 320);
            loginBtn.Click += (s, e) => Login(userText.Text, passText.Text);
            // Botón Salir
            RoundedButton exitBtn = new RoundedButton();
            exitBtn.Text = "SALIR";
            exitBtn.ButtonColor = Color.FromArgb(231, 76, 60);
            exitBtn.Size = new Size(200, 45);
            exitBtn.Location = new Point(100, 380);
            exitBtn.Click += (s, e) => Application.Exit();
            // Agregar controles al panel
            loginPanel.Controls.Add(titleLabel);
            loginPanel.Controls.Add(userLabel);
            loginPanel.Controls.Add(userText);
            loginPanel.Controls.Add(passLabel);
            loginPanel.Controls.Add(passText);
            loginPanel.Controls.Add(infoLabel);
            loginPanel.Controls.Add(loginBtn);
            loginPanel.Controls.Add(exitBtn);
            this.Controls.Add(loginPanel);
        }
        private void CreateMainPanel()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = mainFormColor;
            mainPanel.Visible = false;
            // Barra superior
            Panel topBar = new Panel();
            topBar.Dock = DockStyle.Top;
            topBar.Height = 50;
            topBar.BackColor = Color.FromArgb(44, 62, 80);
            // Título en barra superior
            Label titleLabel = new Label();
            titleLabel.Text = "DATABASE MANAGER - PANEL PRINCIPAL";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Dock = DockStyle.Left;
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            titleLabel.Padding = new Padding(20, 0, 0, 0);
            // Botón minimizar
            RoundedButton minBtn = new RoundedButton();
            minBtn.Text = "_";
            minBtn.ButtonColor = Color.FromArgb(52, 152, 219);
            minBtn.Size = new Size(40, 30);
            minBtn.Location = new Point(this.ClientSize.Width - 100, 10);
            minBtn.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            // Botón cerrar
            RoundedButton closeBtn = new RoundedButton();
            closeBtn.Text = "X";
            closeBtn.ButtonColor = Color.FromArgb(231, 76, 60);
            closeBtn.Size = new Size(40, 30);
            closeBtn.Location = new Point(this.ClientSize.Width - 50, 10);
            closeBtn.Click += (s, e) => Application.Exit();
            topBar.Controls.Add(titleLabel);
            topBar.Controls.Add(minBtn);
            topBar.Controls.Add(closeBtn);
            // Panel de botones superiores
            Panel buttonPanel = new Panel();
            buttonPanel.Size = new Size(this.ClientSize.Width, 60);
            buttonPanel.Location = new Point(0, 50);
            buttonPanel.BackColor = Color.Transparent;
            // Botón Configurar
            RoundedButton configBtn = new RoundedButton();
            configBtn.Text = "CONFIGURAR";
            configBtn.ButtonColor = Color.FromArgb(155, 89, 182);
            configBtn.Size = new Size(120, 35);
            configBtn.Location = new Point(20, 10);
            configBtn.Click += (s, e) => ShowConfigPanel();
            // Botón Cerrar Sesión
            RoundedButton logoutBtn = new RoundedButton();
            logoutBtn.Text = "CERRAR SESIÓN";
            logoutBtn.ButtonColor = Color.FromArgb(230, 126, 34);
            logoutBtn.Size = new Size(120, 35);
            logoutBtn.Location = new Point(150, 10);
            logoutBtn.Click += (s, e) => Logout();
            buttonPanel.Controls.Add(configBtn);
            buttonPanel.Controls.Add(logoutBtn);
            // Panel izquierdo (formulario)
            Panel leftPanel = new Panel();
            leftPanel.Size = new Size(350, this.ClientSize.Height - 70);
            leftPanel.Location = new Point(20, 100);
            leftPanel.BackColor = Color.FromArgb(44, 62, 80);
            // Crear campos del formulario
            CreateFormFields(leftPanel);
            // Crear botones CRUD
            CreateCRUDButtons(leftPanel);
            // DataGridView
            dataGridView = new DataGridView();
            dataGridView.Name = "dgvData";
            dataGridView.Size = new Size(this.ClientSize.Width - 400, this.ClientSize.Height - 70);
            dataGridView.Location = new Point(380, 100);
            dataGridView.BackgroundColor = dataGridViewBgColor;
            dataGridView.DefaultCellStyle.BackColor = columnColor;
            dataGridView.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 60, 114);
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dataGridView.RowHeadersVisible = false;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.CellClick += DataGridView_CellClick;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.ReadOnly = true;
            // Agregar controles al panel principal
            mainPanel.Controls.Add(topBar);
            mainPanel.Controls.Add(buttonPanel);
            mainPanel.Controls.Add(leftPanel);
            mainPanel.Controls.Add(dataGridView);
            this.Controls.Add(mainPanel);
        }
        private void CreateFormFields(Panel panel)
        {
            int yPos = 20;
            string[] fieldNames = { "ID", "Correo", "Contraseña", "URL", "Empresa", "Activo" };
            for (int i = 0; i < fieldNames.Length; i++)
            {
                // Label
                Label label = new Label();
                label.Text = fieldNames[i] + ":";
                label.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                label.ForeColor = Color.White;
                label.Size = new Size(100, 30);
                label.Location = new Point(20, yPos);
                panel.Controls.Add(label);
                labels.Add(label);
                if (fieldNames[i] == "Activo")
                {
                    estadoComboBox = new ComboBox();
                    estadoComboBox.Name = "cbEstado";
                    estadoComboBox.Font = new Font("Segoe UI", 10);
                    estadoComboBox.Size = new Size(250, 30);
                    estadoComboBox.Location = new Point(20, yPos + 30);
                    estadoComboBox.BackColor = Color.FromArgb(52, 73, 94);
                    estadoComboBox.ForeColor = Color.White;
                    estadoComboBox.FlatStyle = FlatStyle.Flat;
                    string[] estados = { "Activo", "Descartado", "Eliminado", "Sin Actualizar",
                                       "Hackeado", "Perdido", "Cuenta Nueva", "Cuenta Extra" };
                    estadoComboBox.Items.AddRange(estados);
                    estadoComboBox.SelectedIndex = 0;
                    panel.Controls.Add(estadoComboBox);
                    textBoxes.Add(null); // Placeholder
                }
                else if (fieldNames[i] == "Correo")
                {
                    Panel textBoxPanel = new Panel();
                    textBoxPanel.Size = new Size(250, 35);
                    textBoxPanel.Location = new Point(20, yPos + 30);
                    textBoxPanel.BackColor = Color.FromArgb(52, 73, 94);
                    textBoxPanel.BorderStyle = BorderStyle.FixedSingle;
                    TextBox textBox = new TextBox();
                    textBox.Name = "txt" + fieldNames[i];
                    textBox.Font = new Font("Segoe UI", 10);
                    textBox.Size = new Size(230, 35);
                    textBox.Location = new Point(10, 0);
                    textBox.BackColor = Color.FromArgb(52, 73, 94);
                    textBox.ForeColor = Color.White;
                    textBox.BorderStyle = BorderStyle.None;
                    // Menú contextual para copiar
                    ContextMenuStrip contextMenu = new ContextMenuStrip();
                    ToolStripMenuItem copyItem = new ToolStripMenuItem("Copiar");
                    copyItem.Click += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(textBox.Text))
                            Clipboard.SetText(textBox.Text);
                    };
                    contextMenu.Items.Add(copyItem);
                    textBox.ContextMenuStrip = contextMenu;
                    textBoxPanel.Controls.Add(textBox);
                    panel.Controls.Add(textBoxPanel);
                    textBoxes.Add(textBox);
                }
                else if (fieldNames[i] == "Contraseña")
                {
                    Panel passwordPanel = new Panel();
                    passwordPanel.Size = new Size(250, 35);
                    passwordPanel.Location = new Point(20, yPos + 30);
                    passwordPanel.BackColor = Color.FromArgb(52, 73, 94);
                    passwordPanel.BorderStyle = BorderStyle.FixedSingle;
                    TextBox textBox = new TextBox();
                    textBox.Name = "txt" + fieldNames[i];
                    textBox.Font = new Font("Segoe UI", 10);
                    textBox.Size = new Size(200, 35);
                    textBox.Location = new Point(10, 0);
                    textBox.BackColor = Color.FromArgb(52, 73, 94);
                    textBox.ForeColor = Color.White;
                    textBox.BorderStyle = BorderStyle.None;
                    textBox.UseSystemPasswordChar = true;
                    // Menú contextual para copiar
                    ContextMenuStrip contextMenu = new ContextMenuStrip();
                    ToolStripMenuItem copyItem = new ToolStripMenuItem("Copiar");
                    copyItem.Click += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(textBox.Text))
                            Clipboard.SetText(textBox.Text);
                    };
                    contextMenu.Items.Add(copyItem);
                    textBox.ContextMenuStrip = contextMenu;
                    // Botón ver/ocultar
                    Button toggleBtn = new Button();
                    toggleBtn.Text = "👁";
                    toggleBtn.BackColor = Color.FromArgb(149, 165, 166);
                    toggleBtn.ForeColor = Color.White;
                    toggleBtn.FlatStyle = FlatStyle.Flat;
                    toggleBtn.FlatAppearance.BorderSize = 0;
                    toggleBtn.Size = new Size(30, 25);
                    toggleBtn.Location = new Point(210, 5);
                    toggleBtn.Font = new Font("Segoe UI", 8);
                    toggleBtn.Click += (s, e) =>
                    {
                        textBox.UseSystemPasswordChar = !textBox.UseSystemPasswordChar;
                        toggleBtn.Text = textBox.UseSystemPasswordChar ? "👁" : "👁‍🗨";
                    };
                    passwordPanel.Controls.Add(textBox);
                    passwordPanel.Controls.Add(toggleBtn);
                    panel.Controls.Add(passwordPanel);
                    textBoxes.Add(textBox);
                }
                else if (fieldNames[i] == "URL")
                {
                    Panel urlPanel = new Panel();
                    urlPanel.Size = new Size(250, 35);
                    urlPanel.Location = new Point(20, yPos + 30);
                    urlPanel.BackColor = Color.FromArgb(52, 73, 94);
                    urlPanel.BorderStyle = BorderStyle.FixedSingle;
                    TextBox textBox = new TextBox();
                    textBox.Name = "txt" + fieldNames[i];
                    textBox.Font = new Font("Segoe UI", 10);
                    textBox.Size = new Size(200, 35);
                    textBox.Location = new Point(10, 0);
                    textBox.BackColor = Color.FromArgb(52, 73, 94);
                    textBox.ForeColor = Color.White;
                    textBox.BorderStyle = BorderStyle.None;
                    // Botón abrir URL
                    Button urlBtn = new Button();
                    urlBtn.Text = "🌐";
                    urlBtn.BackColor = Color.FromArgb(52, 152, 219);
                    urlBtn.ForeColor = Color.White;
                    urlBtn.FlatStyle = FlatStyle.Flat;
                    urlBtn.FlatAppearance.BorderSize = 0;
                    urlBtn.Size = new Size(30, 25);
                    urlBtn.Location = new Point(210, 5);
                    urlBtn.Font = new Font("Segoe UI", 8);
                    urlBtn.Click += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(textBox.Text))
                        {
                            string url = textBox.Text;
                            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                                url = "http://" + url;
                            try
                            {
                                System.Diagnostics.Process.Start(url);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error al abrir URL: " + ex.Message,
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    };
                    urlPanel.Controls.Add(textBox);
                    urlPanel.Controls.Add(urlBtn);
                    panel.Controls.Add(urlPanel);
                    textBoxes.Add(textBox);
                }
                else
                {
                    Panel textBoxPanel = new Panel();
                    textBoxPanel.Size = new Size(250, 35);
                    textBoxPanel.Location = new Point(20, yPos + 30);
                    textBoxPanel.BackColor = Color.FromArgb(52, 73, 94);
                    textBoxPanel.BorderStyle = BorderStyle.FixedSingle;
                    TextBox textBox = new TextBox();
                    textBox.Name = "txt" + fieldNames[i];
                    textBox.Font = new Font("Segoe UI", 10);
                    textBox.Size = new Size(230, 35);
                    textBox.Location = new Point(10, 0);
                    textBox.BackColor = Color.FromArgb(52, 73, 94);
                    textBox.ForeColor = Color.White;
                    textBox.BorderStyle = BorderStyle.None;
                    textBox.ReadOnly = (fieldNames[i] == "ID");
                    textBoxPanel.Controls.Add(textBox);
                    panel.Controls.Add(textBoxPanel);
                    textBoxes.Add(textBox);
                }
                yPos += 70;
            }
        }
        private void CreateCRUDButtons(Panel panel)
        {
            string[] buttonTexts = { "AGREGAR", "ACTUALIZAR", "ELIMINAR", "LIMPIAR",
                                   "BUSCAR", "EXPORTAR", "IMPORTAR", "REFRESCAR" };
            int startY = 450;
            for (int i = 0; i < buttonTexts.Length; i++)
            {
                RoundedButton btn = new RoundedButton();
                btn.Text = buttonTexts[i];
                btn.ButtonColor = buttonColors[i % buttonColors.Length];
                btn.Size = new Size(140, 35);
                int xPos = 20 + (i % 2 * 160);
                int yPos = startY + (i / 2 * 50);
                btn.Location = new Point(xPos, yPos);
                // Asignar eventos
                switch (buttonTexts[i])
                {
                    case "AGREGAR":
                        btn.Click += BtnAgregar_Click;
                        break;
                    case "ACTUALIZAR":
                        btn.Click += BtnActualizar_Click;
                        break;
                    case "ELIMINAR":
                        btn.Click += BtnEliminar_Click;
                        break;
                    case "LIMPIAR":
                        btn.Click += BtnLimpiar_Click;
                        break;
                    case "REFRESCAR":
                        btn.Click += BtnRefrescar_Click;
                        break;
                    default:
                        btn.Click += (s, e) => MessageBox.Show($"Función {buttonTexts[i]} en desarrollo",
                            "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
                panel.Controls.Add(btn);
            }
        }
        private void CreateConfigPanel()
        {
            configPanel = new Panel();
            configPanel.Size = new Size(500, 350);
            configPanel.BackColor = Color.FromArgb(44, 62, 80);
            configPanel.Location = new Point(
                (this.ClientSize.Width - 500) / 2,
                (this.ClientSize.Height - 350) / 2
            );
            configPanel.Visible = false;
            configPanel.BorderStyle = BorderStyle.FixedSingle;
            // Título
            Label title = new Label();
            title.Text = "CONFIGURACIÓN DEL SISTEMA";
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Size = new Size(400, 40);
            title.Location = new Point(50, 20);
            title.TextAlign = ContentAlignment.MiddleCenter;
            // Nueva contraseña
            Label newPassLabel = new Label();
            newPassLabel.Text = "Nueva Contraseña:";
            newPassLabel.Font = new Font("Segoe UI", 12);
            newPassLabel.ForeColor = Color.White;
            newPassLabel.Size = new Size(200, 30);
            newPassLabel.Location = new Point(50, 80);
            TextBox newPassText = new TextBox();
            newPassText.Font = new Font("Segoe UI", 12);
            newPassText.Size = new Size(300, 35);
            newPassText.Location = new Point(50, 110);
            newPassText.BackColor = Color.FromArgb(52, 73, 94);
            newPassText.ForeColor = Color.White;
            newPassText.BorderStyle = BorderStyle.FixedSingle;
            newPassText.UseSystemPasswordChar = true;
            // Confirmar contraseña
            Label confirmPassLabel = new Label();
            confirmPassLabel.Text = "Confirmar Contraseña:";
            confirmPassLabel.Font = new Font("Segoe UI", 12);
            confirmPassLabel.ForeColor = Color.White;
            confirmPassLabel.Size = new Size(200, 30);
            confirmPassLabel.Location = new Point(50, 160);
            TextBox confirmPassText = new TextBox();
            confirmPassText.Font = new Font("Segoe UI", 12);
            confirmPassText.Size = new Size(300, 35);
            confirmPassText.Location = new Point(50, 190);
            confirmPassText.BackColor = Color.FromArgb(52, 73, 94);
            confirmPassText.ForeColor = Color.White;
            confirmPassText.BorderStyle = BorderStyle.FixedSingle;
            confirmPassText.UseSystemPasswordChar = true;
            // Botón Guardar
            RoundedButton saveBtn = new RoundedButton();
            saveBtn.Text = "GUARDAR CAMBIOS";
            saveBtn.ButtonColor = Color.FromArgb(46, 204, 113);
            saveBtn.Size = new Size(180, 40);
            saveBtn.Location = new Point(50, 250);
            saveBtn.Click += (s, e) => SaveConfiguration(newPassText.Text, confirmPassText.Text);
            // Botón Cancelar
            RoundedButton cancelBtn = new RoundedButton();
            cancelBtn.Text = "CANCELAR";
            cancelBtn.ButtonColor = Color.FromArgb(231, 76, 60);
            cancelBtn.Size = new Size(180, 40);
            cancelBtn.Location = new Point(250, 250);
            cancelBtn.Click += (s, e) => HideConfigPanel();
            // Botón Reset
            RoundedButton resetBtn = new RoundedButton();
            resetBtn.Text = "RESET A DEFAULT";
            resetBtn.ButtonColor = Color.FromArgb(241, 196, 15);
            resetBtn.Size = new Size(180, 40);
            resetBtn.Location = new Point(150, 300);
            resetBtn.Click += (s, e) => ResetConfiguration();
            configPanel.Controls.Add(title);
            configPanel.Controls.Add(newPassLabel);
            configPanel.Controls.Add(newPassText);
            configPanel.Controls.Add(confirmPassLabel);
            configPanel.Controls.Add(confirmPassText);
            configPanel.Controls.Add(saveBtn);
            configPanel.Controls.Add(cancelBtn);
            configPanel.Controls.Add(resetBtn);
            this.Controls.Add(configPanel);
        }
        private void InitializeDatabase()
        {
            try
            {
                connection = new MySqlConnection(connectionString);
                // Crear base de datos si no existe
                string createDbQuery = "CREATE DATABASE IF NOT EXISTS databasemanager";
                using (MySqlConnection tempConn = new MySqlConnection("server=localhost;uid=root;pwd=;"))
                {
                    tempConn.Open();
                    MySqlCommand cmd = new MySqlCommand(createDbQuery, tempConn);
                    cmd.ExecuteNonQuery();
                    tempConn.Close();
                }
                connection.Open();
                // Crear tabla cuentas
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS cuentas (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    correo VARCHAR(255) NOT NULL,
                    contraseña VARCHAR(255) NOT NULL,
                    url VARCHAR(500),
                    empresa VARCHAR(255),
                    estado VARCHAR(50) DEFAULT 'Activo',
                    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";
                MySqlCommand tableCmd = new MySqlCommand(createTableQuery, connection);
                tableCmd.ExecuteNonQuery();
                // Crear tabla configuración
                string createConfigQuery = @"
                CREATE TABLE IF NOT EXISTS configuracion (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    usuario VARCHAR(50) NOT NULL DEFAULT 'admin',
                    contraseña VARCHAR(255) NOT NULL DEFAULT 'admin'
                )";
                tableCmd = new MySqlCommand(createConfigQuery, connection);
                tableCmd.ExecuteNonQuery();
                // Verificar si existe configuración
                string checkConfigQuery = "SELECT COUNT(*) FROM configuracion";
                tableCmd = new MySqlCommand(checkConfigQuery, connection);
                int count = Convert.ToInt32(tableCmd.ExecuteScalar());
                if (count == 0)
                {
                    string insertConfigQuery = "INSERT INTO configuracion (usuario, contraseña) VALUES ('admin', 'admin')";
                    tableCmd = new MySqlCommand(insertConfigQuery, connection);
                    tableCmd.ExecuteNonQuery();
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar base de datos: {ex.Message}\n\nAsegúrate de que MySQL esté instalado y funcionando.",
                    "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(configFile))
                {
                    string[] lines = File.ReadAllLines(configFile);
                    if (lines.Length >= 2)
                    {
                        adminUser = lines[0];
                        adminPass = lines[1];
                    }
                }
                // Cargar desde base de datos
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                string query = "SELECT usuario, contraseña FROM configuracion LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    adminUser = reader["usuario"].ToString();
                    adminPass = reader["contraseña"].ToString();
                }
                reader.Close();
                connection.Close();
            }
            catch (Exception)
            {
                // Si hay error, usar valores por defecto
                adminUser = "admin";
                adminPass = "admin";
            }
        }
        private void SaveConfiguration(string newPass, string confirmPass)
        {
            if (newPass != confirmPass)
            {
                MessageBox.Show("Las contraseñas no coinciden", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(newPass))
            {
                MessageBox.Show("La contraseña no puede estar vacía", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                // Guardar en archivo
                File.WriteAllLines(configFile, new string[] { adminUser, newPass });
                // Guardar en base de datos
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                string query = "UPDATE configuracion SET contraseña = @pass WHERE usuario = @user";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@pass", newPass);
                cmd.Parameters.AddWithValue("@user", adminUser);
                cmd.ExecuteNonQuery();
                connection.Close();
                adminPass = newPass;
                MessageBox.Show("Contraseña actualizada exitosamente", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                HideConfigPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar configuración: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ResetConfiguration()
        {
            adminUser = "admin";
            adminPass = "admin";
            try
            {
                File.WriteAllLines(configFile, new string[] { adminUser, adminPass });
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                string query = "UPDATE configuracion SET contraseña = @pass WHERE usuario = @user";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@pass", adminPass);
                cmd.Parameters.AddWithValue("@user", adminUser);
                cmd.ExecuteNonQuery();
                connection.Close();
                MessageBox.Show("Configuración restablecida a valores por defecto", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                HideConfigPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restablecer configuración: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ShowLoginPanel()
        {
            loginPanel.Visible = true;
            mainPanel.Visible = false;
            configPanel.Visible = false;
            this.Text = "Database Manager - Login";
        }
        private void Login(string username, string password)
        {
            if (username == adminUser && password == adminPass)
            {
                loginPanel.Visible = false;
                mainPanel.Visible = true;
                this.Text = "Database Manager - Panel Principal";
                LoadData();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos", "Error de Login",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Logout()
        {
            ShowLoginPanel();
            ClearForm();
        }
        private void ShowConfigPanel()
        {
            configPanel.Visible = true;
            configPanel.BringToFront();
        }
        private void HideConfigPanel()
        {
            configPanel.Visible = false;
        }
        private void LoadData()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                string query = "SELECT id, correo, contraseña, url, empresa, estado FROM cuentas ORDER BY id DESC";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView.DataSource = dt;
                // Renombrar columnas para español
                if (dataGridView.Columns.Count > 0)
                {
                    dataGridView.Columns["id"].HeaderText = "ID";
                    dataGridView.Columns["correo"].HeaderText = "Correo";
                    dataGridView.Columns["contraseña"].HeaderText = "Contraseña";
                    dataGridView.Columns["url"].HeaderText = "URL";
                    dataGridView.Columns["empresa"].HeaderText = "Empresa";
                    dataGridView.Columns["estado"].HeaderText = "Estado";
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView.Rows[e.RowIndex].Cells[0].Value != null)
            {
                // Limpiar textboxes primero
                ClearForm();
                // Llenar textboxes con datos de la fila seleccionada
                if (textBoxes[0] != null)
                    textBoxes[0].Text = dataGridView.Rows[e.RowIndex].Cells["id"].Value.ToString();
                if (textBoxes[1] != null)
                    textBoxes[1].Text = dataGridView.Rows[e.RowIndex].Cells["correo"].Value.ToString();
                if (textBoxes[2] != null)
                    textBoxes[2].Text = dataGridView.Rows[e.RowIndex].Cells["contraseña"].Value.ToString();
                if (textBoxes[3] != null)
                    textBoxes[3].Text = dataGridView.Rows[e.RowIndex].Cells["url"].Value.ToString();
                if (textBoxes[4] != null)
                    textBoxes[4].Text = dataGridView.Rows[e.RowIndex].Cells["empresa"].Value.ToString();
                // Establecer estado en ComboBox
                if (estadoComboBox != null)
                {
                    string estado = dataGridView.Rows[e.RowIndex].Cells["estado"].Value.ToString();
                    estadoComboBox.SelectedItem = estado;
                }
            }
        }
        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                string correo = textBoxes[1]?.Text ?? "";
                string contraseña = textBoxes[2]?.Text ?? "";
                string url = textBoxes[3]?.Text ?? "";
                string empresa = textBoxes[4]?.Text ?? "";
                string estado = estadoComboBox?.SelectedItem?.ToString() ?? "Activo";
                if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contraseña))
                {
                    MessageBox.Show("Correo y contraseña son campos obligatorios", "Advertencia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                string query = @"INSERT INTO cuentas (correo, contraseña, url, empresa, estado)
                               VALUES (@correo, @pass, @url, @empresa, @estado)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@pass", contraseña);
                cmd.Parameters.AddWithValue("@url", url);
                cmd.Parameters.AddWithValue("@empresa", empresa);
                cmd.Parameters.AddWithValue("@estado", estado);
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Registro agregado exitosamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    BtnLimpiar_Click(sender, e);
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar registro: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxes[0]?.Text))
            {
                MessageBox.Show("Seleccione un registro para actualizar", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                int id = Convert.ToInt32(textBoxes[0].Text);
                string correo = textBoxes[1]?.Text ?? "";
                string contraseña = textBoxes[2]?.Text ?? "";
                string url = textBoxes[3]?.Text ?? "";
                string empresa = textBoxes[4]?.Text ?? "";
                string estado = estadoComboBox?.SelectedItem?.ToString() ?? "Activo";
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                string query = @"UPDATE cuentas SET correo = @correo, contraseña = @pass,
                               url = @url, empresa = @empresa, estado = @estado
                               WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@pass", contraseña);
                cmd.Parameters.AddWithValue("@url", url);
                cmd.Parameters.AddWithValue("@empresa", empresa);
                cmd.Parameters.AddWithValue("@estado", estado);
                cmd.Parameters.AddWithValue("@id", id);
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Registro actualizado exitosamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el registro", "Advertencia",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar registro: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxes[0]?.Text))
            {
                MessageBox.Show("Seleccione un registro para eliminar", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult result = MessageBox.Show(
                "¿Está seguro de eliminar este registro?",
                "Confirmar Eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (result == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(textBoxes[0].Text);
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    string query = "DELETE FROM cuentas WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Registro eliminado exitosamente", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                        BtnLimpiar_Click(sender, e);
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar registro: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
        private void BtnRefrescar_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        private void ClearForm()
        {
            foreach (TextBox textBox in textBoxes)
            {
                if (textBox != null)
                    textBox.Clear();
            }
            if (estadoComboBox != null)
                estadoComboBox.SelectedIndex = 0;
        }
        // Métodos para mover la ventana sin bordes
        private bool dragging = false;
        private Point startPoint = new Point(0, 0);
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                startPoint = new Point(e.X, e.Y);
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragging)
            {
                Point p = PointToScreen(new Point(e.X, e.Y));
                this.Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            dragging = false;
        }
    }
}
