using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image speakerImage;
    [SerializeField] private GameObject continueButton;

    [Header("Task UI References")]
    [SerializeField] private GameObject taskPanel;
    [SerializeField] private TMP_Text taskDescriptionText;
    [SerializeField] private Image taskImage;

    [Header("Typewriter Effect")]
    [SerializeField] private bool useEffect = true;
    [SerializeField] private float baseTypingSpeed = 50f;

    private Coroutine typeWriterEffectCoroutine;
    private string finalText;

    public bool IsDialogueActive => dialoguePanel.activeInHierarchy;
    public bool IsTyping => typeWriterEffectCoroutine != null;

    public void ShowDialogue(DialogueBase dialogue, bool showContinue = true)
    {
        dialoguePanel.SetActive(true);
        continueButton.SetActive(showContinue);

        string finalName = !string.IsNullOrEmpty(dialogue.npcNameLocalized?.GetText())
            ? dialogue.npcNameLocalized.GetText()
            : dialogue.npcName;

        speakerNameText.text = finalName;

        if (dialogue.npcImage != null)
        {
            speakerImage.sprite = dialogue.npcImage;
            speakerImage.gameObject.SetActive(true);
        }
        else
        {
            speakerImage.gameObject.SetActive(false);
        }

        string finalDialogueText = !string.IsNullOrEmpty(dialogue.dialogueTextLocalized?.GetText())
            ? dialogue.dialogueTextLocalized.GetText()
            : dialogue.dialogueText;

        if (useEffect)
        {
            if (typeWriterEffectCoroutine != null) StopCoroutine(typeWriterEffectCoroutine);
            typeWriterEffectCoroutine = StartCoroutine(TypeWriterEffect(finalDialogueText, dialogue.typeSpeed));
        }
        else
        {
            dialogueText.text = finalDialogueText;
            dialogueText.maxVisibleCharacters = 99999; // Ensure all characters are visible
        }
    }


    private IEnumerator TypeWriterEffect(string text, float speed)
    {
        finalText = text;

        // 1. Give TMP the full text immediately, but hide it
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;

        if (speed <= 0f) speed = 1f;

        float typingSpeed = speed * baseTypingSpeed; // Characters per second
        float timer = 0f;

        // Force TMP to generate the mesh so we know exactly how many characters there are 
        // (This ignores invisible rich text tags like <color=red>!)
        dialogueText.ForceMeshUpdate();
        int totalVisibleCharacters = dialogueText.textInfo.characterCount;

        // 2. Reveal characters based on total elapsed time, entirely decoupled from framerate
        while (dialogueText.maxVisibleCharacters < totalVisibleCharacters)
        {
            timer += Time.deltaTime;

            // Calculate how many characters SHOULD be visible by now
            int charsToShow = Mathf.FloorToInt(timer * typingSpeed);
            dialogueText.maxVisibleCharacters = charsToShow;

            yield return null; // Wait for the next frame
        }

        // Ensure we end up fully revealed
        dialogueText.maxVisibleCharacters = totalVisibleCharacters;
        typeWriterEffectCoroutine = null;
    }


    public void SkipTyping()
    {
        if (typeWriterEffectCoroutine != null)
        {
            StopCoroutine(typeWriterEffectCoroutine);
            dialogueText.maxVisibleCharacters = 99999;
            typeWriterEffectCoroutine = null;
        }
    }
    

    public void HideDialogue()
    {
        if (typeWriterEffectCoroutine != null)
        {
            StopCoroutine(typeWriterEffectCoroutine);
            typeWriterEffectCoroutine = null;
        }
        dialoguePanel.SetActive(false);
        
    }

    public void ShowTask(string taskDescription, Sprite icon = null)
    {
        taskPanel.SetActive(true);
        taskDescriptionText.text = taskDescription;
        if (icon != null)
        {
            taskImage.sprite = icon;
            taskImage.gameObject.SetActive(true);
        }
        else
        {
            taskImage.gameObject.SetActive(false);
        }
    }

    public void HideTask()
    {
        taskPanel.SetActive(false);
    }
}