using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-200)]
public class EnvironmentControllerTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TurnManager turnManager;


    [Header("Test Values (0-100)")]
    [Range(0, 100)] public float pollutionLevel = MetricDefaults.Pollution;
    [Range(0, 100)] public float biodiversityLevel = MetricDefaults.Biodiversity;
    [Range(0, 100)] public float waterQualityLevel = MetricDefaults.WaterQuality;

    private float _lastPollution, _lastBiodiversity, _lastWaterQuality;

    private void Start()
    {
        UpdateMetrics(pollutionLevel, biodiversityLevel, waterQualityLevel);
    }

    private void OnDisable()
    {
        UpdateMetrics(MetricDefaults.Pollution, MetricDefaults.Biodiversity, MetricDefaults.WaterQuality);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        UpdateMetrics(pollutionLevel, biodiversityLevel, waterQualityLevel);
    }

    private void UpdateMetrics(float pollutionLevel, float biodiversityLevel, float waterQualityLevel)
    {
        if (!Mathf.Approximately(pollutionLevel, _lastPollution))
        {
            UpdateMetric(MetricType.PollutionLevel, pollutionLevel);
            _lastPollution = pollutionLevel;
        }

        if (!Mathf.Approximately(biodiversityLevel, _lastBiodiversity))
        {
            UpdateMetric(MetricType.BiodiversityLevel, biodiversityLevel);
            _lastBiodiversity = biodiversityLevel;
        }

        if (!Mathf.Approximately(waterQualityLevel, _lastWaterQuality))
        {
            UpdateMetric(MetricType.WaterQuality, waterQualityLevel);
            _lastWaterQuality = waterQualityLevel;
        }
    }
    private void UpdateMetric(MetricType type, float value)
    {
        if (turnManager == null)
        {
            Debug.LogError("[EnvironmentControllerTest] Error: TurnManager instance not found.");
            return;
        }

        var data = new Dictionary<MetricType, float> { { type, value } };
        turnManager.onMetricsUpdated.Invoke(data);

        Debug.Log($"[EnvironmentControllerTest] Updated {type}: {value}%");
    }
}
