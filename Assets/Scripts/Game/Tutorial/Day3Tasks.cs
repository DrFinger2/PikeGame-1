using UnityEngine;
using UnityEngine.UI;

public class Day3Tasks : DayTaskBase
{
    [Header("Managers & Systems")]
    [SerializeField] private MilestoneHandler milestoneHandler;
    [SerializeField] private RaccoonDogManager raccoonManager;

    [Header("UI Elements")]
    [SerializeField] private ActionButtonsUI actionButtons;
    [SerializeField] private Button nextDayButton;
    [SerializeField] private Button shopButton;

    [Header("Dialogue References")]
    [SerializeField] private TutorialDialogue day3IntroDialogue;         // E7
    [SerializeField] private TutorialDialogue day3RaccoonChasedDialogue; // E8
    [SerializeField] private TutorialDialogue day3EndOfDayDialogue;      // E9

    [Header("Settings")]
    [SerializeField] private int floatingPlantsRequired = 3;
    [SerializeField] private int requiredRaccoonsChased = 1;
    [SerializeField] private Transform raccoonPosition;

    private int floatingPlantsPlantedCount;
    private int raccoonChaseCount;

    public override void StartDay()
    {

        Events.OnDayStarted.Invoke();
        this.enabled = true;
        raccoonManager.SpawnRaccoonInLocation(raccoonPosition);

        DialogueManager dialogue = DialogueManager.instance;
        TurnManager turn = TurnManager.Instance;
        GameState state = turn.gameState;

        state.AddPoints(extraPointsPerDay);
        milestoneHandler.ForceUnlockMilestone(level: 2, progress: 0.33f);
        floatingPlantsPlantedCount = 0;
        raccoonChaseCount = 0;

        nextDayButton.interactable = false;
        shopButton.interactable = false;
        actionButtons.LockAll();

        // Kicks off E7 Chain: Intro -> Milestone -> Invasive Species Warning
        dialogue.PlayTutorialNode(day3IntroDialogue, () => {
                RaccoonDogMovement.OnRaccoonChased += OnRaccoonDogChased;
        });
    }

    public override void EndDay()
    {
        Events.OnDayEnded.Invoke();
        this.enabled = false;
        PlantEvents.OnPlantPlaced -= OnPlantPlaced;
    }

    public void OnRaccoonDogChased()
    {
        raccoonChaseCount += 1;
        if (raccoonChaseCount >= requiredRaccoonsChased)
        {
            RaccoonDogMovement.OnRaccoonChased -= OnRaccoonDogChased;
            DialogueManager dialogue = DialogueManager.instance;
            raccoonManager.isSpawning = true;

            milestoneHandler.ForceUnlockMilestone(level: 2, progress: 0.66f);
            dialogue.CompleteTask("E7");
            dialogue.PlayTutorialNode(day3RaccoonChasedDialogue, () => {
                    if (!actionButtons.ShowButtons.IsOpen)
                        actionButtons.OpenPlants.ReHighlight();

                    actionButtons.OpenPlants.Button.interactable = true;
                    actionButtons.PlantLumme.Button.interactable = true;
                    
                    PlantEvents.OnPlantPlaced += OnPlantPlaced;
            });
        }
    }

    private void OnPlantPlaced(WetlandPlantType plantType, string plantName)
    {
        floatingPlantsPlantedCount++;

        if (floatingPlantsPlantedCount >= floatingPlantsRequired)
        {

            PlantEvents.OnPlantPlaced -= OnPlantPlaced;
            DialogueManager dialogue = DialogueManager.instance;

            milestoneHandler.ForceUnlockMilestone(level: 2, progress: 1f);
            actionButtons.LockPlanting();
            actionButtons.ShowButtons.Hide();
            
            dialogue.CompleteTask("E8");
            dialogue.PlayTutorialNode( day3EndOfDayDialogue, () => {
                    nextDayButton.interactable = true;
                    shopButton.interactable = true;
                    actionButtons.CutPlants.Button.interactable = true;
                    actionButtons.UnlockPlanting();
                    CompleteDay();
            });
        }
        else
        {
            if (!actionButtons.ShowButtons.IsOpen)
                actionButtons.OpenPlants.ReHighlight();

            actionButtons.PlantLumme.ReHighlight();
        }
    }
}