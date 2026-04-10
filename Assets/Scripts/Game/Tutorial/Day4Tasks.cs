using UnityEngine;
using UnityEngine.UI;

public class Day4Tasks : DayTaskBase
{
    [Header("UI Elements")]
    [SerializeField] private ActionButtonsUI actionButtons;
    [SerializeField] private EventPanelUI eventPanel;
    [SerializeField] private Button questionsButton; // Dedicated quiz button
    [SerializeField] private Button shopButton;
    [SerializeField] private Button nextDayButton;


    [Header("Dialogue References")]
    [SerializeField] private TutorialDialogue day4IntroDialogue;           // E10
    [SerializeField] private TutorialDialogue day4InvasiveClearedDialogue; // E11
    [SerializeField] private TutorialDialogue day4QuizCorrectDialogue;     // E12 (Correct Branch)
    [SerializeField] private TutorialDialogue day4QuizIncorrectDialogue;   // E12 (Incorrect Branch)

    [Header("Settings")]
    [SerializeField] private int invasivePlantsRequired = 3;

    private int invasivePlantsCleared;
    private bool questionAnswered;

    public override void StartDay()
    {
        Events.OnDayStarted.Invoke();
        TurnManager.Instance.gameState.AddPoints(extraPointsPerDay);
        this.enabled = true;
        invasivePlantsCleared = 0;
        questionAnswered = false;

        // Lock the cut tool and quiz button initially (Book is left free to use)
        SetInteractable(false, actionButtons.CutPlants.Button, actionButtons.OpenPlants.Button, questionsButton, nextDayButton, shopButton);


        // Kicks off E10 Chain: Intro -> Warning -> Task
        DialogueManager.instance.PlayTutorialNode(
            node: day4IntroDialogue,
            onDialogueFinished: () =>
            {
                actionButtons.CutPlants.ReHighlight();
                SetInteractable(true, actionButtons.CutPlants.Button);
                PlantEvents.OnPlantRemoved += OnPlantRemoved;
                EventPanelUI.OnQuestionAnswered += OnQuestionAnswered;
            }
        );
    }

    public override void EndDay()
    {
        Events.OnDayEnded.Invoke();
        this.enabled = false;

        PlantEvents.OnPlantRemoved -= OnPlantRemoved;
        EventPanelUI.OnQuestionAnswered -= OnQuestionAnswered;
    }

    private void OnPlantRemoved(bool wasInvasive)
    {
        /*
        if (!wasInvasive) 
            return;
        */ // this check doesnt work for some reason!

        invasivePlantsCleared++;

        if (invasivePlantsCleared >= invasivePlantsRequired)
        {
            PlantEvents.OnPlantRemoved -= OnPlantRemoved;
            SetInteractable(false, actionButtons.CutPlants.Button);

            DialogueManager.instance.CompleteTask("E10");
            DialogueManager.instance.PlayTutorialNode(
                node: day4InvasiveClearedDialogue,
                onDialogueFinished: () =>
                {
                    SetActive(true, questionsButton?.gameObject);
                    SetInteractable(true, questionsButton);
                }
            );
        }
        else
        {
            actionButtons.CutPlants.ReHighlight();
        }
    }

    private void OnQuestionAnswered(AnswerCategory answer)
    {
        if (questionAnswered)
            return;

        questionAnswered = true;
        eventPanel.CloseOutcomePanel();

        SetInteractable(false, questionsButton);
        DialogueManager.instance.CompleteTask("E11");

        bool isCorrect = (answer == AnswerCategory.Good);
        TutorialDialogue selectedDialogue = isCorrect ? day4QuizCorrectDialogue : day4QuizIncorrectDialogue;

        DialogueManager.instance.PlayTutorialNode(
            node: selectedDialogue,
            onDialogueFinished: () =>
            {
                SetInteractable(true, actionButtons.CutPlants.Button, actionButtons.OpenPlants.Button, questionsButton, nextDayButton, shopButton);
                CompleteDay();
            }
        );

    }
}

