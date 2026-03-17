using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MetricsUI : MonoBehaviour
{
    private TurnManager turnManager;
    [SerializeField] private Slider waterQualitySlider;
    [SerializeField] private Slider pollutionLevelSlider;
    [SerializeField] private Slider biodiversityLevelSlider;
    [SerializeField] private bool useMilestoneDerivedValues = true;
    [SerializeField] private bool useAnimalBalanceForFirstSlider = true;
    [SerializeField] private bool preferSimulationValues = false;
    private WetlandProgressionManager wetlandProgressionManager;
    private MilestoneHandler milestoneHandler;
    private bool isBoundToProgressionEvents;

    private void Start()
    {
        // Keep UI driven by TurnManager/GameState metrics in the current gameplay flow.
        preferSimulationValues = false;

        SetSliderReadOnly(waterQualitySlider);
        SetSliderReadOnly(pollutionLevelSlider);
        SetSliderReadOnly(biodiversityLevelSlider);

        turnManager = TurnManager.Instance;
        if (turnManager != null)
        {
            turnManager.onMetricsUpdated.AddListener(UpdateMetricsUI);
            milestoneHandler = turnManager.milestoneHandler;
            if (turnManager.gameState != null && turnManager.gameState.metrics != null)
            {
                UpdateMetricsUI(turnManager.gameState.metrics);
            }
        }

        TryBindWetlandProgression();
    }

    private void Update()
    {
        if (!isBoundToProgressionEvents)
        {
            TryBindWetlandProgression();
        }

        // Keep UI in sync even if event wiring order changes in scene/prefab variants.
        RefreshFromCurrentState();
    }

    private void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.onMetricsUpdated.RemoveListener(UpdateMetricsUI);
        }

        if (wetlandProgressionManager != null && isBoundToProgressionEvents)
        {
            wetlandProgressionManager.onStateChanged.RemoveListener(UpdateFromWetlandProgressionState);
        }
    }

    private void UpdateMetricsUI(Dictionary<MetricType, float> metrics)
    {
        if (preferSimulationValues && wetlandProgressionManager != null && wetlandProgressionManager.State != null)
        {
            return;
        }

        if (metrics == null)
        {
            return;
        }

        foreach(var metric in metrics)
        {
            switch(metric.Key)
            {
                case MetricType.WaterQuality:
                    waterQualitySlider.value = metric.Value/100;
                    break;
                case MetricType.PollutionLevel:
                    pollutionLevelSlider.value = metric.Value/100;
                    break;
                case MetricType.BiodiversityLevel:
                    biodiversityLevelSlider.value = metric.Value/100;
                    break;
            }
        }
    }

    private void UpdateFromWetlandProgressionState()
    {
        RefreshFromCurrentState();
    }

    private void RefreshFromCurrentState()
    {
        if (useMilestoneDerivedValues && TryUpdateFromMilestone())
        {
            return;
        }

        if (preferSimulationValues && wetlandProgressionManager != null)
        {
            WetlandSimulationState state = wetlandProgressionManager.State;
            if (state == null)
            {
                return;
            }

            float firstValue = useAnimalBalanceForFirstSlider ? state.animalBalance : state.waterClarity;
            waterQualitySlider.value = Mathf.Clamp01(firstValue / 100f);
            pollutionLevelSlider.value = Mathf.Clamp01((100f - state.waterClarity) / 100f);
            biodiversityLevelSlider.value = Mathf.Clamp01(state.plantBalance / 100f);
            return;
        }

        if (turnManager == null || turnManager.gameState == null || turnManager.gameState.metrics == null)
        {
            return;
        }

        // Fallback path for scenes using legacy turn metrics.
        UpdateMetricsUI(turnManager.gameState.metrics);
    }

    private bool TryUpdateFromMilestone()
    {
        if (turnManager == null)
        {
            return false;
        }

        if (milestoneHandler == null)
        {
            milestoneHandler = turnManager.milestoneHandler;
        }

        if (milestoneHandler == null)
        {
            return false;
        }

        float biodiversity01 = Mathf.Clamp01(milestoneHandler.GetBiodiversity01());
        biodiversityLevelSlider.value = biodiversity01;
        pollutionLevelSlider.value = Mathf.Clamp01(1f - biodiversity01);
        waterQualitySlider.value = Mathf.Clamp01((biodiversity01 - 0.20f) / 0.80f);
        return true;
    }

    private static void SetSliderReadOnly(Slider slider)
    {
        if (slider == null)
        {
            return;
        }

        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.navigation = new Navigation { mode = Navigation.Mode.None };
    }

    private void TryBindWetlandProgression()
    {
        if (isBoundToProgressionEvents)
        {
            return;
        }

        wetlandProgressionManager = WetlandProgressionManager.Instance;
        if (wetlandProgressionManager == null)
        {
            return;
        }

        wetlandProgressionManager.onStateChanged.AddListener(UpdateFromWetlandProgressionState);
        isBoundToProgressionEvents = true;
        RefreshFromCurrentState();
    }
}
