namespace open_dust_monitor.models
{
    public class TemperatureSnapshot(
        DateTime timestamp,
        string cpuName,
        float cpuPackageUtilization,
        float cpuPackageTemperature)
    {
        public DateTime Timestamp { get; } = timestamp;
        public string CpuName { get; } = cpuName;
        public float CpuPackageUtilization { get; } = cpuPackageUtilization;
        public float CpuPackageTemperature { get; } = cpuPackageTemperature;

        public static string GetCsvRowHeaders()
        {
            return "Timestamp,CpuName,CpuPackageUtilization,CpuPackageTemperature";
        }

        public string GetAsCsvRow()
        {
            return Timestamp + "," + CpuName + "," + CpuPackageUtilization + "," + CpuPackageTemperature;
        }
    }
}