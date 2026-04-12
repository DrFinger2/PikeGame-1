using UnityEngine;
using UnityEngine.UI;

public class PopupHandler : MonoBehaviour
{
    //sorry about this, big crunch
    [SerializeField] private Button dialogueButton;
    [SerializeField] private Button nextTurnButton;
    [SerializeField] private Button bookPopupButton;
    [SerializeField] private GameObject dialoguePanel;

    private bool isAnyPopupOpen;

    // Cached interaction states
    private bool wasDialogueInteractable;
    private bool wasNextTurnInteractable;
    private bool wasBookInteractable;


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

            // Re-enable this specific one after storing and disabling
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
        // Store current states before modifying them
        wasDialogueInteractable = dialogueButton.interactable;
        wasNextTurnInteractable = nextTurnButton.interactable;
        wasBookInteractable = bookPopupButton.interactable;

        Debug.Log(
            $"Is dialogueButton.interactable: {dialogueButton.interactable}, " +
            $"Is nextTurnButton.interactable: {nextTurnButton.interactable}," +
            $"Is bookPopupButton.interactable: {bookPopupButton.interactable}"
        );

        dialogueButton.interactable = false;
        nextTurnButton.interactable = false;
        bookPopupButton.interactable = false;
    }

    //dialogueManager closedialogue also triggers this, others from close button
    public void ClosePopup()
    {
        if (isAnyPopupOpen)
        {
            dialogueButton.interactable = wasDialogueInteractable;
            nextTurnButton.interactable = wasNextTurnInteractable;
            bookPopupButton.interactable = wasBookInteractable;
            isAnyPopupOpen = false;

            Debug.Log(
                $"Is dialogueButton.interactable: {wasDialogueInteractable}, " +
                $"Is nextTurnButton.interactable: {wasNextTurnInteractable}," +
                $"Is bookPopupButton.interactable: {wasBookInteractable}"
            );
        }

    }
}