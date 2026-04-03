using UnityEngine;
using UnityEngine.UI;

public class ProgressionManager : MonoBehaviour
{
    [Header("Day 1: Tools & Reeds")]
    public Button cutPlantsButton;
    public Button openPlantsButton;
    public ShowButtons openPlantsMenu;
    public Button[] reedPlantButtons;

    [Header("Day 2: Economy & Cows")]
    public Button shopButton;
    public Button questionsButton;
    public GameObject coinDisplay;

    [Header("Day 3: Floating Plants")]
    public Button[] floatingPlantButtons;

    [Header("Day 5: Guide")]
    public Button wetlandGuideButton;

    // storing the state
    bool day1Unlocked = false;
    bool day2Unlocked = false;
    bool day3Unlocked = false;
    bool day5Unlocked = false;

    // Awake is completely removed to prevent premature locking.

    private void Start()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.onTurnChanged.AddListener(HandleDayChanged);

            // Perform a clean initialization without blanket-disabling first
            int day = TurnManager.Instance.CurrentTurn;
            day1Unlocked = day >= 1;
            day2Unlocked = day >= 2;
            day3Unlocked = day >= 2;
            day5Unlocked = day >= 5;

            SetInteractable(day1Unlocked, cutPlantsButton, openPlantsButton);
            SetInteractable(day1Unlocked, reedPlantButtons);
            SetInteractable(day2Unlocked, shopButton, questionsButton);
            SetActive(day2Unlocked, coinDisplay, shopButton?.gameObject);
            SetInteractable(day3Unlocked, floatingPlantButtons);
            SetInteractable(day5Unlocked, wetlandGuideButton);
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
        if (!day1Unlocked)
        {
            SetInteractable(true, cutPlantsButton, openPlantsButton);
            SetInteractable(true, reedPlantButtons);
            day1Unlocked = true;
        }
    }
    
    private void UnlockDay2()
    {
        if (!day2Unlocked)
        {
            SetInteractable(true, shopButton, questionsButton);
            SetActive(true, coinDisplay, shopButton?.gameObject);
            day2Unlocked = true;
        }
    }

    private void UnlockDay3()
    {
        if (!day3Unlocked)
        {
            SetInteractable(true, floatingPlantButtons);
            if (openPlantsMenu != null && !openPlantsMenu.IsOpen && openPlantsButton != null)
            {
                UnlockableButton unlockable = openPlantsButton.GetComponent<UnlockableButton>();
                if (unlockable != null)
                {
                    unlockable.ReHighlight();
                }
            }
            day3Unlocked = true;
        }
    }

    private void UnlockDay5()
    {
        if (!day5Unlocked)
        {
            SetInteractable(true, wetlandGuideButton);
            day5Unlocked = true;
        }
        
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