using UnityEngine;

public class ShowButtons : MonoBehaviour
{
    public GameObject button1;
    public GameObject button2;
    public GameObject button3;

    private bool isOpen = false; // remembers current state

    public void ToggleButtons()
    {
        isOpen = !isOpen; // switch true/false

        button1.SetActive(isOpen);
        button2.SetActive(isOpen);
        button3.SetActive(isOpen);
    }
}
