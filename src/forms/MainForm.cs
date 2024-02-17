using open_dust_monitor.models;
using open_dust_monitor.services;
using open_dust_monitor.src.Handler;
using System.Timers;

namespace open_dust_monitor.forms
{
    public partial class MainForm : Form
    {
        private readonly TemperatureService _temperatureService;

        public MainForm()
        {
            InitializeComponent();
            _temperatureService = InstanceHandler.GetTemperatureService();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateFormWithCpuInfo();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
        }

        private void SnapshotTimer_Elapse(object sender, ElapsedEventArgs e)
        {
            //UpdateFormWithCpuInfo();
        }

        private void UpdateFormWithCpuInfo()
        {
            var latestTemperatureSnapshot = _temperatureService.GetLatestTemperatureSnapshot();
            UpdateGridView(latestTemperatureSnapshot);
            AlertIfTemperatureIsOutsideThreshold();
        }

        private void UpdateGridView(TemperatureSnapshot snapshot)
        {
            AddOrUpdateRowInDataGridView(0, "CPU", snapshot.CpuName);
            AddOrUpdateRowInDataGridView(1, "Temperature", snapshot.CpuPackageTemperature + "°C");
            AddOrUpdateRowInDataGridView(2, "Load", snapshot.CpuPackageUtilization + "%");
            AddOrUpdateRowInDataGridView(3, "alertThresholdTemperature", _temperatureService.GetAlertThresholdTemperature() + "°C");
            AddOrUpdateRowInDataGridView(4, "recentAverageTemperature", _temperatureService.GetRecentAverageTemperature() + "°C");
            AddOrUpdateRowInDataGridView(5, "temperatureAverageIsOk", _temperatureService.IsRecentAverageTemperatureWithinThreshold().ToString());
            AddOrUpdateRowInDataGridView(6, "Timestamp", snapshot.Timestamp.ToString());
            AddOrUpdateRowInDataGridView(7, "Interval", timer1.Interval / 1000 + " seconds");
            AddOrUpdateRowInDataGridView(8, "Total Snpashots", _temperatureService.GetTotalTemperatureSnapshotCount().ToString());
        }

        private void AddOrUpdateRowInDataGridView(int rowIndex, string value1, string value2)
        {
            if (rowIndex >= 0 && rowIndex < dataGridView1.Rows.Count)
            {
                dataGridView1.Rows[rowIndex].Cells[0].Value = value1;
                dataGridView1.Rows[rowIndex].Cells[1].Value = value2;
            }
            else
            {
                var newRow = new DataGridViewRow();
                newRow.CreateCells(dataGridView1);
                newRow.Cells[0].Value = value1;
                newRow.Cells[1].Value = value2;
                dataGridView1.Rows.Add(newRow);
            }
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
                    MessageBoxOptions.DefaultDesktopOnly);
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            _temperatureService.StopTemperatureMonitoring();
            this.Close();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
        }

        private void SysTray_Click(object sender, MouseEventArgs e)
        {
            if (this.Visible == false)
            {
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                this.Visible = true;
            }
            else
            {
                this.Visible = false;
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
            }
        }
    }
}