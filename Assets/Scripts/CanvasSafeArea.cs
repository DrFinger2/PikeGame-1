using UnityEngine;
using UnityEngine.EventSystems; // Required for UIBehaviour

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class EventDrivenSafeArea : UIBehaviour // Inherit from UIBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    protected override void Awake()
    {
        base.Awake();
        panel = GetComponent<RectTransform>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ApplySafeArea();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        if (panel == null) return;

        Rect currentSafeArea = Screen.safeArea;

        // Exit early if the safe area hasn't actually changed
        if (currentSafeArea == lastSafeArea) return;
        lastSafeArea = currentSafeArea;

        // Prevent division by zero during Editor initialization
        if (Screen.width == 0 || Screen.height == 0) return;

        Vector2 anchorMin = currentSafeArea.position;
        Vector2 anchorMax = currentSafeArea.position + currentSafeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
    }
}

