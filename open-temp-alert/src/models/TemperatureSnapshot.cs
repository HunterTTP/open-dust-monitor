using System;

namespace open_temp_alert.models
{
    public class TemperatureSnapshot
    {
        public DateTime Timestamp { get; }
        public string CpuName { get; }
        public string SensorName { get; }
        public float Temperature { get; }

        public TemperatureSnapshot(DateTime timestamp, string cpuName, string sensorName, float temperature)
        {
            Timestamp = timestamp;
            CpuName = cpuName;
            SensorName = sensorName;
            Temperature = temperature;
        }
    }
}