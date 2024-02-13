using System;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace open_dust_monitor.services
{
    public class HardwareService
    {
        private readonly Computer _thisComputer;
        private readonly IHardware _thisCpu;
        private readonly ISensor _thisCpuLoadSensor;
        private readonly ISensor _thisCpuTemperatureSensor;

        public HardwareService()
        {
            _thisComputer = FindComputerHardwareList();
            _thisCpu = FindCpu(_thisComputer);
            _thisCpuTemperatureSensor = FindCpuPackageTempSensor(_thisCpu);
            _thisCpuLoadSensor = FindCpuPackageLoadSensor(_thisCpu);
        }

        public string GetCpuName()
        {
            return _thisCpu.Name;
        }

        public float GetCurrentCpuTemperature()
        {
            return (float)Math.Round(_thisCpuTemperatureSensor.Value.GetValueOrDefault(0));
        }

        public float GetCurrentCpuLoad()
        {
            return (float)Math.Round(_thisCpuLoadSensor.Value.GetValueOrDefault(0));
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
            if (cpu == null) throw new InvalidOperationException("No CPU found.");

            return cpu;
        }

        private static bool IsCpu(IHardware hardwareItem)
        {
            return hardwareItem.HardwareType.Equals(HardwareType.Cpu);
        }

        private static ISensor FindCpuPackageTempSensor(IHardware cpu)
        {
            var cpuPackageTempSensor = cpu.Sensors.Where(IsCpuPackageTempSensor).First();
            if (cpuPackageTempSensor == null) throw new Exception("No CPU Temperature sensor found.");

            return cpuPackageTempSensor;
        }

        private static bool IsCpuPackageTempSensor(ISensor sensor)
        {
            return sensor.SensorType == SensorType.Temperature
                   && (sensor.Name == "CPU Package" || sensor.Name == "CPU Total");
        }

        private static ISensor FindCpuPackageLoadSensor(IHardware cpu)
        {
            var cpuPackageLoadSensor = cpu.Sensors.Where(IsCpuLoadSensor).First();
            if (cpuPackageLoadSensor == null) throw new Exception("No CPU Load sensor found.");

            return cpuPackageLoadSensor;
        }

        private static bool IsCpuLoadSensor(ISensor sensor)
        {
            return sensor.SensorType == SensorType.Load
                   && (sensor.Name == "CPU Package" || sensor.Name == "CPU Total");
        }

        public void StopHardwareMonitoring()
        {
            _thisComputer.Close();
        }
    }
}