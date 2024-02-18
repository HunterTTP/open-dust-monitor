namespace open_dust_monitor.models
{
    public class TemperatureSnapshot(
        DateTime timestamp,
        string cpuName,
        float cpuTemperature,
        float cpuLoad,
        string cpuLoadRange)
    {
        public DateTime Timestamp { get; } = timestamp;
        public string CpuName { get; } = cpuName;
        public float CpuTemperature { get; } = cpuTemperature;
        public float CpuLoad { get; } = cpuLoad;
        public string CpuLoadRange { get; } = cpuLoadRange;

        public static string GetCsvRowHeaders()
        {
            return "Timestamp,CpuName,CpuTemperature,CpuLoad,CpuLoadRange";
        }

        public string GetAsCsvRow()
        {
            return Timestamp + "," + CpuName + "," + CpuTemperature + "," + CpuLoad + "," + CpuLoadRange;
        }

        public override string ToString()
        {
            return "Timestamp: " + Timestamp
                + "\n CPU: " + CpuName
                + "\n Temperature: " + CpuTemperature + "°C"
                + "\n Utilization: " + CpuLoad + "%";
        }
    }
}