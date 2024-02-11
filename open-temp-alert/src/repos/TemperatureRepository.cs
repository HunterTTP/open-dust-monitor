using System;
using System.IO;
using open_temp_alert.models;

namespace open_temp_alert.repos
{
    public class TemperatureRepository
    {
        private readonly string _pathToTemperatureHistoryCsv;

        public TemperatureRepository()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _pathToTemperatureHistoryCsv = Path.Combine(baseDirectory, "TemperatureHistory.csv");
            EnsureTemperatureHistoryCsvExists();
        }

        private void EnsureTemperatureHistoryCsvExists()
        {
            if (!File.Exists(_pathToTemperatureHistoryCsv))
            {
                try
                {
                    var newTemperatureHistoryCsv = File.Create(_pathToTemperatureHistoryCsv);
                    var historyCsvWriter = new StreamWriter(newTemperatureHistoryCsv);
                    historyCsvWriter.WriteLine("Timestamp,CpuName,SensorName,Temperature");
                    historyCsvWriter.Close();
                }
                catch (Exception ex)
                {
                    throw new FileLoadException("Could not create TemperatureHistoryCsv: " + ex);
                }
            }
        }

        public void SaveTemperatureSnapshot(TemperatureSnapshot temperatureSnapshot)
        {
            using (var csvAppender = File.AppendText(_pathToTemperatureHistoryCsv))
            {
                csvAppender.WriteLine(temperatureSnapshot.GetAsCsvRow());
            }
        }
    }
}