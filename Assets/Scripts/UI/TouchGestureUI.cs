using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TouchGestureUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform ring;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private float maxScale = 2.5f;

    private void OnEnable()
    {
        StartRipple();
    }

    private void OnDisable()
    {
        DOTween.Kill(ring);
    }

    private void StartRipple()
    {
        if (ring == null) return;
        
        Image img = ring.GetComponent<Image>();
        if (img == null) return;

        ring.localScale = Vector3.zero;
        Color c = img.color;
        c.a = 0f;
        img.color = c;

        Sequence rippleSequence = DOTween.Sequence();
        rippleSequence.SetTarget(ring);

        rippleSequence.AppendCallback(() => {
            ring.localScale = Vector3.zero;
            Color resetColor = img.color;
            resetColor.a = 1f;
            img.color = resetColor;
        });

        rippleSequence.Append(ring.DOScale(maxScale, animationDuration).SetEase(Ease.Linear));
        rippleSequence.Join(img.DOFade(0f, animationDuration).SetEase(Ease.Linear));
        rippleSequence.SetLoops(-1, LoopType.Restart);
    }
}