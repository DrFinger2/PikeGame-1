using UnityEngine;
using UnityEngine.Events;

public class WetlandProgressionManager : MonoBehaviour
{
    public static WetlandProgressionManager Instance;

    [Header("Config")]
    [SerializeField] private WetlandBalanceConfig balanceConfig;
    [SerializeField] private string defaultConfigResourcePath = "WetlandBalanceConfig";
    [SerializeField] private WetlandSimulationState state = new WetlandSimulationState();

    [Header("Behavior")]
    [SerializeField] private bool simulationEnabled = false;
    [SerializeField] private bool initializeFromConfigOnStart = true;
    [SerializeField] private bool resolveOnTurnChanged = true;
    [SerializeField] private bool syncLegacyMetrics = true;
    [SerializeField] private bool verboseLogging = true;

    [Header("Events")]
    public UnityEvent onStateChanged;
    public UnityEvent<int> onStabilityChanged;
    public UnityEvent<int> onCoinsChanged;
    public UnityEvent<string> onDayResolutionSummary;

    public WetlandSimulationState State => state;

    private int lastResolvedDay = 1;
    private TurnManager boundTurnManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (!simulationEnabled)
        {
            enabled = false;
            return;
        }

        EnsureConfig();

        if (initializeFromConfigOnStart)
        {
            ResetStateFromConfig();
        }
        else
        {
            state.ClampAll(StatMin, StatMax);
            lastResolvedDay = Mathf.Max(1, state.day);
            SyncLegacyMetrics();
            DispatchStateEvents();
        }

        TryBindTurnManager();
    }

    private void Update()
    {
        if (boundTurnManager == null)
        {
            TryBindTurnManager();
        }
    }

    private void OnDestroy()
    {
        if (boundTurnManager != null)
        {
            boundTurnManager.onTurnChanged.RemoveListener(HandleTurnChanged);
        }
    }

    public void ResetStateFromConfig()
    {
        EnsureConfig();
        if (balanceConfig == null)
        {
            Debug.LogWarning("WetlandProgressionManager: Missing WetlandBalanceConfig.");
            return;
        }

        state = new WetlandSimulationState
        {
            day = Mathf.Max(1, balanceConfig.startingDay),
            coins = Mathf.Max(0, balanceConfig.startingCoins),
            actionPoints = Mathf.Max(0, balanceConfig.startingActionPoints),

            waterClarity = balanceConfig.startingWaterClarity,
            oxygen = balanceConfig.startingOxygen,
            plantBalance = balanceConfig.startingPlantBalance,
            animalBalance = balanceConfig.startingAnimalBalance,

            invasiveCount = Mathf.Max(0, balanceConfig.startingInvasiveCount),
            grassCount = Mathf.Max(0, balanceConfig.startingGrassCount),
            reedCount = Mathf.Max(0, balanceConfig.startingReedCount),
            floatingLeafCount = Mathf.Max(0, balanceConfig.startingFloatingLeafCount),
            submergedCount = Mathf.Max(0, balanceConfig.startingSubmergedCount)
        };

        state.stability = Mathf.RoundToInt((state.waterClarity + state.oxygen + state.plantBalance + state.animalBalance) / 4f);
        state.ClampAll(StatMin, StatMax);
        lastResolvedDay = state.day;

        SyncLegacyMetrics();
        DispatchStateEvents();
    }

    public void RegisterPlantPlaced(WetlandPlantType plantType, int amount = 1)
    {
        amount = Mathf.Max(0, amount);
        if (amount == 0)
        {
            return;
        }

        switch (plantType)
        {
            case WetlandPlantType.Grass:
                state.grassCount += amount;
                break;
            case WetlandPlantType.Reed:
                state.reedCount += amount;
                break;
            case WetlandPlantType.FloatingLeaf:
                state.floatingLeafCount += amount;
                break;
            case WetlandPlantType.Submerged:
                state.submergedCount += amount;
                break;
        }

        state.ClampAll(StatMin, StatMax);
        DispatchStateEvents();
    }

    public void RegisterInvasiveRemoved(int amount = 1)
    {
        amount = Mathf.Max(0, amount);
        if (amount == 0)
        {
            return;
        }

        state.invasiveCount = Mathf.Max(0, state.invasiveCount - amount);
        state.ClampAll(StatMin, StatMax);
        DispatchStateEvents();
    }

    public void RegisterInvasiveAdded(int amount = 1)
    {
        amount = Mathf.Max(0, amount);
        if (amount == 0)
        {
            return;
        }

        state.invasiveCount += amount;
        state.ClampAll(StatMin, StatMax);
        DispatchStateEvents();
    }

    public void RegisterQuizResult(bool answeredCorrectly)
    {
        if (balanceConfig == null)
        {
            return;
        }

        int reward = answeredCorrectly ? balanceConfig.quizCorrectCoinReward : balanceConfig.quizWrongCoinReward;
        AddCoins(reward);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        state.coins += amount;
        DispatchStateEvents();
    }

    public bool TrySpendCoins(int cost)
    {
        if (cost <= 0)
        {
            return true;
        }

        if (state.coins < cost)
        {
            return false;
        }

        state.coins -= cost;
        DispatchStateEvents();
        return true;
    }

    private void HandleTurnChanged(int newTurn)
    {
        EnsureConfig();
        if (!resolveOnTurnChanged || balanceConfig == null)
        {
            return;
        }

        if (newTurn <= lastResolvedDay)
        {
            return;
        }

        if (state.gameWon || state.gameLost)
        {
            lastResolvedDay = newTurn;
            return;
        }

        WetlandDayResolution resolution = WetlandDayResolver.Resolve(state, balanceConfig, newTurn);
        lastResolvedDay = state.day;

        SyncLegacyMetrics();
        DispatchStateEvents();

        string summary = resolution.GetSummary(state.day, state.stability);
        onDayResolutionSummary?.Invoke(summary);

        if (verboseLogging)
        {
            Debug.Log(summary);
        }
    }

    private void SyncLegacyMetrics()
    {
        if (!syncLegacyMetrics)
        {
            return;
        }

        TurnManager targetTurnManager = boundTurnManager != null ? boundTurnManager : TurnManager.Instance;
        if (targetTurnManager == null || targetTurnManager.gameState == null || targetTurnManager.gameState.metrics == null)
        {
            return;
        }

        targetTurnManager.gameState.metrics[MetricType.WaterQuality] = state.waterClarity;
        targetTurnManager.gameState.metrics[MetricType.PollutionLevel] = 100 - state.waterClarity;
        targetTurnManager.gameState.metrics[MetricType.BiodiversityLevel] = state.plantBalance;

        targetTurnManager.onMetricsUpdated?.Invoke(targetTurnManager.gameState.metrics);
    }

    private void DispatchStateEvents()
    {
        onStateChanged?.Invoke();
        onStabilityChanged?.Invoke(state.stability);
        onCoinsChanged?.Invoke(state.coins);
    }

    private void EnsureConfig()
    {
        if (balanceConfig != null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(defaultConfigResourcePath))
        {
            balanceConfig = Resources.Load<WetlandBalanceConfig>(defaultConfigResourcePath);
        }

        if (balanceConfig == null)
        {
            balanceConfig = ScriptableObject.CreateInstance<WetlandBalanceConfig>();
            if (verboseLogging)
            {
                Debug.LogWarning("WetlandProgressionManager: No balance asset found in Resources. Using runtime default values.");
            }
        }
    }

    private void TryBindTurnManager()
    {
        if (boundTurnManager != null)
        {
            return;
        }

        TurnManager manager = TurnManager.Instance;
        if (manager == null)
        {
            return;
        }

        boundTurnManager = manager;
        boundTurnManager.onTurnChanged.AddListener(HandleTurnChanged);

        if (verboseLogging)
        {
            Debug.Log("WetlandProgressionManager: Bound to TurnManager.");
        }
    }

    private int StatMin => balanceConfig != null ? balanceConfig.minStatValue : 0;
    private int StatMax => balanceConfig != null ? balanceConfig.maxStatValue : 100;

    public void EnableSimulation(bool enabledState)
    {
        simulationEnabled = enabledState;
        enabled = enabledState;
    }
}
