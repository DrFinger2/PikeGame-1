using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;
using System.Linq;


[RequireComponent(typeof(Button))]
public class UnlockableButton : MonoBehaviour
{
    public enum UnlockMode { Manual, UnlockOnEnable, UnlockOnInteractable, AlreadyUnlocked }

    [System.Serializable]
    public class UnlockEvents { public UnityEvent OnUnlock = new(); public UnityEvent OnFirstClick = new(); }

    [Header("Behavior")]
    public UnlockMode unlockMode = UnlockMode.Manual;
    public UnlockEvents Events = new UnlockEvents();

    [Header("Visual Elements")]
    public GameObject glowBackground;
    public GameObject lightRaysBackground;

    [Header("Unlock Animation")]
    public bool playPunchOnUnlock = true;
    public bool playPulseOnUnlock = true;
    public bool playLightRayAnimationOnUnlock = false;

    [Header("Glow Settings")]
    public float pulseDuration = 1f;
    public float minAlpha = 0.1f;
    public float maxAlpha = 0.635f;
    public float fadeOutDuration = 0.4f;


    [Header("Light Rays Settings")]
    public float lightRayMinAlpha = 0.1f;
    public float lightRayMaxAlpha = 0.15f;
    public float lightRayRotationDuration = 14f;

    public Button Button { get; private set; }
    private Image glowImage;
    private Image lightRaysImage;

    private Tween pulseTween;
    private Tween lightRaysPulseTween; // Tween to pulse the light rays
    private Tween rotationTween;
    private Tween lightRaysFadeTween;

    private Selectable[] childSelectables;

    private bool isAcknowledged;
    private bool wasInteractable;

    private void Awake()
    {
        Button = GetComponent<Button>();
        Button.transition = Selectable.Transition.None;
        Button.onClick.AddListener(OnClickAction);

        if (glowBackground)
        {
            glowImage = glowBackground.GetComponent<Image>();
            glowBackground.SetActive(false);
        }

        if (lightRaysBackground)
        {
            lightRaysImage = lightRaysBackground.GetComponent<Image>();
            lightRaysBackground.SetActive(false);
        }

        childSelectables = GetComponentsInChildren<Selectable>(true)
            .Where(s => s != Button && s.GetComponentInParent<UnlockableButton>(true) == this)
            .ToArray();
    }

    public void ReHighlight()
    {
        isAcknowledged = false;

        if (Button != null && Button.interactable)
        {
            if (glowBackground != null)
            {
                glowBackground.SetActive(true);
            }

            if (lightRaysBackground != null && playLightRayAnimationOnUnlock)
            {
                lightRaysBackground.SetActive(true);
                StartLightRaysRotation();
            }

            // Syncs and starts pulsing for both the glow and the light rays
            StartPulse();
        }
    }

    private void OnEnable()
    {
        if (unlockMode == UnlockMode.UnlockOnEnable)
        {
            Unlock();
        }
        else if (unlockMode == UnlockMode.AlreadyUnlocked)
        {
            SetUnlocked();
        }
        else if (unlockMode == UnlockMode.UnlockOnInteractable && Button.interactable)
        {
            Unlock();
        }

        wasInteractable = Button.interactable;

        if (childSelectables != null)
        {
            foreach (var selectable in childSelectables)
            {
                if (selectable != null) selectable.interactable = wasInteractable;
            }
        }
    }

    private void Update()
    {
        if (Button.interactable != wasInteractable)
        {
            wasInteractable = Button.interactable;

            if (childSelectables != null)
            {
                foreach (var selectable in childSelectables)
                {
                    if (selectable != null) selectable.interactable = wasInteractable;
                }
            }

            if (wasInteractable && unlockMode == UnlockMode.UnlockOnInteractable && !isAcknowledged)
            {
                Unlock();
            }
            else if (!isAcknowledged && (glowImage != null || lightRaysImage != null))
            {
                ToggleGlowState(wasInteractable);
            }
        }
    }

    private void ToggleGlowState(bool shouldBeVisible)
    {
        pulseTween?.Kill();
        lightRaysPulseTween?.Kill();
        rotationTween?.Kill();
        lightRaysFadeTween?.Kill();

        if (shouldBeVisible)
        {
            bool glowExists = glowBackground != null && glowImage != null;
            bool raysExist = lightRaysBackground != null && lightRaysImage != null && playLightRayAnimationOnUnlock;

            if (glowExists)
            {
                glowBackground.SetActive(true);
                glowImage.DOFade(maxAlpha, fadeOutDuration).OnComplete(() => StartPulse());
            }

            if (raysExist)
            {
                lightRaysBackground.SetActive(true);
                Tween rayTween = lightRaysImage.DOFade(lightRayMaxAlpha, fadeOutDuration).SetUpdate(true);
                
                // If there is no glow background to trigger the pulse, the light rays trigger it when they finish fading
                if (!glowExists) 
                {
                    rayTween.OnComplete(() => StartPulse());
                }
                
                StartLightRaysRotation();
            }
        }
        else
        {
            if (glowBackground)
            {
                if (glowImage != null) glowImage.DOFade(0f, fadeOutDuration).OnComplete(() => glowBackground.SetActive(false));
                else glowBackground.SetActive(false);
            }

            if (lightRaysBackground)
            {
                if (lightRaysImage != null) lightRaysFadeTween = lightRaysImage.DOFade(0f, fadeOutDuration).OnComplete(() => lightRaysBackground.SetActive(false));
                else lightRaysBackground.SetActive(false);
            }
        }
    }

    public void Unlock()
    {
        if (isAcknowledged) return;

        Events.OnUnlock?.Invoke();
        transform.DOKill();

        bool isStartup = TurnManager.Instance == null || TurnManager.Instance.IsInitializing;

        if (!isStartup && playPunchOnUnlock)
        {
            Vector3 scale = transform.localScale;
            Vector3 rotation = transform.localRotation.eulerAngles;

            transform.localScale = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0, 0, -15f);

            transform.DOScale(scale, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
            transform.DORotate(rotation, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        if (glowBackground && playPulseOnUnlock)
        {
            glowBackground.SetActive(true);
        }

        if (lightRaysBackground && playLightRayAnimationOnUnlock)
        {
            lightRaysBackground.SetActive(true);
            StartLightRaysRotation();

            // If we are NOT pulsing, we still need to fade the rays in manually
            if (lightRaysImage != null && !playPulseOnUnlock)
            {
                Color c = lightRaysImage.color;
                c.a = 0f;
                lightRaysImage.color = c;

                lightRaysFadeTween?.Kill();
                lightRaysFadeTween = lightRaysImage.DOFade(lightRayMaxAlpha, fadeOutDuration).SetUpdate(true);
            }
        }

        // Handles snapping to max alpha and starting the simultaneous pulse for BOTH elements
        if (playPulseOnUnlock)
        {
            StartPulse();
        }
    }

    public void SetUnlocked()
    {
        isAcknowledged = true;
        if (glowBackground) glowBackground.SetActive(false);
        if (lightRaysBackground) lightRaysBackground.SetActive(false);

        pulseTween?.Kill();
        lightRaysPulseTween?.Kill();
        rotationTween?.Kill();
        lightRaysFadeTween?.Kill();
    }

    private void StartPulse()
    {
        pulseTween?.Kill();
        lightRaysPulseTween?.Kill();

        if (glowImage && glowBackground != null && glowBackground.activeSelf)
        {
            Color c = glowImage.color;
            c.a = maxAlpha;
            glowImage.color = c;
            pulseTween = glowImage.DOFade(minAlpha, pulseDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
        }

        if (lightRaysImage && lightRaysBackground != null && lightRaysBackground.activeSelf && playLightRayAnimationOnUnlock)
        {
            Color c = lightRaysImage.color;
            c.a = lightRayMaxAlpha;
            lightRaysImage.color = c;
            lightRaysPulseTween = lightRaysImage.DOFade(lightRayMinAlpha, pulseDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
        }
    }

    private void StartLightRaysRotation()
    {
        rotationTween?.Kill();

        if (lightRaysBackground && lightRaysBackground.activeSelf)
        {
            rotationTween = lightRaysBackground.transform
                .DORotate(new Vector3(0, 0, -360), lightRayRotationDuration, RotateMode.FastBeyond360)
                .SetRelative()
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(true);
        }
    }

    public void OnClickAction()
    {
        if (isAcknowledged || !Button.interactable) return;

        isAcknowledged = true;
        Events.OnFirstClick?.Invoke();

        pulseTween?.Kill();
        lightRaysPulseTween?.Kill();
        rotationTween?.Kill();
        lightRaysFadeTween?.Kill();

        if (glowBackground)
        {
            if (glowImage != null) glowImage.DOFade(0f, fadeOutDuration).SetUpdate(true).OnComplete(() => glowBackground.SetActive(false));
            else glowBackground.SetActive(false);
        }

        if (lightRaysBackground)
        {
            if (lightRaysImage != null) lightRaysImage.DOFade(0f, fadeOutDuration).SetUpdate(true).OnComplete(() => lightRaysBackground.SetActive(false));
            else lightRaysBackground.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        Button.onClick.RemoveListener(OnClickAction);

        pulseTween?.Kill();
        lightRaysPulseTween?.Kill();
        rotationTween?.Kill();
        lightRaysFadeTween?.Kill();

        if (glowImage) glowImage.DOKill();
        if (lightRaysImage) lightRaysImage.DOKill();
        if (lightRaysBackground) lightRaysBackground.transform.DOKill();
    }
}