using System;
using System.IO;

namespace open_temp_alert.repos
{
    public class TemperatureRepository
    {
        private readonly string _pathToTemperatureHistoryCsv;

        public TemperatureRepository()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _pathToTemperatureHistoryCsv = Path.Combine(baseDirectory, "TemperatureHistory.csv");
        }

        public void EnsureTemperatureHistoryCsvExists()
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
                    throw new FileLoadException("Could not create TemperatureHistoryCsv");
                }
            }
        }
    }
}