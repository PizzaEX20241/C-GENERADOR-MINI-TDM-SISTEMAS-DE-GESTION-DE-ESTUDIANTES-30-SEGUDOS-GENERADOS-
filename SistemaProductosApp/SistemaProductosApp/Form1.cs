using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Xml;
namespace SistemaProductosApp
{
    public class Form1 : Form
    {
        // Variables de conexión y controles
        private MySqlConnection connection;
        private string server = "localhost";
        private string userId = "root";
        private string password = "";
        private string dbName = "productos_db";
        // Controles principales
        private DataGridView dataGridView1;
        private TextBox txtNombre;
        private TextBox txtDescripcion;
        private TextBox txtCodigo;
        private ComboBox cmbCategoria;
        private TextBox txtVariantes;
        private Button btnAgregar;
        private Button btnActualizar;
        private Button btnEliminar;
        private Button btnLimpiar;
        private Button btnConfiguracion;
        private Button btnPersonalizar;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        // Panel de configuración
        private Panel panelConfig;
        private Button btnResetDB;
        private Button btnEliminarDB;
        private Button btnActualizarDB;
        private Button btnExportarDB;
        private Button btnCerrarConfig;
        private Label lblConfigTitulo;
        // Panel de personalización
        private Panel panelPersonalizar;
        private Button btnCambiarFondo;
        private Button btnCambiarBotonAgregar;
        private Button btnCambiarBotonActualizar;
        private Button btnCambiarBotonEliminar;
        private Button btnCambiarBotonLimpiar;
        private Button btnCambiarFondoDGV;
        private Button btnSliderBorderRadius;
        private Button btnCerrarPersonalizar;
        private Label lblPersonalizarTitulo;
        private Label lblBorderRadius;
        private TrackBar trackBarBorderRadius;
        private ColorDialog colorDialog;
        // Panel slider borderRadius
        private Panel panelBorderRadius;
        private Button btnAplicarBorderRadius;
        private Button btnCerrarSlider;
        private Label lblSliderTitulo;
        // Colores actuales y configuraciones
        private Color colorFondoActual;
        private Color colorDGVFondoActual;
        private Color colorAgregarActual;
        private Color colorActualizarActual;
        private Color colorEliminarActual;
        private Color colorLimpiarActual;
        private int borderRadiusActual = 10;
        // Lista para códigos generados
        private HashSet<string> codigosGenerados = new HashSet<string>();
        private Random random = new Random();
        // Configuraciones persistentes
        private string configFile = "config_productos.xml";
        public Form1()
        {
            CargarConfiguracion();
            InitializeComponent();
            InitializeDatabase();
            LoadProductos();
            OcultarPaneles();
            AplicarBorderRadiusABotones();
        }
        private void CargarConfiguracion()
        {
            try
            {
                if (File.Exists(configFile))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(configFile);
                    // Cargar colores
                    XmlNode colorsNode = doc.SelectSingleNode("/Configuracion/Colores");
                    if (colorsNode != null)
                    {
                        colorFondoActual = ColorFromString(colorsNode.SelectSingleNode("Fondo")?.InnerText, Color.FromArgb(0, 0, 128));
                        colorDGVFondoActual = ColorFromString(colorsNode.SelectSingleNode("DGVFondo")?.InnerText, Color.WhiteSmoke);
                        colorAgregarActual = ColorFromString(colorsNode.SelectSingleNode("Agregar")?.InnerText, Color.Green);
                        colorActualizarActual = ColorFromString(colorsNode.SelectSingleNode("Actualizar")?.InnerText, Color.Orange);
                        colorEliminarActual = ColorFromString(colorsNode.SelectSingleNode("Eliminar")?.InnerText, Color.Red);
                        colorLimpiarActual = ColorFromString(colorsNode.SelectSingleNode("Limpiar")?.InnerText, Color.Gray);
                    }
                    // Cargar borderRadius
                    XmlNode borderRadiusNode = doc.SelectSingleNode("/Configuracion/BorderRadius");
                    if (borderRadiusNode != null)
                    {
                        borderRadiusActual = Convert.ToInt32(borderRadiusNode.InnerText);
                    }
                }
                else
                {
                    // Valores por defecto
                    colorFondoActual = Color.FromArgb(0, 0, 128);
                    colorDGVFondoActual = Color.WhiteSmoke;
                    colorAgregarActual = Color.Green;
                    colorActualizarActual = Color.Orange;
                    colorEliminarActual = Color.Red;
                    colorLimpiarActual = Color.Gray;
                    borderRadiusActual = 10;
                }
            }
            catch
            {
                // Valores por defecto si hay error
                colorFondoActual = Color.FromArgb(0, 0, 128);
                colorDGVFondoActual = Color.WhiteSmoke;
                colorAgregarActual = Color.Green;
                colorActualizarActual = Color.Orange;
                colorEliminarActual = Color.Red;
                colorLimpiarActual = Color.Gray;
                borderRadiusActual = 10;
            }
        }
        private void GuardarConfiguracion()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("Configuracion");
                doc.AppendChild(root);
                // Guardar colores
                XmlElement colorsNode = doc.CreateElement("Colores");
                root.AppendChild(colorsNode);
                colorsNode.AppendChild(CrearNodoColor(doc, "Fondo", colorFondoActual));
                colorsNode.AppendChild(CrearNodoColor(doc, "DGVFondo", colorDGVFondoActual));
                colorsNode.AppendChild(CrearNodoColor(doc, "Agregar", colorAgregarActual));
                colorsNode.AppendChild(CrearNodoColor(doc, "Actualizar", colorActualizarActual));
                colorsNode.AppendChild(CrearNodoColor(doc, "Eliminar", colorEliminarActual));
                colorsNode.AppendChild(CrearNodoColor(doc, "Limpiar", colorLimpiarActual));
                // Guardar borderRadius
                XmlElement borderRadiusNode = doc.CreateElement("BorderRadius");
                borderRadiusNode.InnerText = borderRadiusActual.ToString();
                root.AppendChild(borderRadiusNode);
                doc.Save(configFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar configuración: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private XmlElement CrearNodoColor(XmlDocument doc, string nombre, Color color)
        {
            XmlElement node = doc.CreateElement(nombre);
            node.InnerText = $"{color.A},{color.R},{color.G},{color.B}";
            return node;
        }
        private Color ColorFromString(string colorString, Color defaultColor)
        {
            try
            {
                if (string.IsNullOrEmpty(colorString)) return defaultColor;
                string[] parts = colorString.Split(',');
                if (parts.Length == 4)
                {
                    return Color.FromArgb(
                        Convert.ToInt32(parts[0]),
                        Convert.ToInt32(parts[1]),
                        Convert.ToInt32(parts[2]),
                        Convert.ToInt32(parts[3]));
                }
                return defaultColor;
            }
            catch
            {
                return defaultColor;
            }
        }
        private void InitializeComponent()
        {
            // Configuración del formulario
            this.ClientSize = new Size(900, 600);
            this.Text = "Sistema de Gestión de Productos";
            this.BackColor = colorFondoActual;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            // DataGridView
            this.dataGridView1 = new DataGridView();
            this.dataGridView1.Location = new Point(20, 20);
            this.dataGridView1.Size = new Size(860, 250);
            this.dataGridView1.BackgroundColor = colorDGVFondoActual;
            this.dataGridView1.ForeColor = Color.Black;
            this.dataGridView1.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 0, 128);
            this.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this.dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.dataGridView1.CellClick += new DataGridViewCellEventHandler(dataGridView1_CellClick);
            // Labels
            this.label1 = new Label();
            this.label1.Text = "Nombre:";
            this.label1.Location = new Point(20, 290);
            this.label1.Size = new Size(150, 25);
            this.label1.ForeColor = Color.White;
            this.label1.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.label2 = new Label();
            this.label2.Text = "Descripción:";
            this.label2.Location = new Point(20, 330);
            this.label2.Size = new Size(150, 25);
            this.label2.ForeColor = Color.White;
            this.label2.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.label3 = new Label();
            this.label3.Text = "Código:";
            this.label3.Location = new Point(20, 370);
            this.label3.Size = new Size(150, 25);
            this.label3.ForeColor = Color.White;
            this.label3.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.label4 = new Label();
            this.label4.Text = "Categoría:";
            this.label4.Location = new Point(20, 410);
            this.label4.Size = new Size(150, 25);
            this.label4.ForeColor = Color.White;
            this.label4.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.label5 = new Label();
            this.label5.Text = "Variantes:";
            this.label5.Location = new Point(20, 450);
            this.label5.Size = new Size(150, 25);
            this.label5.ForeColor = Color.White;
            this.label5.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            // TextBoxes y ComboBox
            this.txtNombre = new TextBox();
            this.txtNombre.Location = new Point(180, 290);
            this.txtNombre.Size = new Size(250, 25);
            this.txtNombre.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.txtDescripcion = new TextBox();
            this.txtDescripcion.Location = new Point(180, 330);
            this.txtDescripcion.Size = new Size(250, 25);
            this.txtDescripcion.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.txtCodigo = new TextBox();
            this.txtCodigo.Location = new Point(180, 370);
            this.txtCodigo.Size = new Size(250, 25);
            this.txtCodigo.ReadOnly = true;
            this.txtCodigo.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.txtCodigo.BackColor = Color.LightGray;
            this.cmbCategoria = new ComboBox();
            this.cmbCategoria.Location = new Point(180, 410);
            this.cmbCategoria.Size = new Size(250, 25);
            this.cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbCategoria.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.cmbCategoria.Items.AddRange(new object[] {
                "Alimentos",
                "Bebidas",
                "Limpieza",
                "Electrónicos",
                "Ropa",
                "Hogar",
                "Oficina",
                "Otros"
            });
            this.txtVariantes = new TextBox();
            this.txtVariantes.Location = new Point(180, 450);
            this.txtVariantes.Size = new Size(250, 25);
            this.txtVariantes.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            this.txtVariantes.Multiline = true;
            this.txtVariantes.Height = 60;
            // Botones principales
            this.btnAgregar = new Button();
            this.btnAgregar.Text = "➕ Agregar";
            this.btnAgregar.Location = new Point(450, 290);
            this.btnAgregar.Size = new Size(130, 40);
            this.btnAgregar.BackColor = colorAgregarActual;
            this.btnAgregar.ForeColor = Color.White;
            this.btnAgregar.FlatStyle = FlatStyle.Flat;
            this.btnAgregar.FlatAppearance.BorderSize = 0;
            this.btnAgregar.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.btnAgregar.Click += new EventHandler(btnAgregar_Click);
            this.btnActualizar = new Button();
            this.btnActualizar.Text = "✏️ Actualizar";
            this.btnActualizar.Location = new Point(450, 340);
            this.btnActualizar.Size = new Size(130, 40);
            this.btnActualizar.BackColor = colorActualizarActual;
            this.btnActualizar.ForeColor = Color.White;
            this.btnActualizar.FlatStyle = FlatStyle.Flat;
            this.btnActualizar.FlatAppearance.BorderSize = 0;
            this.btnActualizar.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.btnActualizar.Click += new EventHandler(btnActualizar_Click);
            this.btnEliminar = new Button();
            this.btnEliminar.Text = "🗑️ Eliminar";
            this.btnEliminar.Location = new Point(450, 390);
            this.btnEliminar.Size = new Size(130, 40);
            this.btnEliminar.BackColor = colorEliminarActual;
            this.btnEliminar.ForeColor = Color.White;
            this.btnEliminar.FlatStyle = FlatStyle.Flat;
            this.btnEliminar.FlatAppearance.BorderSize = 0;
            this.btnEliminar.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.btnEliminar.Click += new EventHandler(btnEliminar_Click);
            this.btnLimpiar = new Button();
            this.btnLimpiar.Text = "🧹 Limpiar";
            this.btnLimpiar.Location = new Point(450, 440);
            this.btnLimpiar.Size = new Size(130, 40);
            this.btnLimpiar.BackColor = colorLimpiarActual;
            this.btnLimpiar.ForeColor = Color.White;
            this.btnLimpiar.FlatStyle = FlatStyle.Flat;
            this.btnLimpiar.FlatAppearance.BorderSize = 0;
            this.btnLimpiar.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.btnLimpiar.Click += new EventHandler(btnLimpiar_Click);
            this.btnConfiguracion = new Button();
            this.btnConfiguracion.Text = "⚙️ Configuración";
            this.btnConfiguracion.Location = new Point(600, 290);
            this.btnConfiguracion.Size = new Size(130, 40);
            this.btnConfiguracion.BackColor = Color.DarkSlateBlue;
            this.btnConfiguracion.ForeColor = Color.White;
            this.btnConfiguracion.FlatStyle = FlatStyle.Flat;
            this.btnConfiguracion.FlatAppearance.BorderSize = 0;
            this.btnConfiguracion.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.btnConfiguracion.Click += new EventHandler(btnConfiguracion_Click);
            this.btnPersonalizar = new Button();
            this.btnPersonalizar.Text = "🎨 Personalizar";
            this.btnPersonalizar.Location = new Point(600, 340);
            this.btnPersonalizar.Size = new Size(130, 40);
            this.btnPersonalizar.BackColor = Color.DarkMagenta;
            this.btnPersonalizar.ForeColor = Color.White;
            this.btnPersonalizar.FlatStyle = FlatStyle.Flat;
            this.btnPersonalizar.FlatAppearance.BorderSize = 0;
            this.btnPersonalizar.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.btnPersonalizar.Click += new EventHandler(btnPersonalizar_Click);
            // Inicializar ColorDialog
            this.colorDialog = new ColorDialog();
            // Crear panel de configuración
            CrearPanelConfiguracion();
            // Crear panel de personalización
            CrearPanelPersonalizar();
            // Crear panel slider borderRadius
            CrearPanelBorderRadius();
            // Agregar controles al formulario
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.txtCodigo);
            this.Controls.Add(this.cmbCategoria);
            this.Controls.Add(this.txtVariantes);
            this.Controls.Add(this.btnAgregar);
            this.Controls.Add(this.btnActualizar);
            this.Controls.Add(this.btnEliminar);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.btnConfiguracion);
            this.Controls.Add(this.btnPersonalizar);
        }
        private void CrearPanelConfiguracion()
        {
            this.panelConfig = new Panel();
            this.panelConfig.Size = this.ClientSize;
            this.panelConfig.Location = new Point(0, 0);
            this.panelConfig.BackColor = Color.FromArgb(30, 30, 60);
            this.panelConfig.Visible = false;
            this.lblConfigTitulo = new Label();
            this.lblConfigTitulo.Text = "⚙️ CONFIGURACIÓN DE BASE DE DATOS";
            this.lblConfigTitulo.Location = new Point(300, 50);
            this.lblConfigTitulo.Size = new Size(400, 40);
            this.lblConfigTitulo.ForeColor = Color.White;
            this.lblConfigTitulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            this.lblConfigTitulo.TextAlign = ContentAlignment.MiddleCenter;
            this.btnResetDB = new Button();
            this.btnResetDB.Text = "🔄 Reiniciar Base de Datos";
            this.btnResetDB.Location = new Point(350, 120);
            this.btnResetDB.Size = new Size(300, 50);
            this.btnResetDB.BackColor = Color.Teal;
            this.btnResetDB.ForeColor = Color.White;
            this.btnResetDB.FlatStyle = FlatStyle.Flat;
            this.btnResetDB.FlatAppearance.BorderSize = 0;
            this.btnResetDB.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnResetDB.Click += new EventHandler(btnResetDB_Click);
            this.btnEliminarDB = new Button();
            this.btnEliminarDB.Text = "🗑️ Eliminar Base de Datos";
            this.btnEliminarDB.Location = new Point(350, 180);
            this.btnEliminarDB.Size = new Size(300, 50);
            this.btnEliminarDB.BackColor = Color.Crimson;
            this.btnEliminarDB.ForeColor = Color.White;
            this.btnEliminarDB.FlatStyle = FlatStyle.Flat;
            this.btnEliminarDB.FlatAppearance.BorderSize = 0;
            this.btnEliminarDB.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnEliminarDB.Click += new EventHandler(btnEliminarDB_Click);
            this.btnActualizarDB = new Button();
            this.btnActualizarDB.Text = "📊 Actualizar Estructura";
            this.btnActualizarDB.Location = new Point(350, 240);
            this.btnActualizarDB.Size = new Size(300, 50);
            this.btnActualizarDB.BackColor = Color.DodgerBlue;
            this.btnActualizarDB.ForeColor = Color.White;
            this.btnActualizarDB.FlatStyle = FlatStyle.Flat;
            this.btnActualizarDB.FlatAppearance.BorderSize = 0;
            this.btnActualizarDB.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnActualizarDB.Click += new EventHandler(btnActualizarDB_Click);
            this.btnExportarDB = new Button();
            this.btnExportarDB.Text = "💾 Exportar Datos";
            this.btnExportarDB.Location = new Point(350, 300);
            this.btnExportarDB.Size = new Size(300, 50);
            this.btnExportarDB.BackColor = Color.ForestGreen;
            this.btnExportarDB.ForeColor = Color.White;
            this.btnExportarDB.FlatStyle = FlatStyle.Flat;
            this.btnExportarDB.FlatAppearance.BorderSize = 0;
            this.btnExportarDB.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnExportarDB.Click += new EventHandler(btnExportarDB_Click);
            this.btnCerrarConfig = new Button();
            this.btnCerrarConfig.Text = "✖️ Cerrar Panel";
            this.btnCerrarConfig.Location = new Point(350, 360);
            this.btnCerrarConfig.Size = new Size(300, 50);
            this.btnCerrarConfig.BackColor = Color.Gray;
            this.btnCerrarConfig.ForeColor = Color.White;
            this.btnCerrarConfig.FlatStyle = FlatStyle.Flat;
            this.btnCerrarConfig.FlatAppearance.BorderSize = 0;
            this.btnCerrarConfig.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnCerrarConfig.Click += new EventHandler(btnCerrarConfig_Click);
            this.panelConfig.Controls.Add(lblConfigTitulo);
            this.panelConfig.Controls.Add(btnResetDB);
            this.panelConfig.Controls.Add(btnEliminarDB);
            this.panelConfig.Controls.Add(btnActualizarDB);
            this.panelConfig.Controls.Add(btnExportarDB);
            this.panelConfig.Controls.Add(btnCerrarConfig);
            this.Controls.Add(panelConfig);
        }
        private void CrearPanelPersonalizar()
        {
            this.panelPersonalizar = new Panel();
            this.panelPersonalizar.Size = this.ClientSize;
            this.panelPersonalizar.Location = new Point(0, 0);
            this.panelPersonalizar.BackColor = Color.FromArgb(60, 30, 60);
            this.panelPersonalizar.Visible = false;
            this.lblPersonalizarTitulo = new Label();
            this.lblPersonalizarTitulo.Text = "🎨 PERSONALIZACIÓN DE INTERFAZ";
            this.lblPersonalizarTitulo.Location = new Point(300, 50);
            this.lblPersonalizarTitulo.Size = new Size(400, 40);
            this.lblPersonalizarTitulo.ForeColor = Color.White;
            this.lblPersonalizarTitulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            this.lblPersonalizarTitulo.TextAlign = ContentAlignment.MiddleCenter;
            this.btnCambiarFondo = new Button();
            this.btnCambiarFondo.Text = "🎨 Cambiar Fondo Formulario";
            this.btnCambiarFondo.Location = new Point(350, 120);
            this.btnCambiarFondo.Size = new Size(300, 50);
            this.btnCambiarFondo.BackColor = Color.SlateBlue;
            this.btnCambiarFondo.ForeColor = Color.White;
            this.btnCambiarFondo.FlatStyle = FlatStyle.Flat;
            this.btnCambiarFondo.FlatAppearance.BorderSize = 0;
            this.btnCambiarFondo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnCambiarFondo.Click += new EventHandler(btnCambiarFondo_Click);
            this.btnCambiarBotonAgregar = new Button();
            this.btnCambiarBotonAgregar.Text = "🟢 Color Botón Agregar";
            this.btnCambiarBotonAgregar.Location = new Point(350, 180);
            this.btnCambiarBotonAgregar.Size = new Size(300, 50);
            this.btnCambiarBotonAgregar.BackColor = colorAgregarActual;
            this.btnCambiarBotonAgregar.ForeColor = Color.White;
            this.btnCambiarBotonAgregar.FlatStyle = FlatStyle.Flat;
            this.btnCambiarBotonAgregar.FlatAppearance.BorderSize = 0;
            this.btnCambiarBotonAgregar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnCambiarBotonAgregar.Click += new EventHandler(btnCambiarBotonAgregar_Click);
            this.btnCambiarBotonActualizar = new Button();
            this.btnCambiarBotonActualizar.Text = "🟠 Color Botón Actualizar";
            this.btnCambiarBotonActualizar.Location = new Point(350, 240);
            this.btnCambiarBotonActualizar.Size = new Size(300, 50);
            this.btnCambiarBotonActualizar.BackColor = colorActualizarActual;
            this.btnCambiarBotonActualizar.ForeColor = Color.White;
            this.btnCambiarBotonActualizar.FlatStyle = FlatStyle.Flat;
            this.btnCambiarBotonActualizar.FlatAppearance.BorderSize = 0;
            this.btnCambiarBotonActualizar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnCambiarBotonActualizar.Click += new EventHandler(btnCambiarBotonActualizar_Click);
            this.btnCambiarBotonEliminar = new Button();
            this.btnCambiarBotonEliminar.Text = "🔴 Color Botón Eliminar";
            this.btnCambiarBotonEliminar.Location = new Point(350, 300);
            this.btnCambiarBotonEliminar.Size = new Size(300, 50);
            this.btnCambiarBotonEliminar.BackColor = colorEliminarActual;
            this.btnCambiarBotonEliminar.ForeColor = Color.White;
            this.btnCambiarBotonEliminar.FlatStyle = FlatStyle.Flat;
            this.btnCambiarBotonEliminar.FlatAppearance.BorderSize = 0;
            this.btnCambiarBotonEliminar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnCambiarBotonEliminar.Click += new EventHandler(btnCambiarBotonEliminar_Click);
            this.btnCambiarBotonLimpiar = new Button();
            this.btnCambiarBotonLimpiar.Text = "⚪ Color Botón Limpiar";
            this.btnCambiarBotonLimpiar.Location = new Point(350, 360);
            this.btnCambiarBotonLimpiar.Size = new Size(300, 50);
            this.btnCambiarBotonLimpiar.BackColor = colorLimpiarActual;
            this.btnCambiarBotonLimpiar.ForeColor = Color.White;
            this.btnCambiarBotonLimpiar.FlatStyle = FlatStyle.Flat;
            this.btnCambiarBotonLimpiar.FlatAppearance.BorderSize = 0;
            this.btnCambiarBotonLimpiar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnCambiarBotonLimpiar.Click += new EventHandler(btnCambiarBotonLimpiar_Click);
            this.btnCambiarFondoDGV = new Button();
            this.btnCambiarFondoDGV.Text = "📊 Color Fondo DataGridView";
            this.btnCambiarFondoDGV.Location = new Point(350, 420);
            this.btnCambiarFondoDGV.Size = new Size(300, 50);
            this.btnCambiarFondoDGV.BackColor = colorDGVFondoActual;
            this.btnCambiarFondoDGV.ForeColor = Color.Black;
            this.btnCambiarFondoDGV.FlatStyle = FlatStyle.Flat;
            this.btnCambiarFondoDGV.FlatAppearance.BorderSize = 0;
            this.btnCambiarFondoDGV.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnCambiarFondoDGV.Click += new EventHandler(btnCambiarFondoDGV_Click);
            this.btnSliderBorderRadius = new Button();
            this.btnSliderBorderRadius.Text = "⭕ Configurar BorderRadius";
            this.btnSliderBorderRadius.Location = new Point(350, 480);
            this.btnSliderBorderRadius.Size = new Size(300, 50);
            this.btnSliderBorderRadius.BackColor = Color.DarkCyan;
            this.btnSliderBorderRadius.ForeColor = Color.White;
            this.btnSliderBorderRadius.FlatStyle = FlatStyle.Flat;
            this.btnSliderBorderRadius.FlatAppearance.BorderSize = 0;
            this.btnSliderBorderRadius.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnSliderBorderRadius.Click += new EventHandler(btnSliderBorderRadius_Click);
            this.btnCerrarPersonalizar = new Button();
            this.btnCerrarPersonalizar.Text = "✖️ Cerrar Panel";
            this.btnCerrarPersonalizar.Location = new Point(350, 540);
            this.btnCerrarPersonalizar.Size = new Size(300, 50);
            this.btnCerrarPersonalizar.BackColor = Color.Gray;
            this.btnCerrarPersonalizar.ForeColor = Color.White;
            this.btnCerrarPersonalizar.FlatStyle = FlatStyle.Flat;
            this.btnCerrarPersonalizar.FlatAppearance.BorderSize = 0;
            this.btnCerrarPersonalizar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.btnCerrarPersonalizar.Click += new EventHandler(btnCerrarPersonalizar_Click);
            this.panelPersonalizar.Controls.Add(lblPersonalizarTitulo);
            this.panelPersonalizar.Controls.Add(btnCambiarFondo);
            this.panelPersonalizar.Controls.Add(btnCambiarBotonAgregar);
            this.panelPersonalizar.Controls.Add(btnCambiarBotonActualizar);
            this.panelPersonalizar.Controls.Add(btnCambiarBotonEliminar);
            this.panelPersonalizar.Controls.Add(btnCambiarBotonLimpiar);
            this.panelPersonalizar.Controls.Add(btnCambiarFondoDGV);
            this.panelPersonalizar.Controls.Add(btnSliderBorderRadius);
            this.panelPersonalizar.Controls.Add(btnCerrarPersonalizar);
            this.Controls.Add(panelPersonalizar);
        }
        private void CrearPanelBorderRadius()
        {
            this.panelBorderRadius = new Panel();
            this.panelBorderRadius.Size = new Size(400, 200);
            this.panelBorderRadius.Location = new Point(250, 200);
            this.panelBorderRadius.BackColor = Color.FromArgb(40, 40, 80);
            this.panelBorderRadius.Visible = false;
            this.panelBorderRadius.BorderStyle = BorderStyle.FixedSingle;
            this.lblSliderTitulo = new Label();
            this.lblSliderTitulo.Text = "⭕ CONFIGURAR BORDER RADIUS (0-100)";
            this.lblSliderTitulo.Location = new Point(50, 20);
            this.lblSliderTitulo.Size = new Size(300, 30);
            this.lblSliderTitulo.ForeColor = Color.White;
            this.lblSliderTitulo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.lblSliderTitulo.TextAlign = ContentAlignment.MiddleCenter;
            this.lblBorderRadius = new Label();
            this.lblBorderRadius.Text = $"Valor: {borderRadiusActual}";
            this.lblBorderRadius.Location = new Point(170, 70);
            this.lblBorderRadius.Size = new Size(60, 25);
            this.lblBorderRadius.ForeColor = Color.White;
            this.lblBorderRadius.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.lblBorderRadius.TextAlign = ContentAlignment.MiddleCenter;
            this.trackBarBorderRadius = new TrackBar();
            this.trackBarBorderRadius.Location = new Point(50, 100);
            this.trackBarBorderRadius.Size = new Size(300, 45);
            this.trackBarBorderRadius.Minimum = 0;
            this.trackBarBorderRadius.Maximum = 100;
            this.trackBarBorderRadius.Value = borderRadiusActual;
            this.trackBarBorderRadius.TickFrequency = 10;
            this.trackBarBorderRadius.ValueChanged += new EventHandler(trackBarBorderRadius_ValueChanged);
            this.btnAplicarBorderRadius = new Button();
            this.btnAplicarBorderRadius.Text = "✅ Aplicar";
            this.btnAplicarBorderRadius.Location = new Point(100, 150);
            this.btnAplicarBorderRadius.Size = new Size(90, 35);
            this.btnAplicarBorderRadius.BackColor = Color.DodgerBlue;
            this.btnAplicarBorderRadius.ForeColor = Color.White;
            this.btnAplicarBorderRadius.FlatStyle = FlatStyle.Flat;
            this.btnAplicarBorderRadius.FlatAppearance.BorderSize = 0;
            this.btnAplicarBorderRadius.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.btnAplicarBorderRadius.Click += new EventHandler(btnAplicarBorderRadius_Click);
            this.btnCerrarSlider = new Button();
            this.btnCerrarSlider.Text = "✖️ Cerrar";
            this.btnCerrarSlider.Location = new Point(210, 150);
            this.btnCerrarSlider.Size = new Size(90, 35);
            this.btnCerrarSlider.BackColor = Color.Gray;
            this.btnCerrarSlider.ForeColor = Color.White;
            this.btnCerrarSlider.FlatStyle = FlatStyle.Flat;
            this.btnCerrarSlider.FlatAppearance.BorderSize = 0;
            this.btnCerrarSlider.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.btnCerrarSlider.Click += new EventHandler(btnCerrarSlider_Click);
            this.panelBorderRadius.Controls.Add(lblSliderTitulo);
            this.panelBorderRadius.Controls.Add(lblBorderRadius);
            this.panelBorderRadius.Controls.Add(trackBarBorderRadius);
            this.panelBorderRadius.Controls.Add(btnAplicarBorderRadius);
            this.panelBorderRadius.Controls.Add(btnCerrarSlider);
            this.Controls.Add(panelBorderRadius);
        }
        private void InitializeDatabase()
        {
            try
            {
                string connectionString = $"Server={server};Uid={userId};Pwd={password};";
                connection = new MySqlConnection(connectionString);
                connection.Open();
                // Verificar si existe la base de datos
                string checkDbQuery = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{dbName}'";
                using (MySqlCommand cmd = new MySqlCommand(checkDbQuery, connection))
                {
                    object result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        // Crear la base de datos si no existe
                        string createDbQuery = $"CREATE DATABASE {dbName} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";
                        using (MySqlCommand createCmd = new MySqlCommand(createDbQuery, connection))
                        {
                            createCmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Base de datos creada exitosamente.", "Información",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    // Usar la base de datos
                    string useDbQuery = $"USE {dbName}";
                    using (MySqlCommand useCmd = new MySqlCommand(useDbQuery, connection))
                    {
                        useCmd.ExecuteNonQuery();
                    }
                    // Verificar y crear/actualizar la tabla
                    VerificarYCrearTabla();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con la base de datos: {ex.Message}\n\nAsegúrate de que MySQL esté ejecutándose.",
                    "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
        private void VerificarYCrearTabla()
        {
            try
            {
                string checkTableQuery = "SHOW TABLES LIKE 'productos'";
                using (MySqlCommand cmd = new MySqlCommand(checkTableQuery, connection))
                {
                    object result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        // Crear tabla con todas las columnas
                        string createTableQuery = @"
                            CREATE TABLE productos (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                nombre VARCHAR(100) NOT NULL,
                                descripcion VARCHAR(255),
                                codigo VARCHAR(20) NOT NULL UNIQUE,
                                categoria VARCHAR(50),
                                variantes TEXT,
                                fecha_registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci";
                        using (MySqlCommand tableCmd = new MySqlCommand(createTableQuery, connection))
                        {
                            tableCmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Tabla 'productos' creada con todas las columnas.", "Información",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Verificar si faltan columnas y agregarlas
                        ActualizarEstructuraTabla();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar/crear tabla: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ActualizarEstructuraTabla()
        {
            try
            {
                // Verificar y agregar columnas faltantes
                string[] columnas = { "descripcion", "categoria", "variantes" };
                foreach (string columna in columnas)
                {
                    string checkColumnQuery = $@"
                        SELECT COUNT(*)
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = '{dbName}'
                        AND TABLE_NAME = 'productos'
                        AND COLUMN_NAME = '{columna}'";
                    using (MySqlCommand cmd = new MySqlCommand(checkColumnQuery, connection))
                    {
                        int existe = Convert.ToInt32(cmd.ExecuteScalar());
                        if (existe == 0)
                        {
                            string tipoColumna = "";
                            if (columna == "descripcion")
                                tipoColumna = "VARCHAR(255)";
                            else if (columna == "categoria")
                                tipoColumna = "VARCHAR(50)";
                            else if (columna == "variantes")
                                tipoColumna = "TEXT";
                            string addColumnQuery = $"ALTER TABLE productos ADD COLUMN {columna} {tipoColumna}";
                            using (MySqlCommand addCmd = new MySqlCommand(addColumnQuery, connection))
                            {
                                addCmd.ExecuteNonQuery();
                            }
                            MessageBox.Show($"Columna '{columna}' agregada a la tabla.", "Actualización",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar estructura: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadProductos()
        {
            try
            {
                // Cargar códigos existentes
                codigosGenerados.Clear();
                string queryCodigos = "SELECT codigo FROM productos";
                using (MySqlCommand cmd = new MySqlCommand(queryCodigos, connection))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            codigosGenerados.Add(reader["codigo"].ToString());
                        }
                    }
                }
                string query = "SELECT id, nombre, descripcion, codigo, categoria, variantes FROM productos ORDER BY id";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
                // Configuración segura de columnas
                ConfigurarColumnasDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ConfigurarColumnasDataGridView()
        {
            if (dataGridView1.Columns.Count == 0)
                return;
            try
            {
                if (dataGridView1.Columns.Contains("id"))
                    dataGridView1.Columns["id"].Visible = false;
                if (dataGridView1.Columns.Contains("nombre"))
                {
                    dataGridView1.Columns["nombre"].HeaderText = "Nombre";
                    
                }
                if (dataGridView1.Columns.Contains("descripcion"))
                {
                    dataGridView1.Columns["descripcion"].HeaderText = "Descripción";
                    
                }
                if (dataGridView1.Columns.Contains("codigo"))
                {
                    dataGridView1.Columns["codigo"].HeaderText = "Código";
                    
                }
                if (dataGridView1.Columns.Contains("categoria"))
                {
                    dataGridView1.Columns["categoria"].HeaderText = "Categoría";
                    
                }
                if (dataGridView1.Columns.Contains("variantes"))
                {
                    dataGridView1.Columns["variantes"].HeaderText = "Variantes";
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al configurar columnas: {ex.Message}", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private string GenerarCodigoProducto()
        {
            string codigo;
            do
            {
                // Generar 5 dígitos aleatorios
                int numeroBase = random.Next(10000, 99999);
                // Generar 3 últimos dígitos para variantes
                int variante = random.Next(100, 999);
                // Combinar: primeros 5 dígitos + guión + 3 dígitos variante
                codigo = $"{numeroBase}-{variante}";
            } while (codigosGenerados.Contains(codigo));
            codigosGenerados.Add(codigo);
            return codigo;
        }
        private void AplicarBorderRadiusABotones()
        {
            // Crear región con bordes redondeados para cada botón
            AplicarBorderRadiusABoton(btnAgregar);
            AplicarBorderRadiusABoton(btnActualizar);
            AplicarBorderRadiusABoton(btnEliminar);
            AplicarBorderRadiusABoton(btnLimpiar);
            AplicarBorderRadiusABoton(btnConfiguracion);
            AplicarBorderRadiusABoton(btnPersonalizar);
            // Aplicar también a botones de los paneles
            if (panelConfig != null)
            {
                foreach (Control control in panelConfig.Controls)
                {
                    if (control is Button)
                        AplicarBorderRadiusABoton((Button)control);
                }
            }
            if (panelPersonalizar != null)
            {
                foreach (Control control in panelPersonalizar.Controls)
                {
                    if (control is Button)
                        AplicarBorderRadiusABoton((Button)control);
                }
            }
            if (panelBorderRadius != null)
            {
                foreach (Control control in panelBorderRadius.Controls)
                {
                    if (control is Button)
                        AplicarBorderRadiusABoton((Button)control);
                }
            }
        }
        private void AplicarBorderRadiusABoton(Button boton)
        {
            if (boton == null) return;
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            int radius = borderRadiusActual;
            // Crear región redondeada
            Rectangle rect = new Rectangle(0, 0, boton.Width, boton.Height);
            if (radius > 0)
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                path.CloseAllFigures();
                boton.Region = new Region(path);
            }
            else
            {
                boton.Region = new Region(rect);
            }
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                if (row.Cells["nombre"].Value != null)
                    txtNombre.Text = row.Cells["nombre"].Value.ToString();
                else
                    txtNombre.Clear();
                if (row.Cells["descripcion"].Value != null)
                    txtDescripcion.Text = row.Cells["descripcion"].Value.ToString();
                else
                    txtDescripcion.Clear();
                if (row.Cells["codigo"].Value != null)
                    txtCodigo.Text = row.Cells["codigo"].Value.ToString();
                else
                    txtCodigo.Clear();
                if (row.Cells["categoria"].Value != null &&
                    row.Cells["categoria"].Value != DBNull.Value)
                    cmbCategoria.Text = row.Cells["categoria"].Value.ToString();
                else
                    cmbCategoria.SelectedIndex = -1;
                if (row.Cells["variantes"].Value != null &&
                    row.Cells["variantes"].Value != DBNull.Value)
                    txtVariantes.Text = row.Cells["variantes"].Value.ToString();
                else
                    txtVariantes.Clear();
            }
        }
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Por favor, complete el nombre del producto.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string codigo = GenerarCodigoProducto();
                string query = @"INSERT INTO productos (nombre, descripcion, codigo, categoria, variantes)
                               VALUES (@nombre, @descripcion, @codigo, @categoria, @variantes)";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text.Trim());
                    if (!string.IsNullOrWhiteSpace(txtDescripcion.Text))
                        cmd.Parameters.AddWithValue("@descripcion", txtDescripcion.Text.Trim());
                    else
                        cmd.Parameters.AddWithValue("@descripcion", DBNull.Value);
                    cmd.Parameters.AddWithValue("@codigo", codigo);
                    if (cmbCategoria.SelectedItem != null)
                        cmd.Parameters.AddWithValue("@categoria", cmbCategoria.SelectedItem.ToString());
                    else
                        cmd.Parameters.AddWithValue("@categoria", DBNull.Value);
                    if (!string.IsNullOrWhiteSpace(txtVariantes.Text))
                        cmd.Parameters.AddWithValue("@variantes", txtVariantes.Text.Trim());
                    else
                        cmd.Parameters.AddWithValue("@variantes", DBNull.Value);
                    cmd.ExecuteNonQuery();
                    txtCodigo.Text = codigo;
                    LoadProductos();
                    MessageBox.Show("✅ Producto agregado exitosamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                MessageBox.Show("Código duplicado. Intente nuevamente.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnAgregar_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar producto: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("Seleccione un producto para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string query = @"UPDATE productos
                               SET nombre = @nombre, descripcion = @descripcion,
                                   categoria = @categoria, variantes = @variantes
                               WHERE codigo = @codigo";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@nombre", txtNombre.Text.Trim());
                    if (!string.IsNullOrWhiteSpace(txtDescripcion.Text))
                        cmd.Parameters.AddWithValue("@descripcion", txtDescripcion.Text.Trim());
                    else
                        cmd.Parameters.AddWithValue("@descripcion", DBNull.Value);
                    cmd.Parameters.AddWithValue("@codigo", txtCodigo.Text.Trim());
                    if (cmbCategoria.SelectedItem != null)
                        cmd.Parameters.AddWithValue("@categoria", cmbCategoria.SelectedItem.ToString());
                    else
                        cmd.Parameters.AddWithValue("@categoria", DBNull.Value);
                    if (!string.IsNullOrWhiteSpace(txtVariantes.Text))
                        cmd.Parameters.AddWithValue("@variantes", txtVariantes.Text.Trim());
                    else
                        cmd.Parameters.AddWithValue("@variantes", DBNull.Value);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        LoadProductos();
                        MessageBox.Show("✅ Producto actualizado exitosamente.", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar producto: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("Seleccione un producto para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult result = MessageBox.Show("¿Está seguro de eliminar este producto?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM productos WHERE codigo = @codigo";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@codigo", txtCodigo.Text.Trim());
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            LoadProductos();
                            btnLimpiar_Click(sender, e);
                            MessageBox.Show("✅ Producto eliminado exitosamente.", "Éxito",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar producto: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtNombre.Clear();
            txtDescripcion.Clear();
            txtCodigo.Clear();
            cmbCategoria.SelectedIndex = -1;
            txtVariantes.Clear();
            dataGridView1.ClearSelection();
        }
        // Métodos para paneles
        private void OcultarPaneles()
        {
            panelConfig.Visible = false;
            panelPersonalizar.Visible = false;
            panelBorderRadius.Visible = false;
        }
        private void btnConfiguracion_Click(object sender, EventArgs e)
        {
            OcultarPaneles();
            panelConfig.Visible = true;
            panelConfig.BringToFront();
        }
        private void btnPersonalizar_Click(object sender, EventArgs e)
        {
            OcultarPaneles();
            panelPersonalizar.Visible = true;
            panelPersonalizar.BringToFront();
        }
        private void btnSliderBorderRadius_Click(object sender, EventArgs e)
        {
            panelBorderRadius.Visible = true;
            panelBorderRadius.BringToFront();
        }
        private void btnCerrarConfig_Click(object sender, EventArgs e)
        {
            panelConfig.Visible = false;
        }
        private void btnCerrarPersonalizar_Click(object sender, EventArgs e)
        {
            panelPersonalizar.Visible = false;
        }
        private void btnCerrarSlider_Click(object sender, EventArgs e)
        {
            panelBorderRadius.Visible = false;
        }
        // TrackBar eventos
        private void trackBarBorderRadius_ValueChanged(object sender, EventArgs e)
        {
            lblBorderRadius.Text = $"Valor: {trackBarBorderRadius.Value}";
        }
        private void btnAplicarBorderRadius_Click(object sender, EventArgs e)
        {
            borderRadiusActual = trackBarBorderRadius.Value;
            AplicarBorderRadiusABotones();
            GuardarConfiguracion();
            MessageBox.Show($"✅ BorderRadius aplicado: {borderRadiusActual}", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        // Métodos de configuración de base de datos
        private void btnResetDB_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("¿Está seguro de reiniciar la base de datos?\n\nSe eliminarán todos los datos y se creará una nueva estructura.",
                "Confirmar Reinicio", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    // Eliminar base de datos existente
                    string dropDbQuery = $"DROP DATABASE IF EXISTS {dbName}";
                    using (MySqlCommand cmd = new MySqlCommand(dropDbQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    // Crear nueva base de datos
                    string createDbQuery = $"CREATE DATABASE {dbName} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";
                    using (MySqlCommand cmd = new MySqlCommand(createDbQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    // Usar la base de datos
                    string useDbQuery = $"USE {dbName}";
                    using (MySqlCommand cmd = new MySqlCommand(useDbQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    // Crear tabla
                    string createTableQuery = @"
                        CREATE TABLE productos (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            nombre VARCHAR(100) NOT NULL,
                            descripcion VARCHAR(255),
                            codigo VARCHAR(20) NOT NULL UNIQUE,
                            categoria VARCHAR(50),
                            variantes TEXT,
                            fecha_registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci";
                    using (MySqlCommand cmd = new MySqlCommand(createTableQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    LoadProductos();
                    MessageBox.Show("✅ Base de datos reiniciada exitosamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al reiniciar base de datos: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnEliminarDB_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("¿Está seguro de ELIMINAR COMPLETAMENTE la base de datos?\n\nEsta acción NO se puede deshacer.",
                "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (result == DialogResult.Yes)
            {
                try
                {
                    string dropDbQuery = $"DROP DATABASE IF EXISTS {dbName}";
                    using (MySqlCommand cmd = new MySqlCommand(dropDbQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("✅ Base de datos eliminada exitosamente.\n\nLa aplicación se cerrará.",
                        "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar base de datos: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnActualizarDB_Click(object sender, EventArgs e)
        {
            try
            {
                ActualizarEstructuraTabla();
                LoadProductos();
                MessageBox.Show("✅ Estructura de base de datos actualizada.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar estructura: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnExportarDB_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Archivos CSV (*.csv)|*.csv|Archivos TXT (*.txt)|*.txt";
                saveFileDialog.Title = "Exportar datos de productos";
                saveFileDialog.FileName = $"productos_export_{DateTime.Now:yyyyMMdd_HHmmss}";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string query = "SELECT nombre, descripcion, codigo, categoria, variantes FROM productos ORDER BY id";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                    {
                        // Escribir encabezados
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            sw.Write(dt.Columns[i].ColumnName);
                            if (i < dt.Columns.Count - 1) sw.Write(",");
                        }
                        sw.WriteLine();
                        // Escribir datos
                        foreach (DataRow row in dt.Rows)
                        {
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string value = row[i].ToString();
                                sw.Write($"\"{value.Replace("\"", "\"\"")}\"");
                                if (i < dt.Columns.Count - 1) sw.Write(",");
                            }
                            sw.WriteLine();
                        }
                    }
                    MessageBox.Show($"✅ Datos exportados exitosamente a:\n{saveFileDialog.FileName}", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar datos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Métodos de personalización
        private void btnCambiarFondo_Click(object sender, EventArgs e)
        {
            colorDialog.Color = colorFondoActual;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                colorFondoActual = colorDialog.Color;
                this.BackColor = colorFondoActual;
                // Actualizar color de labels para mantener contraste
                UpdateLabelColors();
                GuardarConfiguracion();
            }
        }
        private void UpdateLabelColors()
        {
            // Determinar si el fondo es oscuro o claro
            float brightness = colorFondoActual.GetBrightness();
            Color textColor = brightness < 0.5 ? Color.White : Color.Black;
            label1.ForeColor = textColor;
            label2.ForeColor = textColor;
            label3.ForeColor = textColor;
            label4.ForeColor = textColor;
            label5.ForeColor = textColor;
        }
        private void btnCambiarBotonAgregar_Click(object sender, EventArgs e)
        {
            colorDialog.Color = colorAgregarActual;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                colorAgregarActual = colorDialog.Color;
                btnAgregar.BackColor = colorAgregarActual;
                btnCambiarBotonAgregar.BackColor = colorAgregarActual;
                // Ajustar color de texto según el fondo
                float brightness = colorAgregarActual.GetBrightness();
                btnAgregar.ForeColor = brightness < 0.5 ? Color.White : Color.Black;
                GuardarConfiguracion();
            }
        }
        private void btnCambiarBotonActualizar_Click(object sender, EventArgs e)
        {
            colorDialog.Color = colorActualizarActual;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                colorActualizarActual = colorDialog.Color;
                btnActualizar.BackColor = colorActualizarActual;
                btnCambiarBotonActualizar.BackColor = colorActualizarActual;
                float brightness = colorActualizarActual.GetBrightness();
                btnActualizar.ForeColor = brightness < 0.5 ? Color.White : Color.Black;
                GuardarConfiguracion();
            }
        }
        private void btnCambiarBotonEliminar_Click(object sender, EventArgs e)
        {
            colorDialog.Color = colorEliminarActual;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                colorEliminarActual = colorDialog.Color;
                btnEliminar.BackColor = colorEliminarActual;
                btnCambiarBotonEliminar.BackColor = colorEliminarActual;
                float brightness = colorEliminarActual.GetBrightness();
                btnEliminar.ForeColor = brightness < 0.5 ? Color.White : Color.Black;
                GuardarConfiguracion();
            }
        }
        private void btnCambiarBotonLimpiar_Click(object sender, EventArgs e)
        {
            colorDialog.Color = colorLimpiarActual;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                colorLimpiarActual = colorDialog.Color;
                btnLimpiar.BackColor = colorLimpiarActual;
                btnCambiarBotonLimpiar.BackColor = colorLimpiarActual;
                float brightness = colorLimpiarActual.GetBrightness();
                btnLimpiar.ForeColor = brightness < 0.5 ? Color.White : Color.Black;
                GuardarConfiguracion();
            }
        }
        private void btnCambiarFondoDGV_Click(object sender, EventArgs e)
        {
            colorDialog.Color = colorDGVFondoActual;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                colorDGVFondoActual = colorDialog.Color;
                dataGridView1.BackgroundColor = colorDGVFondoActual;
                btnCambiarFondoDGV.BackColor = colorDGVFondoActual;
                float brightness = colorDGVFondoActual.GetBrightness();
                btnCambiarFondoDGV.ForeColor = brightness < 0.5 ? Color.White : Color.Black;
                dataGridView1.ForeColor = brightness < 0.5 ? Color.White : Color.Black;
                GuardarConfiguracion();
            }
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
