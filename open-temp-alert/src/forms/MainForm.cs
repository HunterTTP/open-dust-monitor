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

        private void UpdateFormWithCpuInfo()
        {
            var currentTemp = _temperatureService.GetLatestTemperatureSnapshot();
            label1.Text = "CpuName: " + currentTemp.CpuName +
                          "\nSensorName: " + currentTemp.SensorName +
                          "\nTemperature: " + currentTemp.Temperature + "°C" +
                          "\nTimestamp: " + currentTemp.Timestamp;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _temperatureService.StopTemperatureMonitoring();
        }
    }
}