using open_dust_monitor.models;
using open_dust_monitor.src.Handler;

namespace open_dust_monitor.repositories
{
    public class TemperatureRepository
    {
        private static readonly string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string csvDirectory = Path.Combine(baseDirectory, "csv");
        private static readonly string pathToBaselineSnapshotsCsv = Path.Combine(csvDirectory, "baseline_temperature_snapshots.csv");
        private static readonly string pathToRecentSnapshotsCsv = Path.Combine(csvDirectory, "recent_temperature_snapshots.csv");
        private static List<TemperatureSnapshot> loadedBaselineSnapshots = [];
        private static List<TemperatureSnapshot> loadedRecentSnapshots = [];
        private static readonly int retentionDaysForRecentSnapshots = 3;

        public TemperatureRepository()
        {
            EnsureCsvDirectoryExists(csvDirectory);
            EnsureSnapshotCSVExists(pathToBaselineSnapshotsCsv);
            EnsureSnapshotCSVExists(pathToRecentSnapshotsCsv);
            loadedBaselineSnapshots = GetAllTemperatureSnapshotsFromCsv(pathToBaselineSnapshotsCsv);
            loadedRecentSnapshots = GetAllTemperatureSnapshotsFromCsv(pathToRecentSnapshotsCsv);
        }

        private static void EnsureCsvDirectoryExists(string csvDirectory)
        {
            if (!Directory.Exists(csvDirectory))
            {
                Directory.CreateDirectory(csvDirectory);
            }
        }

        private static void EnsureSnapshotCSVExists(string pathToCsv)
        {
            if (!File.Exists(pathToCsv))
                try
                {
                    var newCsv = File.Create(pathToCsv);
                    var historyCsvWriter = new StreamWriter(newCsv);
                    historyCsvWriter.WriteLine(TemperatureSnapshot.GetCsvRowHeaders());
                    historyCsvWriter.Close();
                    LogHandler.Logger.Debug("EnsureSnapshotCSVExists created=" + pathToCsv);
                }
                catch (Exception ex)
                {
                    throw new FileLoadException("Could not create TemperatureHistoryCsv: " + ex);
                }
        }

        public static List<TemperatureSnapshot> GetAllTemperatureSnapshotsFromCsv(string pathToCsv)
        {
            var retrievedSnapshots = new List<TemperatureSnapshot>();
            using (var reader = new StreamReader(pathToCsv))
            {
                reader.ReadLine();
                while (true)
                {
                    var csvRow = reader.ReadLine();
                    if (csvRow == null) break;
                    var csvRowValues = csvRow.Split(',');
                    var temperatureSnapshot = MapCsvRowToTemperatureSnapshot(csvRowValues);
                    retrievedSnapshots.Add(temperatureSnapshot);
                }
            }
            LogHandler.Logger.Debug("GetAllTemperatureSnapshotsFromCsv count=" + retrievedSnapshots.Count + " pathToCsv =" + pathToCsv);
            return retrievedSnapshots;
        }


        public static void SaveAndLoadBaselineSnapshot(TemperatureSnapshot snapshot, int maxSnapshotsForLoadRange)
        {
            var baselineSnapshotCountForLoadRange = GetSnapshotCountForCpuLoadRange(loadedBaselineSnapshots, snapshot.CpuLoadRange);
            if (baselineSnapshotCountForLoadRange <= maxSnapshotsForLoadRange)
            {
                loadedBaselineSnapshots.Add(snapshot);
                SaveSnapshotToCsv(snapshot, pathToBaselineSnapshotsCsv);
                LogHandler.Logger.Debug("SaveAndLoadBaselineSnapshot saved baselineSnapshot loadRange=" + snapshot.CpuLoadRange);
            }
            else
            {
                LogHandler.Logger.Debug("SaveAndLoadBaselineSnapshot skipped baselineSnapshot loadRange=" + snapshot.CpuLoadRange);
            }
        }

        public static void SaveAndLoadRecentSnapshot(TemperatureSnapshot snapshot)
        {
            loadedRecentSnapshots.Add(snapshot);
            SaveSnapshotToCsv(snapshot, pathToRecentSnapshotsCsv);
            RemoveSnapshotsOutsideRetentionPeriod(loadedRecentSnapshots, pathToRecentSnapshotsCsv, retentionDaysForRecentSnapshots);
            LogHandler.Logger.Debug("SaveAndLoadBaselineSnapshot saved recentSnapshot loadRange=" + snapshot.CpuLoadRange);
        }

        private static void SaveSnapshotToCsv(TemperatureSnapshot snapshot, string pathToCsv)
        {
            using (var csvAppender = File.AppendText(pathToCsv))
            {
                csvAppender.WriteLine(snapshot.GetAsCsvRow());
            }
            LogHandler.Logger.Debug("SaveSnapshotToCsv saved to csv=" + pathToCsv);
        }

        private static void RemoveSnapshotsOutsideRetentionPeriod(List<TemperatureSnapshot> loadedSnapshots, string pathToCsv, int retentionDays)
        {
            var oldestSnapshot = loadedSnapshots[0];
            var retentionCutOffTimestamp = DateTime.Now - TimeSpan.FromDays(retentionDays);
            LogHandler.Logger.Debug("RemoveSnapshotsOutsideRetentionPeriod oldestSnapshotTimestamp=" + oldestSnapshot.Timestamp + " retentionCutOffTimestamp=" + retentionCutOffTimestamp);
            if (oldestSnapshot.Timestamp < retentionCutOffTimestamp)
            {
                loadedSnapshots.RemoveAt(0);
                RemoveOldestSnapshotFromCsv(pathToCsv);
                LogHandler.Logger.Debug("RemoveSnapshotsOutsideRetentionPeriod removed=" + oldestSnapshot.GetAsCsvRow());
            }
            else
            {
                LogHandler.Logger.Debug("RemoveSnapshotsOutsideRetentionPeriod no outdated snapshots found");
            }
        }

        private static void RemoveOldestSnapshotFromCsv(string pathToCsv)
        {
            var lines = File.ReadAllLines(pathToCsv);
            var updatedLines = lines.Where((line, index) => index != 1).ToArray();
            File.WriteAllLines(pathToCsv, updatedLines);
        }

        public static int GetSnapshotCountForCpuLoadRange(List<TemperatureSnapshot> snapshots, string cpuLoadRange)
        {
            var snapshotCountForCpuLoadRange = snapshots.Where(snapshot => snapshot.CpuLoadRange == cpuLoadRange).ToList().Count;
            LogHandler.Logger.Debug("GetSnapshotCountForCpuLoadRange value=" + snapshotCountForCpuLoadRange);
            return snapshotCountForCpuLoadRange;
        }

        public static List<TemperatureSnapshot> GetLoadedBaselineSnapshots()
        {
            return loadedBaselineSnapshots;
        }

        public static List<TemperatureSnapshot> GetLoadedRecentSnapshots()
        {
            return loadedRecentSnapshots;
        }

        public static List<TemperatureSnapshot> GetSnapshotsForLoadRange(List<TemperatureSnapshot> snapshots, string cpuLoadRange)
        {
            return snapshots.Where(snapshot => snapshot.CpuLoadRange == cpuLoadRange).ToList();
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

        public static List<TemperatureSnapshot> GetLoadedRecentTemperatureSnapshots()
        {
            return loadedRecentSnapshots;
        }

        public static int GetLoadedRecentTemperatureSnapshotsCount()
        {
            return loadedRecentSnapshots.Count;
        }

        public static void RemovalAllBaselineTemperatureSnapshots()
        {
            loadedBaselineSnapshots = [];
            RemoveAllTemperatureSnapshotsFromCsv(pathToBaselineSnapshotsCsv);
        }

        public static void RemoveAllTemperatureSnapshotsFromCsv(string pathToCsv)
        {
            var lines = File.ReadAllLines(pathToCsv);
            var updatedLines = lines.Take(1).ToArray();
            File.WriteAllLines(pathToCsv, updatedLines);
        }
    }
}