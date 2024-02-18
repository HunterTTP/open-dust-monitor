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

        public static bool AreRecentAverageTemperaturesWithinThreshold()
        {
            var baselineSnapshots = TemperatureRepository.GetLoadedBaselineSnapshots();
            var baselineIdleSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(baselineSnapshots, "idle");
            var baselineLowSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(baselineSnapshots, "low");
            var baselineMediumSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(baselineSnapshots, "medium");
            var baselineHighSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(baselineSnapshots, "high");
            var baselineMaxSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(baselineSnapshots, "max");

            var recentSnapshots = TemperatureRepository.GetLoadedRecentSnapshots();
            var recentIdleSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(recentSnapshots, "idle");
            var recentLowSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(recentSnapshots, "low");
            var recentMediumSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(recentSnapshots, "medium");
            var recentHighSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(recentSnapshots, "high");
            var recentMaxSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(recentSnapshots, "max");

            return IsAverageTemperatureWithinThreshold(recentIdleSnapshots, baselineIdleSnapshots)
                && IsAverageTemperatureWithinThreshold(recentLowSnapshots, baselineLowSnapshots)
                && IsAverageTemperatureWithinThreshold(recentMediumSnapshots, baselineMediumSnapshots)
                && IsAverageTemperatureWithinThreshold(recentHighSnapshots, baselineHighSnapshots)
                && IsAverageTemperatureWithinThreshold(recentMaxSnapshots, baselineMaxSnapshots);
        }

        public static bool IsAverageTemperatureWithinThreshold(List<TemperatureSnapshot> recentSnapshots, List<TemperatureSnapshot> baselineSnapshots)
        {
            if (recentSnapshots.Count < maximumAlertSnapshots)
            {
                return true;
            }
            return (GetAverageTemperature(recentSnapshots) <= GetAlertThresholdTemperature(baselineSnapshots));
        }

        public static float GetAverageTemperature(List<TemperatureSnapshot> snapshots)
        {
            if (snapshots == null || snapshots.Count == 0) { return 0; }
            var recentAverageTemperature = snapshots
                .Select(snapshot => snapshot.CpuTemperature)
                .DefaultIfEmpty(0)
                .Average();
            return (float)Math.Round(recentAverageTemperature);
        }

        public static float GetAlertThresholdTemperature(List<TemperatureSnapshot> snapshots)
        {
            return (float)Math.Round(GetAverageTemperature(snapshots) + 5f);
        }

        public static string GetSnapshotCountsLabel(List<TemperatureSnapshot> snapshots, string snapshotCategory)
        {
            var idleSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "idle");
            var lowSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "low");
            var mediumSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "medium");
            var highSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "high");
            var maxSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "max");
            return
                snapshotCategory + " Snapshot Counts:"
                + "\n idleSnapshotCount: " + idleSnapshots.Count
                + "\n lowSnapshotCount: " + lowSnapshots.Count
                + "\n mediumSnapshotCount: " + mediumSnapshots.Count
                + "\n highSnapshotCount: " + highSnapshots.Count
                + "\n maxSnapshotCount: " + maxSnapshots.Count
                + "\n totalSnapshotCount: " + snapshots.Count;
        }

        public static string GetSnapshotTemperaturesLabel(List<TemperatureSnapshot> snapshots, string snapshotCategory)
        {
            var idleSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "idle");
            var lowSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "low");
            var mediumSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "medium");
            var highSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "high");
            var maxSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "max");
            return
                snapshotCategory + " Average Temperatures:"
                + "\n idleRecentAverage: " + GetAverageTemperature(idleSnapshots) + "°C"
                + "\n lowRecentAverage: " + GetAverageTemperature(lowSnapshots) + "°C"
                + "\n mediumRecentAverage: " + GetAverageTemperature(mediumSnapshots) + "°C"
                + "\n highRecentAverage: " + GetAverageTemperature(highSnapshots) + "°C"
                + "\n maxRecentAverage: " + GetAverageTemperature(maxSnapshots) + "°C";
        }

        public static string GetSnapshotAlertThresholdsLabel(List<TemperatureSnapshot> snapshots)
        {
            var idleSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "idle");
            var lowSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "low");
            var mediumSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "medium");
            var highSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "high");
            var maxSnapshots = TemperatureRepository.GetSnapshotsForLoadRange(snapshots, "max");
            return
                "Alert Threshold Temperatures:"
                + "\n idleRecentAverage: " + GetAlertThresholdTemperature(idleSnapshots) + "°C"
                + "\n lowRecentAverage: " + GetAlertThresholdTemperature(lowSnapshots) + "°C"
                + "\n mediumRecentAverage: " + GetAlertThresholdTemperature(mediumSnapshots) + "°C"
                + "\n highRecentAverage: " + GetAlertThresholdTemperature(highSnapshots) + "°C"
                + "\n maxRecentAverage: " + GetAlertThresholdTemperature(maxSnapshots) + "°C";
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