using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteInEditMode]
public class ParallaxController : MonoBehaviour
{
    public Camera trackedCamera;
    public Vector3 ParallaxPosition = Vector3.zero;

    [Tooltip("Adjust the zoom level of the parallax camera. >1 zooms in, <1 zooms out.")]
    [SerializeField] private float zoom = 1f;
    public float Zoom 
    { 
        get => zoom; 
        set => zoom = Mathf.Max(1f, value); 
    }
    
    [HideInInspector] public Vector3 startingCameraPosition;
    public bool visibleInEditMode = true;
    [SerializeField, HideInInspector] private ParallaxState state = new();

    private List<ParallaxBackgroundLayer> parallaxLayers = new();
    private List<ParallaxSpriteLayer> parallaxSpriteLayers = new();
    
    private void OnValidate()
    {
        Zoom = zoom;
    }
   
    void Start()
    {
        RefreshAndInitialize();
    }

    private void RefreshAndInitialize()
    {
        if (trackedCamera == null)
        {
            trackedCamera = Camera.main;
        }

        state.Clear();
        RefreshLayers();
        UpdateLayerVisibility();

        if (trackedCamera != null)
        {
            startingCameraPosition = trackedCamera.transform.position;
            state.Update(trackedCamera, ParallaxPosition, zoom, visibleInEditMode);
        }

        RequestUpdate();
    }

#if UNITY_EDITOR
    void Reset()
    {
        gameObject.name = "ParallaxController";
        if (trackedCamera == null)
        {
            trackedCamera = Camera.main;
        }
    }

    void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
        Undo.undoRedoPerformed += OnUndoRedoPerformed;
        RefreshAndInitialize();
    }

    void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
    }

    private void OnUndoRedoPerformed()
    {
        if (trackedCamera != null)
        {
            state.Update(trackedCamera, ParallaxPosition, zoom, visibleInEditMode);
        }
        RequestUpdate();
    }

    void EditorUpdate()
    {
        if (UnityEditor.AnimationMode.InAnimationMode() || Application.isPlaying)
        {
            if (UnityEditor.AnimationMode.InAnimationMode())
            {
                UpdateLayers(false);
            }
            return;
        }
       
        if (trackedCamera == null)
        {
            trackedCamera = Camera.main;
        }

        if (trackedCamera == null)
        {
            return;
        }

        if (state.VisibleInEditMode != visibleInEditMode)
        {
            UpdateLayerVisibility();
        }
        
        if (!visibleInEditMode)
        {
            return;
        }

        if (state.HasChanged(trackedCamera, ParallaxPosition, zoom, visibleInEditMode))
        {
            RefreshLayers();
            UpdateLayers(false);
            state.Update(trackedCamera, ParallaxPosition, zoom, visibleInEditMode);
        }
    }

    public void RegisterUndo()
    {
        if (Application.isPlaying)
        {
            return;
        }

        Undo.RegisterCompleteObjectUndo(this, "Parallax Camera Change");
        
        foreach (var layer in parallaxLayers)
        {
            if (layer != null)
            {
                Undo.RecordObject(layer.transform, "Parallax Layer Change");
                Undo.RecordObject(layer, "Parallax Layer Change");
            }
        }
        
        foreach (var spriteLayer in parallaxSpriteLayers)
        {
            if (spriteLayer != null)
            {
                Undo.RecordObject(spriteLayer.transform, "Parallax Sprite Layer Change");
                Undo.RecordObject(spriteLayer, "Parallax Sprite Layer Change");
            }
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
#endif

    public void SetActive(bool isActive)
    {
        if (isActive)
        {
            gameObject.SetActive(true);
            trackedCamera.gameObject.SetActive(true);
            RefreshAndInitialize();
        }
        else
        {
            trackedCamera.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
    
    void LateUpdate()
    {
        if (!Application.isPlaying || trackedCamera == null)
        {
            return;
        }
        
        UpdateLayerVisibility(true);
        UpdateLayers(true);
    }

    private void UpdateLayers(bool isPlaying)
    {
        state.Clear();

        if (trackedCamera != null)
        {
            state.Update(trackedCamera, ParallaxPosition, zoom, visibleInEditMode);
        }
        
        foreach (var layer in parallaxLayers)
        {
            if (layer != null && layer.gameObject.activeInHierarchy)
            {
                layer.UpdateLayer(state, isPlaying);
            } 
        }
        
        foreach (var layer in parallaxSpriteLayers)
        {
            if (layer != null && layer.gameObject.activeInHierarchy)
            {
                layer.cameraOffset = new Vector2(ParallaxPosition.x, ParallaxPosition.y);
                layer.UpdateWithCamera(trackedCamera, zoom);
            }
        }
    }

    public void RefreshLayers()
    {
        parallaxLayers.Clear();
        parallaxSpriteLayers.Clear();
        GetComponentsInChildren(true, parallaxLayers);
        GetComponentsInChildren(true, parallaxSpriteLayers);
    }

    private void UpdateLayerVisibility(bool forceVisible = false)
    {
        bool shouldBeVisible = forceVisible || visibleInEditMode;
        RefreshLayers();
        
        foreach (var layer in parallaxLayers)
        {
            if (layer != null)
            {
                var renderer = layer.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.enabled != shouldBeVisible)
                {
                    renderer.enabled = shouldBeVisible;
                }
            }
        }
        
        foreach (var spriteLayer in parallaxSpriteLayers)
        {
            if (spriteLayer != null)
            {
                var spriteRenderer = spriteLayer.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = shouldBeVisible;
                }
            }
        }
    }

    public void RequestUpdate()
    {
        if (!Application.isPlaying && visibleInEditMode)
        {
            state.Clear();
        }
    }
}