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

    public int milestone1Progress;
    public int milestone2Progress;
    public int milestone3Progress;

    public int totalMilestoneProgress;
    public int highestMilestoneReached;

    public int currentBiodiversity;
    public int maxBiodiversity;

    [SerializeField] private bool useSimulationBiodiversity = false;

    [SerializeField] private int tileCount;
    [SerializeField] private Sprite milestoneLockedSprite;
    [SerializeField] private Sprite milestoneOneAvailable;
    [SerializeField] private Sprite milestoneTwoAvailable;
    [SerializeField] private Sprite milestoneThreeAvailable;

    // NEW POPUP REFERENCES
    public GameObject milestone1Popup;
    public GameObject milestone2Popup;
    public GameObject milestone3Popup;

    // kept because you said not to remove mechanics
    private bool milestone1reward = false;
    [SerializeField] private GameObject cowCollection;

    [SerializeField] private GameObject fadeObject;
    [SerializeField] private GameObject endScreen;

    [SerializeField] private bool useSceneBiodiversityRecalculation = true;
    [SerializeField] private bool recalculateBiodiversityContinuously = false;
    [SerializeField] private float biodiversityRecalculateInterval = 0.25f;
    [SerializeField] private float biodiversitySmoothingPerSecond = 12f;

    private bool biodiversityUiRefreshScheduled;
    private float biodiversityRecalculateTimer;
    private bool isRecalculatingFromScene;
    private int targetBiodiversity;
    private float currentBiodiversityVisual;

    private WetlandProgressionManager wetlandProgressionManager;
    private bool simulationBound;

    void Start()
    {
        useSimulationBiodiversity = false;
        useSceneBiodiversityRecalculation = true;
        recalculateBiodiversityContinuously = false;

        currentBiodiversity = 0;
        milestone1Progress = 0;
        milestone2Progress = 0;
        milestone3Progress = 0;
        totalMilestoneProgress = 0;
        highestMilestoneReached = 0;
        targetBiodiversity = 0;
        currentBiodiversityVisual = 0f;

        if (milestone1 != null) milestone1.isOn = false;
        if (milestone2 != null) milestone2.isOn = false;
        if (milestone3 != null) milestone3.isOn = false;

        TurnManager.Instance.onTurnChanged.AddListener(ResetBiodiversity);
        TurnManager.Instance.onTurnChanged.AddListener(SpawnMilestoneReward);

        milestone1Button.image.sprite = milestoneLockedSprite;
        milestone2Button.image.sprite = milestoneLockedSprite;
        milestone3Button.image.sprite = milestoneLockedSprite;

        milestone1Button.interactable = false;
        milestone2Button.interactable = false;
        milestone3Button.interactable = false;

        if (useSceneBiodiversityRecalculation)
        {
            tileCount = 0;
            maxBiodiversity = 100;
        }
        else if (!TryBindSimulationProgression())
        {
            tileCount = Mathf.Max(1, GameObject.FindGameObjectsWithTag("Tile").Length - 1);
            maxBiodiversity = Mathf.Max(1, tileCount * 4);
        }
        else
        {
            maxBiodiversity = 100;
            currentBiodiversity = Mathf.Clamp(wetlandProgressionManager.State.plantBalance, 0, maxBiodiversity);
        }

        if (milestoneSlider != null)
        {
            milestoneSlider.interactable = false;
            milestoneSlider.minValue = 0f;
            milestoneSlider.maxValue = 1f;
            milestoneSlider.wholeNumbers = false;
            milestoneSlider.value = 0f;
        }

        UpdateMilestoneButtonLabels();
        UpdateMilestoneInstruction();

        if (useSceneBiodiversityRecalculation)
        {
            RefreshBiodiversityNow();
        }
        else
        {
            UpdateSlider();
        }
    }

    void Update()
    {
        if (useSimulationBiodiversity)
        {
            SyncFromSimulationProgression();
            return;
        }

        if (useSceneBiodiversityRecalculation)
        {
            int previous = currentBiodiversity;
            float step = Mathf.Max(1f, biodiversitySmoothingPerSecond) * Time.deltaTime;

            currentBiodiversityVisual = Mathf.MoveTowards(currentBiodiversityVisual, targetBiodiversity, step);
            currentBiodiversity = Mathf.RoundToInt(currentBiodiversityVisual);
            currentBiodiversity = Mathf.Clamp(currentBiodiversity, 0, maxBiodiversity);

            if (currentBiodiversity != previous)
                UpdateSlider();

            return;
        }

        if (recalculateBiodiversityContinuously)
        {
            biodiversityRecalculateTimer -= Time.deltaTime;
            if (biodiversityRecalculateTimer <= 0f)
            {
                biodiversityRecalculateTimer = Mathf.Max(0.05f, biodiversityRecalculateInterval);
                RecalculateBiodiversityFromScene();
            }
        }
    }

    private void SpawnMilestoneReward(int turnNum)
    {
        milestone1reward = false;
    }

    // POPUP HELPER
    private void ShowMilestonePopup(GameObject popup)
    {
        if (popup != null)
        {
            MilestonePopupHandler popupHandler = FindObjectOfType<MilestonePopupHandler>();
            if (popupHandler != null)
                popupHandler.OpenMilestonePopup(popup);

            else
                popup.SetActive(true);
        }
    }

    // NEW: SINGLE CLICK MILESTONE LOGIC
    public void ProgressMilestone(int milestone)
    {
        switch (milestone)
        {
            case 1:
                if (!milestone1.isOn && IsMilestone1Available())
                {
                    milestone1.isOn = true;
                    milestone1Button.interactable = false;

                    TurnManager.Instance.gameState.currentActionPoints += 10;
                    ShowMilestonePopup(milestone1Popup);
                }
                break;

            case 2:
                if (!milestone2.isOn && IsMilestone2Available())
                {
                    milestone2.isOn = true;
                    milestone2Button.interactable = false;

                    TurnManager.Instance.gameState.currentActionPoints += 15;
                    ShowMilestonePopup(milestone2Popup);
                }
                break;

            case 3:
                if (!milestone3.isOn && IsMilestone3Available())
                {
                    milestone3.isOn = true;
                    milestone3Button.interactable = false;

                    TurnManager.Instance.gameState.currentActionPoints += 20;
                    ShowMilestonePopup(milestone3Popup);

                    StartEndSequence();
                }
                break;
        }

        TurnManager.Instance.onActionPointsChanged.Invoke(TurnManager.Instance.gameState.currentActionPoints);
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        if (milestoneSlider != null)
        {
            milestoneSlider.value = Mathf.Clamp01(Biodiversity01());
        }

        UpdateMilestoneAvailability();
        UpdateMilestoneButtonLabels();
        UpdateMilestoneInstruction();
        PublishMetricsFromBiodiversity();
    }

    private void ResetBiodiversity(int random)
    {
        biodiversityUiRefreshScheduled = false;
        CancelInvoke(nameof(FlushScheduledBiodiversityUiRefresh));

        if (useSimulationBiodiversity && TryBindSimulationProgression())
        {
            SyncFromSimulationProgression();
            return;
        }

        if (useSceneBiodiversityRecalculation)
        {
            RefreshBiodiversityNow();
            return;
        }

        if (recalculateBiodiversityContinuously)
        {
            RecalculateBiodiversityFromScene();
            return;
        }

        currentBiodiversity = 0;
        Invoke("UpdateSlider", 0.6f);
    }

    private void StartEndSequence()
    {
        fadeObject.GetComponent<Fader>().FadeScreen();
    }

    public void EnableEndScreen()
    {
        endScreen.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void IncrementBiodiversity(int amount)
    {
        if (useSimulationBiodiversity && TryBindSimulationProgression())
            return;

        if (useSceneBiodiversityRecalculation)
            return;

        if (recalculateBiodiversityContinuously)
        {
            RecalculateBiodiversityFromScene();
            return;
        }

        currentBiodiversity += amount;
        currentBiodiversity = Mathf.Max(0, currentBiodiversity);
        ScheduleBiodiversityUiRefresh();
    }

    public void RefreshBiodiversityNow()
    {
        RecalculateBiodiversityFromScene(forceUpdate: true);
    }

    private float Biodiversity01()
    {
        if (maxBiodiversity <= 0)
            return 0f;

        return (float)currentBiodiversity / maxBiodiversity;
    }

    private void ScheduleBiodiversityUiRefresh()
    {
        if (biodiversityUiRefreshScheduled)
            return;

        biodiversityUiRefreshScheduled = true;
        Invoke(nameof(FlushScheduledBiodiversityUiRefresh), 0f);
    }

    private void FlushScheduledBiodiversityUiRefresh()
    {
        biodiversityUiRefreshScheduled = false;
        UpdateSlider();
    }

    private void PublishMetricsFromBiodiversity()
    {
        TurnManager manager = TurnManager.Instance;
        if (manager == null || manager.gameState == null || manager.gameState.metrics == null)
            return;

        float biodiversity01 = Mathf.Clamp01(Biodiversity01());
        float biodiversity = biodiversity01 * 100f;
        float hauki = Mathf.Clamp01((biodiversity01 - 0.20f) / 0.80f) * 100f;
        float pollution = Mathf.Clamp((1f - biodiversity01) * 100f, 0f, 100f);

        manager.gameState.metrics[MetricType.BiodiversityLevel] = biodiversity;
        manager.gameState.metrics[MetricType.WaterQuality] = hauki;
        manager.gameState.metrics[MetricType.PollutionLevel] = pollution;
        manager.onMetricsUpdated?.Invoke(manager.gameState.metrics);
    }

    private void RecalculateBiodiversityFromScene(bool forceUpdate = false)
    {
        if (isRecalculatingFromScene)
            return;

        isRecalculatingFromScene = true;

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

        tileCount = validTileCount;
        maxBiodiversity = 100;

        float plantCoverage = validTileCount > 0 ? (float)plantedTileCount / validTileCount : 0f;
        float invasivePressure = validTileCount > 0 ? (float)invasiveTileCount / validTileCount : 0f;

        float target01 = Mathf.Clamp01((plantCoverage * 1.20f) - (invasivePressure * 0.60f));

        int previousTarget = targetBiodiversity;
        targetBiodiversity = Mathf.Clamp(Mathf.RoundToInt(target01 * maxBiodiversity), 0, maxBiodiversity);

        currentBiodiversityVisual = Mathf.Clamp(currentBiodiversityVisual, 0f, maxBiodiversity);

        if (forceUpdate || targetBiodiversity != previousTarget)
            UpdateSlider();

        isRecalculatingFromScene = false;
    }

    public float GetBiodiversity01()
    {
        return Biodiversity01();
    }

    private void UpdateMilestoneInstruction()
    {
        if (milestoneInstructionText == null)
            return;

        int threshold1 = GetThreshold1();
        int threshold2 = GetThreshold2();
        int threshold3 = GetThreshold3();

        if (!milestone1.isOn)
        {
            milestoneInstructionText.text = $"Milestone 1: Reach biodiversity {threshold1}+ then press once.";
            return;
        }

        if (!milestone2.isOn)
        {
            milestoneInstructionText.text = $"Milestone 2: Reach biodiversity {threshold2}+ then press once.";
            return;
        }

        if (!milestone3.isOn)
        {
            milestoneInstructionText.text = $"Milestone 3: Reach biodiversity {threshold3}+ then press once.";
            return;
        }

        milestoneInstructionText.text = "All milestones completed.";
    }

    private void UpdateMilestoneButtonLabels()
    {
        int threshold1 = GetThreshold1();
        int threshold2 = GetThreshold2();
        int threshold3 = GetThreshold3();

        SetMilestoneButtonText(milestone1Button, milestone1.isOn, threshold1);
        SetMilestoneButtonText(milestone2Button, milestone2.isOn, threshold2);
        SetMilestoneButtonText(milestone3Button, milestone3.isOn, threshold3);
    }

    private static void SetMilestoneButtonText(Button button, bool completed, int threshold)
    {
        if (button == null)
            return;

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null)
            return;

        if (completed)
        {
            text.text = "Done";
            return;
        }

        text.text = $"Press (AP)\nBio {threshold}+";
    }

    private void UpdateMilestoneAvailability()
    {
        SetMilestoneAvailability(milestone1Button, milestone1, IsMilestone1Available(), milestoneOneAvailable);
        SetMilestoneAvailability(milestone2Button, milestone2, IsMilestone2Available(), milestoneTwoAvailable);
        SetMilestoneAvailability(milestone3Button, milestone3, IsMilestone3Available(), milestoneThreeAvailable);
    }

    private void SetMilestoneAvailability(Button button, Toggle milestoneToggle, bool isAvailable, Sprite availableSprite)
    {
        if (button == null || milestoneToggle == null)
            return;

        if (milestoneToggle.isOn)
        {
            button.interactable = false;
            return;
        }

        button.interactable = isAvailable;
        button.image.sprite = isAvailable ? availableSprite : milestoneLockedSprite;
    }

    private bool IsMilestone1Available()
    {
        return currentBiodiversity >= GetThreshold1();
    }

    private bool IsMilestone2Available()
    {
        if (!milestone1.isOn)
            return false;

        return currentBiodiversity >= GetThreshold2();
    }

    private bool IsMilestone3Available()
    {
        if (!milestone2.isOn)
            return false;

        return currentBiodiversity >= GetThreshold3();
    }

    private int GetThreshold1()
    {
        return Mathf.CeilToInt(maxBiodiversity * 0.33f);
    }

    private int GetThreshold2()
    {
        return Mathf.CeilToInt(maxBiodiversity * 0.66f);
    }

    private int GetThreshold3()
    {
        return Mathf.CeilToInt(maxBiodiversity * 1.00f);
    }

    private bool TryBindSimulationProgression()
    {
        if (!useSimulationBiodiversity)
            return false;

        if (wetlandProgressionManager != null)
            return true;

        wetlandProgressionManager = WetlandProgressionManager.Instance;
        if (wetlandProgressionManager == null)
            return false;

        if (!simulationBound)
        {
            wetlandProgressionManager.onStateChanged.AddListener(SyncFromSimulationProgression);
            simulationBound = true;
        }

        return true;
    }

    private void SyncFromSimulationProgression()
    {
        if (!TryBindSimulationProgression())
            return;

        WetlandSimulationState state = wetlandProgressionManager.State;
        if (state == null)
            return;

        maxBiodiversity = 100;
        currentBiodiversity = Mathf.Clamp(state.plantBalance, 0, maxBiodiversity);
        UpdateSlider();
    }

    private void OnDestroy()
    {
        if (simulationBound && wetlandProgressionManager != null)
        {
            wetlandProgressionManager.onStateChanged.RemoveListener(SyncFromSimulationProgression);
        }
    }
}
