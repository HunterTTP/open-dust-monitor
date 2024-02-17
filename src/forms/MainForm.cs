using open_dust_monitor.services;
using open_dust_monitor.src.Handler;

namespace open_dust_monitor.src.forms
{
    public partial class MainForm : Form
    {
        private readonly TemperatureService _temperatureService;

        public MainForm()
        {
            InitializeComponent();
            _temperatureService = InstanceHandler.GetTemperatureService();
            this.Resize += new EventHandler(MainForm_Minimize);
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
            notifyIcon1.MouseDown += new MouseEventHandler(NotifyIcon_Clicked);
            timer1.Tick += new EventHandler(Timer_Tick);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _ = UpdateFormWithCpuInfo();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _ = UpdateFormWithCpuInfo();
        }

        private async Task UpdateFormWithCpuInfo()
        {
            var latestTemperatureSnapshot = await Task.Run(() => _temperatureService.GetLatestTemperatureSnapshot());
            var snapshotLabel = await Task.Run(() => _temperatureService.GetTemperatureSnapshotLabel(latestTemperatureSnapshot, timer1.Interval));
            label1.Text = snapshotLabel;
            AlertIfTemperatureIsOutsideThreshold();
        }

        private void AlertIfTemperatureIsOutsideThreshold()
        {
            if (!_temperatureService.IsRecentAverageTemperatureWithinThreshold())
                MessageBox.Show(
                    "Your PC is running warmer than usual. Please clean the fans.",
                    "Open Dust Monitor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
        }

        private void MainForm_Minimize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _temperatureService.StopTemperatureMonitoring();
        }

        private void NotifyIcon_Clicked(object sender, MouseEventArgs e)
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