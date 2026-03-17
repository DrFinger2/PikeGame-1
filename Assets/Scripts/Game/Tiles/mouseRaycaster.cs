using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class mouseRaycaster : MonoBehaviour
{

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


    void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Touch.Touch.performed += ctx =>
        {
            touchPosition = ctx.ReadValue<Vector2>();
            CheckForNPCHit();
        };

        //inputActions.Touch.Touch.canceled += ctz => isTouching = false;
    }

    // Update is called once per frame
    void Update()
    {   
        
        if (Touchscreen.current == null)
        {
            touchPosition = Mouse.current.position.ReadValue();
        }
        else
        {
            mousePos = touchPosition;
        }
        
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (plane.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
            projectedPos = worldPos;
        }
        
        var newSelectedTile = CheckTileHitting();
        
        if (selectedTile != null && newSelectedTile != selectedTile)
        {
            newSelectedTile.GetComponent<gameTile>().StartHover();
            tm.selectedTile = newSelectedTile.GetComponent<gameTile>(); //send selected tile to tilemanager instnace
            selectedTile.GetComponent<gameTile>().ClearHover();
        }

        selectedTile = newSelectedTile;
       
    }

    public void CheckForNPCHit()
    {
        Ray ray = cam.ScreenPointToRay(touchPosition);
        RaycastHit hit;
        UnityEngine.Debug.Log("1");
        if (Physics.Raycast(ray, out hit))
        {
            UnityEngine.Debug.Log(hit.transform.name);
            if (hit.collider != null && hit.collider.CompareTag("NPC"))
            {
                DialogueManager.instance.InteractWithNPC();
            }

            if (hit.collider != null && hit.collider.CompareTag("RaccoonDog"))
            {
                UnityEngine.Debug.Log("3");
                hit.transform.GetComponent<RaccoonDogMovement>().touched = true;
            }
        }
        
    }


    public GameObject CheckTileHitting()
    {
        RaycastHit hit;
        
        Physics.Raycast(new Vector3(projectedPos.x, projectedPos.y+1, projectedPos.z), new Vector3(0, projectedPos.y-1, 0).normalized, out hit); //fire ray directly above tilemap
        UnityEngine.Debug.DrawLine(new Vector3(projectedPos.x, projectedPos.y + 1, projectedPos.z), worldPos);
        
        if (hit.collider != null && hit.collider.CompareTag("Tile")) //check if ray hits a tile
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
