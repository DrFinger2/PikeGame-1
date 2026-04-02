using UnityEngine;
using UnityEngine.UI;

public class DayProgressionManager : MonoBehaviour
{
    [Header("Day 1: Tools & Reeds")]
    public Button cutPlantsButton;
    public Button  openPlantsButton;
    public Button[] reedPlantButtons;

    [Header("Day 2: Economy & Cows")]
    public Button shopButton;
    public GameObject coinDisplay;

    [Header("Day 3: Floating Plantss")]
    //public GameObject smallFishFeature;
    public Button[] floatingPlantButtons;

    [Header("Day 5: Guide")]
    public Button wetlandGuideButton;

    private void Awake()
    {
        LockFeatures();

    }
    
    private void Start()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.onTurnChanged.AddListener(HandleDayChanged);
            HandleDayChanged(TurnManager.Instance.CurrentTurn);
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
        if (day >= 1) UnlockDay1();
        if (day >= 2) UnlockDay2();
        if (day >= 3) UnlockDay3();
        if (day >= 5) UnlockDay5();
    }

    private void LockFeatures()
    {
        SetInteractable(false, cutPlantsButton, openPlantsButton);
        SetInteractable(false, reedPlantButtons);
        SetInteractable(false, shopButton);
        SetActive(false, coinDisplay, shopButton?.gameObject /*, smallFishFeature*/);
        SetInteractable(false, floatingPlantButtons);
        SetInteractable(false, wetlandGuideButton);
    }

    private void UnlockDay1()
    {
        SetInteractable(true, cutPlantsButton, openPlantsButton);
        SetInteractable(true, reedPlantButtons);
    }

    private void UnlockDay2()
    {
        SetInteractable(true, shopButton);
        SetActive(true, coinDisplay, shopButton?.gameObject);
    }

    private void UnlockDay3()
    {
        /*SetActive(true, smallFishFeature);*/
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