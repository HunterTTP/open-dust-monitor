using System;
using System.Timers;
using System.Windows.Forms;
using open_temp_alert.services;

namespace open_temp_alert.forms
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateFormWithCpuInfo();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _temperatureService.StopTemperatureMonitoring();
        }

        private void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateFormWithCpuInfo();
        }

        private void button1_Click(object sender, EventArgs e)
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
                          "\nlatestTemperature: " + latestTemperatureSnapshot.Temperature + "°C" +
                          "\nalertThresholdTemperature: " + alertThresholdTemperature + "°C" +
                          "\nrecentAverageTemperature: " + recentAverageTemperature + "°C" +
                          "\ntemperatureAverageIsOk: " + temperatureAverageIsOk +
                          "\nLatest Timestamp: " + latestTemperatureSnapshot.Timestamp +
                          "\nTotal Snapshots: " + totalSnapshots;
            AlertIfTemperatureIsOutsideThreshold();
        }

        private void AlertIfTemperatureIsOutsideThreshold()
        {
            if (!_temperatureService.IsRecentAverageTemperatureWithinThreshold())
            {
                MessageBox.Show(
                    "Your PC has running warmer than usual. Please clean the fans.",
                    "Open Temp Alert",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly
                );
            }
        }
    }
}