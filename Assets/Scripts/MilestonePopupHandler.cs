using UnityEngine;

public class MilestonePopupHandler : MonoBehaviour
{
    public GameObject popup;
    private bool isPopupOpen = false;

    public void Awake()
    {
        if (popup == null)
        {
            Transform firstChild = this.transform.GetChild(0);
            if (firstChild != null) popup = firstChild.gameObject;
        }
    }
    
    public void OpenMilestonePopup()
    {
        if (isPopupOpen)
            return;

        isPopupOpen = true;
        popup.SetActive(true);
    }

    public void CloseMilestonePopup()
    {
        popup.SetActive(false);
        isPopupOpen = false;
    }
}
