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

    public UnityEvent<int> onTurnChanged;
    public UnityEvent<int> onActionPointsChanged;
    public UnityEvent<Dictionary<MetricType, float>> onMetricsUpdated;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private bool enableSimulationIntegration = false;
    //and more...


    //create global instance so we can access this easily
    private void Awake()
    {   
        milestoneHandler = GetComponent<MilestoneHandler>();
        
        if(Instance == null && Instance != this)
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
    //make sure initial event subscriptions go through before sending them
    private IEnumerator DelayedInitialize()
    {
        yield return null;
        Initialize();
    }
    //invoke events at game start to update all base values etc.
    private void Initialize()
    {
        onTurnChanged?.Invoke(currentTurn);
        onActionPointsChanged?.Invoke(gameState.currentActionPoints);
        onMetricsUpdated?.Invoke(gameState.metrics);
    }
    //check if action can be executed and execute it, trigger for example from a button
    /*
    public void ExecuteAction(WetlandAction action)
    {
        if(action.CanBeExecuted(gameState))
        {
            action.ExecuteAction(gameState);
            onActionPointsChanged?.Invoke(gameState.currentActionPoints);
        }
    }
    */
    //handle end turn logic and trigger events to update UI etc. for next turn 
    public void EndTurn()
    {
        gameState.EndTurn();
        currentTurn++;
        onTurnChanged?.Invoke(currentTurn);
        onActionPointsChanged?.Invoke(gameState.currentActionPoints);
        onMetricsUpdated?.Invoke(gameState.metrics);
        ToggleEndTurnButton(false);
    }
    public void ToggleEndTurnButton(bool state)
    {
        if(endTurnButton != null)
        {
            endTurnButton.interactable = state;
        }
    }
    
}
