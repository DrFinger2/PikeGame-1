using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class ParallaxGroup : MonoBehaviour
{
    [Tooltip("The alpha (opacity) of all child ParallaxBackgroundLayer and ParallaxSpriteLayer components.")]
    [Range(0f, 1f)]
    public float alpha = 1f;

#if UNITY_EDITOR
    private float lastAlpha = -1f;
    private ParallaxController parentCamera;

    private void OnEnable()
    {
        parentCamera = GetComponentInParent<ParallaxController>();
    }

    private void Update()
    {
        if (!Application.isPlaying && parentCamera != null && lastAlpha != alpha)
        {
            lastAlpha = alpha;
            parentCamera.RequestUpdate();
        }
    }

    public void RecordUndoForChildren()
    {
        Undo.RegisterCompleteObjectUndo(this, "Change Parallax Group Alpha");

        foreach (var layer in GetComponentsInChildren<ParallaxBackgroundLayer>(true))
        {
            var meshRenderer = layer.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Undo.RecordObject(meshRenderer, "Change Parallax Group Alpha");
            }
        }

        foreach (var layer in GetComponentsInChildren<ParallaxSpriteLayer>(true))
        {
            foreach (var spriteRenderer in layer.targetRenderers)
            {
                if (spriteRenderer != null)
                {
                    Undo.RecordObject(spriteRenderer, "Change Parallax Group Alpha");
                }
            }
        }
    }
#endif
}