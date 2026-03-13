using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CanvasCameraAligner : MonoBehaviour
{
    [Tooltip("The World Space Canvas this camera should point at.")]
    public Canvas targetCanvas;

    [Tooltip("The Camera component attached to this GameObject.")]
    [SerializeField] private Camera cam;

    private void Awake()
    {
        // Fallback just in case you forget to assign it in the Inspector
        if (cam == null) cam = GetComponent<Camera>();
        
        AlignCameraToCanvas();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            AlignCameraToCanvas();
        }
    }


    public void AlignCameraToCanvas()
    {
        // Double-check for Edit Mode reloads
        if (cam == null) cam = GetComponent<Camera>();
        if (targetCanvas == null) return;

        RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
        
        // 1. Force orthographic mode
        cam.orthographic = true;

        // 2. Match the exact rotation of the Canvas
        transform.rotation = canvasRect.rotation;

        // 3. Automate the distance and clipping planes
        float automaticSafeDistance = 50f;
        transform.position = canvasRect.position - (transform.forward * automaticSafeDistance);
        
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = automaticSafeDistance * 2f; 

        // 4. Perfectly size the camera's viewing volume to match the canvas height
        cam.orthographicSize = (canvasRect.rect.height * canvasRect.lossyScale.y) / 2f;
    }
}