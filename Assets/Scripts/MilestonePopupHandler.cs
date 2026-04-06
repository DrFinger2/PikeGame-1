using UnityEngine;

public class MilestonePopupHandler : MonoBehaviour
{
    private bool isPopupOpen = false;

    // Open a milestone popup
    public void OpenMilestonePopup(GameObject popup)
    {
        if (isPopupOpen)
            return;

        isPopupOpen = true;
        popup.SetActive(true);
    }

    // Close a milestone popup
    public void CloseMilestonePopup(GameObject popup)
    {
        popup.SetActive(false);
        isPopupOpen = false;
    }
}
