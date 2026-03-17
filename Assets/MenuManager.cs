using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCG;
    [SerializeField] private float fadeDuration;
    [SerializeField] private Button[] menuButtons;
    [SerializeField] private TMP_Text startGame;
    [SerializeField] private TMP_Text quitGame;
    [SerializeField] private TMP_Text credits;
    [SerializeField] private TMP_Text language;
    [SerializeField] private TMP_Text back;
    [SerializeField] private LocalizedText startGameText;
    [SerializeField] private LocalizedText quitGameText;
    [SerializeField] private LocalizedText creditsText;
    [SerializeField] private LocalizedText languageText;
    [SerializeField] private LocalizedText backText;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject creditsPanel;
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
        ToggleButtons(true);
        UpdateText();
        LanguageManager.Instance.onLanguageChanged.AddListener(UpdateText);
    }
    public void EnableMain()
    {
        StopCreditScroll();
        creditsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
    public void EnableCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
        StartCreditScroll();
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
        startGame.text = startGameText.GetText();
        quitGame.text = quitGameText.GetText();
        credits.text = creditsText.GetText();
        language.text = languageText.GetText();
        back.text = backText.GetText();
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
        fadeCG.blocksRaycasts = true; //doesnt seem to work
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
                button.interactable = state;
            }
        }
    }
}
