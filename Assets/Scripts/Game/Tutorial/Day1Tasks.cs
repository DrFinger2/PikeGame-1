using UnityEngine;
using UnityEngine.UI;



public class Day1Tasks : DayTaskBase
{
    [Header("Managers & Systems")]
    [SerializeField] private MilestoneHandler milestoneHandler;

    [Header("UI Elements")]
    [SerializeField] private ActionButtonsUI actionButtons;
    [SerializeField] private Button nextDayButton;

    [Header("Dialogue References")]
    [SerializeField] private TutorialDialogue day1StartDialogue;
    [SerializeField] private TutorialDialogue day1ReedsClearedDialogue;
    [SerializeField] private TutorialDialogue day1ReedsPlantedDialogue;

    [Header("Settings")]
    [SerializeField] private float requiredPlantAmount = 2f;
    int plantsPlaced = 0;

    public override void StartDay()
    {

        Events.OnDayStarted.Invoke();
        this.enabled = true;

        DialogueManager dialogue = DialogueManager.instance;
        actionButtons.LockAll();

        // Kicks off Chain 1: 01 -> 02 -> 03 -> 04 -> 05 -> 06
        dialogue.PlayTutorialNode(day1StartDialogue, () =>
        {
            PlantEvents.OnPlantRemoved += OnPlantRemoved;
            actionButtons.CutPlants.Button.interactable = true;
        });
    }

    public override void EndDay()
    {
        Events.OnDayEnded.Invoke();
        this.enabled = false;
    }

    public void OnReedsClearedClicked()
    {
        DialogueManager dialogue = DialogueManager.instance;

        milestoneHandler.ForceUnlockMilestone(level: 1, progress: 0.33f);
        actionButtons.CutPlants.Button.interactable = false;

        dialogue.CompleteTask("E1");
        dialogue.PlayTutorialNode(day1ReedsClearedDialogue, () =>
        {
            PlantEvents.OnPlantPlaced += OnPlantPlaced;
            actionButtons.OpenPlants.Button.interactable = true;
            actionButtons.PlantSuovehka.Button.interactable = true;
        });
    }

    public void OnReedPlantsPlantedClicked()
    {
        DialogueManager dialogue = DialogueManager.instance;

        milestoneHandler.ForceUnlockMilestone(level: 1, progress: 0.66f);
        dialogue.CompleteTask("E2");
        dialogue.PlayTutorialNode(day1ReedsPlantedDialogue, () =>
        {
            actionButtons.CutPlants.Button.interactable = true;
            actionButtons.OpenPlants.Button.interactable = true;
            actionButtons.PlantSuovehka.Button.interactable = true;
            nextDayButton.interactable = true;
            CompleteDay();
        });
    }

    public void OnPlantRemoved(bool wasInvasive)
    {
        OnReedsClearedClicked();
        PlantEvents.OnPlantRemoved -= OnPlantRemoved;
    }

    public void OnPlantPlaced(WetlandPlantType plantType, string plantName)
    {
        plantsPlaced += 1;

        if (plantsPlaced >= requiredPlantAmount)
        {
            actionButtons.ShowButtons.Hide();
            actionButtons.LockAll();

            OnReedPlantsPlantedClicked();
            PlantEvents.OnPlantPlaced -= OnPlantPlaced;
        }
        else
        {
            if (!actionButtons.ShowButtons.IsOpen)
                actionButtons.OpenPlants.ReHighlight();

            actionButtons.PlantSuovehka.ReHighlight();
        }
    }
}