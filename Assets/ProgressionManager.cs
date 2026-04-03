using UnityEngine;
using UnityEngine.UI;

public class ProgressionManager : MonoBehaviour
{
    [Header("Day 1: Tools & Reeds")]
    public Button cutPlantsButton;
    public Button openPlantsButton;
    public Button[] reedPlantButtons;

    [Header("Day 2: Economy & Cows")]
    public Button shopButton;
    public Button questionsButton;
    public GameObject coinDisplay;

    [Header("Day 3: Floating Plants")]
    public Button[] floatingPlantButtons;

    [Header("Day 5: Guide")]
    public Button wetlandGuideButton;

    // Awake is completely removed to prevent premature locking.

    private void Start()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.onTurnChanged.AddListener(HandleDayChanged);

            // Perform a clean initialization without blanket-disabling first
            int day = TurnManager.Instance.CurrentTurn;
            bool passedDay1 = day >= 1;
            bool passedDay2 = day >= 2;
            bool passedDay3 = day >= 2;
            bool passedDay5 = day >= 5;

            SetInteractable(passedDay1, cutPlantsButton, openPlantsButton);
            SetInteractable(passedDay1, reedPlantButtons);
            SetInteractable(passedDay2, shopButton, questionsButton);
            SetActive(passedDay2, coinDisplay, shopButton?.gameObject);
            SetInteractable(passedDay3, floatingPlantButtons);
            SetInteractable(passedDay5, wetlandGuideButton);
        }
    }
    

    private void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.onTurnChanged.RemoveListener(HandleDayChanged);
        }
    }

    private void HandleDayChanged(int day)
    {
        // Unlock things progressively as the days advance in-game
        if (day >= 1) UnlockDay1();
        if (day >= 2) UnlockDay2();
        if (day >= 3) UnlockDay3();
        if (day >= 5) UnlockDay5();
    }

    private void UnlockDay1()
    {
        SetInteractable(true, cutPlantsButton, openPlantsButton);
        SetInteractable(true, reedPlantButtons);
    }

    private void UnlockDay2()
    {
        SetInteractable(true, shopButton, questionsButton);
        SetActive(true, coinDisplay, shopButton?.gameObject);
    }

    private void UnlockDay3()
    {
        SetInteractable(true, floatingPlantButtons);
    }

    private void UnlockDay5()
    {
        SetInteractable(true, wetlandGuideButton);
    }

    private void SetInteractable(bool state, params Button[] buttons)
    {
        if (buttons == null)
            return;
            
        foreach (var btn in buttons)
        {
            if (btn != null) btn.interactable = state;
        }
    }

    private void SetActive(bool state, params GameObject[] objects)
    {
        if (objects == null)
            return;
            
        foreach (var obj in objects)
        {
            if (obj != null) obj.SetActive(state);
        }
    }
}