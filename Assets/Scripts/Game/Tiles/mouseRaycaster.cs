using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class mouseRaycaster : MonoBehaviour
{

    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private Vector2 mousePos;
    public Vector3 worldPos;
    [SerializeField] private Vector3 projectedPos;
    public Camera cam;
    public GameObject tileHoverOver;

    private tileManager tm;
    [SerializeField] private GameObject selectedTile;

    private InputSystem_Actions inputActions;

    //touch screen input
 
    private bool useTouch = true;
    public bool isTouching;
    public Vector2 touchPosition;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tm = tileManager.Instance;
        
    }


    // Update is called once per frame
    void Update()
    {
        if (tm.toolBeingUsed) return;

        bool tappedThisFrame = false;
        Vector2 interactionPos = Vector2.zero;

        // 1. Check Enhanced Touch (Simulator / Mobile)
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            var activeTouch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];
            mousePos = activeTouch.screenPosition;

            // Check if the touch just started this frame
            if (activeTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                tappedThisFrame = true;
                interactionPos = activeTouch.screenPosition;
            }
        }
        // 2. Check Physical Mouse (Game View / PC)
        else if (Mouse.current != null)
        {
            mousePos = Mouse.current.position.ReadValue();

            // Check if the left mouse button was clicked this frame
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                tappedThisFrame = true;
                interactionPos = mousePos;
            }
        }

        // Safety check to prevent camera mathematical crashes
        if (float.IsInfinity(mousePos.x) || float.IsInfinity(mousePos.y) ||
            float.IsNaN(mousePos.x) || float.IsNaN(mousePos.y))
        {
            return;
        }

        // Always update the tile hover visuals
        PerformRaycast(mousePos);

        // Fire NPC check with the EXACT position of the tap/click, not a stale coordinate
        if (tappedThisFrame)
        {
            CheckForNPCHit(interactionPos);
        }
    }
    
    

    public void PerformRaycast(Vector2 screenPosition)
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = cam.ScreenPointToRay(screenPosition);
        if (plane.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
            projectedPos = worldPos;
        }

        var newSelectedTile = CheckTileHitting();

        // I've fixed the logic bug here so the very first tile you touch registers immediately!
        if (newSelectedTile != selectedTile)
        {
            if (selectedTile != null)
            {
                selectedTile.GetComponent<gameTile>().ClearHover();
            }

            if (newSelectedTile != null)
            {
                newSelectedTile.GetComponent<gameTile>().StartHover();
                tm.selectedTile = newSelectedTile.GetComponent<gameTile>(); // Send to tilemanager
            }
            else
            {
                tm.selectedTile = null;
            }
        }

        selectedTile = newSelectedTile;
    }
    

    public void CheckForNPCHit(Vector2 screenPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            
            if (hit.collider != null && hit.collider.CompareTag("NPC"))
            {
                DialogueManager.instance.InteractWithNPC();
            }

            if (hit.collider != null && hit.collider.CompareTag("RaccoonDog"))
            {
                hit.transform.GetComponent<RaccoonDogMovement>().touched = true;
            }
        }
    }


    public GameObject CheckTileHitting()
    {
        RaycastHit hit;

        Vector3 startPos = new Vector3(projectedPos.x, projectedPos.y + 1, projectedPos.z);
        Vector3 direction = Vector3.down;


        if (Physics.Raycast(startPos, direction, out hit, 100f, tileLayerMask))
        {
            return hit.collider.gameObject;
        }

        return selectedTile;
    }
    
    

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(worldPos, 0.5f);
    }

    private IEnumerator ClearHoverHelper()
    {
        yield return new WaitForSeconds(1);
    }
}
