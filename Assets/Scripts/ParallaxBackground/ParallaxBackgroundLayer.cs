using UnityEngine;
using static ParallaxConstants;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class ParallaxBackgroundLayer : MonoBehaviour
{
    [Header("Texture Settings")]
    public Texture2D layerTexture;
    public Color tintColor = Color.white;
    [Range(0, 1)] public float alpha = 1f;

    [Header("Scrolling Settings")]
    [Tooltip("Speed in world units per second. Positive X moves texture right, positive Y moves texture up. Only works in Play Mode")]
    public Vector2 scrollSpeed = Vector2.zero;

    [Header("Parallax Settings")]
    [Range(0.1f, MAX_LAYER_DEPTH)] public float baseDepth = 10f;
    public bool lockYAxis = false;

    [Header("Tiling & Fade")]
    public Vector2 tiling = Vector2.one;
    public bool enableTiling = true;
    public bool enableFade = true;

    [SerializeField, HideInInspector]
    private Vector2 textureOffset;
    
    private Vector2 scrollAccumulator;
    private MaterialPropertyBlock propBlock;
    private MeshRenderer meshRenderer;
    private ParallaxController parallaxCamera;
    private ParallaxGroup parallaxGroup;

#if UNITY_EDITOR
    void Reset()
    {
        gameObject.name = "ParallaxBackgroundLayer";
        
        var mf = GetComponent<MeshFilter>();
        var tempQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        if (mf != null && tempQuad != null)
        {
            mf.sharedMesh = tempQuad.GetComponent<MeshFilter>().sharedMesh;
        }
        DestroyImmediate(tempQuad);

        var mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            var mat = Resources.Load<Material>("Materials/URP_ParallaxMaterial");
            mr.sharedMaterial = mat != null ? mat : new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        }
        
        Initialize();
    }

    void OnValidate()
    {
        if (parallaxCamera == null)
        {
            parallaxCamera = GetComponentInParent<ParallaxController>();
        }
        
        if (parallaxCamera != null)
        {
            parallaxCamera.RequestUpdate();
        }
    }
#endif

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        Initialize();
        if (parallaxCamera != null)
        {
            parallaxCamera.RefreshLayers();
        }
        scrollAccumulator = Vector2.zero;
    }
    
    private void Initialize()
    {
        if (propBlock == null)
        {
            propBlock = new MaterialPropertyBlock();
        }
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        if (parallaxCamera == null)
        {
            parallaxCamera = GetComponentInParent<ParallaxController>();
        }
        if (parallaxGroup == null)
        {
            parallaxGroup = GetComponentInParent<ParallaxGroup>();
        }
    }
    
    public void UpdateLayer(ParallaxState state, bool isPlaying)
    {
        if (meshRenderer == null || parallaxCamera == null)
        {
            Initialize();
        }
        
        float effectiveDepth = baseDepth / state.ZoomLevel;
        UpdateQuadTransform(state, effectiveDepth);
        UpdateTextureOffset(state, isPlaying, effectiveDepth);
        UpdateMaterialProperties(state, effectiveDepth);
    }

    private void UpdateQuadTransform(ParallaxState state, float effectiveDepth)
    {
        transform.rotation = state.CameraRotation;
        transform.position = state.CameraPosition + state.CameraRotation * Vector3.forward * effectiveDepth;
        
        float visibleHeight = 2 * effectiveDepth * Mathf.Tan(state.CameraFieldOfView * 0.5f * Mathf.Deg2Rad);
        float visibleWidth = visibleHeight * state.CameraAspectRatio;
        transform.localScale = new Vector3(visibleWidth, visibleHeight, 1);
    }

    private void UpdateTextureOffset(ParallaxState state, bool isPlaying, float effectiveDepth)
    {
        Vector3 relativeCameraDisplacement = state.CameraPosition - parallaxCamera.startingCameraPosition;
        Vector3 totalDisplacement = relativeCameraDisplacement + state.ParallaxPosition;

        if (lockYAxis)
        {
            totalDisplacement.y = 0;
        }

        Vector3 rightDirection = state.CameraRotation * Vector3.right;
        Vector3 upDirection = state.CameraRotation * Vector3.up;

        float textureAspectRatio = (layerTexture != null && layerTexture.height > 0) ? (float)layerTexture.width / layerTexture.height : 1f;
        float aspectCorrection = state.CameraAspectRatio / textureAspectRatio;
        
        Vector2 baseTiling = new Vector2(
            enableTiling ? aspectCorrection * tiling.x : 1f, 
            enableTiling ? tiling.y : 1f
        );

        Vector2 parallaxTerm = Vector2.zero;
        if (transform.localScale.x > 0.001f)
        {
            float parallaxStrength = 1f / baseDepth;
            parallaxTerm = new Vector2(
                (-Vector3.Dot(totalDisplacement, rightDirection) * parallaxStrength / transform.localScale.x) * baseTiling.x,
                (-Vector3.Dot(totalDisplacement, upDirection) * parallaxStrength / transform.localScale.y) * baseTiling.y
            );
        }

        if (isPlaying)
        {
            float heightAtZoom1 = 2f * baseDepth * Mathf.Tan(state.CameraFieldOfView * 0.5f * Mathf.Deg2Rad);
            float widthAtZoom1 = heightAtZoom1 * state.CameraAspectRatio;

            if (widthAtZoom1 > 0.001f)
            {
                Vector2 scrollIncrement = new Vector2(
                    (scrollSpeed.x * Time.deltaTime / widthAtZoom1) * baseTiling.x,
                    (scrollSpeed.y * Time.deltaTime / heightAtZoom1) * baseTiling.y
                );
                scrollAccumulator -= scrollIncrement;
            }
        }
        
        Vector2 finalTiling = baseTiling / state.ZoomLevel;
        Vector2 centeringTerm = (baseTiling - finalTiling) * 0.5f;

        textureOffset = parallaxTerm + centeringTerm + scrollAccumulator;
    }

    private void UpdateMaterialProperties(ParallaxState state, float effectiveDepth)
    {
        if (meshRenderer == null || propBlock == null || layerTexture == null)
        {
            return;
        }

        meshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetTexture("_MainTex", layerTexture);

        float groupAlpha = (parallaxGroup != null) ? parallaxGroup.alpha : 1f;
        float finalAlpha = alpha * groupAlpha;

        float textureAspectRatio = (layerTexture != null && layerTexture.height > 0) 
            ? (float)layerTexture.width / layerTexture.height : 1f;
        float aspectCorrection = state.CameraAspectRatio / textureAspectRatio;
        Vector2 baseTiling = new Vector2(
            enableTiling ? aspectCorrection * tiling.x : 1f, 
            enableTiling ? tiling.y : 1f
        );
        
        Vector4 tilingOffset = new Vector4(
            baseTiling.x / state.ZoomLevel,
            baseTiling.y / state.ZoomLevel,
            textureOffset.x,
            textureOffset.y
        );
        
        propBlock.SetVector("_Tiling", tilingOffset);
        propBlock.SetColor("_Color", new Color(tintColor.r, tintColor.g, tintColor.b, finalAlpha));
        propBlock.SetFloat("_UseFade", enableFade ? 1.0f : 0.0f);
        meshRenderer.SetPropertyBlock(propBlock);
    }
}