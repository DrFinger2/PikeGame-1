using UnityEngine;
using Utils;

[ExecuteAlways] // This allows the script to run in the editor
[RequireComponent(typeof(Camera))]
public class JournalCamera : SingletonMonoBehaviour<JournalCamera>
{
    [Header("Core Components")]
    [SerializeField] private Journal journal;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 cameraRotation = new Vector3(90, 0, 180);
    [SerializeField] private float distanceFromJournal = 1f;
    [Tooltip("How quickly the camera moves to its target. Higher is faster.")]
    [SerializeField] private float cameraSmoothingSpeed = 5f;

    [SerializeField, HideInInspector]
    private Camera m_camera;

    // --- Target transform for smoothing ---
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // This function is called when the script is loaded or a value is changed in the Inspector.
    void OnEnable()
    {
        m_camera = this.gameObject.GetComponent<Camera>();
        if (m_camera == null)
        {
            Debug.LogWarning("Unable to get the camera component!");
        }
        // Initialize targets to the camera's current transform to prevent snapping on start.
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

#if UNITY_EDITOR
    // This is called when a value is changed in the Inspector in Edit Mode.
    private void OnValidate()
    {
        // To provide live feedback in the editor, we snap the camera to its position
        // when a setting is changed. This only runs when the game is not playing.
        if (!Application.isPlaying && journal != null)
        {
            JournalPage currentPage = journal.GetCurrentPage();
            if (currentPage != null)
            {
                SnapToPage(currentPage);
            }
        }
    }
#endif

    /// <summary>
    /// Sets the target for the camera to smoothly move towards during gameplay.
    /// </summary>
    public void CenterCameraOnPage(JournalPage journalPage)
    {
        if (journalPage == null || m_camera == null)
        {
            return;
        }

        // Calculate the desired position and rotation based on the page's center.
        Vector3 pageCenter = journalPage.GetCenterPosition();
        targetRotation = Quaternion.Euler(cameraRotation);

        // To calculate the position correctly, we find the direction vector based on the target rotation.
        Vector3 direction = targetRotation * Vector3.forward;
        targetPosition = pageCenter - (direction * distanceFromJournal);
    }

    /// <summary>
    /// Instantly moves (snaps) the camera to the target page. Used for live editor updates.
    /// </summary>
    private void SnapToPage(JournalPage journalPage)
    {
        if (journalPage == null || m_camera == null)
        {
            return;
        }
        
        Vector3 pageCenter = journalPage.GetCenterPosition();
        Quaternion rotation = Quaternion.Euler(cameraRotation);

        Vector3 direction = rotation * Vector3.forward;
        Vector3 position = pageCenter - (direction * distanceFromJournal);

        transform.position = position;
        transform.rotation = rotation;
        
        // Also update the smoothing targets so the camera doesn't jump when entering play mode.
        targetPosition = position;
        targetRotation = rotation;
    }

    // LateUpdate is called after all Update functions have been called.
    // This is the best place to apply camera movement.
    void LateUpdate()
    {
        if (m_camera == null) return;
        
        // Only perform smoothing when the application is playing.
        if (Application.isPlaying)
        {
            // Use Lerp and Slerp to smoothly move the camera towards its target position and rotation.
            float lerpFactor = Time.deltaTime * cameraSmoothingSpeed;
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpFactor);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lerpFactor);
        }
    }
}
