using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShowButtons : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] GameObject button1;
    [SerializeField] GameObject button2;
    [SerializeField] GameObject button3;

    public bool IsOpen { get; private set; } = false;

    public void ToggleButtons()
    {
        IsOpen = !IsOpen;
        this.gameObject.SetActive(IsOpen);
        button1.SetActive(IsOpen);
        button2.SetActive(IsOpen);
        button3.SetActive(IsOpen);
    }
    
    public void Show()
    {
        IsOpen = true;
        this.gameObject.SetActive(true);
        button1.SetActive(true);
        button2.SetActive(true);
        button3.SetActive(true);
    }
    
    public void Hide()
    {
        IsOpen = false;
        this.gameObject.SetActive(false);
        button1.SetActive(false);
        button2.SetActive(false);
        button3.SetActive(false);
    }
}
