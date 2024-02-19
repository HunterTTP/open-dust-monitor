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
        private static readonly int userNotificationFrequencyHours = 72;
        private static DateTime userLastNotified;

        public TemperatureSnapshot GetLatestTemperatureSnapshot()
        {
            var dateTime = DateTime.Now;
            var cpuName = _hardwareService.GetCpuName();
            var cpuTemperature = _hardwareService.GetCurrentCpuTemperature();
            var cpuLoad = _hardwareService.GetCurrentCpuLoad();
            var cpuLoadRange = HardwareService.GetCurrentCpuLoadRange(cpuLoad);
            var snapshot = new TemperatureSnapshot(dateTime, cpuName, cpuTemperature, cpuLoad, cpuLoadRange);
            ProcessLatestSnapshot(snapshot);
            return snapshot;
        }

        public static void ProcessLatestSnapshot(TemperatureSnapshot snapshot)
        {
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

        public static string GetLatestSnapshotLabel(TemperatureSnapshot snapshot)
        {
            return "Latest Snapshot: \n " + snapshot.GetAsFormattedString();
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
                + "\n idle: " + idleSnapshots.Count
                + "\n low: " + lowSnapshots.Count
                + "\n medium: " + mediumSnapshots.Count
                + "\n high: " + highSnapshots.Count
                + "\n max: " + maxSnapshots.Count
                + "\n total: " + snapshots.Count;
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
                + "\n idle: " + GetAverageTemperature(idleSnapshots) + "°C"
                + "\n low: " + GetAverageTemperature(lowSnapshots) + "°C"
                + "\n medium: " + GetAverageTemperature(mediumSnapshots) + "°C"
                + "\n high: " + GetAverageTemperature(highSnapshots) + "°C"
                + "\n max: " + GetAverageTemperature(maxSnapshots) + "°C";
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
                + "\n idle: " + GetAlertThresholdTemperature(idleSnapshots) + "°C"
                + "\n low: " + GetAlertThresholdTemperature(lowSnapshots) + "°C"
                + "\n medium: " + GetAlertThresholdTemperature(mediumSnapshots) + "°C"
                + "\n high: " + GetAlertThresholdTemperature(highSnapshots) + "°C"
                + "\n max: " + GetAlertThresholdTemperature(maxSnapshots) + "°C";
        }

        public void StopTemperatureMonitoring()
        {
            _hardwareService.StopHardwareMonitoring();
        }

        public static int GetSnapshotIntervalMillis()
        {
            return snapshotIntervalMillis;
        }

        public static int GetMaximumAlertSnapshotsCount()
        {
            return maximumAlertSnapshots;
        }

        public static bool WasUserRecentlyNotified()
        {
            return userLastNotified >= (DateTime.Now - TimeSpan.FromHours(userNotificationFrequencyHours));
        }

        public static void UserWasNotified()
        {
            userLastNotified = DateTime.Now;
        }

        public static void ResetBaselineTemperatures()
        {
            TemperatureRepository.RemovalAllBaselineTemperatureSnapshots();

        }
    }
}