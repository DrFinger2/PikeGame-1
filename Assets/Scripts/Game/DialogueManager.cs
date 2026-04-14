using System;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueEvents
    {
        public UnityEvent<string> OnTaskAssigned = new();
        public UnityEvent<string> OnTaskCompleted = new();
        public UnityEvent OnTutorialCompleted = new();
        public UnityEvent OnDialogueClosed = new();
    }

    private class DialogueState
    {
        public bool tutorialActive = true;
        public bool hasGivenHintThisTurn = false;
        public string currentTaskId = "";
        public Action onSequenceFinishedCallback;

        // REPLACED indices with the explicit hard-reference
        public TutorialDialogue currentActiveNode;
    }

    public static DialogueManager instance;

    [Header("Core References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private DialogueDatabase dialogueDB;
    private RandomEventSystem randomEventSystem;
    private TurnManager turnManager;

    public DialogueEvents Events = new DialogueEvents();

    [Header("Sound")]
    [SerializeField] private string[] soundIDs;

    private DialogueState state = new DialogueState();
    public bool isSequenceActive { get; private set; } = false;
    public bool IsDialogueActive => dialogueUI.IsDialogueActive;


    

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        randomEventSystem = RandomEventSystem.instance;
        turnManager = TurnManager.Instance;
        turnManager.onTurnChanged.AddListener(OnTurnChanged);

        if (state.currentActiveNode == null)
        {
            dialogueUI.HideDialogue();
            dialogueUI.HideTask();
        }

        // The day scripts (ProgressionManager/Day1Tasks) will now explicitly trigger 
        // the first dialogue, so we don't need to auto-loop the old database list here.
    }

    private void OnTurnChanged(int turnNumber)
    {
        state.hasGivenHintThisTurn = false;
        if (!state.tutorialActive)
        {
            dialogueUI.HideDialogue();
        }
    }

    // --- NEW EXPLICIT TUTORIAL PIPELINE ---
    // DayXTasks calls this directly with the hard-referenced TutorialDialogue asset
    public void PlayTutorialNode(TutorialDialogue node, Action finished = null)
    {
        state.tutorialActive = true;
        state.currentActiveNode = node;
        state.onSequenceFinishedCallback = finished;

        if (node == null)
        {
            dialogueUI.HideDialogue();
            dialogueUI.HideTask();
            return;
        }
        
        dialogueUI.ShowDialogue(node, true);

        if (!string.IsNullOrEmpty(node.taskDescription))
        {
            ShowTask(node.taskId, node.taskDescription);
        }
        else
        {
            dialogueUI.HideTask();
        }
    }

    // DayXTasks calls this to finish a task and fire YOUR existing UnityEvents
    public void CompleteTask(string taskID)
    {
        if (state.currentTaskId == taskID)
        {
            if (state.currentActiveNode != null)
                state.currentActiveNode.isCompleted = true;

            Events.OnTaskCompleted?.Invoke(taskID);
            state.currentTaskId = "";
            dialogueUI.HideTask();
        }
    }

    // Called by Day5Tasks (or Manager) when the absolute final tutorial is done
    public void FinishEntireTutorialSequence()
    {
        state.tutorialActive = false;
        state.currentActiveNode = null;
        Events.OnTutorialCompleted?.Invoke();
    }

    private void ShowTask(string taskId, string taskDescription, Sprite icon = null)
    {
        state.currentTaskId = taskId;
        dialogueUI.ShowTask(taskDescription, icon);
        Events.OnTaskAssigned?.Invoke(taskId);
    }



    public void GiveHintForNextEvent()
    {
        if (state.hasGivenHintThisTurn)
        {
            ShowRandomDialogue();
            return;
        }

        WetlandEvent nextEvent = randomEventSystem.CheckNextEvent();
        if (nextEvent != null)
        {
            EventHintDialogue hint = dialogueDB.GetHintForEvent(nextEvent);
            if (hint != null)
            {
                dialogueUI.ShowDialogue(hint, false);
                state.hasGivenHintThisTurn = true;
                RandomDialogueSoundEffectPlayer();
            }
        }
    }

    public void ShowRandomDialogue()
    {
        RandomDialogue randomDialogue = dialogueDB.GetRandomDialogue(turnManager.CurrentTurn);
        if (randomDialogue != null)
        {
            if (randomDialogue.isJoke) SoundManager.Instance.PlayGameSound("joke01");
            else RandomDialogueSoundEffectPlayer();
            dialogueUI.ShowDialogue(randomDialogue, false);
        }
    }

    public void RandomDialogueSoundEffectPlayer()
    {
        int i = UnityEngine.Random.Range(0, soundIDs.Length);
        SoundManager.Instance.PlayGameSound(soundIDs[i]);
    }

    public void InteractWithNPC()
    {
        if (dialogueUI.IsDialogueActive)
        {
            if (dialogueUI.IsTyping)
            {
                dialogueUI.SkipTyping();
            }
            else
            {
                FinishCurrentSequence();
            }
            return;
        }
        
        else if (!state.hasGivenHintThisTurn)
            GiveHintForNextEvent();
        else
            ShowRandomDialogue();
    }

    public void NextDialogue()
    {
        if (!dialogueUI.IsDialogueActive)
            return;

        if (dialogueUI.IsTyping)
        {
            dialogueUI.SkipTyping();
            return;
        }

        // If we are in a multi-page tutorial (Page 1 -> Page 2), play the next page
        if (state.tutorialActive && state.currentActiveNode != null)
        {
            if (state.currentActiveNode.nextDialogue != null)
            {
                PlayTutorialNode(state.currentActiveNode.nextDialogue, state.onSequenceFinishedCallback);
                return;
            }
            else
            {
                FinishCurrentSequence();
                return;
            }
        }

        ShowRandomDialogue();
    }

    private void FinishCurrentSequence()
    {
        Events.OnDialogueClosed?.Invoke();
        dialogueUI.HideDialogue();

        Action callback = state.onSequenceFinishedCallback;
        state.onSequenceFinishedCallback = null;
        state.currentActiveNode = null;
        callback?.Invoke();
    }

}
