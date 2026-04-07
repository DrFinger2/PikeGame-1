using UnityEngine;
using UnityEngine.UI;

public class Day5Tasks : DayTaskBase
{
    [Header("UI Elements")]
    [SerializeField] private Button wetlandGuideButton;

    [Header("Dialogue References")]
    [SerializeField] private TutorialDialogue day5IntroDialogue;
    [SerializeField] private TutorialDialogue day5GuideOpenedDialogue;

    private bool guideOpened;

    public override void StartDay()
    {
        Events.OnDayStarted.Invoke();
        this.enabled = true;
        guideOpened = false;

        SetInteractable(false, wetlandGuideButton);

        DialogueManager.instance.PlayTutorialNode(day5IntroDialogue);
    }

    public override void EndDay()
    {
        Events.OnDayEnded.Invoke();
        this.enabled = false;
    }

    public void UnlockGuide()
    {
        SetInteractable(true, wetlandGuideButton);
    }

    public void OnGuideBookOpened()
    {
        if (guideOpened) return;
        guideOpened = true;
        
        DialogueManager.instance.CompleteTask("task_guide_opened");
        DialogueManager.instance.PlayTutorialNode(day5GuideOpenedDialogue);
    }

    public void OnTutorialComplete()
    {
        DialogueManager.instance.CompleteTask("task_tutorial_complete");
        DialogueManager.instance.FinishEntireTutorialSequence(); 
        
        CompleteDay();
    }
}