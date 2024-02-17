﻿using open_dust_monitor.models;
using open_dust_monitor.repositories;
using open_dust_monitor.src.Handler;

namespace open_dust_monitor.services
{
    public class TemperatureService
    {
        private readonly HardwareService _hardwareService = InstanceHandler.GetHardwareService();
        private readonly TemperatureRepository _temperatureRepository = InstanceHandler.GetTemperatureRepository();

        public TemperatureSnapshot GetLatestTemperatureSnapshot()
        {
            var latestTemperatureSnapshot = new TemperatureSnapshot(
                DateTime.Now,
                _hardwareService.GetCpuName(),
                _hardwareService.GetCurrentCpuLoad(),
                _hardwareService.GetCurrentCpuTemperature());
            _temperatureRepository.SaveTemperatureSnapshot(latestTemperatureSnapshot);
            return latestTemperatureSnapshot;
        }

        public string GetLoadSensorName()
        {
            var cpuLoadSensor = _hardwareService.GetCpuLoadSensor();
            return cpuLoadSensor.Name;
        }

        public int GetTotalTemperatureSnapshotCount()
        {
            return _temperatureRepository.GetAllLoadedTemperatureSnapshots().Count;
        }

        public List<TemperatureSnapshot> GetAllTemperatureSnapshots()
        {
            return _temperatureRepository.GetAllLoadedTemperatureSnapshots();
        }

        public bool IsRecentAverageTemperatureWithinThreshold()
        {
            return GetRecentAverageTemperature() <= GetAlertThresholdTemperature();
        }

        public float GetAlertThresholdTemperature()
        {
            return (float)Math.Round(GetBaselineTemperature() * 1.15f);
        }

        private float GetBaselineTemperature()
        {
            var temperatureSnapshots = _temperatureRepository.GetAllLoadedTemperatureSnapshots();
            var endDate = temperatureSnapshots.Min(snapshot => snapshot.Timestamp).AddDays(7);
            return temperatureSnapshots
                .Where(snapshot => snapshot.Timestamp <= endDate)
                .Select(snapshot => snapshot.CpuPackageTemperature)
                .DefaultIfEmpty(0)
                .Average();
        }

        public float GetRecentAverageTemperature()
        {
            var temperatureSnapshots = _temperatureRepository.GetAllLoadedTemperatureSnapshots();
            var endDate = temperatureSnapshots.Max(snapshot => snapshot.Timestamp).AddDays(-7);
            var recentAverageTemperature = temperatureSnapshots
                .Where(snapshot => snapshot.Timestamp >= endDate)
                .Select(snapshot => snapshot.CpuPackageTemperature)
                .DefaultIfEmpty(0)
                .Average();
            return (float)Math.Round(recentAverageTemperature);
        }

        public List<TemperatureSnapshot> GetRecentSnapshots()
        {
            var snapshots = _temperatureRepository.GetAllLoadedTemperatureSnapshots().OrderBy(snapshot => snapshot.Timestamp).ToList();
            return snapshots.Take(20).ToList();
        }

        public void StopTemperatureMonitoring()
        {
            _hardwareService.StopHardwareMonitoring();
        }

        internal string GetTemperatureSnapshotLabel(TemperatureSnapshot snapshot, int timerInterval)
        {
            return "Latest Snapshot:" +
            "\n Timestamp: " + snapshot.Timestamp +
            "\n CPU: " + snapshot.CpuName +
            "\n Temperature: " + snapshot.CpuPackageTemperature + "°C" +
            "\n Utilization: " + snapshot.CpuPackageUtilization + "%" +
            "\n" +
            "\nKey Variables:" +
            "\n alertThresholdTemperature: " + GetAlertThresholdTemperature() + "°C" +
            "\n recentAverageTemperature: " + GetRecentAverageTemperature() + "°C" +
            "\n recentAverageIsOk: " + IsRecentAverageTemperatureWithinThreshold().ToString() +
            "\n totalSnapshots: " + GetTotalTemperatureSnapshotCount().ToString() +
            "\n snapshotFrequency: " + timerInterval / 1000 + " seconds";
        }
    }
}