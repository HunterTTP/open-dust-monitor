using open_dust_monitor.models;

namespace open_dust_monitor.repositories
{
    public class TemperatureRepository
    {
        private static readonly string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string _pathToTemperatureHistoryCsv = Path.Combine(baseDirectory, "temperature_history.csv");
        private List<TemperatureSnapshot> loadedSnapshots = [];

        public TemperatureRepository()
        {
            EnsureTemperatureHistoryCsvExists();
            loadedSnapshots = GetAllTemperatureSnapshotsFromCsv();
        }

        private static void EnsureTemperatureHistoryCsvExists()
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

        public List<TemperatureSnapshot> GetLoadedIdleSnapshots()
        {
            return loadedSnapshots.Where(snapshot => snapshot.CpuLoadRange == "idle").ToList();
        }

        public List<TemperatureSnapshot> GetLoadedLowSnapshots()
        {
            return loadedSnapshots.Where(snapshot => snapshot.CpuLoadRange == "low").ToList();
        }

        public List<TemperatureSnapshot> GetLoadedMediumSnapshots()
        {
            return loadedSnapshots.Where(snapshot => snapshot.CpuLoadRange == "medium").ToList();
        }

        public List<TemperatureSnapshot> GetLoadedHighSnapshots()
        {
            return loadedSnapshots.Where(snapshot => snapshot.CpuLoadRange == "high").ToList();
        }

        public List<TemperatureSnapshot> GetLoadedMaxSnapshots()
        {
            return loadedSnapshots.Where(snapshot => snapshot.CpuLoadRange == "max").ToList();
        }

        public void SaveTemperatureSnapshot(TemperatureSnapshot snapshot)
        {
            loadedSnapshots.Add(snapshot);
            using (var csvAppender = File.AppendText(_pathToTemperatureHistoryCsv))
            {
                csvAppender.WriteLine(snapshot.GetAsCsvRow());
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
            var dateTime = DateTime.Parse(csvRowValues[0]);
            var cpuName = csvRowValues[1];
            var cpuTemperature = float.Parse(csvRowValues[2]);
            var cpuLoad = float.Parse(csvRowValues[3]);
            var cpuLoadRange = csvRowValues[4];
            return new TemperatureSnapshot(dateTime, cpuName, cpuTemperature, cpuLoad, cpuLoadRange);
        }

        public List<TemperatureSnapshot> GetLoadedTemperatureSnapshots()
        {
            return loadedSnapshots;
        }

        public int GetLoadedTemperatureSnapshotsCount()
        {
            return loadedSnapshots.Count;
        }
    }
}