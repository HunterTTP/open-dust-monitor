using open_dust_monitor.src.Handler;

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
            Application.Run(new src.forms.MainForm());
        }
    }
}