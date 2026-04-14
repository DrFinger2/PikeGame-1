using UnityEngine;

public class ScreenPointerUI : MonoBehaviour
{
    // Singleton Instance
    public static ScreenPointerUI Instance { get; private set; }


    [Header("References")]
    public RectTransform arrowUI;

    [Header("Settings")]
    public float edgePadding = 50f;
    public Vector3 offset = new Vector3(100f, 100f, 0f);

    private Camera mainCamera;
    private Transform target;

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
        arrowUI?.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!target || !arrowUI || !mainCamera) return;

        // 1. Get the true screen position of the target
        Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(target.position);
        bool isBehind = targetScreenPos.z < 0;

        // If behind the camera, flip the coordinates to the opposite side of the screen
        if (isBehind)
        {
            targetScreenPos = new Vector3(Screen.width - targetScreenPos.x, Screen.height - targetScreenPos.y, 0);
        }

        // 2. Calculate where the arrow SHOULD sit on the screen (clamped to edges)
        Vector3 clampedPos = targetScreenPos;
        clampedPos.x = Mathf.Clamp(clampedPos.x, edgePadding, Screen.width - edgePadding);
        clampedPos.y = Mathf.Clamp(clampedPos.y, edgePadding, Screen.height - edgePadding);
        clampedPos.z = 0;

        // 3. Apply your offset to get the FINAL physical position of the UI element
        Vector3 finalPosition = clampedPos + offset;
        arrowUI.position = finalPosition;

        // 4. Calculate rotation pointing FROM the final UI position TO the target's screen position
        Vector3 dirToTarget = targetScreenPos - finalPosition;

        // If the target is behind us, invert the direction so the arrow points 
        // outward towards the edge of the screen instead of inward
        if (isBehind)
        {
            dirToTarget = -dirToTarget;
        }

        // 5. Apply Rotation
        if (dirToTarget.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;

            // Uncomment if your raw arrow sprite naturally points UP instead of RIGHT
            // angle -= 90f; 

            arrowUI.localEulerAngles = new Vector3(0, 0, angle);
        }
    }
  
  
  
    

    public void PointAt(GameObject obj)
    {
        target = obj?.transform;
        arrowUI?.gameObject.SetActive(target != null);
    }

    public void StopPointing()
    {
        target = null;
        arrowUI?.gameObject.SetActive(false);
    }
}
