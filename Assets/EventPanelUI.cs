using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Events; // Added for UnityEvent

public class EventPanelUI : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onPanelOpened; // Triggered when panel starts opening
    public UnityEvent onPanelClosed; // Triggered when outcome is closed

    [Header("Panels")]
    [SerializeField] private GameObject eventPopupPanel;
    [SerializeField] private GameObject outcomePopupPanel;

    [Header("Text Boxes")]
    [SerializeField] private TMP_Text eventNameText;
    [SerializeField] private TMP_Text eventDescriptionText;
    [SerializeField] private TMP_Text goodResponseButtonText;
    [SerializeField] private TMP_Text neutralResponseButtonText;
    [SerializeField] private TMP_Text badResponseButtonText;
    [SerializeField] private TMP_Text outcomeText;
    [SerializeField] private TMP_Text closeButtonText;
    [SerializeField] private TMP_Text bonusAPIndicatorText;

    private WetlandEvent currentEvent;
    private TurnManager turnManager;
    private SoundManager soundManager;

    public WetlandEvent CurrentEvent => currentEvent;


    [Header("Buttons")]
    [SerializeField] private Button goodResponseButton;
    [SerializeField] private Button neutralResponseButton;
    [SerializeField] private Button badResponseButton;
    [SerializeField] private Button okButton;
    [SerializeField] private Button closeOutcomePanelButton;

    [Header("Button Sprites")]
    [SerializeField] private Sprite defaultButtonSprite;
    [SerializeField] private Sprite selectedButtonSprite;

    private AnswerCategory? selectedAnswer = null;

    [Header("Localized Texts")]
    [SerializeField] private TMP_Text selectButtonText;
    [SerializeField] private TMP_Text choiceButtonText;
    [SerializeField] private LocalizedText selectButtonTextLocalized;
    [SerializeField] private LocalizedText choiceButtonTextLocalized;
    [SerializeField] private LocalizedText closeButtonTextLocalized;

    private void Start()
    {
        turnManager = TurnManager.Instance;
        soundManager = SoundManager.Instance;

        goodResponseButton.onClick.AddListener(() => SelectResponse(AnswerCategory.Good));
        neutralResponseButton.onClick.AddListener(() => SelectResponse(AnswerCategory.Neutral));
        badResponseButton.onClick.AddListener(() => SelectResponse(AnswerCategory.Bad));
        okButton.onClick.AddListener(ConfirmSelection);
        closeOutcomePanelButton.onClick.AddListener(CloseOutcomePanel);

        okButton.interactable = false;

        choiceButtonText.text = choiceButtonTextLocalized.GetText();
        selectButtonText.text = selectButtonTextLocalized.GetText();
        closeButtonText.text = closeButtonTextLocalized.GetText();
    }

    // Unified entry point
    public void OpenPanel(bool loadNewEvent)
    {
        onPanelOpened?.Invoke(); // Trigger opened event
        gameObject.SetActive(true);

        if (loadNewEvent || currentEvent == null)
        {
            GetNewEvent();
        }
        else
        {
            SetupEventUI();
        }

        ShowEventUI();
        EnableAnswerButtons(true);
        okButton.interactable = selectedAnswer.HasValue;
    }

    public void GetNewEvent()
    {
        currentEvent = turnManager.gameState.CurrentEvent;
        Debug.Log(currentEvent.name);
        SetupEventUI();
        ResetSelectionState();
    }

    private void SetupEventUI()
    {
        eventNameText.text = currentEvent.eventNameLocalized.GetText() ?? string.Empty;
        eventDescriptionText.text = currentEvent.eventDescriptionLocalized.GetText() ?? string.Empty;

        var goodResponse = currentEvent.GetResponseFromAnswer(AnswerCategory.Good);
        goodResponseButtonText.text = goodResponse != null ? (goodResponse.responseTextLocalized.GetText() ?? string.Empty) : string.Empty;

        var neutralResponse = currentEvent.GetResponseFromAnswer(AnswerCategory.Neutral);
        neutralResponseButtonText.text = neutralResponse != null ? (neutralResponse.responseTextLocalized.GetText() ?? string.Empty) : string.Empty;

        var badResponse = currentEvent.GetResponseFromAnswer(AnswerCategory.Bad);
        badResponseButtonText.text = badResponse != null ? (badResponse.responseTextLocalized.GetText() ?? string.Empty) : string.Empty;

        ShuffleButtons();
    }

    public void ShowEventUI()
    {
        eventPopupPanel.transform.localScale = Vector3.zero;
        eventPopupPanel.SetActive(true);
        eventPopupPanel.transform.DOScale(Vector3.one, 0.4f).SetUpdate(true);

        if (soundManager != null)
            soundManager.PlayUISound("eventPopup");
    }

    public void HideEventUI()
    {
        eventPopupPanel.transform.DOScale(Vector3.zero, 0.2f)
            .SetUpdate(true)
            .OnComplete(() => eventPopupPanel.SetActive(false));
    }

    private void ResetSelectionState()
    {
        selectedAnswer = null;
        ResetButtonColors();
        okButton.interactable = false;
    }

    private void ResetButtonColors()
    {
        goodResponseButton.GetComponent<Image>().sprite = defaultButtonSprite;
        neutralResponseButton.GetComponent<Image>().sprite = defaultButtonSprite;
        badResponseButton.GetComponent<Image>().sprite = defaultButtonSprite;
    }

    private void SelectResponse(AnswerCategory answer)
    {
        ResetButtonColors();
        selectedAnswer = answer;

        Button selectedButton = GetButtonForAnswer(answer);
        selectedButton.GetComponent<Image>().sprite = selectedButtonSprite;

        okButton.interactable = true;
    }

    private Button GetButtonForAnswer(AnswerCategory answer)
    {
        switch (answer)
        {
            case AnswerCategory.Good: return goodResponseButton;
            case AnswerCategory.Neutral: return neutralResponseButton;
            case AnswerCategory.Bad: return badResponseButton;
            default: return null;
        }
    }

    private void ConfirmSelection()
    {
        if (selectedAnswer.HasValue)
        {
            turnManager.gameState.HandleRandomEvent(selectedAnswer.Value);
            DisplayEventOutcome(selectedAnswer.Value);
            ResetSelectionState();
        }
    }

    private void DisplayEventOutcome(AnswerCategory answer)
    {
        EventResponse selectedResponse = currentEvent.GetResponseFromAnswer(answer);

        outcomePopupPanel.transform.localScale = Vector3.zero;
        outcomePopupPanel.SetActive(true);

        outcomeText.text = selectedResponse.outcomeTextLocalized.GetText();

        int bonusAP = 0;
        switch (answer)
        {
            case AnswerCategory.Good: bonusAP = 3; break;
            case AnswerCategory.Neutral: bonusAP = 2; break;
            case AnswerCategory.Bad: bonusAP = 1; break;
        }

        if (bonusAPIndicatorText != null)
            bonusAPIndicatorText.text = $"+{bonusAP} AP";

        outcomePopupPanel.transform.DOScale(Vector3.one, 0.4f).SetUpdate(true);
    }

    private void CloseOutcomePanel()
    {
        outcomePopupPanel.transform.DOScale(Vector3.zero, 0.2f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                outcomePopupPanel.SetActive(false);
                onPanelClosed?.Invoke(); // Trigger closed event after animation
            });

        //turnManager.ToggleEndTurnButton(true);
    }

    private void ShuffleButtons()
    {
        List<Button> buttons = new List<Button> { goodResponseButton, neutralResponseButton, badResponseButton };
        List<int> positions = new List<int>();

        for (int i = 0; i < buttons.Count; i++)
            positions.Add(buttons[i].transform.GetSiblingIndex());

        ShuffleList(positions);

        for (int i = 0; i < buttons.Count; i++)
            buttons[i].transform.SetSiblingIndex(positions[i]);
    }

    private void EnableAnswerButtons(bool state)
    {
        goodResponseButton.interactable = state;
        neutralResponseButton.interactable = state;
        badResponseButton.interactable = state;
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
