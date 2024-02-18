using open_dust_monitor.services;
using open_dust_monitor.src.Handler;
using System.Resources;

namespace open_dust_monitor.src.forms
{
    public partial class MainForm : Form
    {
        private readonly TemperatureService _temperatureService;
        private Label MainLabel;
        private System.Windows.Forms.Timer MainTimer;
        private NotifyIcon MainNotifyIcon;

        public MainForm()
        {
            _temperatureService = InstanceHandler.GetTemperatureService();
            InitializeComponent();
            InitializeEventHandlers();
            InitializeUi();
        }

        private void InitializeEventHandlers()
        {
            CreateMainTimer();
            this.Resize += new EventHandler(MainForm_Minimize);
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
        }

        private void CreateMainTimer()
        {
            MainTimer = new System.Windows.Forms.Timer();
            MainTimer.Enabled = true;
            MainTimer.Interval = 2000;
            MainTimer.Tick += new EventHandler(Timer_Tick);
        }

        private void InitializeUi()
        {
            string iconPath = Path.Combine(Application.StartupPath, "images", "logo.ico");
            this.Icon = new Icon(iconPath);
            CreateMainLabel();
            CreateMainNotifyIcon();
        }

        private void CreateMainLabel()
        {
            MainLabel = new Label();
            MainLabel.AutoSize = true;
            MainLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MainLabel.Location = new Point(14, 14);
            MainLabel.Name = "label1";
            MainLabel.Size = new Size(72, 21);
            MainLabel.TabIndex = 0;
            MainLabel.Text = "Loading..";
            this.Controls.Add(MainLabel);
        }

        private void CreateMainNotifyIcon()
        {
            string iconPath = Path.Combine(Application.StartupPath, "images", "logo.ico");
            MainNotifyIcon = new NotifyIcon();
            MainNotifyIcon.Icon = new Icon(iconPath);
            MainNotifyIcon.Text = "Loading..";
            MainNotifyIcon.Visible = true;
            MainNotifyIcon.MouseDown += new MouseEventHandler(NotifyIcon_Clicked);
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            _ = UpdateFormWithCpuInfo();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _ = UpdateFormWithCpuInfo();
        }

        private async Task UpdateFormWithCpuInfo()
        {
            var latestTemperatureSnapshot = await Task.Run(() => _temperatureService.GetLatestTemperatureSnapshot());
            var snapshotLabel = await Task.Run(() => _temperatureService.GetTemperatureSnapshotLabel(latestTemperatureSnapshot, MainTimer.Interval));
            MainLabel.Text = snapshotLabel;
            AlertIfTemperatureIsOutsideThreshold();
        }

        private void AlertIfTemperatureIsOutsideThreshold()
        {
            if (!_temperatureService.AreRecentAverageTemperaturesNormal())
                MessageBox.Show(
                    "Your PC is running warmer than usual. Please clean the fans.",
                    "Open Dust Monitor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
        }

        private void MainForm_Minimize(object? sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _temperatureService.StopTemperatureMonitoring();
        }

        private void NotifyIcon_Clicked(object? sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }
    }
}