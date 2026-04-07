using UnityEngine;
using UnityEngine.UI;

public class EventPanelButtonHolder : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public EventPanelUI eventPanelUI;
    public Button openButton;

    [Header("TutorialSettings")]
    public bool IsInTutorialMode = true;

    private int openCount = 0;
    private int maxOpens = 3;
    private bool isPanelOpen = false;

    private void Start()
    {
        TurnManager.Instance.onTurnChanged.AddListener(OnTurnChanged);
        openButton.onClick.AddListener(OpenEventPanel);
        eventPanelUI.onPanelOpened.AddListener(HandlePanelOpened);
        eventPanelUI.onPanelClosed.AddListener(HandlePanelClosed);
    }

    private void OnTurnChanged(int turn)
    {
        if (!IsInTutorialMode)
        {
            ResetDailyLimit();
        }
    }


    private void OpenEventPanel()
    {
        // 1. Block if a panel instance is already open
        if (isPanelOpen)
            return;

        // 2 . Skip first day (event data does not exist)
        if (eventPanelUI.CurrentEvent == null)
        {
            return;
        }

        if (openCount >= maxOpens)
        {
            openButton.interactable = false;
            return;
        }


        TurnManager.Instance.gameState.GetRandomEvent();
        eventPanelUI.OpenPanel(true);

        openCount++;

        if (openCount >= maxOpens)
        {
            openButton.interactable = false;
        }
    }

    private void HandlePanelOpened()
    {
        isPanelOpen = true;
    }

    private void HandlePanelClosed()
    {
        isPanelOpen = false;
    }

    public void ResetDailyLimit()
    {
   
        openCount = 0;
        isPanelOpen = false;
        openButton.interactable = true;
        
    }
}

