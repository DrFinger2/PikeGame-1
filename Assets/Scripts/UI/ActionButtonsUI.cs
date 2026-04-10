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
}
