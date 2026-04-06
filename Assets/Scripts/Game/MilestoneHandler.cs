using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MilestoneHandler : MonoBehaviour
{
    public Button milestoneButton;
    public Slider milestoneSlider;

    public Toggle milestone1;
    public Toggle milestone2;
    public Toggle milestone3;

    public Button milestone1Button;
    public Button milestone2Button;
    public Button milestone3Button;

    [SerializeField] private TMP_Text milestoneInstructionText;

    public int currentBiodiversity;
    public int maxBiodiversity;

    [SerializeField] private int tileCount;

    [SerializeField] private Sprite milestoneLockedSprite;
    [SerializeField] private Sprite milestoneOneAvailable;
    [SerializeField] private Sprite milestoneTwoAvailable;
    [SerializeField] private Sprite milestoneThreeAvailable;

    [SerializeField] private GameObject fadeObject;
    [SerializeField] private GameObject endScreen;

    // Popup system
    [SerializeField] private PopupHandler popupHandler;
    [SerializeField] private GameObject milestonePopup;
    [SerializeField] private TMP_Text milestonePopupText;

    private int targetBiodiversity;
    private float currentBiodiversityVisual;

    void Start()
    {
        currentBiodiversity = 0;
        maxBiodiversity = 100;

        if (milestone1 != null) milestone1.isOn = false;
        if (milestone2 != null) milestone2.isOn = false;
        if (milestone3 != null) milestone3.isOn = false;

        TurnManager.Instance.onTurnChanged.AddListener(ResetBiodiversity);

        milestone1Button.image.sprite = milestoneLockedSprite;
        milestone2Button.image.sprite = milestoneLockedSprite;
        milestone3Button.image.sprite = milestoneLockedSprite;

        milestone1Button.interactable = false;
        milestone2Button.interactable = false;
        milestone3Button.interactable = false;

        if (milestoneSlider != null)
        {
            milestoneSlider.interactable = false;
            milestoneSlider.minValue = 0f;
            milestoneSlider.maxValue = 1f;
            milestoneSlider.value = 0f;
        }

        UpdateMilestoneButtonLabels();
        UpdateMilestoneInstruction();
        RefreshBiodiversityNow();
    }

    void Update()
    {
        float step = 12f * Time.deltaTime;
        currentBiodiversityVisual = Mathf.MoveTowards(currentBiodiversityVisual, targetBiodiversity, step);
        currentBiodiversity = Mathf.RoundToInt(currentBiodiversityVisual);
        currentBiodiversity = Mathf.Clamp(currentBiodiversity, 0, maxBiodiversity);

        UpdateSlider();
    }

    // One-click milestone system
    public void ProgressMilestone(int milestone)
    {
        switch (milestone)
        {
            case 1:
                if (!milestone1.isOn && IsMilestone1Available())
                {
                    CompleteMilestone(milestone1, milestone1Button, 10);
                }
                break;

            case 2:
                if (!milestone2.isOn && IsMilestone2Available())
                {
                    CompleteMilestone(milestone2, milestone2Button, 15);
                }
                break;

            case 3:
                if (!milestone3.isOn && IsMilestone3Available())
                {
                    CompleteMilestone(milestone3, milestone3Button, 20);
                    StartEndSequence();
                }
                break;
        }

        UpdateSlider();
    }

    public float GetBiodiversity01()
    {
        if (maxBiodiversity <= 0)
            return 0f;

        return (float)currentBiodiversity / maxBiodiversity;
    }

    public void IncrementBiodiversity(int amount)
    {
        currentBiodiversity += amount;
        currentBiodiversity = Mathf.Clamp(currentBiodiversity, 0, maxBiodiversity);

        targetBiodiversity = currentBiodiversity; // keep smoothing in sync
    }

    // Handles reward + popup
    private void CompleteMilestone(Toggle toggle, Button button, int rewardAP)
    {
        toggle.isOn = true;
        button.interactable = false;

        // Give AP
        TurnManager.Instance.gameState.currentActionPoints += rewardAP;
        TurnManager.Instance.onActionPointsChanged.Invoke(
            TurnManager.Instance.gameState.currentActionPoints
        );

        ShowMilestonePopup(rewardAP);
    }

    // Popup display
    private void ShowMilestonePopup(int rewardAP)
    {
        if (popupHandler != null && milestonePopup != null)
        {
            if (milestonePopupText != null)
            {
                milestonePopupText.text = $"Milestone reached!\n+{rewardAP} Action Points";
            }

            popupHandler.OpenPopup(milestonePopup);
        }
    }

    private void ResetBiodiversity(int turn)
    {
        RefreshBiodiversityNow();
    }

    private void StartEndSequence()
    {
        fadeObject.GetComponent<Fader>().FadeScreen();
    }

    public void EnableEndScreen()
    {
        endScreen.SetActive(true);
    }

    private void UpdateSlider()
    {
        if (milestoneSlider != null)
        {
            milestoneSlider.value = Mathf.Clamp01((float)currentBiodiversity / maxBiodiversity);
        }

        UpdateMilestoneAvailability();
        UpdateMilestoneButtonLabels();
        UpdateMilestoneInstruction();
    }

    public void RefreshBiodiversityNow()
    {
        RecalculateBiodiversityFromScene(true);
    }

    private void RecalculateBiodiversityFromScene(bool forceUpdate = false)
    {
        gameTile[] tileObjects = FindObjectsOfType<gameTile>();

        int validTileCount = 0;
        int plantedTileCount = 0;
        int invasiveTileCount = 0;

        foreach (gameTile tile in tileObjects)
        {
            if (tile == null || tile.tileType == tileManager.TileType.Forest)
                continue;

            validTileCount++;

            if (tile.grownPlant != null)
                plantedTileCount++;

            tileWeedsGrowth weeds = tile.GetComponent<tileWeedsGrowth>();
            if (weeds != null && weeds.growStage >= 3)
                invasiveTileCount++;
        }

        float plantCoverage = validTileCount > 0 ? (float)plantedTileCount / validTileCount : 0f;
        float invasivePressure = validTileCount > 0 ? (float)invasiveTileCount / validTileCount : 0f;

        float target01 = Mathf.Clamp01((plantCoverage * 1.2f) - (invasivePressure * 0.6f));
        targetBiodiversity = Mathf.RoundToInt(target01 * maxBiodiversity);
    }

    private void UpdateMilestoneInstruction()
    {
        if (milestoneInstructionText == null) return;

        if (!milestone1.isOn)
        {
            milestoneInstructionText.text = $"Milestone 1: Reach {GetThreshold1()}+ biodiversity.";
            return;
        }

        if (!milestone2.isOn)
        {
            milestoneInstructionText.text = $"Milestone 2: Reach {GetThreshold2()}+ biodiversity.";
            return;
        }

        if (!milestone3.isOn)
        {
            milestoneInstructionText.text = $"Milestone 3: Reach {GetThreshold3()}+ biodiversity.";
            return;
        }

        milestoneInstructionText.text = "All milestones completed.";
    }

    private void UpdateMilestoneButtonLabels()
    {
        SetMilestoneButtonText(milestone1Button, milestone1.isOn, GetThreshold1());
        SetMilestoneButtonText(milestone2Button, milestone2.isOn, GetThreshold2());
        SetMilestoneButtonText(milestone3Button, milestone3.isOn, GetThreshold3());
    }

    private static void SetMilestoneButtonText(Button button, bool completed, int threshold)
    {
        if (button == null) return;

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null) return;

        if (completed)
        {
            text.text = "Done";
        }
        else
        {
            text.text = $"Claim\nBio {threshold}+";
        }
    }

    private void UpdateMilestoneAvailability()
    {
        SetMilestoneAvailability(milestone1Button, milestone1, IsMilestone1Available(), milestoneOneAvailable);
        SetMilestoneAvailability(milestone2Button, milestone2, IsMilestone2Available(), milestoneTwoAvailable);
        SetMilestoneAvailability(milestone3Button, milestone3, IsMilestone3Available(), milestoneThreeAvailable);
    }

    private void SetMilestoneAvailability(Button button, Toggle toggle, bool available, Sprite sprite)
    {
        if (button == null || toggle == null) return;

        if (toggle.isOn)
        {
            button.interactable = false;
            return;
        }

        button.interactable = available;
        button.image.sprite = available ? sprite : milestoneLockedSprite;
    }

    private bool IsMilestone1Available() => currentBiodiversity >= GetThreshold1();
    private bool IsMilestone2Available() => milestone1.isOn && currentBiodiversity >= GetThreshold2();
    private bool IsMilestone3Available() => milestone2.isOn && currentBiodiversity >= GetThreshold3();

    private int GetThreshold1() => Mathf.CeilToInt(maxBiodiversity * 0.33f);
    private int GetThreshold2() => Mathf.CeilToInt(maxBiodiversity * 0.66f);
    private int GetThreshold3() => Mathf.CeilToInt(maxBiodiversity);
}