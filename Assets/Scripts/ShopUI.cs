using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public GameObject cowButton;

    public void ToggleShop()
    {
        cowButton.SetActive(!cowButton.activeSelf);
    }
}