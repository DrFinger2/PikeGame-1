using UnityEngine;

public class FocusUI : MonoBehaviour
{
    // Singleton Instance
    public static FocusUI Instance { get; private set; }


    [Header("References")]
    public RectTransform pointerUI;
    public RectTransform touchUI;



    [Header("Settings")]
    public float edgePadding = 50f;
    public Vector3 pointerOffset = new Vector3(100f, 100f, 0f);

    private Camera mainCamera;
    private Transform pointerTarget;
    private Transform touchTarget;
    private void Awake()
    {
        if (Instance != this)
        {
            Instance = this;
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        pointerUI?.gameObject.SetActive(false);
        touchUI?.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!mainCamera) 
            return;

        if (pointerUI && pointerTarget)
        {
            Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(pointerTarget.position);
            bool isBehindCamera = targetScreenPos.z < 0;
            if (isBehindCamera)
            {
                targetScreenPos = new Vector3(Screen.width - targetScreenPos.x, Screen.height - targetScreenPos.y, 0);
            }

            Vector3 clampedPos = targetScreenPos;
            clampedPos.x = Mathf.Clamp(clampedPos.x, edgePadding, Screen.width - edgePadding);
            clampedPos.y = Mathf.Clamp(clampedPos.y, edgePadding, Screen.height - edgePadding);
            clampedPos.z = 0;

            Vector3 finalPosition = clampedPos + pointerOffset;
            pointerUI.position = finalPosition;
            Vector3 dirToTarget = targetScreenPos - finalPosition;

            if (isBehindCamera)
            {
                dirToTarget = -dirToTarget;
            }

            if (dirToTarget.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
                pointerUI.localEulerAngles = new Vector3(0, 0, angle);
            }
        }

        if (touchUI && touchTarget)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(touchTarget.position);
            
            if (screenPos.z > 0)
            {
                screenPos.z = 0;
                touchUI.position = screenPos;
            }
        }
        
    }
  
  
  
    

    public void ShowPointer(GameObject obj)
    {
        pointerTarget = obj?.transform;
        pointerUI?.gameObject.SetActive(pointerTarget != null);
    }


    public void HidePointer()
    {
        pointerTarget = null;
        pointerUI?.gameObject.SetActive(false);
    }


    public void ShowTouchGesture(GameObject obj)
    {
        touchTarget = obj?.transform;
        touchUI?.gameObject.SetActive(touchTarget != null);
    }
    

    public void HideTouchGesture()
    {
        touchTarget = null;
        touchUI?.gameObject.SetActive(false);
    }
}
