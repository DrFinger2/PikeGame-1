using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class ActionButton : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public tileAction action;
    private TurnManager turnManager;
    private tileManager tm;
    public TextMeshProUGUI buttonText;
    [SerializeField]private bool selected = false;
    private Vector3 originalPosition;
    private mouseRaycaster mouseRaycaster;
    private RectTransform rect;


    void Start()
    {
        rect = GetComponent<RectTransform>();
        originalPosition = rect.anchoredPosition;
        tm = tileManager.Instance;
        turnManager = TurnManager.Instance;
        mouseRaycaster = turnManager.gameObject.GetComponent<mouseRaycaster>();
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
            if (Touchscreen.current == null)
            {
                var mousePos = Mouse.current.position.ReadValue();
                transform.position = mousePos;
            }
            else
            {
                transform.position = mouseRaycaster.touchPosition;
            }
            
            
        }
        
        
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Mouse enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Mouse exit");
    }
    
    public void OnPointerClick (PointerEventData eventData)
    {   
        /*
        if (tm.selectedTile != null)
        {
            action.affectTile(tm.selectedTile);
            Debug.Log("clicked!" + TurnManager.Instance.gameState.currentActionPoints);
        }
        */
    }
    
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        selected = true;
        tm.toolBeingUsed = true;
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
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
        
    }
    
    /*
    public void ClickButton()
    {
        if (tm.selectedTile != null && action != null)
        {
            action.affectTile(tm.selectedTile);
        }
        
    }
    */
    
}
