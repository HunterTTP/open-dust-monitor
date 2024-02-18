using open_dust_monitor.services;
using open_dust_monitor.src.Handler;

namespace open_dust_monitor.src.forms
{
    public partial class MainForm : Form
    {
        private readonly TemperatureService _temperatureService;
        private readonly Label MainLabel = new();
        private readonly System.Windows.Forms.Timer MainTimer = new();
        private readonly NotifyIcon MainNotifyIcon = new();
        private readonly string iconPath = Path.Combine(Application.StartupPath, "images", "logo.ico");

        public MainForm()
        {
            _temperatureService = InstanceHandler.GetTemperatureService();
            InitializeComponent();
            InitializeUi();
        }

        private void InitializeUi()
        {
            ConfigureMainForm();
            AddMainLabel();
            AddMainNotifyIcon();
            AddMainTimer();
        }

        private void ConfigureMainForm()
        {
            this.Icon = new Icon(iconPath);
            this.Text = "Open Dust Monitor";
            this.ClientSize = new Size(450, 975);
            this.BackColor = SystemColors.ButtonHighlight;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Load += MainForm_Load;
            this.Resize += new EventHandler(MainForm_Minimize);
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
        }

        private void AddMainLabel()
        {
            MainLabel.AutoSize = true;
            MainLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MainLabel.Location = new Point(15, 20);
            MainLabel.Text = "Loading..";
            this.Controls.Add(MainLabel);
        }

        private void AddMainNotifyIcon()
        {
            MainNotifyIcon.Icon = new Icon(iconPath);
            MainNotifyIcon.Text = "Loading..";
            MainNotifyIcon.Visible = true;
            MainNotifyIcon.MouseDown += new MouseEventHandler(NotifyIcon_Clicked);
        }

        private void AddMainTimer()
        {
            MainTimer.Enabled = true;
            MainTimer.Interval = _temperatureService.GetSnapshotIntervalMillis();
            MainTimer.Tick += new EventHandler(Timer_Tick);
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