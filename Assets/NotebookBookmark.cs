using UnityEngine;
using DG.Tweening;

public class NotebookBookmark : MonoBehaviour
{
    public int targetPageIndex;

    [Header("Target Settings (Pop-out state)")]
    public float targetX = 0.5f;
    public Vector3 targetScale = new Vector3(1.1f, 1.1f, 1.1f);
    public Vector3 targetRotation = new Vector3(0, 0, -5f);

    [Header("Animation Settings")]
    public float duration = 0.25f;
    public Ease easeType = Ease.OutQuad;

    private float baselineX;
    private Vector3 baselineScale;
    private Vector3 baselineRotation;
    
    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        baselineX = rect.pivot.x;
        baselineScale = rect.localScale;
        baselineRotation = rect.localEulerAngles;
    }

    [ContextMenu("Capture Current as Target")]
    public void SetTargetFromCurrent()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        
        targetX = rect.pivot.x;
        targetScale = rect.localScale;
        targetRotation = rect.localEulerAngles;
        Debug.Log($"<color=cyan>Target captured for {gameObject.name}!</color>");
    }

    public void Open()
    {
        rect.DOKill();
        rect.DOPivotX(targetX, duration).SetEase(easeType);
        rect.DOScale(targetScale, duration).SetEase(easeType);
        rect.DORotate(targetRotation, duration).SetEase(easeType);
    }

    public void Close()
    {
        rect.DOKill();
        rect.DOPivotX(baselineX, duration).SetEase(easeType);
        rect.DOScale(baselineScale, duration).SetEase(easeType);
        rect.DORotate(baselineRotation, duration).SetEase(easeType);
    }
}