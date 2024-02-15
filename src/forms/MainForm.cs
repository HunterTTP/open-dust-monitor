using LiveCharts.Defaults;
using LiveCharts.Wpf;
using open_dust_monitor.models;
using open_dust_monitor.services;
using open_dust_monitor.src.Handler;
using System.Runtime.InteropServices;
using System.Timers;

namespace open_dust_monitor.forms
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public readonly TemperatureService _temperatureService;
        private readonly LiveCharts.WinForms.CartesianChart _temperatureChart;

        public MainForm()
        {
            InitializeComponent();
            _temperatureService = new TemperatureService();
            _temperatureChart = CreateTemperatureChart();
        }

        private void MainForm_Load_1(object sender, EventArgs e)
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
            UpdateFormWithCpuInfo();
        }


        private void SnapshotButton_Click(object sender, EventArgs e)
        {
            UpdateFormWithCpuInfo();
        }

        private void UpdateFormWithCpuInfo()
        {
            var latestTemperatureSnapshot = _temperatureService.GetLatestTemperatureSnapshot();
            UpdateGridView(latestTemperatureSnapshot);
            UpdateTemperatureChart(latestTemperatureSnapshot);
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

        private void UpdateTemperatureChart(TemperatureSnapshot snapshot)
        {
            var temperatureSeries = _temperatureChart.Series[0] as LineSeries;
            var loadSeries = _temperatureChart.Series[1] as LineSeries;
            if (temperatureSeries != null)
            {
                var point = new DateTimePoint(snapshot.Timestamp, snapshot.CpuPackageTemperature);
                temperatureSeries.Values.Add(point);
                _temperatureChart.Update(true, true);
            }
            if (loadSeries != null)
            {
                var point = new DateTimePoint(snapshot.Timestamp, snapshot.CpuPackageUtilization);
                loadSeries.Values.Add(point);
                _temperatureChart.Update(true, true);
            }
        }

        private LiveCharts.WinForms.CartesianChart CreateTemperatureChart()
        {
            var cartesianChart1 = ChartHandler.CreateTemperatureChart(panel1.Height, panel1.Width);
            this.panel1.Controls.Add(cartesianChart1);
            return cartesianChart1;
        }

        private void TitleBar_Drag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0);
            }
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

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }
    }
}