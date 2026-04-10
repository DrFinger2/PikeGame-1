using UnityEngine;
using UnityEngine.UI;

public class Day5Tasks : DayTaskBase
{
    [Header("UI Elements")]
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

    public override void StartDay()
    {
        Events.OnDayStarted.Invoke();
        this.enabled = true;

        SetInteractable(false, nextDayButton, questionButton, shopButton, npcButton);
        SetInteractable(false,
            actionButtons.CutPlants.Button,
            actionButtons.OpenPlants.Button,
            actionButtons.OpenBook.Button
        );

        DialogueManager.instance.PlayTutorialNode(
            node: day5IntroDialogue,
            onDialogueFinished: () =>
            {
                SetInteractable(true, actionButtons.OpenBook.Button);
                DialogueManager.instance.CompleteTask("E13");
                NotebookPageHandler.OnBookOpened.AddListener(OnBookOpened);

            }
        );
    }
    public void OnBookOpened()
    {
        NotebookPageHandler.OnBookOpened.RemoveListener(OnBookOpened);
        DialogueManager.instance.PlayTutorialNode(
            day5NotebookDialogue,
            () =>
            {
                DialogueManager.instance.CompleteTask("E14");
                PlayPikeSequence();
            }
        );
    }


    public override void EndDay()
    {
        Events.OnDayEnded.Invoke();
        this.enabled = false;
    }

    private void PlayPikeSequence()
    {
        DialogueManager.instance.PlayTutorialNode(
            node: day5PikeDialogue,
            onDialogueFinished: () =>
            {
                DialogueManager.instance.CompleteTask("E15");
                PlayConclusionSequence();
            }
        );
    }

    private void PlayConclusionSequence()
    {
        DialogueManager.instance.PlayTutorialNode(
            node: day5ConclusionDialogue,
            onDialogueFinished: () =>
            {
                DialogueManager.instance.CompleteTask("E16");
                SetInteractable(true, nextDayButton, questionButton, shopButton, npcButton);
                SetInteractable(true,
                    actionButtons.CutPlants.Button,
                    actionButtons.OpenPlants.Button,
                    actionButtons.OpenBook.Button
                );
                CompleteDay();
            }
        );
    }
}
