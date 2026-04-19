using UnityEngine;
using UnityEngine.UI;

public class Day5Tasks : DayTaskBase
{

    [Header("UI Elements")]
    [SerializeField] private EventPanelButtonHolder eventPanelButtons;
    [SerializeField] private ActionButtonsUI actionButtons;
    [SerializeField] private Button questionButton;
    [SerializeField] private Button nextDayButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button npcButton;


    [Header("Dialogue References")]
    [SerializeField] private TutorialDialogue day5IntroDialogue;      // E13
    [SerializeField] private TutorialDialogue day5NotebookDialogue;   // E14
    [SerializeField] private TutorialDialogue day5PikeDialogue;       // E15
    [SerializeField] private TutorialDialogue day5ConclusionDialogue; // E16

    [Header("Pike Controller")]
    [SerializeField] PikeRelease pikeRelease;

    public override void StartDay()
    {
        Events.OnDayStarted.Invoke();
        this.enabled = true;

        
        DialogueManager dialogue = DialogueManager.instance;
        TurnManager turn = TurnManager.Instance;
        GameState state = turn.gameState;

        state.AddPoints(extraPointsPerDay);

        nextDayButton.interactable = false;
        questionButton.interactable = false;
        shopButton.interactable = false;
        npcButton.interactable = false;
        actionButtons.LockAll();

        dialogue.PlayTutorialNode(day5IntroDialogue, () =>
        {
            actionButtons.OpenBook.Button.interactable = true;
            dialogue.CompleteTask("E13");
            NotebookPageHandler.OnBookOpened.AddListener(OnBookOpened);
        });
    }

    public void OnBookOpened()
    {

        NotebookPageHandler.OnBookClosed.AddListener(OnBookClosed);
        NotebookPageHandler.OnBookOpened.RemoveListener(OnBookOpened);
        DialogueManager dialogue = DialogueManager.instance;

        dialogue.PlayTutorialNode(day5NotebookDialogue, () =>
        {

            dialogue.CompleteTask("E14");

        }, false, 0.6f);
    }

    public void OnBookClosed()
    {
        NotebookPageHandler.OnBookClosed.RemoveListener(OnBookClosed);
        Invoke(nameof(PlayPikeSequence), 0.5f); // delay the next sequence by couple seconds so that the book closing animation has time to play..
    }

    public override void EndDay()
    {
        Events.OnDayEnded.Invoke();
        this.enabled = false;
    }

    private void PlayPikeSequence()
    {
        
        DialogueManager dialogue = DialogueManager.instance;
        pikeRelease.ReleasePike();

        dialogue.PlayTutorialNode(day5PikeDialogue, () =>
        {
            dialogue.CompleteTask("E15");
            PlayConclusionSequence();
        }, animatePopup: true );
    }


    private void PlayConclusionSequence()
    {
        DialogueManager dialogue = DialogueManager.instance;
        MilestoneHandler milestone = MilestoneHandler.Instance;

        dialogue.PlayTutorialNode(day5ConclusionDialogue, () =>
        {
            dialogue.CompleteTask("E16");
            nextDayButton.interactable = true;
            questionButton.interactable = true;
            shopButton.interactable = true;
            npcButton.interactable = true;
            actionButtons.UnlockAll();
            eventPanelButtons.ExitTutorialMode();
            milestone.ExitTutorialMode();
            CompleteDay();
        });
    }
}

