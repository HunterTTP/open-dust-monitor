using open_dust_monitor.models;

namespace open_dust_monitor.repositories
{
    public class TemperatureRepository
    {
        private readonly string _pathToTemperatureHistoryCsv;
        private List<TemperatureSnapshot> _loadedIdleSnapshots = [];
        private List<TemperatureSnapshot> _loadedLowSnapshots = [];
        private List<TemperatureSnapshot> _loadedMediumSnapshots = [];
        private List<TemperatureSnapshot> _loadedHighSnapshots = [];
        private List<TemperatureSnapshot> _loadedMaxSnapshots = [];

        public TemperatureRepository()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _pathToTemperatureHistoryCsv = Path.Combine(baseDirectory, "temperature_history.csv");
            EnsureTemperatureHistoryCsvExists();
            LoadTemperatureSnapshots(GetAllTemperatureSnapshotsFromCsv());
        }

        public void LoadTemperatureSnapshots(List<TemperatureSnapshot> snapshots)
        {
            _loadedIdleSnapshots = snapshots.Where(snapshot => snapshot.CpuLoadRange == "idle").ToList();
            _loadedLowSnapshots = snapshots.Where(snapshot => snapshot.CpuLoadRange == "low").ToList();
            _loadedMediumSnapshots = snapshots.Where(snapshot => snapshot.CpuLoadRange == "medium").ToList();
            _loadedHighSnapshots = snapshots.Where(snapshot => snapshot.CpuLoadRange == "high").ToList();
            _loadedMaxSnapshots = snapshots.Where(snapshot => snapshot.CpuLoadRange == "max").ToList();
        }

        public void LoadTemperatureSnapshot(TemperatureSnapshot snapshot)
        {
            if (snapshot.CpuLoadRange.Equals("idle"))
            {
                _loadedIdleSnapshots.Add(snapshot);
            }
            else if (snapshot.CpuLoadRange.Equals("low"))
            {
                _loadedLowSnapshots.Add(snapshot);
            }
            else if (snapshot.CpuLoadRange.Equals("medium"))
            {
                _loadedMediumSnapshots.Add(snapshot);
            }
            else if (snapshot.CpuLoadRange.Equals("high"))
            {
                _loadedHighSnapshots.Add(snapshot);
            }
            else if (snapshot.CpuLoadRange.Equals("max"))
            {
                _loadedMaxSnapshots.Add(snapshot);
            }
        }

        public List<TemperatureSnapshot> GetLoadedIdleSnapshots()
        {
            return _loadedIdleSnapshots;
        }

        public List<TemperatureSnapshot> GetLoadedLowSnapshots()
        {
            return _loadedLowSnapshots;
        }

        public List<TemperatureSnapshot> GetLoadedMediumSnapshots()
        {
            return _loadedMediumSnapshots;
        }

        public List<TemperatureSnapshot> GetLoadedHighSnapshots()
        {
            return _loadedHighSnapshots;
        }

        public List<TemperatureSnapshot> GetLoadedMaxSnapshots()
        {
            return _loadedMaxSnapshots;
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

        public void SaveTemperatureSnapshot(TemperatureSnapshot snapshot)
        {
            LoadTemperatureSnapshot(snapshot);
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
            return _loadedIdleSnapshots
                .Concat(_loadedLowSnapshots)
                .Concat(_loadedMediumSnapshots)
                .Concat(_loadedHighSnapshots)
                .Concat(_loadedMaxSnapshots)
                .ToList();
        }

        public int GetLoadedTemperatureSnapshotsCount()
        {
            return _loadedIdleSnapshots.Count
                + _loadedLowSnapshots.Count
                + _loadedMediumSnapshots.Count
                + _loadedHighSnapshots.Count
                + _loadedMaxSnapshots.Count;
        }
    }
}