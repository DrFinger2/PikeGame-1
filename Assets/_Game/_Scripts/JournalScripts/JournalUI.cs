using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class JournalUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Journal journal;

    [Header("UI Controls")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button closeButton;

    [Header("Canvas Groups")]
    [SerializeField] CanvasGroup background;
    [SerializeField] CanvasGroup foreground;

    [Header("Settings")]
    [SerializeField] float fadeInTime = 0.5f;
    [SerializeField] float fadeOutTime = 0.3f;

    private void Awake()
    {
        if (journal == null)
            journal = Journal.Instance;

        if (nextButton != null)
            nextButton.onClick.AddListener(OpenNextPage);
        if (previousButton != null)
            previousButton.onClick.AddListener(OpenPreviousPage);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseJournal);
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OpenNextPage);
        if (previousButton != null)
            previousButton.onClick.RemoveListener(OpenPreviousPage);
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseJournal);
    }

    // ==========================================
    // PUBLIC METHODS
    // ==========================================
    public void CloseJournal()
    {
        if (journal != null)
        {
            fadeOutUI(fadeOutTime); // Trigger the fade out when closing
            journal.CloseJournal(fadeOutTime);
            Debug.Log($"Close journal: {journal.CurrentPageNumber}");
        }
    }

    public void OpenJournal()
    {
        if (journal != null)
        {
            fadeInUI(fadeOutTime);
            journal.OpenJournal(fadeOutTime);
        }
    }
    private void OpenNextPage()
    {
        if (journal != null)
        {
            journal.OpenNextPage();
            Debug.Log($"Moved to page: {journal.CurrentPageNumber}");
        }
    }

    private void OpenPreviousPage()
    {
        if (journal != null)
        {
            journal.CloseCurrentPage();
            Debug.Log($"Moved to page: {journal.CurrentPageNumber}");
        }
    }

    // ==========================================
    // PRIVATE METHODS
    // ==========================================
    private void fadeInUI(float fadeTime = 1f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(background, 1f, fadeTime, true));
        StartCoroutine(FadeCanvasGroup(foreground, 1f, fadeTime, true));
    }

    private void fadeOutUI(float fadeTime = 1f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(background, 0f, fadeTime, false));
        StartCoroutine(FadeCanvasGroup(foreground, 0f, fadeTime, false));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration, bool interactable)
    {
        if (cg == null) yield break;

        float startAlpha = cg.alpha;
        float time = 0;

        // If we are fading in, we want to block raycasts immediately so it feels responsive
        if (interactable)
        {
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;

        // If we are fading out, turn off interaction AFTER the fade is done
        if (!interactable)
        {
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }
}
