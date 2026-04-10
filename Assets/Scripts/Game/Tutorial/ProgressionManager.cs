using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    [Header("Day References")]
    [SerializeField] private DayTaskBase day1Tasks;
    [SerializeField] private DayTaskBase day2Tasks;
    [SerializeField] private DayTaskBase day3Tasks;
    [SerializeField] private DayTaskBase day4Tasks;
    [SerializeField] private DayTaskBase day5Tasks;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        day1Tasks.EndDay();
        day2Tasks.EndDay();
        day3Tasks.EndDay();
        day4Tasks.EndDay();
        day5Tasks.EndDay();

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.onTurnChanged.AddListener(OnTurnChanged);
        }
        else
        {
            Debug.LogError("ProgressionManager could not find TurnManager.Instance!");
        }
    }

    private void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.onTurnChanged.RemoveListener(OnTurnChanged);
        }
    }

    private void OnTurnChanged(int currentTurn)
    {
        StartDay(currentTurn);
    }

    public void StartDay(int dayNumber)
    {
        switch (dayNumber)
        {
            case 1: day1Tasks.StartDay();  break;
            case 2: day1Tasks.EndDay(); day2Tasks.StartDay(); break;
            case 3: day2Tasks.EndDay(); day3Tasks.StartDay(); break;
            case 4: day3Tasks.EndDay(); day4Tasks.StartDay(); break;
            case 5: day4Tasks.EndDay(); day5Tasks.StartDay(); break;
            default: Debug.Log($"No tasks assigned for Day {dayNumber}"); break;
        }
    }
}