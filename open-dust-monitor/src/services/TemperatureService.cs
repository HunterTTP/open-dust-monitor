using System;
using System.Linq;
using LibreHardwareMonitor.Hardware;
using open_temp_alert.models;
using open_temp_alert.repos;

namespace open_temp_alert.services
{
    public class TemperatureService
    {
        private readonly Computer _computer;
        private readonly IHardware _cpu;
        private readonly ISensor _cpuPackageTempSensor;
        private readonly TemperatureRepository _temperatureRepository;

        public TemperatureService()
        {
            _computer = FindComputerHardwareList();
            _cpu = FindCpu(_computer);
            _cpuPackageTempSensor = FindCpuPackageTempSensor(_cpu);
            _temperatureRepository = new TemperatureRepository();
        }

        public TemperatureSnapshot GetLatestTemperatureSnapshot()
        {
            var latestTemperatureSnapshot = new TemperatureSnapshot(
                DateTime.Now,
                _cpu.Name,
                _cpuPackageTempSensor.Name,
                _cpuPackageTempSensor.Value.GetValueOrDefault(0)
            );
            _temperatureRepository.SaveTemperatureSnapshot(latestTemperatureSnapshot);
            return latestTemperatureSnapshot;
        }

        public int GetTotalTemperatureSnapshotCount()
        {
            return _temperatureRepository.GetAllTemperatureSnapshots().Count;
        }

        public void StopTemperatureMonitoring()
        {
            _computer.Close();
        }

        public bool IsRecentAverageTemperatureWithinThreshold()
        {
            return GetRecentAverageTemperature() <= GetAlertThresholdTemperature();
        }

        public float GetAlertThresholdTemperature()
        {
            return (float)Math.Round(GetBaselineTemperature() * 1.15f);
        }

        private float GetBaselineTemperature()
        {
            var temperatureSnapshots = _temperatureRepository.GetAllTemperatureSnapshots();
            var endDate = temperatureSnapshots.Min(snapshot => snapshot.Timestamp).AddDays(7);
            return temperatureSnapshots
                .Where(snapshot => snapshot.Timestamp <= endDate)
                .Select(snapshot => snapshot.Temperature)
                .DefaultIfEmpty(0)
                .Average();
        }

        public float GetRecentAverageTemperature()
        {
            var temperatureSnapshots = _temperatureRepository.GetAllTemperatureSnapshots();
            var endDate = temperatureSnapshots.Max(snapshot => snapshot.Timestamp).AddDays(-3);
            var recentAverageTemperature = temperatureSnapshots
                .Where(snapshot => snapshot.Timestamp >= endDate)
                .Select(snapshot => snapshot.Temperature)
                .DefaultIfEmpty(0)
                .Average();
            return (float)Math.Round(recentAverageTemperature);
        }

        private static Computer FindComputerHardwareList()
        {
            var computer = new Computer { IsCpuEnabled = true };
            computer.Open();
            return computer;
        }

        private static IHardware FindCpu(IComputer computer)
        {
            var cpu = computer.Hardware.Where(IsCpu).First();
            if (cpu == null)
            {
                throw new InvalidOperationException("No CPU found.");
            }

            return cpu;
        }

        private static bool IsCpu(IHardware hardwareItem)
        {
            return hardwareItem.HardwareType.Equals(HardwareType.Cpu);
        }

        private static ISensor FindCpuPackageTempSensor(IHardware cpu)
        {
            var cpuPackageTempSensor = cpu.Sensors.Where(IsCpuPackageTemp).First();
            if (cpuPackageTempSensor == null)
            {
                throw new InvalidOperationException("No CPU Package Temp sensor found.");
            }

            return cpuPackageTempSensor;
        }

        private static bool IsCpuPackageTemp(ISensor sensor)
        {
            return sensor.SensorType == SensorType.Temperature && sensor.Name.Equals("CPU Package");
        }
    }
}