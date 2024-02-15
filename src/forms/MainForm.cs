using System;
using System.Timers;
using System.Windows.Forms;
using LiveCharts.Wpf;
using LiveCharts;
using open_dust_monitor.services;
using System.Runtime.InteropServices;
using LiveCharts.Defaults;
using open_dust_monitor.models;

namespace open_dust_monitor.forms
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private readonly TemperatureService _temperatureService;
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

        private void timer1_Elapsed_1(object sender, ElapsedEventArgs e)
        {
            UpdateFormWithCpuInfo();
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

        private void CartesianChart1OnDataClick(object sender, ChartPoint chartPoint)
        {
            MessageBox.Show("You clicked (" + chartPoint.X + "," + chartPoint.Y + ")");
        }

        private void button1_Click_1(object sender, EventArgs e)
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
            AddOrUpdateRowInDataGridView(2, "Load", snapshot.CpuPackageTemperature + "%");
            AddOrUpdateRowInDataGridView(3, "alertThresholdTemperature", _temperatureService.GetAlertThresholdTemperature() + "°C");
            AddOrUpdateRowInDataGridView(4, "recentAverageTemperature", _temperatureService.GetRecentAverageTemperature() + "°C");
            AddOrUpdateRowInDataGridView(5, "temperatureAverageIsOk", _temperatureService.IsRecentAverageTemperatureWithinThreshold().ToString());
            AddOrUpdateRowInDataGridView(6, "Timestamp", snapshot.Timestamp.ToString());
            AddOrUpdateRowInDataGridView(7, "Interval", timer1.Interval / 60000 + " minutes");
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
                    MessageBoxOptions.DefaultDesktopOnly
                );
        }
        
        private void UpdateTemperatureChart(TemperatureSnapshot snapshot)
        {

            var temperatureSeries = _temperatureChart.Series[0] as LineSeries;
            var loadSeries = _temperatureChart.Series[1] as LineSeries;

            if (temperatureSeries != null)
            {
                var point = new DateTimePoint(snapshot.Timestamp, snapshot.CpuPackageTemperature);
                temperatureSeries.Values.RemoveAt(0);
                temperatureSeries.Values.Add(point);
                _temperatureChart.Update(true, true);
            }

            if (loadSeries != null)
            {
                var point = new DateTimePoint(snapshot.Timestamp, snapshot.CpuPackageUtilization);
                loadSeries.Values.RemoveAt(0);
                loadSeries.Values.Add(point);
                _temperatureChart.Update(true, true);
            }

        }

        private LiveCharts.WinForms.CartesianChart CreateTemperatureChart()
        {
            var temperatureSnapshots = _temperatureService.GetRecentSnaptshots();

            LiveCharts.WinForms.CartesianChart cartesianChart1 = new()
            {
                Width = panel1.Width,
                Height = panel1.Height,
                Visible = false,
                Series = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Temperature",
                Values = new ChartValues<DateTimePoint>(temperatureSnapshots.Select(snapshot => new DateTimePoint(snapshot.Timestamp, snapshot.CpuPackageTemperature)))
            },
            new LineSeries
            {
                Title = "Utilization",
                Values = new ChartValues<DateTimePoint>(temperatureSnapshots.Select(snapshot => new DateTimePoint(snapshot.Timestamp, snapshot.CpuPackageUtilization)))
            }
        }
            };

            cartesianChart1.AxisX.Add(new Axis
            {
                Title = "Time",
                LabelFormatter = value => new DateTime((long)value).ToString("MMM dd")
            });

            cartesianChart1.AxisY.Add(new Axis
            {
                Title = "Temperature",
                LabelFormatter = value => value.ToString("N0") + "°"
            });

            cartesianChart1.DataClick += CartesianChart1OnDataClick;

            this.panel1.Controls.Clear();
            this.panel1.Controls.Add(cartesianChart1);
            cartesianChart1.LegendLocation = LegendLocation.None;
            cartesianChart1.Visible = true;

            return cartesianChart1;
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _temperatureService.StopTemperatureMonitoring();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
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

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}