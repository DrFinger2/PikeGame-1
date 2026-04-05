using UnityEngine;
using UnityEngine.UI;

public class PopupHandler : MonoBehaviour
{
    [SerializeField] private Button dialogueButton;
    [SerializeField] private Button nextTurnButton;
    [SerializeField] private Button bookPopupButton;
    [SerializeField] private GameObject dialoguePanel;

    private bool isAnyPopupOpen;

    // NEW: Generic popup open
    public void OpenPopup(GameObject popup)
    {
        if (!isAnyPopupOpen)
        {
            isAnyPopupOpen = true;
            DisableButtonInteractions();
            popup.SetActive(true);
        }
    }

    // Existing systems
    public void BookPopupOpen()
    {
        if (!isAnyPopupOpen)
        {
            isAnyPopupOpen = true;
            DisableButtonInteractions();
        }
    }

    public void DialoguePopupOpen()
    {
        if (!isAnyPopupOpen && !dialoguePanel.activeSelf)
        {
            Debug.Log("dialoguepanel popup open triggered");
            isAnyPopupOpen = true;
            DisableButtonInteractions();
            dialogueButton.interactable = true;
        }
    }

    public void EventPopupOpen()
    {
        if (!isAnyPopupOpen)
        {
            isAnyPopupOpen = true;
            DisableButtonInteractions();
        }
    }

    private void DisableButtonInteractions()
    {
        dialogueButton.interactable = false;
        nextTurnButton.interactable = false;
        bookPopupButton.interactable = false;
    }

    // NEW: Close ANY popup
    public void ClosePopup(GameObject popup)
    {
        popup.SetActive(false);

        dialogueButton.interactable = true;
        nextTurnButton.interactable = true;
        bookPopupButton.interactable = true;

        isAnyPopupOpen = false;
    }
}
