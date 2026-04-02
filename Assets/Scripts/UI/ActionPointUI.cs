using TMPro;
using UnityEngine;

public class ActionPointUI : MonoBehaviour
{
    private TurnManager turnManager;
    [SerializeField] private TMP_Text apValueText;
    private int currentPoints = 0;

    private void Start()
    {
        turnManager = TurnManager.Instance;

        turnManager.onActionPointsChanged.AddListener((int points) =>
        {
            currentPoints = points;
            if (this.isActiveAndEnabled) 
                UpdateActionPointsUI(currentPoints);
        });
    }

    private void OnEnable()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.gameState != null)
        {
            currentPoints = TurnManager.Instance.gameState.currentActionPoints;
            UpdateActionPointsUI(currentPoints);
        }
        else
        {
            UpdateActionPointsUI(currentPoints); 
        }
    }
    
    private void UpdateActionPointsUI(int actionPoints)
    {
        if (apValueText != null)
        {
            apValueText.text = actionPoints.ToString();
        }
    }
}