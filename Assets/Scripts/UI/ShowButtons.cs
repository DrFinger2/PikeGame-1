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
        button1.SetActive(IsOpen);
        button2.SetActive(IsOpen);
        button3.SetActive(IsOpen);
    }
}
