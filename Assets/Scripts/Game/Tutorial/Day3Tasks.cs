using UnityEngine;
using UnityEngine.UI;

public class Day3Tasks : DayTaskBase
{
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

    private int floatingPlantsPlantedCount;
    private int raccoonChaseCount;


    public override void StartDay()
    {
        Events.OnDayStarted.Invoke();
        
        this.enabled = true;
        
        floatingPlantsPlantedCount = 0;
        raccoonChaseCount = 0;
        
        SetInteractable(false,
            nextDayButton,
            shopButton,
            actionButtons.OpenPlants.Button,
            actionButtons.PlantLumme.Button,
            actionButtons.PlantRantakukka.Button,
            actionButtons.PlantSuovehka.Button
        );

        // Kicks off E7 Chain: Intro -> Milestone -> Invasive Species Warning
        DialogueManager.instance.PlayTutorialNode(
            node: day3IntroDialogue,
            onDialogueFinished: () => {
                RaccoonDogMovement.OnRaccoonChased += OnRaccoonDogChased;
            }
        );
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
        if(raccoonChaseCount >= requiredRaccoonsChased)
        {
            RaccoonDogMovement.OnRaccoonChased -= OnRaccoonDogChased;
            DialogueManager.instance.CompleteTask("E7");
            DialogueManager.instance.PlayTutorialNode(
                node: day3RaccoonChasedDialogue,
                onDialogueFinished: () =>{
                    if (!actionButtons.ShowButtons.IsOpen)
                        actionButtons.OpenPlants.ReHighlight();
                    
                    SetInteractable(true, actionButtons.OpenPlants.Button, actionButtons.PlantLumme.Button);
                    PlantEvents.OnPlantPlaced += OnPlantPlaced;
                }
            );
        }
    }

    private void OnPlantPlaced(WetlandPlantType plantType, string plantName)
    {
        floatingPlantsPlantedCount++;
        
        if (floatingPlantsPlantedCount >= floatingPlantsRequired)
        {
            PlantEvents.OnPlantPlaced -= OnPlantPlaced;
            SetInteractable(false, actionButtons.OpenPlants.Button, actionButtons.PlantLumme.Button);
            actionButtons.ShowButtons.Hide();

            DialogueManager.instance.CompleteTask("E8");
    
            DialogueManager.instance.PlayTutorialNode(
                node: day3EndOfDayDialogue,
                onDialogueFinished: () =>
                {
                    SetInteractable(true, nextDayButton);
                    SetInteractable(true,
                        shopButton,
                        nextDayButton,
                        actionButtons.OpenPlants.Button,
                        actionButtons.PlantLumme.Button,
                        actionButtons.PlantRantakukka.Button,
                        actionButtons.PlantSuovehka.Button
                    );
                    CompleteDay();
                }
            );
        }
    }
}