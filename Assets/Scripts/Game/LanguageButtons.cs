using System.Configuration.Assemblies;
using UnityEngine;
using UnityEngine.UI;
public class LanguageButtons : MonoBehaviour
{
    [SerializeField] Button finnishButton;
    [SerializeField] Button swedishButton;
    [SerializeField] Button englishButton;

    void Start()
    {
        finnishButton.onClick.RemoveAllListeners();
        swedishButton.onClick.RemoveAllListeners();
        englishButton.onClick.RemoveAllListeners();

        finnishButton.onClick.AddListener(ChangeToFinnish);
        swedishButton.onClick.AddListener(ChangeToSwedish);
        englishButton.onClick.AddListener(ChangeToEnglish);
    }

    void ChangeToFinnish()
    {
        LanguageManager manager = LanguageManager.Instance;
        manager.SwitchLanguage(Language.FI);
    }

    void ChangeToSwedish()
    {
        LanguageManager manager = LanguageManager.Instance;
        manager.SwitchLanguage(Language.SW);
    }

    void ChangeToEnglish()
    {
        LanguageManager manager = LanguageManager.Instance;
        manager.SwitchLanguage(Language.EN);
    }
}
