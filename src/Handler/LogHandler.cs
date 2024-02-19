using Serilog;

namespace open_dust_monitor.src.Handler
{
    internal class LogHandler
    {
        private static ILogger _logger = null!;

        public static void ConfigureLogger()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.File("logs/app.log-.txt",
                              rollingInterval: RollingInterval.Day,
                              retainedFileCountLimit: 3,
                              rollOnFileSizeLimit: true)
                .CreateLogger();
        }

        public static ILogger Logger => _logger;
    }
}
