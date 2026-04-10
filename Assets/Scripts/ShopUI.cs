using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public GameObject cowButton;
    public bool IsOpen { get; private set; } = false;

    private void Start()
    {
        TurnManager.Instance.onTurnChanged.AddListener(TurnChanged);
    }

    private void OnDestroy()
    {
        TurnManager.Instance.onTurnChanged.RemoveListener(TurnChanged);
    }

    private void TurnChanged(int turn)
    {
        CloseShop();
    }

    public void ToggleShop()
    {
        IsOpen = !IsOpen;
        cowButton.SetActive(IsOpen);
    }
    public void OpenShop()
    {
        IsOpen = true;
        cowButton.SetActive(true);
    }    
    public void CloseShop()
    {
        IsOpen = false;
        cowButton.SetActive(false);
    }
}