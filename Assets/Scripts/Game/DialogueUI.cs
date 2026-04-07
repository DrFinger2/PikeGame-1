using System.Collections;
using TMPro;
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

    public void ShowDialogue(DialogueBase dialogue)
    {
        dialoguePanel.SetActive(true);
        continueButton.SetActive(true);

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
            dialogueText.text = "";
            if (typeWriterEffectCoroutine != null) StopCoroutine(typeWriterEffectCoroutine);

            // Pass the safely grabbed text into the Typewriter
            typeWriterEffectCoroutine = StartCoroutine(TypeWriterEffect(finalDialogueText, dialogue.typeSpeed));
        }
        else
        {
            // Pass the safely grabbed text directly
            dialogueText.text = finalDialogueText;
        }
    }


    private IEnumerator TypeWriterEffect(string text, float speed)
    {
        finalText = text;
        dialogueText.text = "";

        // FIX: If speed is 0 or negative, force it to 1 so we don't divide by zero!
        if (speed <= 0f) speed = 1f;

        float typingSpeed = speed * baseTypingSpeed;
        float timePerChar = 1f / typingSpeed;

        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];
            if (i < text.Length - 1)
            {
                yield return new WaitForSeconds(timePerChar);
            }
        }
        typeWriterEffectCoroutine = null;
    }
    
    public void SkipTyping()
    {
        if (typeWriterEffectCoroutine != null)
        {
            StopCoroutine(typeWriterEffectCoroutine);
            dialogueText.text = finalText;
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