using System.Reflection;
using System.Text.Json;

namespace open_dust_monitor.src.Handler
{
    public class SettingsHandler
    {
        private static readonly string settingsFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "app.settings");
        public DateTime UserLastNotified { get; set; }

        public SettingsHandler()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (!File.Exists(settingsFilePath))
            {
                CreateDefaultSettingsFile();
                return;
            }

            string jsonString = File.ReadAllText(settingsFilePath);
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("UserLastNotified", out JsonElement userLastNotifiedElement))
                {
                    UserLastNotified = userLastNotifiedElement.GetDateTime();
                }
                else
                {
                    UserLastNotified = DateTime.MinValue;
                    LogHandler.Logger.Error("UserLastNotified setting could not be loaded.");
                }
            }
        }


        private void CreateDefaultSettingsFile()
        {
            UserLastNotified = DateTime.MinValue;
            SaveSettings();
        }

        public void SaveSettings()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText(settingsFilePath, jsonString);
        }

    }
}
