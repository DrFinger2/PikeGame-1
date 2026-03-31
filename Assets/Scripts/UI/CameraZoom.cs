using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


/// <summary>
/// WARNING: Terrible code ahead. This class is doing way too much.
/// Fix this before it breeds. 
/// - Math -> CameraZoomMath
/// - Raycasting -> CameraZoomRaycaster
/// - Touch logic -> CameraZoomInput
/// </summary>
public class CameraZoom : MonoBehaviour
{
    private Camera cam;

    [Header("Touch Sensitivities")]
    [Tooltip("1 = Pure 1:1 proportional zoom. Lower values (e.g., 0.5) make the zoom feel heavier.")]
    public float zoomSpeedMultiplier = 0.74f;
    [Tooltip("How many FOV units from the min/max limits should the zoom start slowing down?")]
    public float zoomEdgeZone = 2.5f;

    [Header("Game Feel (Smoothing)")]
    public float panSmoothTime = 0.0225f;
    public float zoomSmoothTime = 0.0315f;

    [Header("Game Feel (Flick Momentum)")]
    public float panFriction = 14f;
    public float zoomFriction = 9.3f;

    [Header("Relative Zoom Constraints")]
    public float maxZoomInAmount = 13f;

    [Header("Pan Limits (At Max Zoom Out)")]
    public float minPanX = 0.88f;
    public float maxPanX = 3;
    public float minPanZ = -2.5f;
    public float maxPanZ = 2.2f;

    [Header("PC Debug Controls")]
    public float debugPanSpeed = 15f;
    public float debugKeyZoomSpeed = 15f;

    private float prevDistance;
    private Vector3 initialPosition;

    private Vector3 targetPosition;
    private float targetFOV;

    private Vector3 panVelocity;
    private float zoomVelocity;

    private Vector3 activePanMomentum;
    private float activeZoomMomentum;

    private Vector3 flatForward, flatRight;
    private float initialFOV, minFOV;
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    private float worldMinX, worldMaxX, worldMinZ, worldMaxZ;

    private bool isActivelyPanning;
    private bool isActivelyZooming;
    private bool isTouchOverUI;

    private bool needsBoundsRecalculation = false;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        SafeArea.OnResize.AddListener(OnScreenResized);
    }

    // FIX: Clean up the listener
    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        SafeArea.OnResize.RemoveListener(OnScreenResized);
    }

    private void OnScreenResized()
    {
        if (cam != null)
        {
             CalculateAbsoluteWorldBounds();
        }
    }

    void Start()
    {
        cam = GetComponent<Camera>();

        initialPosition = cam.transform.position;
        targetPosition = initialPosition;

        initialFOV = cam.fieldOfView;
        targetFOV = initialFOV;
        minFOV = initialFOV - maxZoomInAmount;

        CalculateAbsoluteWorldBounds();
    }

    void Update()
    {
        flatForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        flatRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        isActivelyPanning = false;
        isActivelyZooming = false;

        HandleTouchInput();

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleDebugInput();
#endif
    }

    void LateUpdate()
    {
        ClampAndSmoothCamera();
    }
    
    private void HandleTouchInput()
    {
        if (Touchscreen.current == null) return;

        int touchCount = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
        var touches = Touchscreen.current.touches;


        if (touchCount == 0)
        {
            isTouchOverUI = false;
        }
        else
        {
            // Just check the active fingers. If any of them just tapped a UI element, lock the camera.
            for (int i = 0; i < Mathf.Min(touchCount, 2); i++)
            {
                if (touches[i].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touches[i].touchId.ReadValue()))
                        isTouchOverUI = true;
                }
            }
        }

        if (isTouchOverUI)
        {
            activePanMomentum = Vector3.zero;
            activeZoomMomentum = 0f;
            return;
        }
        // --------------------------------

        if (touchCount == 2)
        {
            isActivelyZooming = true;

            Vector2 t1 = touches[0].position.ReadValue();
            Vector2 t2 = touches[1].position.ReadValue();
            float currentDist = Vector2.Distance(t1, t2);

            if (touches[1].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                prevDistance = currentDist;

            if (currentDist > 1f && prevDistance > 1f)
            {
                float previousTargetFOV = targetFOV;

                float rawPinchRatio = prevDistance / currentDist;
                float adjustedPinchRatio = Mathf.Lerp(1f, rawPinchRatio, zoomSpeedMultiplier);

                float proposedFOV = targetFOV * adjustedPinchRatio;
                float rawDelta = proposedFOV - targetFOV;

                float distanceToEdge = (rawDelta < 0) ? (targetFOV - minFOV) : (initialFOV - targetFOV);

                if (distanceToEdge < zoomEdgeZone && distanceToEdge > 0)
                {
                    float dampFactor = distanceToEdge / zoomEdgeZone;
                    rawDelta *= Mathf.SmoothStep(0f, 1f, dampFactor);
                }

                targetFOV += rawDelta;
                activeZoomMomentum = (targetFOV - previousTargetFOV) / Time.deltaTime;
            }

            activePanMomentum = Vector3.zero;
            prevDistance = currentDist;
        }
        else if (touchCount == 1 && tileManager.Instance.toolBeingUsed == false)
        {
            isActivelyPanning = true;

            Vector2 touchPos = touches[0].position.ReadValue();
            Vector2 touchDelta = touches[0].delta.ReadValue();

            Ray rayNow = cam.ScreenPointToRay(touchPos);
            Ray rayPrev = cam.ScreenPointToRay(touchPos - touchDelta);

            if (groundPlane.Raycast(rayNow, out float enterNow) && groundPlane.Raycast(rayPrev, out float enterPrev))
            {
                Vector3 worldNow = rayNow.GetPoint(enterNow);
                Vector3 worldPrev = rayPrev.GetPoint(enterPrev);

                Vector3 worldDelta = worldPrev - worldNow;

                targetPosition += worldDelta;
                activePanMomentum = worldDelta / Time.deltaTime;
                activeZoomMomentum = 0f;
            }
        }
        else
        {
            if (activePanMomentum.sqrMagnitude > 0.001f)
            {
                targetPosition += activePanMomentum * Time.deltaTime;
                activePanMomentum = Vector3.Lerp(activePanMomentum, Vector3.zero, panFriction * Time.deltaTime);
            }

            if (Mathf.Abs(activeZoomMomentum) > 0.001f)
            {
                targetFOV += activeZoomMomentum * Time.deltaTime;
                activeZoomMomentum = Mathf.Lerp(activeZoomMomentum, 0f, zoomFriction * Time.deltaTime);
            }
        }
    }

    private void HandleDebugInput()
    {
        if (Keyboard.current == null) return;

        if (tileManager.Instance.toolBeingUsed == false)
        {
            float x = (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0);
            float z = (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0);

            if (x != 0 || z != 0)
            {
                isActivelyPanning = true;

                float currentZoomRatio = targetFOV / initialFOV;
                Vector3 moveDelta = (flatRight * x + flatForward * z).normalized * (debugPanSpeed * currentZoomRatio) * Time.deltaTime;
                targetPosition += moveDelta;
                activePanMomentum = moveDelta / Time.deltaTime;
            }
        }

        if (Keyboard.current.eKey.isPressed || Keyboard.current.qKey.isPressed)
        {
            isActivelyZooming = true;
            float previousTargetFOV = targetFOV;

            float dir = Keyboard.current.eKey.isPressed ? -1f : 1f;
            float rawDelta = dir * debugKeyZoomSpeed * Time.deltaTime;
            float distanceToEdge = (rawDelta < 0) ? (targetFOV - minFOV) : (initialFOV - targetFOV);
            if (distanceToEdge < zoomEdgeZone && distanceToEdge > 0)
            {
                float dampFactor = distanceToEdge / zoomEdgeZone;
                rawDelta *= Mathf.SmoothStep(0f, 1f, dampFactor);
            }

            targetFOV += rawDelta;
            activeZoomMomentum = (targetFOV - previousTargetFOV) / Time.deltaTime;
        }
    }

    private void ClampAndSmoothCamera()
    {
        Vector3 realPos = cam.transform.position;
        float realFOV = cam.fieldOfView;

        if (targetFOV <= minFOV || targetFOV >= initialFOV)
        {
            targetFOV = Mathf.Clamp(targetFOV, minFOV, initialFOV);
            activeZoomMomentum = 0f;
        }

        cam.transform.position = targetPosition;
        cam.fieldOfView = targetFOV;
        GetGroundFootprint(out float curMinX, out float curMaxX, out float curMinZ, out float curMaxZ);

        float shiftX = 0f;
        float shiftZ = 0f;

        if (curMinX < worldMinX) shiftX = worldMinX - curMinX;
        else if (curMaxX > worldMaxX) shiftX = worldMaxX - curMaxX;

        if (curMinZ < worldMinZ) shiftZ = worldMinZ - curMinZ;
        else if (curMaxZ > worldMaxZ) shiftZ = worldMaxZ - curMaxZ;

        if (shiftX != 0f)
        {
            targetPosition.x += shiftX;
            activePanMomentum.x = 0f;
        }
        if (shiftZ != 0f)
        {
            targetPosition.z += shiftZ;
            activePanMomentum.z = 0f;
        }

        if (isActivelyZooming)
        {
            cam.fieldOfView = targetFOV;
            zoomVelocity = activeZoomMomentum;
        }
        else
        {
            float safeZoomSmooth = Mathf.Max(zoomSmoothTime, Time.deltaTime);
            cam.fieldOfView = Mathf.SmoothDamp(realFOV, targetFOV, ref zoomVelocity, safeZoomSmooth);
        }

        if (isActivelyPanning)
        {
            cam.transform.position = targetPosition;
            panVelocity = activePanMomentum;
        }
        else
        {
            float safePanSmooth = Mathf.Max(panSmoothTime, Time.deltaTime);
            cam.transform.position = Vector3.SmoothDamp(realPos, targetPosition, ref panVelocity, safePanSmooth);
        }
    }

    private void CalculateAbsoluteWorldBounds()
    {
        Vector3 cachedPosition = cam.transform.position;
        float cachedFOV = cam.fieldOfView;

        cam.fieldOfView = initialFOV;

        Vector3[] panCorners = new Vector3[] {
            initialPosition + new Vector3(minPanX, 0, minPanZ),
            initialPosition + new Vector3(minPanX, 0, maxPanZ),
            initialPosition + new Vector3(maxPanX, 0, minPanZ),
            initialPosition + new Vector3(maxPanX, 0, maxPanZ)
        };

        worldMinX = float.MaxValue; worldMaxX = float.MinValue;
        worldMinZ = float.MaxValue; worldMaxZ = float.MinValue;

        foreach (var pos in panCorners)
        {
            cam.transform.position = pos;
            GetGroundFootprint(out float minX, out float maxX, out float minZ, out float maxZ);

            if (minX < worldMinX) worldMinX = minX;
            if (maxX > worldMaxX) worldMaxX = maxX;
            if (minZ < worldMinZ) worldMinZ = minZ;
            if (maxZ > worldMaxZ) worldMaxZ = maxZ;
        }

        cam.transform.position = cachedPosition;
        cam.fieldOfView = cachedFOV;
    }


    private void GetGroundFootprint(out float minX, out float maxX, out float minZ, out float maxZ)
    {
        Vector3[] corners = new Vector3[] {
            new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,0,0), new Vector3(1,1,0)
        };

        minX = float.MaxValue; maxX = float.MinValue;
        minZ = float.MaxValue; maxZ = float.MinValue;

        foreach (var c in corners)
        {
            Ray r = cam.ViewportPointToRay(c);
            if (groundPlane.Raycast(r, out float distance))
            {
                Vector3 point = r.GetPoint(distance);
                if (point.x < minX) minX = point.x;
                if (point.x > maxX) maxX = point.x;
                if (point.z < minZ) minZ = point.z;
                if (point.z > maxZ) maxZ = point.z;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        float width = worldMaxX - worldMinX;
        float depth = worldMaxZ - worldMinZ;
        Vector3 center = new Vector3(worldMinX + (width / 2f), 0f, worldMinZ + (depth / 2f));

        Gizmos.DrawWireCube(center, new Vector3(width, 0.1f, depth));
    }
}
