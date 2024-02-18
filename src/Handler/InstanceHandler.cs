using open_dust_monitor.repositories;
using open_dust_monitor.services;

namespace open_dust_monitor.src.Handler
{
    public class InstanceHandler
    {
        private static HardwareService? _hardwareService;
        private static TemperatureService? _temperatureService;
        private static TemperatureRepository? _temperatureRepository;

        public static void CreateAllInstances()
        {
            _hardwareService = GetHardwareService();
            _temperatureService = GetTemperatureService();
            _temperatureRepository = GetTemperatureRepository();
            LogHandler.Logger.Information("CreateAllInstances complete");
        }

        public static HardwareService GetHardwareService()
        {
            _hardwareService ??= new HardwareService();
            LogHandler.Logger.Debug("GetHardwareService complete");
            return _hardwareService;
        }

        public static TemperatureService GetTemperatureService()
        {
            _temperatureService ??= new TemperatureService();
            LogHandler.Logger.Debug("GetTemperatureService complete");
            return _temperatureService;
        }

        public static TemperatureRepository GetTemperatureRepository()
        {
            _temperatureRepository ??= new TemperatureRepository();
            LogHandler.Logger.Debug("GetTemperatureRepository complete");
            return _temperatureRepository;
        }
    }
}