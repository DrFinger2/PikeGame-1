using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using Utils;

[RequireComponent(typeof(CanvasGroup))]
public class PlantPanel : SingletonMonoBehaviour<PlantPanel>
{
    [SerializeField] private TextMeshProUGUI plantText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;
    private Coroutine fadeRoutine;


    private void Awake()
    {
        ActionButton.OnActiveToolChanged += HandleActiveToolChanged;
    }

    private void OnDestroy()
    {
        ActionButton.OnActiveToolChanged -= HandleActiveToolChanged;
    }

    private void HandleActiveToolChanged(LocalizedText toolName)
    {
        if (toolName == null)
        {
            Hide();
        }

        string name = toolName?.GetText();
        if (string.IsNullOrEmpty(name))
        {
            Hide();
        }
        else
        {
            SetText(toolName.GetText());
            Show();
        }
    }
    

    public void Show()
    {
        TriggerFade(1f, true);
    }
    public void Hide()
    {
        TriggerFade(0f, false);
    }

    
    public void SetText(string plantType)
    {
        plantText?.SetText(plantType);
    }

    private void TriggerFade(float targetAlpha, bool finalState)
    {
        gameObject.SetActive(true);
        if (fadeRoutine != null) 
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, finalState));
    }

    private IEnumerator FadeRoutine(float target, bool activeAtEnd)
    {
        float start = canvasGroup.alpha;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = target;
        gameObject.SetActive(activeAtEnd);
    }
}