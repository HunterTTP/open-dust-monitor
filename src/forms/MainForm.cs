using open_dust_monitor.repositories;
using open_dust_monitor.services;
using open_dust_monitor.src.Handler;

namespace open_dust_monitor.src.forms
{
    public partial class MainForm : Form
    {
        private readonly TemperatureService _temperatureService = InstanceHandler.GetTemperatureService();
        private readonly Label LatestSnapshotLabel = new();
        private readonly Label RecentSnapshotsCountLabel = new();
        private readonly Label RecentSnapshotsTemperaturesLabel = new();
        private readonly Label BaselineSnapshotsCountLabel = new();
        private readonly Label AlertThresholdTemperaturesLabel = new();
        private readonly System.Windows.Forms.Timer MainTimer = new();
        private readonly NotifyIcon MainNotifyIcon = new();
        private readonly string iconPath = Path.Combine(Application.StartupPath, "images", "logo.ico");

        public MainForm()
        {
            InitializeComponent();
            InitializeClassInstances();
            InitializeUi();
        }

        private static void InitializeClassInstances()
        {
            InstanceHandler.CreateAllInstances();
        }

        private void InitializeUi()
        {
            ConfigureMainForm();
            AddLabelTable();
            AddMainNotifyIcon();
            AddMainTimer();
            LogHandler.Logger.Information("UI Initialized");
        }

        private void ConfigureMainForm()
        {
            this.Icon = new Icon(iconPath);
            this.Text = "Open Dust Monitor";
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.BackColor = Color.FromArgb(95, 95, 95);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Load += MainFormLoad;
            this.Resize += new EventHandler(MainForm_Minimize);
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
            LogHandler.Logger.Debug("ConfigureMainForm complete");
        }

        private void AddLabelTable()
        {
            var tableLayoutPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 5,
                Dock = DockStyle.Fill,
                AutoSize = true,
            };

            for (int i = 0; i < tableLayoutPanel.RowCount; i++)
            {
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            ConfigureLabelProperties(LatestSnapshotLabel, "Loading..");
            tableLayoutPanel.Controls.Add(LatestSnapshotLabel, 0, 0);

            ConfigureLabelProperties(RecentSnapshotsCountLabel, "Loading..");
            tableLayoutPanel.Controls.Add(RecentSnapshotsCountLabel, 0, 1);

            ConfigureLabelProperties(BaselineSnapshotsCountLabel, "Loading..");
            tableLayoutPanel.Controls.Add(BaselineSnapshotsCountLabel, 1, 1);

            ConfigureLabelProperties(RecentSnapshotsTemperaturesLabel, "Loading..");
            tableLayoutPanel.Controls.Add(RecentSnapshotsTemperaturesLabel, 0, 2);

            ConfigureLabelProperties(AlertThresholdTemperaturesLabel, "Loading..");
            tableLayoutPanel.Controls.Add(AlertThresholdTemperaturesLabel, 1, 2);

            this.Controls.Add(tableLayoutPanel);
            LogHandler.Logger.Debug("AddLabelTable complete");
        }

        private void ConfigureLabelProperties(Label label, string text)
        {
            label.AutoSize = true;
            label.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label.ForeColor = Color.White;
            label.Text = text;
            label.Padding = new Padding(10);
        }


        private void AddMainNotifyIcon()
        {
            MainNotifyIcon.Icon = new Icon(iconPath);
            MainNotifyIcon.Text = "Loading..";
            MainNotifyIcon.Visible = true;
            MainNotifyIcon.MouseDown += new MouseEventHandler(NotifyIcon_Clicked);
            LogHandler.Logger.Debug("AddMainNotifyIcon complete");
        }

        private void AddMainTimer()
        {
            MainTimer.Enabled = true;
            MainTimer.Interval = TemperatureService.GetSnapshotIntervalMillis();
            MainTimer.Tick += new EventHandler(TimerTick);
            LogHandler.Logger.Debug("AddMainTimer complete");
        }

        private void MainFormLoad(object? sender, EventArgs e)
        {
            _ = UpdateFormWithCpuInfo();
            LogHandler.Logger.Debug("MainFormLoad complete");
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            _ = UpdateFormWithCpuInfo();
        }

        private async Task UpdateFormWithCpuInfo()
        {
            var latestTemperatureSnapshot = await Task.Run(() => _temperatureService.GetLatestTemperatureSnapshot());
            var recentSnapshots = TemperatureRepository.GetLoadedRecentSnapshots();
            var baselineSnapshots = TemperatureRepository.GetLoadedBaselineSnapshots();
            LatestSnapshotLabel.Text = "Latest Snapshot: \n" + latestTemperatureSnapshot.ToString();
            RecentSnapshotsCountLabel.Text = TemperatureService.GetSnapshotCountsLabel(recentSnapshots, "Recent");
            RecentSnapshotsTemperaturesLabel.Text = TemperatureService.GetSnapshotTemperaturesLabel(recentSnapshots, "Recent");
            BaselineSnapshotsCountLabel.Text = TemperatureService.GetSnapshotCountsLabel(baselineSnapshots, "Baseline");
            AlertThresholdTemperaturesLabel.Text = TemperatureService.GetSnapshotAlertThresholdsLabel(baselineSnapshots);
            AlertIfTemperatureIsOutsideThreshold();
            LogHandler.Logger.Debug("UpdateFormWithCpuInfo complete");
        }

        private static void AlertIfTemperatureIsOutsideThreshold()
        {
            if (!TemperatureService.AreRecentAverageTemperaturesWithinThreshold())
            {
                MessageBox.Show(
                    "Your PC is running warmer than usual. Please clean the fans.",
                    "Open Dust Monitor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
                LogHandler.Logger.Warning("AlertIfTemperatureIsOutsideThreshold alerted for hot temperature");
            }

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