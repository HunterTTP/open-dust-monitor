using System;
using System.Timers;
using System.Windows.Forms;
using open_dust_monitor.services;

namespace open_dust_monitor.forms
{
    public partial class MainForm : Form
    {
        private readonly TemperatureService _temperatureService;

        public MainForm()
        {
            InitializeComponent();
            FormClosing += MainForm_FormClosing;
            _temperatureService = new TemperatureService();
        }

        private void MainForm_Load_1(object sender, EventArgs e)
        {
            UpdateFormWithCpuInfo();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _temperatureService.StopTemperatureMonitoring();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
        }

        private void timer1_Elapsed_1(object sender, ElapsedEventArgs e)
        {
            UpdateFormWithCpuInfo();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            UpdateFormWithCpuInfo();
        }

        private void UpdateFormWithCpuInfo()
        {
            var latestTemperatureSnapshot = _temperatureService.GetLatestTemperatureSnapshot();
            var alertThresholdTemperature = _temperatureService.GetAlertThresholdTemperature();
            var recentAverageTemperature = _temperatureService.GetRecentAverageTemperature();
            var temperatureAverageIsOk = _temperatureService.IsRecentAverageTemperatureWithinThreshold();
            var totalSnapshots = _temperatureService.GetTotalTemperatureSnapshotCount();
            label1.Text = "CpuName: " + latestTemperatureSnapshot.CpuName +
                          "\nlatestTemperature: " + latestTemperatureSnapshot.CpuPackageTemperature + "°C" +
                          "\nlatestLoad: " + latestTemperatureSnapshot.CpuPackageUtilization + "%" +
                          "\nalertThresholdTemperature: " + alertThresholdTemperature + "°C" +
                          "\nrecentAverageTemperature: " + recentAverageTemperature + "°C" +
                          "\ntemperatureAverageIsOk: " + temperatureAverageIsOk +
                          "\nLatest Timestamp: " + latestTemperatureSnapshot.Timestamp +
                          "\ntimer1_Interval: " + timer1.Interval / 60000 + " minutes" +
                          "\nTotal Snapshots: " + totalSnapshots;
            AlertIfTemperatureIsOutsideThreshold();
        }

        private void AlertIfTemperatureIsOutsideThreshold()
        {
            if (!_temperatureService.IsRecentAverageTemperatureWithinThreshold())
                MessageBox.Show(
                    "Your PC has running warmer than usual. Please clean the fans.",
                    "Open Dust Monitor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly
                );
        }
    }
}