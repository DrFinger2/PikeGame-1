using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShowButtons : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] GameObject button1;
    [SerializeField] GameObject button2;
    [SerializeField] GameObject button3;

    private bool isOpen = false;


    public void ToggleButtons()
    {
        isOpen = !isOpen;
        button1.SetActive(isOpen);
        button2.SetActive(isOpen);
        button3.SetActive(isOpen);
    }
}
