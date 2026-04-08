using System;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events; // Added for UnityEvent
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI; 

public class ActionButton : Selectable, IPointerClickHandler, IDragHandler
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
    private mouseRaycaster mouseRaycaster;


    protected override void Start()
    {
        base.Start();
        rect = GetComponent<RectTransform>();
        originalPosition = rect.anchoredPosition;
        tm = tileManager.Instance;
        turnManager = TurnManager.Instance;
        if (turnManager != null)
            mouseRaycaster = turnManager?.gameObject?.GetComponent<mouseRaycaster>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!selected || !IsInteractable()) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rect.parent,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            rect.localPosition = localPoint;
        }

        // 3. Feed the perfectly accurate UI pointer position to the raycaster
        if (mouseRaycaster != null)
        {
            mouseRaycaster.PerformRaycast(eventData.position);
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

        // 4. Force one final accurate raycast right before evaluating the drop
        if (mouseRaycaster != null)
        {
            mouseRaycaster.PerformRaycast(pointerEventData.position);
        }

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