using TMPro;
using UnityEngine;



public class LocalizedTextUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textComponent;
    [SerializeField] LocalizedText localizedText;
    void Awake()
    {
        ApplyLocalizedText();
    }

    void OnEnable()
    {
        ApplyLocalizedText();
    }

    void Reset()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        LanguageManager manager = LanguageManager.Instance;
        if (manager) manager.onLanguageChanged.AddListener(ApplyLocalizedText);
    }


    void OnDestroy()
    {
        LanguageManager manager = LanguageManager.Instance;
        if (manager) manager.onLanguageChanged.RemoveListener(ApplyLocalizedText);
    }

    void ApplyLocalizedText()
    {
        string text = localizedText.GetText();
        textComponent.text = text;
    }
}

