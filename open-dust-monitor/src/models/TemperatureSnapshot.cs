using System;

namespace open_dust_monitor.models
{
    public class TemperatureSnapshot
    {
        public DateTime Timestamp { get; }
        public string CpuName { get; }
        public float CpuPackageUtilization { get; }
        public float CpuPackageTemperature { get; }

        public TemperatureSnapshot(
            DateTime timestamp,
            string cpuName,
            float cpuPackageUtilization,
            float cpuPackageTemperature)
        {
            Timestamp = timestamp;
            CpuName = cpuName;
            CpuPackageUtilization = cpuPackageUtilization;
            CpuPackageTemperature = cpuPackageTemperature;
        }

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