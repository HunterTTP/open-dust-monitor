using open_dust_monitor.models;

namespace open_dust_monitor.repositories
{
    public class TemperatureRepository
    {
        private readonly string _pathToTemperatureHistoryCsv;
        private List<TemperatureSnapshot> _loadedSnapshots = [];

        public TemperatureRepository()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _pathToTemperatureHistoryCsv = Path.Combine(baseDirectory, "temperature_history.csv");
            EnsureTemperatureHistoryCsvExists();
            _loadedSnapshots.AddRange(GetAllTemperatureSnapshotsFromCsv());
        }

        private void EnsureTemperatureHistoryCsvExists()
        {
            if (!File.Exists(_pathToTemperatureHistoryCsv))
                try
                {
                    var newTemperatureHistoryCsv = File.Create(_pathToTemperatureHistoryCsv);
                    var historyCsvWriter = new StreamWriter(newTemperatureHistoryCsv);
                    historyCsvWriter.WriteLine(TemperatureSnapshot.GetCsvRowHeaders());
                    historyCsvWriter.Close();
                }
                catch (Exception ex)
                {
                    throw new FileLoadException("Could not create TemperatureHistoryCsv: " + ex);
                }
        }

        public void SaveTemperatureSnapshot(TemperatureSnapshot temperatureSnapshot)
        {
            _loadedSnapshots.Add(temperatureSnapshot);
            using (var csvAppender = File.AppendText(_pathToTemperatureHistoryCsv))
            {
                csvAppender.WriteLine(temperatureSnapshot.GetAsCsvRow());
            }
        }

        public List<TemperatureSnapshot> GetAllTemperatureSnapshotsFromCsv()
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
                float.Parse(csvRowValues[2]),
                float.Parse(csvRowValues[3]));
        }

        public List<TemperatureSnapshot> GetLoadedTemperatureSnapshots() { return _loadedSnapshots; }

        public int GetLoadedTemperatureSnapshotsCount() { return _loadedSnapshots.Count; }
    }
}