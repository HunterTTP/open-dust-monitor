using System;
using System.Timers;
using System.Windows.Forms;
using LiveCharts.Wpf;
using LiveCharts;
using open_dust_monitor.services;
using System.Runtime.InteropServices;

namespace open_dust_monitor.forms
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private readonly TemperatureService _temperatureService;
        private readonly LiveCharts.WinForms.CartesianChart temperatureChart;

        public MainForm()
        {
            InitializeComponent();
            _temperatureService = new TemperatureService();
            temperatureChart = CreateTemperatureChart();
        }

        private void MainForm_Load_1(object sender, EventArgs e)
        {
            UpdateFormWithCpuInfo();
            CreateTemperatureChart();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
        }

        private void NotifyIcon1_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
        }

        private void timer1_Elapsed_1(object sender, ElapsedEventArgs e)
        {
            UpdateFormWithCpuInfo();
            temperatureChart.Series[1].Values.Add(30d);
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
            temperatureChart.Series[1].Values.Add(30d);
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

        private LiveCharts.WinForms.CartesianChart CreateTemperatureChart()
        {
            LiveCharts.WinForms.CartesianChart cartesianChart1 = new()
            {
                Width = panel1.Width,
                Height = panel1.Height,
                Visible = false,
                Series = new SeriesCollection
              {
                new LineSeries
                {
                  Title = "Series 1",
                  Values = new ChartValues<double> {4, 6, 5, 2, 7}
                },
                new LineSeries
                {
                  Title = "Series 2",
                  Values = new ChartValues<double> {6, 7, 3, 4, 6},
                  PointGeometry = null
                },
                new LineSeries
                {
                  Title = "Series 3",
                  Values = new ChartValues<double> {5, 2, 8, 3},
                  PointGeometry = DefaultGeometries.Square,
                  PointGeometrySize = 15
                }
              }
            };

            cartesianChart1.AxisX.Add(new Axis
            {
                Title = "Month",
                Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" }
            });

            cartesianChart1.AxisY.Add(new Axis
            {
                Title = "Temperature",
                LabelFormatter = value => value.ToString("C")
            });

            cartesianChart1.LegendLocation = LegendLocation.Right;

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
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.Visible = true;
        }
    }
}