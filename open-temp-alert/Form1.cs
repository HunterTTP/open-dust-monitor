using System;
using System.Timers;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;

namespace open_temp_alert
{
    public partial class Form1 : Form
    {
        private Computer _computer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _computer = GetAllComputerHardware();
            UpdateFormWithCpuInfo();
        }

        private void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateFormWithCpuInfo();
        }

        private void UpdateFormWithCpuInfo()
        {
            var cpu = GetCpu(_computer);
            var cpuPackageTemp = GetCpuPackageTemp(cpu);
            label1.Text = "CPU: " + cpu.Name +
                          "\nTemperature: " + cpuPackageTemp + @"°C" +
                          "\nRefreshed on: " + DateTime.Now;
        }

        private static Computer GetAllComputerHardware()
        {
            Computer computer = new Computer
            {
                IsCpuEnabled = true
            };
            computer.Open();
            return computer;
        }

        private static IHardware GetCpu(Computer computer)
        {
            foreach (var hardwareItem in computer.Hardware)
            {
                if (hardwareItem.HardwareType.Equals(HardwareType.Cpu))
                {
                    return hardwareItem;
                }
            }

            return null;
        }

        private static float GetCpuPackageTemp(IHardware hardwareItem)
        {
            hardwareItem.Update();
            foreach (var sensor in hardwareItem.Sensors)
            {
                if (IsCpuPackageTemp(sensor))
                {
                    return sensor.Value.GetValueOrDefault(0);
                }
            }

            return 0;
        }

        private static bool IsCpuPackageTemp(ISensor sensor)
        {
            var sensorName = sensor.Name;
            var sensorType = sensor.SensorType;
            return sensorType == SensorType.Temperature && sensorName.Equals("CPU Package");
        }

        private void Form1_Exit(object sender, EventArgs e)
        {
            _computer.Close();
        }
    }
}