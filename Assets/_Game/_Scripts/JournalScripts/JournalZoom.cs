using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch; 
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(Camera))]
public class JournalCamera : MonoBehaviour
{
    public static JournalCamera Instance;

    [Header("Targets")]
    [SerializeField] private Transform m_JournalTarget;
    [SerializeField] private MeshRenderer m_PageMesh;

    [Header("Zoom Settings (FOV)")]
    [SerializeField] private float m_ZoomRange = 40f;
    [SerializeField] private float m_ZoomSensitivity = 0.6f;

    [Header("Panning Settings (XY Plane)")]
    [SerializeField] private float m_PanSensitivity = 0.01f;
    [SerializeField] private Vector2 m_PanLimits = new Vector2(2.5f, 2.5f);

    [Header("Smoothing")]
    [Tooltip("How fast the camera tracks your finger/mouse (snappy)")]
    [SerializeField] private float m_SmoothTime = 0.15f;
    [Tooltip("How smoothly the camera glides when turning to a cover (cinematic)")]
    [SerializeField] private float m_AutoSmoothTime = 0.6f; // --- NEW ---

    private Camera m_Cam;
    private float m_MaxFOV;
    private float m_MinFOV;
    private float m_TargetFOV;

    private Vector3 m_InitialLocalPos;
    private Vector2 m_TargetOffset;
    private Vector2 m_CurrentOffset;
    private Vector2 m_PanVelocity;

    private float m_CurrentFOV;
    private float m_FOVVelocity;

    private float m_ActiveSmoothTime; // --- NEW ---

    void OnEnable() { EnhancedTouchSupport.Enable(); }
    void OnDisable() { EnhancedTouchSupport.Disable(); }

    void Awake()
    {
        Instance = this;
        m_Cam = GetComponent<Camera>();
        m_MaxFOV = m_Cam.fieldOfView;
        m_MinFOV = m_MaxFOV - m_ZoomRange;
        m_CurrentFOV = m_MaxFOV;
        m_TargetFOV = m_MaxFOV;
        m_ActiveSmoothTime = m_SmoothTime; // Start with default

        if (m_JournalTarget != null)
            m_InitialLocalPos = m_JournalTarget.localPosition;
    }

    void Update()
    {
        if (m_JournalTarget == null || !Application.isPlaying) return;

        HandleInput();
        ApplyTransform();
    }

    private void HandleInput()
    {
        var activeTouches = Touch.activeTouches;

        // --- NEW: Detect if the user is interacting to restore snappy movement ---
        bool hasInput = activeTouches.Count > 0 ||
                        (Mouse.current != null && (Mouse.current.leftButton.isPressed || Mathf.Abs(Mouse.current.scroll.ReadValue().y) > 0.1f));

        if (hasInput)
        {
            m_ActiveSmoothTime = m_SmoothTime;
        }

        // --- 1. PANNING ---
        if (activeTouches.Count == 1)
        {
            Vector2 delta = activeTouches[0].delta;
            m_TargetOffset.x += delta.x * m_PanSensitivity;
            m_TargetOffset.y += delta.y * m_PanSensitivity;
        }
        else if (activeTouches.Count == 0 && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector2 mDelta = Mouse.current.delta.ReadValue();
            m_TargetOffset.x += mDelta.x * m_PanSensitivity;
            m_TargetOffset.y += mDelta.y * m_PanSensitivity;
        }

        // --- 2. ZOOMING ---
        if (activeTouches.Count == 2)
        {
            var t0 = activeTouches[0];
            var t1 = activeTouches[1];
            float prevMag = ((t0.screenPosition - t0.delta) - (t1.screenPosition - t1.delta)).magnitude;
            float currentMag = (t0.screenPosition - t1.screenPosition).magnitude;

            // Zoom IN = decrease FOV, Zoom OUT = increase FOV
            m_TargetFOV -= (currentMag - prevMag) * m_ZoomSensitivity;
        }
        else if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.1f)
            {
                m_TargetFOV -= (scroll / 120f) * m_ZoomSensitivity * 10f;
            }
        }

        m_TargetOffset.x = Mathf.Clamp(m_TargetOffset.x, -m_PanLimits.x, m_PanLimits.x);
        m_TargetOffset.y = Mathf.Clamp(m_TargetOffset.y, -m_PanLimits.y, m_PanLimits.y);
        m_TargetFOV = Mathf.Clamp(m_TargetFOV, m_MinFOV, m_MaxFOV);
    }

    private void ApplyTransform()
    {
        // --- UPDATED: Use m_ActiveSmoothTime instead of m_SmoothTime ---
        m_CurrentOffset = Vector2.SmoothDamp(m_CurrentOffset, m_TargetOffset, ref m_PanVelocity, m_ActiveSmoothTime);
        m_CurrentFOV = Mathf.SmoothDamp(m_CurrentFOV, m_TargetFOV, ref m_FOVVelocity, m_ActiveSmoothTime);

        // X, Y panning only to maintain constant depth
        m_JournalTarget.localPosition = m_InitialLocalPos + new Vector3(m_CurrentOffset.x, m_CurrentOffset.y, 0);
        m_Cam.fieldOfView = m_CurrentFOV;
    }

    public void AutoFocus(int pageIndex, int totalPages, float fovPercent = 0.0f)
    {
        // 1. Determine Horizontal Offset
        float horizontalOffset = 0f;

        if (pageIndex == 0 && m_PageMesh != null) // Front Cover
        {
            horizontalOffset = -m_PageMesh.bounds.extents.x;
        }
        else if (pageIndex == totalPages && m_PageMesh != null) // Back Cover
        {
            horizontalOffset = m_PageMesh.bounds.extents.x;
        }
        else // Middle Pages
        {
            horizontalOffset = 0f;
        }

        // 2. Apply the Reset
        m_TargetOffset = new Vector2(horizontalOffset, 0f);

        // Reset FOV to the specified zoom level (0.0 = fully zoomed out)
        m_TargetFOV = Mathf.Lerp(m_MaxFOV, m_MinFOV, fovPercent);

        // --- NEW: Trigger the slower, cinematic smooth time ---
        m_ActiveSmoothTime = m_AutoSmoothTime;
    }
}
