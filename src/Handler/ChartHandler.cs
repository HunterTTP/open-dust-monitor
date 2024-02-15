using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace open_dust_monitor.src.Handler
{
    public static class ChartHandler
    {
        public static LiveCharts.WinForms.CartesianChart CreateTemperatureChart(int height, int width)
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
                Values = new ChartValues<DateTimePoint>()
            };
            var utilizationSeries = new LineSeries
            {
                Title = "Utilization",
                Values = new ChartValues<DateTimePoint>()
            };
            cartesianChart.Series.Add(temperatureSeries);
            cartesianChart.Series.Add(utilizationSeries);
            cartesianChart.AxisX.Add(new Axis
            {
                Title = "Time",
                LabelFormatter = value => new DateTime((long)value).ToString("HH:mm")
            });
            cartesianChart.AxisY.Add(new Axis
            {
                Title = "Temperature (°C)",
                LabelFormatter = value => value.ToString("N1") + "°C"
            });
            cartesianChart.LegendLocation = LegendLocation.None;
            cartesianChart.Visible = true;
            return cartesianChart;
        }
    }
}
