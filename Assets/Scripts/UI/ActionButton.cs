using System;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events; // Added for UnityEvent
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI; 

public class ActionButton : Selectable, IPointerClickHandler
{
    // Temporary fix for larger architectural problem
    public static event Action<LocalizedText> OnActiveToolChanged;

    [System.Serializable]
    public class ActionEvents { public UnityEvent onClick = new(); public UnityEvent onDown = new(); }
    public ActionEvents ButtonEvents = new ActionEvents();


    public tileAction action;
    private TurnManager turnManager;
    private tileManager tm;
    public TextMeshProUGUI buttonText;
    [SerializeField] private bool selected = false;
    private Vector3 originalPosition;
    //private mouseRaycaster mouseRaycaster;
    private RectTransform rect;


    protected override void Start()
    {
        base.Start();

        rect = GetComponent<RectTransform>();
        originalPosition = rect.anchoredPosition;
        tm = tileManager.Instance;
        turnManager = TurnManager.Instance;
        //mouseRaycaster = turnManager.gameObject.GetComponent<mouseRaycaster>();
    }

    void Update()
    {
        /* broken code, drag doesn't work?
        if (selected)
        {
            Vector2 inputPos;
            if (Touchscreen.current != null && mouseRaycaster.isTouching)
            {
                inputPos = mouseRaycaster.touchPosition;
            }
            else
            {
                inputPos = Mouse.current.position.ReadValue();
            }
        }
        */
        
        if (selected)
        {   
            if (Pointer.current != null)
            {
                transform.position = Pointer.current.position.ReadValue();
            }
        }
    }
    
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {   
        // Prevents clicking if the object is disabled or locked
        if (!IsActive() || !IsInteractable()) return;

        // Fire the new onClick event
        ButtonEvents.onClick?.Invoke();

        /*
        if (tm.selectedTile != null)
        {
            action.affectTile(tm.selectedTile);
            Debug.Log("clicked!" + TurnManager.Instance.gameState.currentActionPoints);
        }
        */
    }
    
    public override void OnPointerDown(PointerEventData pointerEventData)
    {
        base.OnPointerDown(pointerEventData);

        if (!IsInteractable()) return;

        selected = true;
        tm.toolBeingUsed = true;

        if (action != null)
        {
            OnActiveToolChanged?.Invoke(action.actionName);
            ButtonEvents.onDown?.Invoke();
        }
    }

    public override void OnPointerUp(PointerEventData pointerEventData)
    {
        base.OnPointerUp(pointerEventData);

        if (!selected) return;

        Debug.Log("Pointer up!");
        if (tm.selectedTile != null)
        {
            action.affectTile(tm.selectedTile);
            Debug.Log("clicked!" + TurnManager.Instance.gameState.currentActionPoints);
        }
        selected = false;
        tm.toolBeingUsed = false;
        tm.selectedTile = null;
        rect.anchoredPosition = originalPosition;
        
        OnActiveToolChanged?.Invoke(null);
    }
}