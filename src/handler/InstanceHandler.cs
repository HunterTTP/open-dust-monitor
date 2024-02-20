using open_dust_monitor.src.repositories;
using open_dust_monitor.src.services;

namespace open_dust_monitor.src.handler
{
    public class InstanceHandler
    {
        private static HardwareService? _hardwareService;
        private static TemperatureService? _temperatureService;
        private static TemperatureRepository? _temperatureRepository;
        private static SettingsHandler? _settingsHandler;

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

        public static SettingsHandler GetSettingsHandler()
        {
            _settingsHandler ??= new SettingsHandler();
            LogHandler.Logger.Debug("GetSettingsHandler complete");
            return _settingsHandler;
        }
    }
}