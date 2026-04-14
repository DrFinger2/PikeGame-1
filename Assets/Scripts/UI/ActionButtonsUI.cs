using UnityEngine;
using UnityEngine.UI;

public class ActionButtonsUI : MonoBehaviour
{
    [Header("Main Buttons")]
    [SerializeField] private UnlockableButton cutPlants;
    [SerializeField] private UnlockableButton openPlants;
    [SerializeField] private UnlockableButton openBook;
    [SerializeField] private ShowButtons showButtons;

    [Header("Sub Buttons")]
    [field: SerializeField] private UnlockableButton plantSuovehka;
    [field: SerializeField] private UnlockableButton plantRantakukka;
    [field: SerializeField] private UnlockableButton plantLumme;

    // -- Main button get methods -- 
    public ShowButtons ShowButtons => showButtons;
    public UnlockableButton CutPlants => cutPlants;
    public UnlockableButton OpenPlants => openPlants;
    public UnlockableButton OpenBook => openBook;

    // -- Sub button get methods -- 
    public UnlockableButton PlantSuovehka => plantSuovehka;
    public UnlockableButton PlantRantakukka => plantRantakukka;
    public UnlockableButton PlantLumme => plantLumme;

    public void LockAll()
    {
        CutPlants.Button.interactable = false;
        OpenPlants.Button.interactable = false;
        OpenBook.Button.interactable = false;
        PlantSuovehka.Button.interactable = false;
        PlantRantakukka.Button.interactable = false;
        PlantLumme.Button.interactable = false;
    }

    public void UnlockAll()
    {
        CutPlants.Button.interactable = true;
        OpenPlants.Button.interactable = true;
        OpenBook.Button.interactable = true;
        PlantSuovehka.Button.interactable = true;
        PlantRantakukka.Button.interactable = true;
        PlantLumme.Button.interactable = true;
    }

    public void LockPlanting()
    {
        PlantSuovehka.Button.interactable = false;
        PlantRantakukka.Button.interactable = false;
        PlantLumme.Button.interactable = false;
        openPlants.Button.interactable = false;
    }

    public void UnlockPlanting()
    {
        PlantSuovehka.Button.interactable = true;
        PlantRantakukka.Button.interactable = true;
        PlantLumme.Button.interactable = true;
        openPlants.Button.interactable = true;
    }
}