using LibreHardwareMonitor.Hardware;
using open_dust_monitor.src.Handler;

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
            _thisComputer = FindComputerHardwareInfo();
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
            _thisCpu.Update();
            return (float)Math.Round(_thisCpuTemperatureSensor.Value.GetValueOrDefault(0));
        }

        public float GetCurrentCpuLoad()
        {
            _thisCpu.Update();
            return (float)Math.Round(_thisCpuLoadSensor.Value.GetValueOrDefault(0));
        }

        public static string GetCurrentCpuLoadRange(float cpuLoad)
        {
            if (cpuLoad <= 10)
            {
                return "idle";
            }
            else if (cpuLoad <= 30)
            {
                return "low";
            }
            else if (cpuLoad <= 70)
            {
                return "medium";
            }
            else if (cpuLoad <= 90)
            {
                return "high";
            }
            else
            {
                return "max";
            }
        }

        public ISensor GetCpuLoadSensor()
        {
            return _thisCpuLoadSensor;
        }

        private static Computer FindComputerHardwareInfo()
        {
            var computer = new Computer { IsCpuEnabled = true };
            computer.Open();
            return computer;
        }

        private static IHardware FindCpu(IComputer computer)
        {
            var cpu = computer.Hardware.Where(IsCpu).First() ?? throw new InvalidOperationException("No CPU found.");
            LogHandler.Logger.Information("FindCpu name=" + cpu.Name);
            return cpu;
        }

        private static bool IsCpu(IHardware hardwareItem)
        {
            return hardwareItem.HardwareType.Equals(HardwareType.Cpu);
        }

        private static ISensor FindCpuPackageTempSensor(IHardware cpu)
        {
            var cpuPackageTempSensor = cpu.Sensors.Where(IsCpuPackageTempSensor).First() ?? throw new Exception("No CPU Temperature sensor found.");
            LogHandler.Logger.Information("FindCpuPackageTempSensor name=" + cpuPackageTempSensor.Name);
            return cpuPackageTempSensor;
        }

        private static bool IsCpuPackageTempSensor(ISensor sensor)
        {
            return sensor.SensorType == SensorType.Temperature && (sensor.Name == "CPU Package" || sensor.Name == "CPU Total");
        }

        private static ISensor FindCpuPackageLoadSensor(IHardware cpu)
        {
            var cpuPackageLoadSensor = cpu.Sensors.Where(IsCpuLoadSensor).First() ?? throw new Exception("No CPU Load sensor found.");
            LogHandler.Logger.Information("FindCpuPackageLoadSensor name=" + cpuPackageLoadSensor.Name);
            return cpuPackageLoadSensor;
        }

        private static bool IsCpuLoadSensor(ISensor sensor)
        {
            return sensor.SensorType == SensorType.Load && (sensor.Name == "CPU Package" || sensor.Name == "CPU Total");
        }

        public void StopHardwareMonitoring()
        {
            _thisComputer.Close();
            LogHandler.Logger.Information("StopHardwareMonitoring complete");
        }
    }
}