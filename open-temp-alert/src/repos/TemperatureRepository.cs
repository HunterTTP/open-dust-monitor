using System;
using System.Collections.Generic;
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
            _pathToTemperatureHistoryCsv = Path.Combine(baseDirectory, "temperature_history.csv");
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

        public List<TemperatureSnapshot> GetAllTemperatureSnapshots()
        {
            var temperatureSnapshots = new List<TemperatureSnapshot>();
            using (var reader = new StreamReader(_pathToTemperatureHistoryCsv))
            {
                reader.ReadLine(); //skip header
                string csvRow;
                while ((csvRow = reader.ReadLine()) != null)
                {
                    var csvRowValues = csvRow.Split(',');
                    var temperatureSnapshot = MapCsvRowToTemperatureSnapshot(csvRowValues);
                    temperatureSnapshots.Add(temperatureSnapshot);
                }
            }

            return temperatureSnapshots;
        }

        private static TemperatureSnapshot MapCsvRowToTemperatureSnapshot(string[] csvRowValues)
        {
            return new TemperatureSnapshot(
                DateTime.Parse(csvRowValues[0]),
                csvRowValues[1],
                csvRowValues[2],
                float.Parse(csvRowValues[3]));
        }
    }
}