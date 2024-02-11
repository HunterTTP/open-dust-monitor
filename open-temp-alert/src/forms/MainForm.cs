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

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateFormWithCpuInfo();
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
            var latestTemperature = latestTemperatureSnapshot.Temperature;
            var totalTemperatureSnapshots = _temperatureService.GetTotalTemperatureSnapshotCount();
            var alertThresholdTemperature = _temperatureService.GetAlertThresholdTemperature();
            var isTemperatureWithinThreshold = _temperatureService.IsTemperatureInsideAlertThreshold(latestTemperature);
            label1.Text = "CpuName: " + latestTemperatureSnapshot.CpuName +
                          "\nCurrent Temp: " + latestTemperatureSnapshot.Temperature + "°C" +
                          "\nAlert Threshold: " + alertThresholdTemperature + "°C" +
                          "\nTemp is normal: " + isTemperatureWithinThreshold +
                          "\nTimestamp: " + latestTemperatureSnapshot.Timestamp +
                          "\nSaved Snapshots: " + totalTemperatureSnapshots;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _temperatureService.StopTemperatureMonitoring();
        }
    }
}