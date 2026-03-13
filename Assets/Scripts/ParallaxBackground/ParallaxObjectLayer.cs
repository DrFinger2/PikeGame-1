using UnityEngine;
using static ParallaxConstants;

[ExecuteInEditMode]
public class ParallaxObjectLayer : MonoBehaviour
{
    public enum FacingAxis { Forward, Back, Up, Down, Left, Right }

    [Header("Settings")]
    [Tooltip("Distance from camera. Higher values = further away.")]
    [Range(0.1f, MAX_LAYER_DEPTH)]
    public float baseDepth = 10f;

    [Tooltip("Local offset relative to the camera center.")]
    public Vector2 offset = Vector2.zero;

    [Header("Orientation")]
    [Tooltip("Which local axis should point towards the camera's view direction?")]
    public FacingAxis forwardDirection = FacingAxis.Forward;

    private ParallaxController controller;

    void OnEnable()
    {
        if (controller == null)
            controller = FindFirstObjectByType<ParallaxController>();
    }

    void LateUpdate()
    {
        UpdatePositionAndRotation();
    }

    private void UpdatePositionAndRotation()
    {
        Camera cam = Camera.main;
        float zoom = 1f;
        Vector2 globalParallaxOffset = Vector2.zero;

        // 1. Get Data from Controller
        if (controller != null)
        {
            if (controller.trackedCamera != null) cam = controller.trackedCamera;
            zoom = controller.Zoom;
            globalParallaxOffset = controller.ParallaxPosition;
        }

        if (cam == null) return;

        // 2. Calculate Depth and Parallax
        float effectiveDepth = Mathf.Max(baseDepth / zoom, 0.001f);
        
        // Parallax Logic: Global offset is divided by depth (creating the speed difference)
        Vector2 parallaxDisplacement = globalParallaxOffset / effectiveDepth;
        Vector2 totalOffset = parallaxDisplacement + offset;

        // 3. Calculate World Position
        // Start at camera center at specific depth
        Vector3 centerPos = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, effectiveDepth));
        
        // Apply offsets along camera's local axes
        Vector3 finalPos = centerPos + 
                           (cam.transform.right * totalOffset.x) + 
                           (cam.transform.up * totalOffset.y);

        transform.position = finalPos;

        // 4. Calculate Rotation
        Quaternion baseRotation = cam.transform.rotation;
        Quaternion axisCorrection = Quaternion.identity;

        switch (forwardDirection)
        {
            case FacingAxis.Back:    axisCorrection = Quaternion.Euler(0, 180, 0); break;
            case FacingAxis.Up:      axisCorrection = Quaternion.Euler(90, 0, 0); break;
            case FacingAxis.Down:    axisCorrection = Quaternion.Euler(-90, 0, 0); break;
            case FacingAxis.Left:    axisCorrection = Quaternion.Euler(0, 90, 0); break;
            case FacingAxis.Right:   axisCorrection = Quaternion.Euler(0, -90, 0); break;
            default:                 axisCorrection = Quaternion.identity; break;
        }

        transform.rotation = baseRotation * axisCorrection;
    }
}