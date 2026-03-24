using UnityEngine;

[ExecuteAlways]
public class JournalPage : MonoBehaviour
{
    [Header("Settings")]
    // This now correctly represents the unique index of this page sheet in the journal.
    [HideInInspector] public int pageNumber = -1;
    
    [Header("Animation")]
    [Range(0f, 1f)] public float time = 0f;

    [Header("References")]
    [SerializeField] private MeshRenderer pageMesh;
    [SerializeField] private BookMark bookmarkFront;
    [SerializeField] private BookMark bookmarkBack;
    [SerializeField] public AnimationClip clip;

    private float m_LastTime = -1f;

    public void OnEnable()
    {
        // Reset the animation time tracker when the object is enabled.
        m_LastTime = -1f;
    }

    /// <summary>
    /// FIX: This method is now much simpler and more reliable.
    /// It returns the world-space center of the page's visible mesh.
    /// </summary>
    public Vector3 GetCenterPosition()
    {
        if (pageMesh != null)
        {
            // Renderer.bounds.center gives the accurate, real-time center of the object in world space.
            return pageMesh.bounds.center;
        }
        
        // Fallback to the GameObject's transform position if the mesh isn't assigned.
        Debug.LogWarning($"Page Mesh Renderer is not assigned on {gameObject.name}, falling back to transform position.");
        return transform.position;
    }
    
    // NOTE: The old, complex center calculation methods (UpdateCenterPosition, RotatePointAlongPivot) have been removed
    // as they are no longer needed.

    /// <summary>
    /// Applies textures to the front and back materials of the page.
    /// </summary>
    public void SetupPage(Texture2D frontTexture, Texture2D backTexture)
    {
        if (pageMesh == null)
        {
            Debug.LogError("Page Mesh Renderer is not assigned on " + gameObject.name);
            return;
        }

        const string texturePropertyName = "_BaseMap";

        // Material at index 2 is the front face
        MaterialPropertyBlock frontBlock = new MaterialPropertyBlock();
        pageMesh.GetPropertyBlock(frontBlock, 2);
        frontBlock.SetTexture(texturePropertyName, frontTexture);
        pageMesh.SetPropertyBlock(frontBlock, 2);

        // Material at index 1 is the back face
        MaterialPropertyBlock backBlock = new MaterialPropertyBlock();
        pageMesh.GetPropertyBlock(backBlock, 1);
        backBlock.SetTexture(texturePropertyName, backTexture);
        pageMesh.SetPropertyBlock(backBlock, 1);
    }

    /// <summary>
    /// Applies data to the front and back bookmarks.
    /// </summary>
    public void SetupBookmarks(BookMarkData frontData, BookMarkData backData)
    {
        if (frontData == null || backData == null)
            return;
            
        if (bookmarkFront != null)
        {
            bookmarkFront.ApplyBookmarkData(frontData);
        }
        if (bookmarkBack != null)
        {
            bookmarkBack.ApplyBookmarkData(backData);
        }
    }

    /// <summary>
    /// Samples the page turn animation based on the 'time' variable.
    /// </summary>
    void LateUpdate()
    {
        if (clip == null)
            return;
        
        // Only update the animation if the time has changed.
        if (time != m_LastTime)
        {
            time = Mathf.Clamp01(time);
            clip.SampleAnimation(gameObject, time * clip.length);
            
            m_LastTime = time;
        }
    }
}
