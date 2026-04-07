using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class Day1Tasks : DayTaskBase
{
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
        TurnManager.Instance.gameState.AddPoints(extraPointsPerDay);
        this.enabled = true;
        SetInteractable(false,
            actionButtons.CutPlants.Button,
            actionButtons.OpenPlants.Button
        );
        SetInteractable(false, actionButtons.PlantSuovehka.Button);

        // Kicks off Chain 1: 01 -> 02 -> 03 -> 04 -> 05 -> 06
        DialogueManager.instance.PlayTutorialNode(
            node: day1StartDialogue,
            onDialogueFinished: () =>
            {
                PlantEvents.OnPlantRemoved += OnPlantRemoved;
                SetInteractable(true, actionButtons.CutPlants.Button);
            }
        );
    }

    public override void EndDay()
    {
        Events.OnDayEnded.Invoke();
        this.enabled = false;
    }

    public void OnReedsClearedClicked()
    {
        SetInteractable(false, actionButtons.CutPlants.Button);
        DialogueManager.instance.CompleteTask("E1");
        DialogueManager.instance.PlayTutorialNode(
            node: day1ReedsClearedDialogue,
            onDialogueFinished: () =>
            {
                PlantEvents.OnPlantPlaced += OnPlantPlaced;
                
                SetInteractable(true, actionButtons.OpenPlants.Button);
                SetInteractable(true, actionButtons.PlantSuovehka.Button);
            }
        );
    }


    public void OnReedPlantsPlantedClicked()
    {
        // Tells DialogueManager the task from node 10 is done
        DialogueManager.instance.CompleteTask("E2");

        // Kicks off Chain 3: 11 -> 12 -> 13
        DialogueManager.instance.PlayTutorialNode(
            node: day1ReedsPlantedDialogue,
            onDialogueFinished: () =>
            {
                SetInteractable(true, actionButtons.CutPlants.Button);
                SetInteractable(true, actionButtons.OpenPlants.Button);
                SetInteractable(true, actionButtons.PlantSuovehka.Button, nextDayButton);
                CompleteDay();
            }
        );
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
            SetInteractable(false, actionButtons.CutPlants.Button);
            SetInteractable(false, actionButtons.OpenPlants.Button);
            SetInteractable(false, actionButtons.PlantSuovehka.Button);
            OnReedPlantsPlantedClicked();
            PlantEvents.OnPlantPlaced -= OnPlantPlaced;
        }
    }
}
