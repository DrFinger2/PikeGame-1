using UnityEngine;
using UnityEngine.UI;

public class EventPanelButtonHolder : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject eventPanelUI;
    public Button openButton;

    private int openCount = 0;
    private int maxOpens = 3;

    private EventPanelUI panel;

    private void Start()
    {
        panel = eventPanelUI.GetComponent<EventPanelUI>();
        openButton.onClick.AddListener(OpenEventPanel);
    }

    private void OpenEventPanel()
    {
        // ❗ Prevent opening before the first event exists
        if (panel.CurrentEvent == null)
        {
            Debug.Log("No event available yet. Start a new day first.");
            return;
        }

        if (openCount >= maxOpens)
        {
            Debug.Log("EventPanelUI can no longer be opened today.");
            openButton.interactable = false;
            return;
        }

        // Reopen the existing event
        panel.OpenPanel(false);

        openCount++;
        Debug.Log($"EventPanelUI opened {openCount} times today.");
    }

    public void ResetDailyLimit()
    {
        openCount = 0;
        openButton.interactable = true;
        Debug.Log("Daily EventPanelUI limit reset.");
    }
}
