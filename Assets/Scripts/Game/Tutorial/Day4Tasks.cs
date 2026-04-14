using UnityEngine;
using UnityEngine.UI;

public class Day4Tasks : DayTaskBase
{
    [Header("Managers & Systems")]
    [SerializeField] private MilestoneHandler milestoneHandler;

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
        this.enabled = true;


        DialogueManager dialogue = DialogueManager.instance;
        TurnManager turn = TurnManager.Instance;
        GameState state = turn.gameState;

        state.AddPoints(extraPointsPerDay);

        invasivePlantsCleared = 0;
        questionAnswered = false;

        actionButtons.CutPlants.Button.interactable = false;
        actionButtons.OpenPlants.Button.interactable = false;
        questionsButton.interactable = false;
        nextDayButton.interactable = false;
        shopButton.interactable = false;

        dialogue.PlayTutorialNode(day4IntroDialogue,() => {
                actionButtons.CutPlants.ReHighlight();
                actionButtons.CutPlants.Button.interactable = true;
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
        if (!wasInvasive)  this check doesnt work for some reason!
            return;
        */ 

        invasivePlantsCleared++;

        if (invasivePlantsCleared >= invasivePlantsRequired)
        {
            PlantEvents.OnPlantRemoved -= OnPlantRemoved;
            DialogueManager dialogue = DialogueManager.instance;

            actionButtons.CutPlants.Button.interactable = false;

            dialogue.CompleteTask("E10");
            dialogue.PlayTutorialNode( day4InvasiveClearedDialogue, () => {
                    questionsButton?.gameObject.SetActive(true);
                    questionsButton.interactable = true;
            });
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

        DialogueManager dialogue = DialogueManager.instance;

        questionAnswered = true;
        eventPanel.CloseOutcomePanel();
        questionsButton.interactable = false;

        dialogue.CompleteTask("E11");

        bool isCorrect = (answer == AnswerCategory.Good);
        TutorialDialogue selectedDialogue = isCorrect ? day4QuizCorrectDialogue : day4QuizIncorrectDialogue;

        dialogue.PlayTutorialNode( selectedDialogue, () => {
                actionButtons.CutPlants.Button.interactable = true;
                actionButtons.OpenPlants.Button.interactable = true;
                questionsButton.interactable = true;
                nextDayButton.interactable = true;
                shopButton.interactable = true;
                CompleteDay();
        });
    }
}
