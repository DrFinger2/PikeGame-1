using System.Collections.Generic;
using UnityEngine;


// ---------------------
// NOT IMPLEMENTED!
// ---------------------
public class EnvironmentSystem : MonoBehaviour
{

    [Header("Game Systems")]
    [SerializeField] GameState state;
    [SerializeField] TurnManager turnManager;

    // State
    [SerializeField, HideInInspector]  float pollutionLevel = 0;
    [SerializeField, HideInInspector]  float waterQualityLevel = 0;
    [SerializeField, HideInInspector]  float bioDiversityLevel = 0;

    void Start()
    {
        TurnManager manager = TurnManager.Instance;
        if (manager != null)
        {
            manager.onMetricsUpdated.AddListener(HandleStateChange);
        }
    }

    private void HandleStateChange(Dictionary<MetricType, float> metrics)
    {
        foreach (var metric in metrics)
        {
            switch (metric.Key)
            {
                case MetricType.WaterQuality:
                    SetWaterQualityLevel(metric.Value); break;
                case MetricType.PollutionLevel:
                    SetPollutionLevel(metric.Value); break;
                case MetricType.BiodiversityLevel:
                    SetBiodiversityLevel(metric.Value); break;
            }
        }
    }

    public bool SetPollutionLevel(float level)
    {
        if (this.pollutionLevel == level)
            return false;

        this.pollutionLevel = level;
        return true;
    }

    public bool SetWaterQualityLevel(float level)
    {
        if (this.waterQualityLevel == level)
            return false;

        this.waterQualityLevel = level;
        return true;
    }

    public bool SetBiodiversityLevel(float level)
    {
        if (this.bioDiversityLevel == level)
            return false;

        this.bioDiversityLevel = level;
        return true;
    }


    // Get Methods
    public float GetPollutionLevel()
    {
        return this.pollutionLevel;
    }


    public float GetWaterQualityLevel()
    {
        return this.waterQualityLevel;
    }



    public float GetBiodiversityLevel()
    {
        return this.bioDiversityLevel;
    }
}
