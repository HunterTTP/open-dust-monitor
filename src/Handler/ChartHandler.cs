using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using open_dust_monitor.models;

namespace open_dust_monitor.src.Handler
{
    public class ChartHandler
    {
        public static ChartValues<DateTimePoint> TemperatureSeries { get; set; } = new ChartValues<DateTimePoint>();
        public static ChartValues<DateTimePoint> LoadSeries { get; set; } = new ChartValues<DateTimePoint>();

        public static LiveCharts.WinForms.CartesianChart CreateTemperatureChart(int height, int width, TemperatureSnapshot temperatureSnapshot)
        {
            var cartesianChart = new LiveCharts.WinForms.CartesianChart
            {
                Width = width,
                Height = height,
                Visible = true
            };

            var temperatureSeries = new LineSeries
            {
                Title = "Temperature",
                Values = TemperatureSeries,
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 5,
            };

            var utilizationSeries = new LineSeries
            {
                Title = "Utilization",
                Values = LoadSeries,
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 5
            };
            
            TemperatureSeries.Add(new DateTimePoint(temperatureSnapshot.Timestamp, temperatureSnapshot.CpuPackageTemperature));
            LoadSeries.Add(new DateTimePoint(temperatureSnapshot.Timestamp, temperatureSnapshot.CpuPackageUtilization));
            
            cartesianChart.Series.Add(temperatureSeries);
            cartesianChart.Series.Add(utilizationSeries);

            cartesianChart.AxisX.Add(new Axis
            {
                LabelFormatter = value => new DateTime((long)value).ToString("HH:mm"),
                ShowLabels = false
            });

            cartesianChart.AxisY.Add(new Axis
            {
                LabelFormatter = value => value.ToString("N1"),
                MinValue = 0,
                MaxValue = 100
            });

            cartesianChart.LegendLocation = LegendLocation.None;
            cartesianChart.Visible = true;
            return cartesianChart;
        }
    }
}
