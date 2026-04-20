using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Fader fader;
    [SerializeField] private CanvasGroup parentCanvasGroup;

    [Header("Win Screen")]
    [SerializeField] private GameObject winScreenObject;
    [SerializeField] private Button closeWinButton;
    [SerializeField] private CanvasGroup winScreenGroup;

    [Header("Loss Screen")]
    [SerializeField] private GameObject lossScreenObject;
    [SerializeField] private Button closeLossButton;
    [SerializeField] private CanvasGroup lossScreenGroup;

    public void Awake()
    {
        closeWinButton.onClick.RemoveAllListeners();
        closeLossButton.onClick.RemoveAllListeners();
        closeLossButton.onClick.AddListener(QuitGame);
        closeWinButton.onClick.AddListener(QuitGame);
    }


    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(0);
    }


    public void QuitGame()
    {
        Application.Quit();
    }

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

 
}