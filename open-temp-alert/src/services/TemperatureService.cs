using System;
using System.Linq;
using LibreHardwareMonitor.Hardware;
using open_temp_alert.models;

namespace open_temp_alert.services
{
    public class TemperatureService
    {
        private readonly Computer _computer;
        private readonly IHardware _cpu;
        private readonly ISensor _cpuPackageTempSensor;

        public TemperatureService()
        {
            _computer = FindComputerHardwareList();
            _cpu = FindCpu(_computer);
            _cpuPackageTempSensor = FindCpuPackageTempSensor(_cpu);
        }

        public IHardware GetCpu()
        {
            return _cpu;
        }

        public TemperatureSnapshot GetLatestTemperatureSnapshot()
        {
            return new TemperatureSnapshot(
                DateTime.Now,
                _cpu.Name,
                _cpuPackageTempSensor.Name,
                _cpuPackageTempSensor.Value.GetValueOrDefault(0)
            );
        }

        public float GetCpuPackageTemp()
        {
            _cpu.Update();
            return _cpuPackageTempSensor.Value.GetValueOrDefault(0);
        }

        public void StopTemperatureMonitoring()
        {
            _computer.Close();
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