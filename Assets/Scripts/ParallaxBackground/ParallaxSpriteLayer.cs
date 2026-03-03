using UnityEngine;
using static ParallaxConstants;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ParallaxSpriteLayer : MonoBehaviour
{
    public enum HorizontalAlignment { Center, Left, Right }

    [Header("Layer Settings")]
    [Tooltip("Base distance from camera along its forward axis. Larger values result in slower movement, creating a sense of distance.")]
    [Range(0.1f, MAX_LAYER_DEPTH)]
    public float baseDepth = 5f;

    [Header("Target SpriteRenderers")]
    [Tooltip("SpriteRenderers to position and display. Index 0 is primary, subsequent are ghosts for Right, Left, Up, Down.")]
    public SpriteRenderer[] targetRenderers = new SpriteRenderer[5];

    [Tooltip("Sprite asset to assign to all renderers.")]
    public Sprite sprite;

    [Tooltip("Scale multiplier for the sprites (width, height).")]
    public Vector2 spriteSize = Vector2.one;

    [HideInInspector] public Vector2 cameraOffset = Vector2.zero;

    [Header("Offset Control")]
    [Tooltip("User-controlled offset (additional adjustment)")]
    public Vector2 userOffset = Vector2.zero;

    [Tooltip("Horizontal alignment of primary relative to viewport.")]
    public HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;

    [Header("Wrapping Direction")]
    [Tooltip("Enable infinite horizontal scrolling (wrapping).")]
    public bool enableHorizontalWrapping = true;

    [Tooltip("Enable infinite vertical scrolling (wrapping).")]
    public bool enableVerticalWrapping = true;

    [Header("Gizmos")]
    [Tooltip("Color of the viewport plane gizmo.")]
    public Color gizmoColor = Color.yellow;
    
    [Tooltip("Color for sprite bounds when inside plane.")]
    public Color spriteBoundsColor = Color.green;

    private Vector2 wrappedOffset;
    private Camera currentCamera;
    private bool isVisible = true;
    private float currentZoom = 1f;
    private ParallaxGroup parallaxGroup;

    // Cache the visual size of the sprite for calculations
    private Vector2 cachedSpriteExtents; 

    void OnValidate()
    {
        UpdateWithParentCamera();
    }

#if UNITY_EDITOR
    void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
        UpdateWithParentCamera();
    }
    
    void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }
    
    void EditorUpdate()
    {
        if (!Application.isPlaying && currentCamera != null && transform.hasChanged)
        {
            UpdateAll();
            transform.hasChanged = false;
        }
    }

    void Reset()
    {
        gameObject.name = "ParallaxSpriteLayer";
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
        int existingCount = Mathf.Min(childRenderers.Length, 5);

        targetRenderers = new SpriteRenderer[5];

        for (int i = 0; i < existingCount; i++)
        {
            targetRenderers[i] = childRenderers[i];
        }

        string[] childNames = { "Primary", "Right Ghost", "Left Ghost", "Up Ghost", "Down Ghost" };
        for (int i = existingCount; i < 5; i++)
        {
            GameObject newChild = new GameObject(childNames[i]);
            newChild.transform.SetParent(transform);
            newChild.transform.localPosition = Vector3.zero;
            targetRenderers[i] = newChild.AddComponent<SpriteRenderer>();
        }

        if (sprite == null && targetRenderers[0] != null && targetRenderers[0].sprite != null)
        {
            sprite = targetRenderers[0].sprite;
        }
    }
#else 
    void OnEnable()
    {
        UpdateWithParentCamera();
    }
#endif

    void LateUpdate()
    {
        if (Application.isPlaying)
        {
            UpdateAll();
        }
    }

    public void SetVisibility(bool visible)
    {
        isVisible = visible;
        if (enabled)
        {
            UpdateAll();
        }
    }

    public void UpdateWithCamera(Camera cam, float zoom = 1f)
    {
        currentCamera = cam;
        currentZoom = zoom;
        if (enabled)
        {
            UpdateAll();
        }
    }

    private void UpdateWithParentCamera()
    {
        if (currentCamera == null)
        {
            ParallaxController parallaxController = GetComponentInParent<ParallaxController>();
            if (parallaxController != null)
            {
                UpdateWithCamera(parallaxController.trackedCamera, parallaxController.Zoom);
            }
        }

        if (parallaxGroup == null)
        {
            parallaxGroup = GetComponentInParent<ParallaxGroup>();
        }
    }

    private void UpdateAll()
    {
        if (targetRenderers == null || targetRenderers.Length == 0 || targetRenderers[0] == null)
            return;

        if (currentCamera == null)
        {
            UpdateWithParentCamera();
            if (currentCamera == null) return;
        }

        if (parallaxGroup == null)
            parallaxGroup = GetComponentInParent<ParallaxGroup>();

        // 1. Calculate Dimensions
        float effectiveDepth = Mathf.Max(baseDepth / currentZoom, 0.001f);
        Vector2 parallaxDisplacement = cameraOffset / effectiveDepth;
        Vector2 totalOffset = parallaxDisplacement + userOffset;

        // Calculate Plane Dimensions (Width/Height at this depth)
        Vector3 p0 = currentCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, effectiveDepth));
        Vector3 p1 = currentCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, effectiveDepth));
        Vector3 p2 = currentCamera.ViewportToWorldPoint(new Vector3(0.5f, 0, effectiveDepth));
        Vector3 p3 = currentCamera.ViewportToWorldPoint(new Vector3(0.5f, 1, effectiveDepth));

        float planeWorldWidth = Vector3.Distance(p0, p1);
        float planeWorldHeight = Vector3.Distance(p2, p3);

        if (planeWorldWidth <= 0 || planeWorldHeight <= 0) return;

        // 2. Alignment & Wrapping Logic
        float vx = horizontalAlignment == HorizontalAlignment.Left ? 0f :
                   horizontalAlignment == HorizontalAlignment.Right ? 1f : 0.5f;

        float horizontalAlignmentOffset = (vx - 0.5f) * planeWorldWidth;
        float totalEffectiveXOffset = totalOffset.x + horizontalAlignmentOffset;

        wrappedOffset = totalOffset;

        if (enableHorizontalWrapping)
        {
            wrappedOffset.x = totalEffectiveXOffset % planeWorldWidth;
            if (wrappedOffset.x > planeWorldWidth / 2f) wrappedOffset.x -= planeWorldWidth;
            if (wrappedOffset.x < -planeWorldWidth / 2f) wrappedOffset.x += planeWorldWidth;
            wrappedOffset.x -= horizontalAlignmentOffset;
        }

        if (enableVerticalWrapping)
        {
            wrappedOffset.y = totalOffset.y % planeWorldHeight;
            if (wrappedOffset.y > planeWorldHeight / 2f) wrappedOffset.y -= planeWorldHeight;
            if (wrappedOffset.y < -planeWorldHeight / 2f) wrappedOffset.y += planeWorldHeight;
        }

        // 3. Update Primary Sprite
        float groupAlpha = (parallaxGroup != null) ? parallaxGroup.alpha : 1f;

        AssignSprite(targetRenderers[0]);
        UpdateTransform(targetRenderers[0], Vector3.zero, effectiveDepth);
        UpdateCachedSpriteSize(targetRenderers[0]); // Cache size for intersection checks
        SetAlpha(targetRenderers[0], isVisible ? groupAlpha : 0);

        // 4. Ghost Calculation
        // Note: Using transform.right/up from camera handles rotation correctly for offsets
        Vector3 w_vec = currentCamera.transform.right * planeWorldWidth;
        Vector3 h_vec = currentCamera.transform.up * planeWorldHeight;

        Vector3[] ghostOffsets = {
            enableHorizontalWrapping ? w_vec : Vector3.zero,
            enableHorizontalWrapping ? -w_vec : Vector3.zero,
            enableVerticalWrapping ? h_vec : Vector3.zero,
            enableVerticalWrapping ? -h_vec : Vector3.zero
        };

        for (int i = 1; i < targetRenderers.Length; i++)
        {
            SpriteRenderer ghostRenderer = targetRenderers[i];
            if (ghostRenderer == null) continue;

            Vector3 offset = (i - 1 < ghostOffsets.Length) ? ghostOffsets[i - 1] : Vector3.zero;
            
            // Skip disabled wrap directions
            if (offset == Vector3.zero)
            {
                SetAlpha(ghostRenderer, 0);
                continue;
            }

            // Calculate potential position
            Vector3 targetPos = targetRenderers[0].transform.position + offset;
            
            // Check Visibility in Local Space (Fixes intersection bugs)
            bool shouldBeEnabled = isVisible && IsVisibleInCamera(targetPos, planeWorldWidth, planeWorldHeight, effectiveDepth);

            SetAlpha(ghostRenderer, shouldBeEnabled ? groupAlpha : 0);
            if (shouldBeEnabled)
            {
                AssignSprite(ghostRenderer);
                UpdateTransform(ghostRenderer, offset, effectiveDepth);
            }
        }
    }

    // New Helper: Checks visibility by converting world position to Camera Local Space
    // This ignores World Rotation and relies on pure relative distances
    private bool IsVisibleInCamera(Vector3 worldPos, float planeWidth, float planeHeight, float depth)
    {
        if (currentCamera == null) return false;

        // Convert world position to Camera Local Space
        Vector3 localPos = currentCamera.transform.InverseTransformPoint(worldPos);

        // The center of the parallax plane in local space is (0, 0, depth)
        // We check if the distance from center is within (PlaneExtent + SpriteExtent)
        
        float xDist = Mathf.Abs(localPos.x);
        float yDist = Mathf.Abs(localPos.y);

        float xThreshold = (planeWidth / 2f) + cachedSpriteExtents.x;
        float yThreshold = (planeHeight / 2f) + cachedSpriteExtents.y;

        return xDist <= xThreshold && yDist <= yThreshold;
    }

    private void UpdateCachedSpriteSize(SpriteRenderer rend)
    {
        if (rend != null && rend.sprite != null)
        {
            // Calculate extent (half size) in local units, scaled by transform scale
            // We use the larger dimension to be safe (simplifies rotation logic to a circle/box approximation)
            Vector2 size = rend.sprite.bounds.size;
            Vector2 scaledSize = Vector2.Scale(size, spriteSize); 
            cachedSpriteExtents = scaledSize * 0.5f; 
        }
        else
        {
            cachedSpriteExtents = Vector2.zero;
        }
    }

    private void UpdateTransform(SpriteRenderer rend, Vector3 worldOffset, float effectiveDepth)
    {
        if (rend == null || currentCamera == null) return;

        float vx = horizontalAlignment == HorizontalAlignment.Left ? 0f :
                   horizontalAlignment == HorizontalAlignment.Right ? 1f : 0.5f;

        Vector3 basePos = currentCamera.ViewportToWorldPoint(new Vector3(vx, 0.5f, effectiveDepth));
        Vector3 logicalOffsetWS = currentCamera.transform.right * wrappedOffset.x +
                                  currentCamera.transform.up * wrappedOffset.y;

        Transform t = rend.transform;
        t.position = basePos + logicalOffsetWS + worldOffset;
        t.rotation = currentCamera.transform.rotation;
        t.localScale = new Vector3(spriteSize.x, spriteSize.y, 1f);
    }

    private void AssignSprite(SpriteRenderer rend)
    {
        if (rend != null && rend.sprite != sprite)
            rend.sprite = sprite;
    }

    private void SetAlpha(SpriteRenderer rend, float alpha)
    {
        if (rend == null) return;

        bool shouldBeEnabled = alpha > 0.001f;
        if (rend.enabled != shouldBeEnabled)
            rend.enabled = shouldBeEnabled;

        rend.color = new Color(1f, 1f, 1f, alpha);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (currentCamera == null) UpdateWithParentCamera();
        if (currentCamera == null) return;
        
        UpdateAll(); 

        float effectiveDepth = baseDepth / currentZoom;
        
        // 1. Draw Viewport Plane (oriented correctly using Camera Matrix)
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = currentCamera.transform.localToWorldMatrix;
        Gizmos.color = gizmoColor;
        
        // Calculate viewport size at this depth
        float h = 2f * effectiveDepth * Mathf.Tan(currentCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float w = h * currentCamera.aspect;
        
        // Draw wire cube at local position (0,0,depth) with size (w,h,0)
        Gizmos.DrawWireCube(new Vector3(0, 0, effectiveDepth), new Vector3(w, h, 0));

        // 2. Draw Sprite Bounds (oriented correctly using Sprite Matrix)
        Gizmos.matrix = oldMatrix; // Reset first

        if (targetRenderers[0] != null && targetRenderers[0].enabled)
        {
            Transform t = targetRenderers[0].transform;
            Gizmos.matrix = t.localToWorldMatrix;
            Gizmos.color = spriteBoundsColor;
            
            if (targetRenderers[0].sprite != null)
            {
                // Draw based on the actual sprite bounds (accounts for pivot)
                Bounds b = targetRenderers[0].sprite.bounds;
                Gizmos.DrawWireCube(b.center, b.size);
            }
        }
        
        // Restore matrix
        Gizmos.matrix = oldMatrix;
    }
#endif
}