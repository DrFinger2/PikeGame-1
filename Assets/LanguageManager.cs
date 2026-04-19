using UnityEngine;
using UnityEngine.Events;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;
    public Language currentLanguage = Language.FI;
    public UnityEvent onLanguageChanged;
    [SerializeField] private DialogueDatabase[] dialogueDatabases;


    private Language previousLanguage = Language.FI;


    private void OnValidate()
    {
        if (currentLanguage != previousLanguage)
        {
            onLanguageChanged?.Invoke();
            previousLanguage = currentLanguage;
        }
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        previousLanguage = currentLanguage;

        transform.SetParent(null);  // Force the object to the root of the hierarchy so DontDestroyOnLoad works
        DontDestroyOnLoad(gameObject);
    }



    public void SwitchLanguage(string language)
    {
        switch (language)
        {
            case "fi":
                currentLanguage = Language.FI;
                break;
            case "sw":
                currentLanguage = Language.SW;
                break;
            case "en":
                currentLanguage = Language.EN;
                break;
        }
        onLanguageChanged?.Invoke();
    }

    public void SwitchLanguage(Language language)
    {

        currentLanguage = language;
        onLanguageChanged?.Invoke();
    }
}

public enum Language
{
    FI,
    SW,
    EN,
}

