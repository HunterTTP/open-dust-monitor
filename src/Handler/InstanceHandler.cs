using open_dust_monitor.repositories;
using open_dust_monitor.services;

namespace open_dust_monitor.src.Handler
{
    public static class InstanceHandler
    {
        private static HardwareService? _hardwareService;
        private static TemperatureService? _temperatureService;
        private static TemperatureRepository? _temperatureRepository;

        public static HardwareService GetHardwareService()
        {
            if (_hardwareService == null) { _hardwareService = new HardwareService(); }
            return _hardwareService;
        }

        public static TemperatureService GetTemperatureService()
        {
            if (_temperatureService == null) { _temperatureService = new TemperatureService(); }
            return _temperatureService;
        }

        public static TemperatureRepository GetTemperatureRepository()
        {
            if (_temperatureRepository == null) { _temperatureRepository = new TemperatureRepository(); }
            return _temperatureRepository;
        }
    }
}