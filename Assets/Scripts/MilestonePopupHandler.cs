using UnityEngine;

public class MilestonePopupHandler : MonoBehaviour
{
    public GameObject popup;
    private bool isPopupOpen = false;

    // Open a milestone popup
    public void OpenMilestonePopup()
    {
        if (isPopupOpen)
            return;

        isPopupOpen = true;
        popup.SetActive(true);
    }

    // Close a milestone popup
    public void CloseMilestonePopup()
    {
        popup.SetActive(false);
        isPopupOpen = false;
    }
}
