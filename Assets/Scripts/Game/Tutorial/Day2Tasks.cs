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
        Events.OnDayStarted.Invoke();
        TurnManager.Instance.gameState.AddPoints(extraPointsPerDay);
        this.enabled = true;
        grassPlantsPlantedCount = 0;
        shopVisited = false;
        actionButtons.ShowButtons.Hide();


        SetInteractable(false,
            actionButtons.CutPlants.Button,
            actionButtons.OpenPlants.Button,
            actionButtons.PlantSuovehka.Button,
            actionButtons.PlantRantakukka.Button);
        SetInteractable(false, shopButton, nextDayButton);
        SetActive(false, coinDisplay);

        // Kicks off Event 4
        DialogueManager.instance.PlayTutorialNode(
            node: day2IntroDialogue,
            onDialogueFinished: () =>
            {
                SetInteractable(true, actionButtons.OpenPlants.Button, actionButtons.PlantRantakukka.Button);
                if (!actionButtons.ShowButtons.IsOpen)
                {
                    actionButtons.OpenPlants.ReHighlight();
                }
                PlantEvents.OnPlantPlaced += OnGrassPlantPlanted;
            }
        );
    }

    public override void EndDay()
    {
        Events.OnDayEnded.Invoke();
        this.enabled = false;
    }

    private void PlayMilestoneDialogue()
    {
        DialogueManager.instance.PlayTutorialNode(
            node: day2MilestoneDialogue,
            onDialogueFinished: () =>
            {
                SetInteractable(true,
                    shopButton,
                    nextDayButton,
                    actionButtons.CutPlants.Button,
                    actionButtons.OpenPlants.Button,
                    actionButtons.PlantRantakukka.Button,
                    actionButtons.PlantSuovehka.Button
                );

                SetActive(true, coinDisplay, shopButton?.gameObject);
            }
        );
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
            PlantEvents.OnPlantPlaced -= OnGrassPlantPlanted;
            milestoneHandler.ForceUnlockMilestone(level: 1, progress: 1.0f);
            DialogueManager.instance.CompleteTask("E4");
             
            actionButtons.ShowButtons.Hide();
            SetInteractable(false, actionButtons.OpenPlants.Button);
            SetInteractable(false, actionButtons.PlantRantakukka.Button);
     
            DialogueManager.instance.PlayTutorialNode(
                node: day2GrassPlantedDialogue,
                onDialogueFinished: () =>
                {
                    DialogueManager.instance.CompleteTask("E5");
                    PlayMilestoneDialogue();
                }
            );
        }
        else
        {
            if (!actionButtons.ShowButtons.IsOpen)
                actionButtons.OpenPlants.ReHighlight();
                
            actionButtons.PlantRantakukka.ReHighlight();
            
        }
    }
}