using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCG;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Button[] menuButtons;
    [SerializeField] private TMP_Text startGame;
    [SerializeField] private TMP_Text quitGame;
    [SerializeField] private TMP_Text credits;
    [SerializeField] private LocalizedText startGameText;
    [SerializeField] private LocalizedText quitGameText;
    [SerializeField] private LocalizedText creditsText;
    [SerializeField] private TMP_Text languageButtonTMP;
    [SerializeField] private LocalizedText languageButtonText;
    [SerializeField] private TMP_Text fiButtonTMP;
    [SerializeField] private TMP_Text sweButtonTMP;
    [SerializeField] private TMP_Text engButtonTMP;
    [SerializeField] private LocalizedText fiButtonText;
    [SerializeField] private LocalizedText sweButtonText;
    [SerializeField] private LocalizedText engButtonText;
    [SerializeField] private TMP_Text backButtonTMP;
    [SerializeField] private LocalizedText backButtonText;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject languagePanel;
    [SerializeField] private RectTransform creditsScroll;
    [SerializeField] private float creditsTopPos;
    [SerializeField] private float scrollSpeed;

    private Vector3 creditsStartPos;
    private Coroutine creditsCoroutine;

    private void Start()
    {
        if (fadeCG != null)
        {
            fadeCG.blocksRaycasts = false;
            fadeCG.alpha = 0;
        }

        if (languagePanel != null)
            languagePanel.SetActive(false);

        ToggleButtons(true);
        UpdateText();

        creditsStartPos = creditsScroll.localPosition;
        LanguageManager.Instance.onLanguageChanged.AddListener(UpdateText);
    }

    public void EnableMain()
    {
        StopCreditScroll();
        creditsPanel.SetActive(false);
        languagePanel.SetActive(false);
        mainPanel.SetActive(true);
        ToggleButtons(true);
    }

    public void EnableCredits()
    {
        mainPanel.SetActive(false);
        languagePanel.SetActive(false);
        creditsPanel.SetActive(true);
        ToggleButtons(false);
        StartCreditScroll();
    }

    public void EnableLanguage()
    {
        ToggleButtons(false);
        languagePanel.SetActive(true);
        UpdateText();
    }

    public void CloseLanguage()
    {
        languagePanel.SetActive(false);
        ToggleButtons(true);
    }

    private void StartCreditScroll()
    {
        creditsScroll.localPosition = creditsStartPos;
        creditsCoroutine = StartCoroutine(ScrollCredits());
    }

    private void StopCreditScroll()
    {
        if (creditsCoroutine != null)
        {
            StopCoroutine(creditsCoroutine);
            creditsCoroutine = null;
        }
        creditsScroll.localPosition = creditsStartPos;
    }

    private IEnumerator ScrollCredits()
    {
        while (creditsScroll.localPosition.y < creditsTopPos)
        {
            creditsScroll.localPosition += new Vector3(0, scrollSpeed * Time.deltaTime, 0);
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        EnableMain();
    }

    private void UpdateText()
    {
        if (startGame != null) startGame.text = startGameText.GetText();
        if (quitGame != null) quitGame.text = quitGameText.GetText();
        if (credits != null) credits.text = creditsText.GetText();
        if (languageButtonTMP != null) languageButtonTMP.text = languageButtonText.GetText();
        if (fiButtonTMP != null) fiButtonTMP.text = fiButtonText.GetText();
        if (sweButtonTMP != null) sweButtonTMP.text = sweButtonText.GetText();
        if (engButtonTMP != null) engButtonTMP.text = engButtonText.GetText();
        if (backButtonTMP != null) backButtonTMP.text = backButtonText.GetText();
    }

    public void StartGame(int sceneNumber)
    {
        ToggleButtons(false);
        StartCoroutine(FadeToBlackAndLoad(sceneNumber));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator FadeToBlackAndLoad(int sceneNumber)
    {
        if (fadeCG == null)
        {
            SceneManager.LoadScene(sceneNumber);
            yield break;
        }

        fadeCG.blocksRaycasts = true;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCG.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        fadeCG.alpha = 1f;
        SceneManager.LoadScene(sceneNumber);
    }

    private void ToggleButtons(bool state)
    {
        if (menuButtons != null)
        {
            foreach (Button button in menuButtons)
            {
                if (button != null)
                    button.interactable = state;
            }
        }
    }
}