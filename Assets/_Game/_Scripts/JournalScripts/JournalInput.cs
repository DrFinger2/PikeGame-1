using UnityEngine;
using UnityEngine.InputSystem; // Required for the new system

public partial class JournalInput : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] Camera journalCamera;
    [SerializeField] Journal journal;

    [Header("Input Action References")]
    [SerializeField] InputActionReference nextPageAction;
    [SerializeField] InputActionReference prevPageAction;
    [SerializeField] InputActionReference clickAction;
    [SerializeField] InputActionReference pointerPositionAction;

    private GameObject _lastHitObject;

    private void OnEnable()
    {
        // InputActionReferences must be enabled to work
        nextPageAction.action.Enable();
        prevPageAction.action.Enable();
        clickAction.action.Enable();
        pointerPositionAction.action.Enable();
    }

    private void OnDisable()
    {
        nextPageAction.action.Disable();
        prevPageAction.action.Disable();
        clickAction.action.Disable();
        pointerPositionAction.action.Disable();
    }

    void Start()
    {
        if (journal == null)
            journal = Journal.Instance;
    }

    private void Update()
    {
        if (!Application.isPlaying || journal == null)
            return;

        // .triggered is the equivalent of GetKeyDown
        if (nextPageAction.action.triggered)
        {
            journal.OpenNextPage();
        }

        if (prevPageAction.action.triggered)
        {
            journal.CloseCurrentPage();
        }

        UpdateHoveredGameObject();

        if (clickAction.action.triggered)
        {
            if (_lastHitObject == null) return;

            BookMark mark = _lastHitObject.GetComponent<BookMark>();
            if (mark != null)
            {
                journal.OpenPageNumber(mark.PageNumber);
            }
        }
    }

    private void UpdateHoveredGameObject()
    {
        // Read the pointer position (Mouse or Touch) from the action
        Vector2 mousePos = pointerPositionAction.action.ReadValue<Vector2>();
        Ray ray = journalCamera.ScreenPointToRay(mousePos);
        
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            _lastHitObject = hitInfo.collider.gameObject;
        }
        else
        {
            _lastHitObject = null;
        }
    }
}