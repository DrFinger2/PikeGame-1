using UnityEngine;
using UnityEngine.Events;

public class EndGameController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Fader fader;
    [SerializeField] private CanvasGroup parentCanvasGroup;

    [Header("Win Screen")]
    [SerializeField] private GameObject winScreenObject;
    [SerializeField] private CanvasGroup winScreenGroup;

    [Header("Loss Screen")]
    [SerializeField] private GameObject lossScreenObject;
    [SerializeField] private CanvasGroup lossScreenGroup;

    public void TriggerWinSequence()
    {
        if (fader != null)
        {
            fader.onFaded.AddListener(ShowWinScreen);
            fader.FadeScreen();
        }
        else
        {
            ShowWinScreen();
        }
    }

    public void TriggerLossSequence()
    {
        if (fader != null)
        {
            fader.onFaded.AddListener(ShowLossScreen);
            fader.FadeScreen();
        }
        else
        {
            ShowLossScreen();
        }
    }

    private void ShowWinScreen()
    {
        if (winScreenObject != null) winScreenObject.SetActive(true);
        if (parentCanvasGroup != null) parentCanvasGroup.interactable = false;

        if (winScreenGroup != null)
        {
            winScreenGroup.ignoreParentGroups = true;
            winScreenGroup.interactable = true;
            winScreenGroup.blocksRaycasts = true;
        }

        if (fader != null) fader.onFaded.RemoveListener(ShowWinScreen);
    }

    private void ShowLossScreen()
    {
        if (lossScreenObject != null) lossScreenObject.SetActive(true);
        if (parentCanvasGroup != null) parentCanvasGroup.interactable = false;
        
        if (lossScreenGroup != null)
        {
            lossScreenGroup.ignoreParentGroups = true;
            lossScreenGroup.interactable = true;
            lossScreenGroup.blocksRaycasts = true;
        }

        if (fader != null) fader.onFaded.RemoveListener(ShowLossScreen);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}