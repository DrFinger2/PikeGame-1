using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public WarningMessagesUI warningMessages;
    public MilestoneHandler milestoneHandler;
    public static TurnManager Instance;
    public GameState gameState;
    public int CurrentTurn => currentTurn;
    private int currentTurn = 1;

    // POS solution
    public bool IsInitializing { get; private set; } = true;
    public UnityEvent<int> onTurnChanged;
    public UnityEvent<int> onActionPointsChanged;
    public UnityEvent<Dictionary<MetricType, float>> onMetricsUpdated;
    

    [SerializeField] private Button endTurnButton;
    [SerializeField] private bool enableSimulationIntegration = false;

    private void Awake()
    {
        milestoneHandler = GetComponent<MilestoneHandler>();

        if (Instance == null && Instance != this)
        {
            Instance = this;
            gameState = GetComponent<GameState>();

            WetlandProgressionManager progressionManager = GetComponent<WetlandProgressionManager>();

            if (enableSimulationIntegration && progressionManager == null)
            {
                progressionManager = gameObject.AddComponent<WetlandProgressionManager>();
            }

            if (progressionManager != null)
            {
                progressionManager.EnableSimulation(enableSimulationIntegration);
            }
        }
        else
        {
            Debug.Log("duplicate turnmanager");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(DelayedInitialize());
    }

    private IEnumerator DelayedInitialize()
    {
        yield return null;
        Initialize();
    }

    private void Initialize()
    {
        onTurnChanged?.Invoke(currentTurn);
        onActionPointsChanged?.Invoke(gameState.currentActionPoints);
        onMetricsUpdated?.Invoke(gameState.metrics);
        IsInitializing = false;
    }

    public void EndTurn()
    {
        gameState.EndTurn();
        currentTurn++;

        // Reset daily limit for the 3-times button
        EventPanelButtonHolder holder = FindObjectOfType<EventPanelButtonHolder>();
        if (holder != null)
        {
            holder.ResetDailyLimit();
        }

        onTurnChanged?.Invoke(currentTurn);
        onActionPointsChanged?.Invoke(gameState.currentActionPoints);
        onMetricsUpdated?.Invoke(gameState.metrics);

        //ToggleEndTurnButton(false);
    }

    public void ToggleEndTurnButton(bool state)
    {
        if (endTurnButton != null)
        {
            endTurnButton.interactable = state;
        }
    }
}

