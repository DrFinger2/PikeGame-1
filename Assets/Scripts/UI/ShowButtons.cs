using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShowButtons : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Button button1;
    [SerializeField] Button button2;
    [SerializeField] Button button3;

    private bool isOpen = false;


    public void ToggleButtons()
    {
        isOpen = !isOpen;
        button1.gameObject.SetActive(isOpen);
        button2.gameObject.SetActive(isOpen);
        button3.gameObject.SetActive(isOpen);
    }
}
