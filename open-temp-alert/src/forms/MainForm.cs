using System;
using System.Timers;
using System.Windows.Forms;
using open_temp_alert.services;

namespace open_temp_alert.forms
{
    public partial class MainForm : Form
    {
        private readonly HardwareService _hardwareService;

        public MainForm()
        {
            InitializeComponent();
            FormClosing += MainForm_FormClosing;
            _hardwareService = new HardwareService();
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
            var cpu = _hardwareService.GetCpu();
            var cpuPackageTemp = _hardwareService.GetCpuPackageTemp();
            label1.Text = "CPU: " + cpu.Name +
                          "\nTemperature: " + cpuPackageTemp + "°C" +
                          "\nRefreshed on: " + DateTime.Now;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _hardwareService.CloseHardwareMonitoring();
        }
    }
}