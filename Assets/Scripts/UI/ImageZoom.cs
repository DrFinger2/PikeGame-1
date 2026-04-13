using UnityEngine;
using UnityEngine.UI;

public class ImageZoom : MonoBehaviour
{
    public RectTransform imageRect;
    public GameObject overlay;

    private Vector2 originalPosition;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalPivot;
    private Vector3 originalScale;

    private bool zoomed = false;

    void Start()
    {
        // Save original layout
        originalPosition = imageRect.anchoredPosition;
        originalAnchorMin = imageRect.anchorMin;
        originalAnchorMax = imageRect.anchorMax;
        originalPivot = imageRect.pivot;
        originalScale = imageRect.localScale;
    }

    public void ToggleZoom()
    {
        zoomed = !zoomed;

        if (zoomed)
        {
            // Move to center
            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = Vector2.zero;

            imageRect.localScale = originalScale * 3f;
            overlay.SetActive(true);
        }
        else
        {
            // Restore everything
            imageRect.anchorMin = originalAnchorMin;
            imageRect.anchorMax = originalAnchorMax;
            imageRect.pivot = originalPivot;
            imageRect.anchoredPosition = originalPosition;

            imageRect.localScale = originalScale;
            overlay.SetActive(false);
        }
    }
}