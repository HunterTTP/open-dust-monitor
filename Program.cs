using open_dust_monitor.src.handler;

namespace open_dust_monitor
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            LogHandler.ConfigureLogger();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.Run(new src.forms.MainForm());
        }
    }
}