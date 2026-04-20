using System;
using UnityEngine;
using UnityEngine.UI;

public class Day2Tasks : DayTaskBase
{
    [Header("Managers & Systems")]
    [SerializeField] private MilestoneHandler milestoneHandler;

    [Header("UI Elements")]
    [SerializeField] private ActionButtonsUI actionButtons;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button nextDayButton;
    [SerializeField] private GameObject coinDisplay;
    

    [Header("Dialogue References")]
    [SerializeField] private TutorialDialogue day2IntroDialogue;         // E4
    [SerializeField] private TutorialDialogue day2GrassPlantedDialogue;  // E5
    [SerializeField] private TutorialDialogue day2MilestoneDialogue;     // E6


    [Header("Settings")]
    [SerializeField] private int grassPlantsRequired = 2; // Adjusted to 2 based on your Day 2 notes

    private int grassPlantsPlantedCount;
    private bool shopVisited;

    public override void StartDay()
    {
        this.enabled = true;
        Events.OnDayStarted.Invoke();

        TurnManager turn = TurnManager.Instance;
        DialogueManager dialogue = DialogueManager.instance;
        GameState state = turn.gameState;
        tileManager tile = tileManager.Instance;
        
        state.AddPoints(extraPointsPerDay);
        tile?.OverwriteAllWeeds(1);
        
        shopVisited = false;
        grassPlantsPlantedCount = 0;
        actionButtons.ShowButtons.Hide();
        actionButtons.LockAll();
        
        shopButton.interactable = false;
        nextDayButton.interactable = false;
        coinDisplay.SetActive(false);

        dialogue.PlayTutorialNode(day2IntroDialogue, () => {
                actionButtons.OpenPlants.Button.interactable = true;
                actionButtons.PlantRantakukka.Button.interactable = true;
                
                if (!actionButtons.ShowButtons.IsOpen)
                {
                    actionButtons.OpenPlants.ReHighlight();
                }
                PlantEvents.OnPlantPlaced += OnGrassPlantPlanted;
        });
    }

    public override void EndDay()
    {
        Events.OnDayEnded.Invoke();
        this.enabled = false;
    }

    private void PlayMilestoneDialogue()
    {
        DialogueManager dialogue = DialogueManager.instance;
        dialogue.PlayTutorialNode(day2MilestoneDialogue,() => {
                shopButton.interactable = true;
                nextDayButton.interactable = true;
                actionButtons.CutPlants.Button.interactable = true;
                actionButtons.OpenPlants.Button.interactable = true;
                actionButtons.PlantRantakukka.Button.interactable = true;
                actionButtons.PlantSuovehka.Button.interactable = true;

                coinDisplay.SetActive(true);
                if (shopButton != null) shopButton.gameObject.SetActive(true);
        });
    }

    public void OnShopVisited()
    {
        if (shopVisited)
            return;
        shopVisited = true;

        DialogueManager.instance.CompleteTask("E6");

        CompleteDay();
    }

    public void OnGrassPlantPlanted(WetlandPlantType plantType, string plantName)
    {
         grassPlantsPlantedCount += 1;

        if (grassPlantsPlantedCount >= grassPlantsRequired)
        {
            DialogueManager dialogue = DialogueManager.instance;
            PlantEvents.OnPlantPlaced -= OnGrassPlantPlanted;

            dialogue.CompleteTask("E4");

            milestoneHandler.ForceUnlockMilestone(level: 1, progress: 1.0f); 
            actionButtons.ShowButtons.Hide();
            actionButtons.OpenPlants.Button.interactable = false;
            actionButtons.PlantRantakukka.Button.interactable = false;
     

            dialogue.PlayTutorialNode( day2GrassPlantedDialogue,() => {
                    DialogueManager.instance.CompleteTask("E5");
                    PlayMilestoneDialogue();
            });
        }
        else
        {
            if (!actionButtons.ShowButtons.IsOpen)
                actionButtons.OpenPlants.ReHighlight();
                
            actionButtons.PlantRantakukka.ReHighlight();  
        }
    }
}