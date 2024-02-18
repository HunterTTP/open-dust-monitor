using open_dust_monitor.models;
using open_dust_monitor.repositories;
using open_dust_monitor.src.Handler;

namespace open_dust_monitor.services
{
    public class TemperatureService
    {
        private readonly HardwareService _hardwareService = InstanceHandler.GetHardwareService();
        private static readonly int snapshotIntervalMillis = 2000;
        private static readonly int minimumMonitoringMinutes = 120;
        private static readonly int maximumAlertSnapshots = (minimumMonitoringMinutes * 60) / (snapshotIntervalMillis / 1000);

        public TemperatureSnapshot GetLatestTemperatureSnapshot()
        {
            var dateTime = DateTime.Now;
            var cpuName = _hardwareService.GetCpuName();
            var cpuTemperature = _hardwareService.GetCurrentCpuTemperature();
            var cpuLoad = _hardwareService.GetCurrentCpuLoad();
            var cpuLoadRange = HardwareService.GetCurrentCpuLoadRange(cpuLoad);
            var snapshot = new TemperatureSnapshot(dateTime, cpuName, cpuTemperature, cpuLoad, cpuLoadRange);
            SaveLatestSnapshot(snapshot);
            return snapshot;
        }

        public static void SaveLatestSnapshot(TemperatureSnapshot snapshot)
        {
            LogHandler.Logger.Debug("SaveLatestSnapshot snapshot=" + snapshot);
            TemperatureRepository.SaveAndLoadRecentSnapshot(snapshot);
            TemperatureRepository.SaveAndLoadBaselineSnapshot(snapshot, maximumAlertSnapshots);
        }

        public bool AreRecentAverageTemperaturesNormal()
        {
            return IsRecentAverageTemperatureNormal(TemperatureRepository.GetLoadedIdleSnapshots())
                && IsRecentAverageTemperatureNormal(TemperatureRepository.GetLoadedLowSnapshots())
                && IsRecentAverageTemperatureNormal(TemperatureRepository.GetLoadedMediumSnapshots())
                && IsRecentAverageTemperatureNormal(TemperatureRepository.GetLoadedHighSnapshots())
                && IsRecentAverageTemperatureNormal(TemperatureRepository.GetLoadedMaxSnapshots());
        }

        public bool IsRecentAverageTemperatureNormal(List<TemperatureSnapshot> snapshots)
        {
            if (snapshots.Count < maximumAlertSnapshots) { return true; } //not enough snapshots to alert properly
            return (GetRecentAverageTemperature(snapshots) <= GetAlertThresholdTemperature(snapshots));
        }

        public static float GetRecentAverageTemperature(List<TemperatureSnapshot> snapshots)
        {
            if (snapshots == null || snapshots.Count == 0) { return 0; }
            var endDate = snapshots.Max(snapshot => snapshot.Timestamp).AddDays(-7);
            var recentAverageTemperature = snapshots
                .Where(snapshot => snapshot.Timestamp >= endDate)
                .Select(snapshot => snapshot.CpuTemperature)
                .DefaultIfEmpty(0)
                .Average();
            return (float)Math.Round(recentAverageTemperature);
        }

        public float GetAlertThresholdTemperature(List<TemperatureSnapshot> snapshots)
        {
            return (float)Math.Round(GetBaselineTemperature(snapshots) + 5f);
        }

        private float GetBaselineTemperature(List<TemperatureSnapshot> snapshots)
        {
            if (snapshots == null || snapshots.Count == 0) { return 0; }
            var endDate = snapshots.Min(snapshot => snapshot.Timestamp).AddDays(7);
            return snapshots
                .Where(snapshot => snapshot.Timestamp <= endDate)
                .Select(snapshot => snapshot.CpuTemperature)
                .DefaultIfEmpty(0)
                .Average();
        }

        public string GetTemperatureSnapshotLabel(TemperatureSnapshot snapshot, int timerInterval)
        {
            var idleSnapshots = TemperatureRepository.GetLoadedIdleSnapshots();
            var lowSnapshots = TemperatureRepository.GetLoadedLowSnapshots();
            var mediumSnapshots = TemperatureRepository.GetLoadedMediumSnapshots();
            var highSnapshots = TemperatureRepository.GetLoadedHighSnapshots();
            var maxSnapshots = TemperatureRepository.GetLoadedMaxSnapshots();
            return
                "Latest Snapshot:"
                + "\n Timestamp: " + snapshot.Timestamp
                + "\n CPU: " + snapshot.CpuName
                + "\n Temperature: " + snapshot.CpuTemperature + "°C"
                + "\n Utilization: " + snapshot.CpuLoad + "%"
                + "\n\n"
                + "Snapshots:"
                + "\n idleSnapshotCount: " + idleSnapshots.Count
                + "\n lowSnapshotCount: " + lowSnapshots.Count
                + "\n mediumSnapshotCount: " + mediumSnapshots.Count
                + "\n highSnapshotCount: " + highSnapshots.Count
                + "\n maxSnapshotCount: " + maxSnapshots.Count
                + "\n totalSnapshotCount: " + TemperatureRepository.GetLoadedRecentTemperatureSnapshots().Count
                + "\n snapshotFrequency: " + timerInterval / 1000 + " seconds"
                + "\n\n"
                + "Average Temperatures:"
                + "\n idleRecentAverage: " + GetRecentAverageTemperature(idleSnapshots) + "°C"
                + "\n lowRecentAverage: " + GetRecentAverageTemperature(lowSnapshots) + "°C"
                + "\n mediumRecentAverage: " + GetRecentAverageTemperature(mediumSnapshots) + "°C"
                + "\n highRecentAverage: " + GetRecentAverageTemperature(highSnapshots) + "°C"
                + "\n maxRecentAverage: " + GetRecentAverageTemperature(maxSnapshots) + "°C"
                + "\n\n"
                + "Alert Thresholds:"
                + "\n idleAlertThreshold: " + GetAlertThresholdTemperature(idleSnapshots) + "°C"
                + "\n lowAlertThreshold: " + GetAlertThresholdTemperature(lowSnapshots) + "°C"
                + "\n mediumAlertThreshold: " + GetAlertThresholdTemperature(mediumSnapshots) + "°C"
                + "\n highAlertThreshold: " + GetAlertThresholdTemperature(highSnapshots) + "°C"
                + "\n maxAlertThreshold: " + GetAlertThresholdTemperature(maxSnapshots) + "°C"
                + "\n recentAveragesAreOk: " + AreRecentAverageTemperaturesNormal().ToString();
        }

        public void StopTemperatureMonitoring()
        {
            _hardwareService.StopHardwareMonitoring();
        }

        public static int GetSnapshotIntervalMillis()
        {
            return snapshotIntervalMillis;
        }
    }
}