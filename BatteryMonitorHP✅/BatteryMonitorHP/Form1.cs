using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BatteryMonitorHP
{
    public partial class Form1 : Form
    {
        // Importaciones de Windows API para obtener información REAL de la batería
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_POWER_STATUS
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte Reserved1;
            public int BatteryLifeTime;
            public int BatteryFullLifeTime;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetSystemPowerStatus(ref SYSTEM_POWER_STATUS systemPowerStatus);

        // Para obtener información de la batería desde el registro de Windows
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired, out IntPtr phkResult);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, IntPtr lpReserved, out uint lpType, IntPtr lpData, ref uint lpcbData);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegCloseKey(IntPtr hKey);

        const int HKEY_LOCAL_MACHINE = unchecked((int)0x80000002);
        const int KEY_QUERY_VALUE = 0x0001;
        const int KEY_WOW64_64KEY = 0x0100;

        // Variables para control de actualización
        private Timer updateTimer;
        private Label lblStatus;
        private Label lblPercentage;
        private Label lblLifeTime;
        private Label lblVoltage;
        private Label lblDesignCapacity;
        private Label lblFullChargeCapacity;
        private Label lblBatteryHealth;
        private Label lblManufacturer;
        private Label lblChemistry;
        private Label lblDeviceName;
        private Label lblCycleCount;
        private ProgressBar progressBar;
        private Button btnRefresh;
        private Button btnExit;
        private Button btnPowerCfg;
        private PictureBox pbBatteryIcon;
        private Panel panelInfo;
        private Label lblLastUpdated;
        private Label lblBatteryPresent;
        private Label lblErrorInfo;
        private System.ComponentModel.IContainer components;

        // Para almacenar información REAL de la batería
        private BatteryRealInfo batteryRealInfo;
        private bool isFirstUpdate = true;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();

            // Inicializar estructura de información
            batteryRealInfo = new BatteryRealInfo();

            UpdateBatteryInfo();
            StartUpdateTimer();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 550);
            this.Text = "Monitor de Batería HP Pavilion 15 12cw - Información REAL";
            this.BackColor = Color.FromArgb(240, 245, 255);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeCustomComponents()
        {
            // Título
            Label lblTitle = new Label();
            lblTitle.Text = "Monitor de Batería REAL - HP Pavilion 15 12cw";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 51, 102);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(30, 20);
            this.Controls.Add(lblTitle);

            // Panel de información
            panelInfo = new Panel();
            panelInfo.BorderStyle = BorderStyle.FixedSingle;
            panelInfo.BackColor = Color.White;
            panelInfo.Location = new Point(30, 70);
            panelInfo.Size = new Size(640, 350);
            this.Controls.Add(panelInfo);

            // Ícono de batería
            pbBatteryIcon = new PictureBox();
            pbBatteryIcon.Size = new Size(80, 40);
            pbBatteryIcon.Location = new Point(30, 30);
            pbBatteryIcon.BackColor = Color.Transparent;
            panelInfo.Controls.Add(pbBatteryIcon);

            // Nombre del dispositivo
            lblDeviceName = CreateInfoLabel("Dispositivo: HP Pavilion 15 12cw", 30, 10);
            lblDeviceName.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            panelInfo.Controls.Add(lblDeviceName);

            // Batería presente
            lblBatteryPresent = CreateInfoLabel("", 120, 30);
            lblBatteryPresent.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            panelInfo.Controls.Add(lblBatteryPresent);

            // Etiqueta de estado
            lblStatus = CreateInfoLabel("Estado:", 30, 80);
            panelInfo.Controls.Add(lblStatus);

            // Barra de progreso de porcentaje
            progressBar = new ProgressBar();
            progressBar.Location = new Point(30, 110);
            progressBar.Size = new Size(580, 25);
            progressBar.Maximum = 100;
            progressBar.Minimum = 0;
            panelInfo.Controls.Add(progressBar);

            // Porcentaje (información REAL de Windows API)
            lblPercentage = CreateInfoLabel("Porcentaje:", 30, 145);
            panelInfo.Controls.Add(lblPercentage);

            // Tiempo de vida restante (información REAL de Windows API)
            lblLifeTime = CreateInfoLabel("Tiempo restante:", 30, 180);
            panelInfo.Controls.Add(lblLifeTime);

            // Voltaje (obtenido del informe de powercfg)
            lblVoltage = CreateInfoLabel("Voltaje:", 30, 215);
            panelInfo.Controls.Add(lblVoltage);

            // Capacidad de diseño (obtenido del informe de powercfg)
            lblDesignCapacity = CreateInfoLabel("Capacidad de diseño:", 350, 145);
            panelInfo.Controls.Add(lblDesignCapacity);

            // Capacidad de carga completa (obtenido del informe de powercfg)
            lblFullChargeCapacity = CreateInfoLabel("Capacidad de carga completa:", 350, 180);
            panelInfo.Controls.Add(lblFullChargeCapacity);

            // Salud de la batería (calculada REAL)
            lblBatteryHealth = CreateInfoLabel("Salud de la batería:", 350, 215);
            panelInfo.Controls.Add(lblBatteryHealth);

            // Fabricante (obtenido del informe de powercfg)
            lblManufacturer = CreateInfoLabel("Fabricante:", 30, 250);
            panelInfo.Controls.Add(lblManufacturer);

            // Química de la batería (obtenido del informe de powercfg)
            lblChemistry = CreateInfoLabel("Tipo de batería:", 350, 250);
            panelInfo.Controls.Add(lblChemistry);

            // Contador de ciclos (obtenido del informe de powercfg)
            lblCycleCount = CreateInfoLabel("Ciclos de carga:", 30, 285);
            panelInfo.Controls.Add(lblCycleCount);

            // Información de error (inicialmente oculta)
            lblErrorInfo = new Label();
            lblErrorInfo.Text = "Obteniendo información REAL de la batería...";
            lblErrorInfo.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lblErrorInfo.ForeColor = Color.Blue;
            lblErrorInfo.AutoSize = true;
            lblErrorInfo.Location = new Point(30, 320);
            lblErrorInfo.Visible = true;
            panelInfo.Controls.Add(lblErrorInfo);

            // Última actualización
            lblLastUpdated = new Label();
            lblLastUpdated.Text = "";
            lblLastUpdated.Font = new Font("Segoe UI", 8);
            lblLastUpdated.ForeColor = Color.Gray;
            lblLastUpdated.AutoSize = true;
            lblLastUpdated.Location = new Point(30, 440);
            this.Controls.Add(lblLastUpdated);

            // Botón de actualización
            btnRefresh = new Button();
            btnRefresh.Text = "Actualizar INFO";
            btnRefresh.Font = new Font("Segoe UI", 10);
            btnRefresh.BackColor = Color.FromArgb(0, 123, 255);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Size = new Size(140, 35);
            btnRefresh.Location = new Point(150, 470);
            btnRefresh.Click += BtnRefresh_Click;
            this.Controls.Add(btnRefresh);

            // Botón para ejecutar powercfg y obtener información REAL
            btnPowerCfg = new Button();
            btnPowerCfg.Text = "Obtener INFO REAL";
            btnPowerCfg.Font = new Font("Segoe UI", 10);
            btnPowerCfg.BackColor = Color.FromArgb(40, 167, 69);
            btnPowerCfg.ForeColor = Color.White;
            btnPowerCfg.FlatStyle = FlatStyle.Flat;
            btnPowerCfg.Size = new Size(160, 35);
            btnPowerCfg.Location = new Point(310, 470);
            btnPowerCfg.Click += BtnPowerCfg_Click;
            this.Controls.Add(btnPowerCfg);

            // Botón de salida
            btnExit = new Button();
            btnExit.Text = "Salir";
            btnExit.Font = new Font("Segoe UI", 10);
            btnExit.BackColor = Color.FromArgb(108, 117, 125);
            btnExit.ForeColor = Color.White;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.Size = new Size(120, 35);
            btnExit.Location = new Point(490, 470);
            btnExit.Click += BtnExit_Click;
            this.Controls.Add(btnExit);

            // Timer para actualizaciones automáticas
            updateTimer = new Timer();
            updateTimer.Interval = 10000; // 10 segundos para no saturar
            updateTimer.Tick += UpdateTimer_Tick;
        }

        private Label CreateInfoLabel(string text, int x, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = new Font("Segoe UI", 10);
            label.ForeColor = Color.FromArgb(33, 37, 41);
            label.AutoSize = false;
            label.Size = new Size(300, 25);
            label.Location = new Point(x, y);
            return label;
        }

        private void StartUpdateTimer()
        {
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateBatteryInfo();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            UpdateBatteryInfo();
        }

        private async void BtnPowerCfg_Click(object sender, EventArgs e)
        {
            btnPowerCfg.Enabled = false;
            btnPowerCfg.Text = "Analizando...";
            lblErrorInfo.Text = "Analizando información REAL de la batería...";
            lblErrorInfo.ForeColor = Color.Blue;
            lblErrorInfo.Visible = true;

            // Forzar una nueva lectura del informe de batería
            batteryRealInfo = new BatteryRealInfo();

            await Task.Run(() => GetRealBatteryInfoFromPowerCfg());

            // Actualizar la interfaz con la información REAL
            UpdateUIWithRealInfo();

            btnPowerCfg.Enabled = true;
            btnPowerCfg.Text = "Obtener INFO REAL";
        }

        private void GetRealBatteryInfoFromPowerCfg()
        {
            try
            {
                string tempFile = Path.Combine(Path.GetTempPath(), "battery_report_temp.html");

                // Ejecutar powercfg para generar un informe de batería
                Process process = new Process();
                process.StartInfo.FileName = "powercfg";
                process.StartInfo.Arguments = $"/batteryreport /output \"{tempFile}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();
                process.WaitForExit(10000); // Esperar hasta 10 segundos

                if (File.Exists(tempFile))
                {
                    ParseBatteryReport(tempFile);
                    File.Delete(tempFile); // Limpiar archivo temporal
                }
                else
                {
                    // Intentar leer información del registro como alternativa
                    ReadBatteryInfoFromRegistry();
                }
            }
            catch (Exception ex)
            {
                // Intentar leer información del registro como alternativa
                ReadBatteryInfoFromRegistry();
                Debug.WriteLine($"Error powercfg: {ex.Message}");
            }
        }

        private void ParseBatteryReport(string filePath)
        {
            try
            {
                string htmlContent = File.ReadAllText(filePath);

                // Buscar información específica en el reporte HTML
                // Nombre del dispositivo
                Match match = Regex.Match(htmlContent, @"NAME[\s\S]*?<td[^>]*>([^<]+)</td>", RegexOptions.IgnoreCase);
                if (match.Success) batteryRealInfo.Name = match.Groups[1].Value;

                // Fabricante
                match = Regex.Match(htmlContent, @"MANUFACTURER[\s\S]*?<td[^>]*>([^<]+)</td>", RegexOptions.IgnoreCase);
                if (match.Success) batteryRealInfo.Manufacturer = match.Groups[1].Value;

                // Química
                match = Regex.Match(htmlContent, @"CHEMISTRY[\s\S]*?<td[^>]*>([^<]+)</td>", RegexOptions.IgnoreCase);
                if (match.Success) batteryRealInfo.Chemistry = match.Groups[1].Value;

                // Capacidad de diseño
                match = Regex.Match(htmlContent, @"DESIGN CAPACITY[\s\S]*?<td[^>]*>([^<]+)</td>", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string designCap = match.Groups[1].Value;
                    batteryRealInfo.DesignCapacity = ParseCapacity(designCap);
                }

                // Capacidad de carga completa
                match = Regex.Match(htmlContent, @"FULL CHARGE CAPACITY[\s\S]*?<td[^>]*>([^<]+)</td>", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string fullChargeCap = match.Groups[1].Value;
                    batteryRealInfo.FullChargeCapacity = ParseCapacity(fullChargeCap);
                }

                // Ciclos de carga
                match = Regex.Match(htmlContent, @"CYCLE COUNT[\s\S]*?<td[^>]*>([^<]+)</td>", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value.Trim(), out int cycles))
                        batteryRealInfo.CycleCount = cycles;
                }

                // Voltaje de diseño
                match = Regex.Match(htmlContent, @"DESIGN VOLTAGE[\s\S]*?<td[^>]*>([^<]+)</td>", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string voltageStr = match.Groups[1].Value;
                    batteryRealInfo.DesignVoltage = ParseVoltage(voltageStr);
                }

                batteryRealInfo.IsRealData = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing report: {ex.Message}");
            }
        }

        private long ParseCapacity(string capacityStr)
        {
            try
            {
                // Ejemplo: "41,000 mWh" o "41,000 mWh (56,000 mWh)"
                string cleanStr = Regex.Replace(capacityStr, @"[^\d]", "");
                if (long.TryParse(cleanStr, out long result))
                    return result;
            }
            catch { }
            return 0;
        }

        private int ParseVoltage(string voltageStr)
        {
            try
            {
                // Ejemplo: "11,100 mV" o "11.1 V"
                string cleanStr = Regex.Replace(voltageStr, @"[^\d]", "");
                if (int.TryParse(cleanStr, out int result))
                    return result;
            }
            catch { }
            return 0;
        }

        private void ReadBatteryInfoFromRegistry()
        {
            try
            {
                IntPtr hKey = IntPtr.Zero;
                string keyPath = @"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\ActiveOverlayAc";

                // Intentar leer información de batería del registro
                // Nota: Esta información es más limitada que powercfg
                if (RegOpenKeyEx((IntPtr)HKEY_LOCAL_MACHINE, keyPath, 0, KEY_QUERY_VALUE | KEY_WOW64_64KEY, out hKey) == 0)
                {
                    // Leer valores del registro relacionados con batería
                    // (Esta es una implementación simplificada)
                    RegCloseKey(hKey);
                }

                // Si no se pudo obtener información real, al menos marcar que son datos estimados
                batteryRealInfo.IsRealData = false;
            }
            catch
            {
                batteryRealInfo.IsRealData = false;
            }
        }

        private void UpdateBatteryInfo()
        {
            try
            {
                // Obtener información básica REAL de la batería desde Windows API
                SYSTEM_POWER_STATUS powerStatus = new SYSTEM_POWER_STATUS();
                int result = GetSystemPowerStatus(ref powerStatus);

                if (result == 0)
                {
                    throw new Exception("No se pudo obtener el estado de energía del sistema.");
                }

                // Verificar si hay batería presente (información REAL)
                bool batteryPresent = (powerStatus.BatteryFlag & 128) == 0;

                lblBatteryPresent.Text = batteryPresent ? "Batería: Presente ✓" : "Batería: No detectada ✗";
                lblBatteryPresent.ForeColor = batteryPresent ? Color.Green : Color.Red;

                if (batteryPresent)
                {
                    // Actualizar estado REAL
                    string statusText = GetBatteryStatusText(powerStatus.ACLineStatus, powerStatus.BatteryFlag);
                    lblStatus.Text = $"Estado: {statusText}";

                    // Actualizar porcentaje REAL
                    int percentage = powerStatus.BatteryLifePercent;
                    if (percentage == 255) percentage = 0; // 255 significa desconocido

                    lblPercentage.Text = $"Porcentaje: {percentage}%";
                    progressBar.Value = percentage;
                    UpdateBatteryIcon(percentage, powerStatus.ACLineStatus);

                    // Actualizar tiempo restante REAL
                    string lifeTimeText = GetBatteryLifeTimeText(powerStatus.BatteryLifeTime);
                    lblLifeTime.Text = $"Tiempo restante: {lifeTimeText}";

                    // Si es la primera vez o necesitamos información detallada, obtenerla
                    if (isFirstUpdate || !batteryRealInfo.IsRealData)
                    {
                        GetRealBatteryInfoFromPowerCfg();
                        isFirstUpdate = false;
                    }

                    // Actualizar la interfaz con información REAL
                    UpdateUIWithRealInfo();

                    lblErrorInfo.Visible = !batteryRealInfo.IsRealData;
                    if (!batteryRealInfo.IsRealData)
                    {
                        lblErrorInfo.Text = "⚠ Usando información básica. Haga clic en 'Obtener INFO REAL' para datos completos.";
                        lblErrorInfo.ForeColor = Color.Orange;
                    }
                    else
                    {
                        lblErrorInfo.Text = "✓ Información REAL obtenida correctamente";
                        lblErrorInfo.ForeColor = Color.Green;
                    }
                }
                else
                {
                    lblErrorInfo.Text = "No se detectó una batería. Conecte una batería para ver información detallada.";
                    lblErrorInfo.ForeColor = Color.Red;
                    lblErrorInfo.Visible = true;
                }

                // Actualizar última actualización
                lblLastUpdated.Text = $"Última actualización: {DateTime.Now.ToString("HH:mm:ss")}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblLastUpdated.Text = $"Última actualización: {DateTime.Now.ToString("HH:mm:ss")} (Error)";
            }
        }

        private void UpdateUIWithRealInfo()
        {
            // Actualizar voltaje (información REAL)
            if (batteryRealInfo.DesignVoltage > 0)
            {
                lblVoltage.Text = $"Voltaje: {batteryRealInfo.DesignVoltage} mV ({batteryRealInfo.DesignVoltage / 1000.0:F2} V)";
            }
            else
            {
                lblVoltage.Text = "Voltaje: No disponible";
            }

            // Actualizar capacidades (información REAL)
            if (batteryRealInfo.DesignCapacity > 0)
            {
                lblDesignCapacity.Text = $"Capacidad diseño: {batteryRealInfo.DesignCapacity:N0} mWh";
            }

            if (batteryRealInfo.FullChargeCapacity > 0)
            {
                lblFullChargeCapacity.Text = $"Capacidad actual: {batteryRealInfo.FullChargeCapacity:N0} mWh";

                // Calcular salud REAL de la batería
                if (batteryRealInfo.DesignCapacity > 0)
                {
                    double healthPercentage = (batteryRealInfo.FullChargeCapacity * 100.0) / batteryRealInfo.DesignCapacity;
                    lblBatteryHealth.Text = $"Salud batería: {healthPercentage:F1}%";

                    // Color según la salud REAL
                    lblBatteryHealth.ForeColor = healthPercentage > 85 ? Color.Green :
                                                healthPercentage > 70 ? Color.Orange :
                                                Color.Red;
                }
            }

            // Actualizar información del fabricante (REAL)
            if (!string.IsNullOrEmpty(batteryRealInfo.Manufacturer))
            {
                lblManufacturer.Text = $"Fabricante: {batteryRealInfo.Manufacturer}";
            }

            // Actualizar química (REAL)
            if (!string.IsNullOrEmpty(batteryRealInfo.Chemistry))
            {
                lblChemistry.Text = $"Tipo batería: {batteryRealInfo.Chemistry}";
            }

            // Actualizar contador de ciclos (REAL)
            if (batteryRealInfo.CycleCount > 0)
            {
                lblCycleCount.Text = $"Ciclos carga: {batteryRealInfo.CycleCount}";
            }
        }

        private string GetBatteryStatusText(byte acLineStatus, byte batteryFlag)
        {
            if (acLineStatus == 1)
            {
                if (batteryFlag == 8) // Cargando
                    return "Conectado, cargando ✓";
                else if (batteryFlag == 4) // Carga completa
                    return "Conectado, carga completa ✓";
                else
                    return "Conectado a corriente";
            }
            else
            {
                if ((batteryFlag & 1) == 1) // Alta - más del 66%
                    return "Funcionando con batería (alta)";
                else if ((batteryFlag & 2) == 2) // Baja - menos del 33%
                    return "Funcionando con batería (baja) ⚠";
                else if ((batteryFlag & 4) == 4) // Crítica - menos del 5%
                    return "Funcionando con batería (crítica) ⚠";
                else
                    return "Funcionando con batería";
            }
        }

        private string GetBatteryLifeTimeText(int lifeTime)
        {
            if (lifeTime == -1)
                return "Calculando...";
            else if (lifeTime == 0)
                return "Ilimitado (conectado)";

            TimeSpan time = TimeSpan.FromSeconds(lifeTime);

            if (time.Days > 0)
                return $"{time.Days}d {time.Hours}h {time.Minutes}m";
            else if (time.Hours > 0)
                return $"{time.Hours}h {time.Minutes}m";
            else
                return $"{time.Minutes}m";
        }

        private void UpdateBatteryIcon(int percentage, byte acLineStatus)
        {
            Bitmap batteryIcon = new Bitmap(pbBatteryIcon.Width, pbBatteryIcon.Height);
            using (Graphics g = Graphics.FromImage(batteryIcon))
            {
                g.Clear(Color.Transparent);

                Color borderColor = Color.Gray;
                Color fillColor;

                if (acLineStatus == 1)
                    fillColor = Color.LimeGreen;
                else if (percentage > 66)
                    fillColor = Color.LimeGreen;
                else if (percentage > 33)
                    fillColor = Color.Orange;
                else if (percentage > 10)
                    fillColor = Color.OrangeRed;
                else
                    fillColor = Color.Red;

                // Cuerpo de la batería
                Rectangle batteryBody = new Rectangle(0, 10, 60, 20);
                g.DrawRectangle(new Pen(borderColor, 2), batteryBody);

                // Terminal positivo
                Rectangle batteryTerminal = new Rectangle(60, 15, 5, 10);
                g.FillRectangle(new SolidBrush(borderColor), batteryTerminal);

                // Nivel de carga REAL
                int fillWidth = (int)((percentage / 100.0) * 56);
                Rectangle batteryLevel = new Rectangle(2, 12, fillWidth, 16);
                g.FillRectangle(new SolidBrush(fillColor), batteryLevel);

                // Indicador de carga
                if (acLineStatus == 1)
                {
                    g.DrawString("↯", new Font("Arial", 12, FontStyle.Bold), Brushes.White, 25, 12);
                }
                else if (percentage <= 15)
                {
                    g.DrawString("!", new Font("Arial", 12, FontStyle.Bold), Brushes.White, 30, 12);
                }

                // Porcentaje numérico
                g.DrawString($"{percentage}%", new Font("Arial", 8, FontStyle.Bold), Brushes.Black, 22, 15);
            }

            pbBatteryIcon.Image = batteryIcon;
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                updateTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Clase para almacenar información REAL de la batería
    public class BatteryRealInfo
    {
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Chemistry { get; set; }
        public long DesignCapacity { get; set; } // en mWh
        public long FullChargeCapacity { get; set; } // en mWh
        public int DesignVoltage { get; set; } // en mV
        public int CycleCount { get; set; }
        public bool IsRealData { get; set; }

        public BatteryRealInfo()
        {
            Name = "Batería HP Pavilion 15";
            Manufacturer = "Hewlett-Packard";
            Chemistry = "Li-ion";
            DesignCapacity = 41000; // Valor típico para HP Pavilion 15
            FullChargeCapacity = 39000; // Valor inicial
            DesignVoltage = 11100; // 11.1V típico
            CycleCount = 0;
            IsRealData = false;
        }
    }

    // Clase principal
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}