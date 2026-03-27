using UnityEngine;

[ExecuteAlways]
public class JournalPage : MonoBehaviour
{
    [Header("Settings")]
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
        m_LastTime = -1f;
        ApplyJournalLayer();
    }

    void Start()
    {
        ApplyJournalLayer();
    }

    void OnValidate()
    {
        ApplyJournalLayer();
    }

    private void ApplyJournalLayer()
    {
        Journal journal = Journal.Instance;

        if (journal == null)
        {
            return;
        }

        if (journal != null)
        {
            int journalLayer = journal.gameObject.layer;
            
            this.gameObject.layer = journalLayer;

            if (pageMesh != null)
            {
                pageMesh.gameObject.layer = journalLayer;
            }
        }
    }

    public Vector3 GetCenterPosition()
    {
        if (pageMesh != null)
        {
            return pageMesh.bounds.center;
        }
        
        Debug.LogWarning($"Page Mesh Renderer is not assigned on {gameObject.name}, falling back to transform position.");
        return transform.position;
    }

    public void SetupPage(Texture2D frontTexture, Texture2D backTexture)
    {
        if (pageMesh == null)
        {
            Debug.LogError("Page Mesh Renderer is not assigned on " + gameObject.name);
            return;
        }

        const string texturePropertyName = "_BaseMap";

        MaterialPropertyBlock frontBlock = new MaterialPropertyBlock();
        pageMesh.GetPropertyBlock(frontBlock, 2);
        frontBlock.SetTexture(texturePropertyName, frontTexture);
        pageMesh.SetPropertyBlock(frontBlock, 2);

        MaterialPropertyBlock backBlock = new MaterialPropertyBlock();
        pageMesh.GetPropertyBlock(backBlock, 1);
        backBlock.SetTexture(texturePropertyName, backTexture);
        pageMesh.SetPropertyBlock(backBlock, 1);
    }

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

    void LateUpdate()
    {
        if (clip == null)
            return;
        
        if (time != m_LastTime)
        {
            time = Mathf.Clamp01(time);
            clip.SampleAnimation(gameObject, time * clip.length);
            
            m_LastTime = time;
        }
    }
}