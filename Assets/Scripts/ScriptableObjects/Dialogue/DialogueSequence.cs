using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSequence", menuName = "Dialogue/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    [Header("Sequence Settings")]
    public string sequenceName; // e.g., "Day 1 Intro"
    
    [Header("Dialogues in Order")]
    public List<TutorialDialogue> dialogues = new List<TutorialDialogue>();

    public void ResetSequence()
    {
        foreach (var dialogue in dialogues)
        {
            dialogue.ResetTutorialState();
        }
    }
}