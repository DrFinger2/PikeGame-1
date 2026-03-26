using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using NUnit.Framework.Constraints;


public class GameState : MonoBehaviour
{
    public int currentActionPoints = 4;
    private int currentTurnBonusPoints;
    public Dictionary<MetricType, float> metrics;
    public List<OngoingEffect> activeEffects;
    private List<MetricEffect> pendingEffects;
    private RandomEventSystem randomEventSystem;
    public static Vector2 MetricRange { get; } = new Vector2(0, 100);

    public UnityEvent onEventChoiceMade;
    public UnityEvent onNewEvent;
    public WetlandEvent CurrentEvent => currentEvent;
    private WetlandEvent currentEvent;

    private void Start()
    {
        InitializeStats();
        randomEventSystem = RandomEventSystem.instance;
    }
    //create list and assign level start metrics
    private void InitializeStats()
    {
        activeEffects = new List<OngoingEffect>();
        pendingEffects = new List<MetricEffect>();
        metrics = new Dictionary<MetricType, float> {
            {MetricType.WaterQuality, MetricDefaults.WaterQuality },
            {MetricType.BiodiversityLevel, MetricDefaults.Biodiversity },
            {MetricType.PollutionLevel, MetricDefaults.Pollution }
        };
    }

    public void EndTurn()
    {
        HandleOngoingEffects();
        ApplyPendingEffects();
        GetRandomEvent();
        ResetActionPoints();
        HandleSaveGame();
    }
    //apply active effects each turn and remove when amount of active turns done
    private void HandleOngoingEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            foreach (var metricEffect in effect.effects)
            {
                ApplyMetricEffect(metricEffect);
            }
            effect.remainingTurns--;
            if (effect.remainingTurns <= 0)
            {
                activeEffects.RemoveAt(i);
            }
        }
    }
    //apply all the effects that were applied from actions this turn
    private void ApplyPendingEffects()
    {
        if (pendingEffects.Count > 0)
        {
            foreach (var effect in pendingEffects)
            {
                ApplyMetricEffect(effect);
            }
            pendingEffects.Clear();
        }
    }
    public void GetRandomEvent()
    {
        currentEvent = randomEventSystem.GetNextEvent();
        //Debug.Log(currentEvent.name);
        onNewEvent?.Invoke();
    }
    public void HandleRandomEvent(AnswerCategory answer)
    {
        Debug.Log($"selected {answer.ToString()} response");
        switch (answer)
        {
            case AnswerCategory.Good:
                currentTurnBonusPoints += 3;
                break;
            case AnswerCategory.Neutral:
                currentTurnBonusPoints += 2;
                break;
            case AnswerCategory.Bad:
                currentTurnBonusPoints += 1;
                break;
        }
        currentActionPoints += currentTurnBonusPoints;
        TurnManager.Instance.onActionPointsChanged?.Invoke(currentActionPoints);
        onEventChoiceMade?.Invoke();
    }
    private void HandleSaveGame()
    {
        //save logic?
    }
    private void ResetActionPoints()
    {
        currentActionPoints = 4;
        currentTurnBonusPoints = 0;
    }
    //add action effects to list so they can be applied at end turn
    public void QueueMetricEffect(MetricEffect effect)
    {
        pendingEffects.Add(effect);
    }
    //change metric values based on effects
    public void ApplyMetricEffect(MetricEffect effect)
    {
        metrics[effect.type] += effect.value;
        metrics[effect.type] = Mathf.Clamp(metrics[effect.type], MetricRange[0], MetricRange[1]);
    }
    //add multi turn effect to list
    public void AddOngoingEffect(OngoingEffect effect)
    {
        activeEffects.Add(effect);
    }
}
