using TMPro;
using UnityEngine;

public class ButtonTextLocalized : MonoBehaviour
{
    [SerializeField] private LocalizedText buttonText;
    private TMP_Text buttonTextObject;
    private void Start()
    {
        buttonTextObject = GetComponent<TMP_Text>();
        LanguageManager.Instance.onLanguageChanged.AddListener(SetText);
        SetText();
    }
    private void SetText()
    {
        buttonTextObject.text = buttonText.GetText();
    }
}

// This is also set to MainMenu Credits as the text holder?