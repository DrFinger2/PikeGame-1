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

    public void BookPopupOpen()
    {
        if(!isAnyPopupOpen)
        {
            isAnyPopupOpen = true;
            DisableButtonInteractions();
        }
    }
    public void DialoguePopupOpen()
    {
        if(!isAnyPopupOpen && !dialoguePanel.activeSelf)
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
    //dialogueManager closedialogue also triggers this, others from close button
    public void ClosePopup()
    {
        dialogueButton.interactable = true;
        nextTurnButton.interactable = false;
        bookPopupButton.interactable = true;
        isAnyPopupOpen = false;
    }
}
