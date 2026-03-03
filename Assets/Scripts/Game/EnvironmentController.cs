using System.Collections.Generic;
using UnityEngine;


public class EnvironmentController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TurnManager turnManager;
    [SerializeField] EnvironmentView environmentView;



    void Start()
    {
        if (turnManager != null)
            turnManager.onMetricsUpdated.AddListener(HandleMetricUpdated);
        else
            Debug.Log("[Environment Controller]: Could not find turn manager! missing reference!");
        
    }

    private void HandleMetricUpdated(Dictionary<MetricType, float> metrics)
    {
        foreach (KeyValuePair<MetricType, float> metric in metrics)
        {
            float scale = GameState.MetricRange[1]; 
            float level = metric.Value / scale;

            switch (metric.Key)
            {
                case MetricType.PollutionLevel:
                    SetPollutionLevel(level);
                    break;
                case MetricType.BiodiversityLevel:
                    SetBiodiversityLevel(level);
                    break;
                case MetricType.WaterQuality:
                    SetWaterQualityLevel(level);
                    break;
            }
        }
    }

    private void SetPollutionLevel(float level)
    {
        level = Mathf.Clamp01(level);
        environmentView.ShowPollutionLevel(level);
        Debug.Log($"[Environment Controller]: Pollution level set to: {level}");
    }

    private void SetBiodiversityLevel(float level)
    {
        level = Mathf.Clamp01(level);
        environmentView.ShowBiodiversityLevel(level);
        Debug.Log($"[Environment Controller]: Biodiversity level set to: {level}");
    }

    private void SetWaterQualityLevel(float level)
    {
        level = Mathf.Clamp01(level);
        environmentView.ShowWaterQualityLevel(level);
        Debug.Log($"[Environment Controller]: Water Quality level set to: {level}");
    }

}