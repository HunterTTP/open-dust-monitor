using open_dust_monitor.models;
using open_dust_monitor.repositories;
using open_dust_monitor.src.Handler;

namespace open_dust_monitor.services
{
    public class TemperatureService
    {
        private readonly HardwareService _hardwareService = InstanceHandler.GetHardwareService();
        private readonly TemperatureRepository _temperatureRepository = InstanceHandler.GetTemperatureRepository();
        private static readonly int snapshotIntervalMillis = 2000;
        private static readonly int minimumMonitoringMinutes = 120;
        private static readonly int minimumAlertSnapshots = (minimumMonitoringMinutes * 60) / (snapshotIntervalMillis / 1000);

        public TemperatureSnapshot GetLatestTemperatureSnapshot()
        {
            var dateTime = DateTime.Now;
            var cpuName = _hardwareService.GetCpuName();
            var cpuTemperature = _hardwareService.GetCurrentCpuTemperature();
            var cpuLoad = _hardwareService.GetCurrentCpuLoad();
            var cpuLoadRange = _hardwareService.GetCurrentCpuLoadRange(cpuLoad);
            var snapshot = new TemperatureSnapshot(dateTime, cpuName, cpuTemperature, cpuLoad, cpuLoadRange);
            _temperatureRepository.SaveTemperatureSnapshot(snapshot);
            return snapshot;
        }

        public bool AreRecentAverageTemperaturesNormal()
        {
            return
                IsRecentAverageTemperatureNormal(_temperatureRepository.GetLoadedIdleSnapshots())
                && IsRecentAverageTemperatureNormal(_temperatureRepository.GetLoadedLowSnapshots())
                && IsRecentAverageTemperatureNormal(_temperatureRepository.GetLoadedMediumSnapshots())
                && IsRecentAverageTemperatureNormal(_temperatureRepository.GetLoadedHighSnapshots())
                && IsRecentAverageTemperatureNormal(_temperatureRepository.GetLoadedMaxSnapshots());
        }

        public bool IsRecentAverageTemperatureNormal(List<TemperatureSnapshot> snapshots)
        {
            if (snapshots.Count < minimumAlertSnapshots) { return true; }
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
            var idleSnapshots = _temperatureRepository.GetLoadedIdleSnapshots();
            var lowSnapshots = _temperatureRepository.GetLoadedLowSnapshots();
            var mediumSnapshots = _temperatureRepository.GetLoadedMediumSnapshots();
            var highSnapshots = _temperatureRepository.GetLoadedHighSnapshots();
            var maxSnapshots = _temperatureRepository.GetLoadedMaxSnapshots();
            return
                "Latest Snapshot:"
                + "\n Timestamp: " + snapshot.Timestamp
                + "\n CPU: " + snapshot.CpuName
                + "\n Temperature: " + snapshot.CpuTemperature + "°C"
                + "\n Utilization: " + snapshot.CpuLoad + "%"
                + "\n\n"
                + "Snapshots:"
                + "\n idleSnapshotCount: " + idleSnapshots.Count()
                + "\n lowSnapshotCount: " + lowSnapshots.Count()
                + "\n mediumSnapshotCount: " + mediumSnapshots.Count()
                + "\n highSnapshotCount: " + highSnapshots.Count()
                + "\n maxSnapshotCount: " + maxSnapshots.Count()
                + "\n totalSnapshotCount: " + _temperatureRepository.GetLoadedTemperatureSnapshots().Count()
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

        public int GetSnapshotIntervalMillis()
        {
            return snapshotIntervalMillis;
        }
    }
}