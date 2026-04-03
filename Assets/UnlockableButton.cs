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

    [Header("Unlock Animation")]
    public float pulseDuration = 1f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.8f;
    public float fadeOutDuration = 0.4f;

    [Header("First Click Animation")]
    public float punchAmount = 0.1f;
    public float punchDuration = 0.5f;

    public Button Button { get; private set; }
    private Image glowImage;
    private Tween pulseTween;
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

        // Find all Selectables, but ONLY keep them if THIS is their closest UnlockableButton parent
        childSelectables = GetComponentsInChildren<Selectable>(true)
            .Where(s => s != Button && s.GetComponentInParent<UnlockableButton>(true) == this)
            .ToArray();
    }


    public void ReHighlight()
    {
        isAcknowledged = false;

        if (Button != null && Button.interactable && glowBackground != null)
        {
            glowBackground.SetActive(true);
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

        // Sync children on enable
        if (childSelectables != null)
        {
            foreach (var selectable in childSelectables)
            {
                if (selectable != null)
                {
                    selectable.interactable = wasInteractable;
                }
            }
        }
    }

    private void Update()
    {
        if (Button.interactable != wasInteractable)
        {
            wasInteractable = Button.interactable;

            // Sync children when the state changes
            if (childSelectables != null)
            {
                foreach (var selectable in childSelectables)
                {
                    if (selectable != null)
                    {
                        selectable.interactable = wasInteractable;
                    }
                }
            }

            if (wasInteractable && unlockMode == UnlockMode.UnlockOnInteractable && !isAcknowledged)
            {
                Unlock();
            }
            else if (!isAcknowledged && glowImage != null)
            {
                ToggleGlowState(wasInteractable);
            }
        }
    }

    private void ToggleGlowState(bool shouldBeVisible)
    {
        pulseTween?.Kill();

        if (shouldBeVisible)
        {
            glowBackground.SetActive(true);
            glowImage.DOFade(maxAlpha, fadeOutDuration).OnComplete(() => StartPulse());
        }
        else
        {
            glowImage.DOFade(0f, fadeOutDuration).OnComplete(() => glowBackground.SetActive(false));
        }
    }

    public void Unlock()
    {
        if (isAcknowledged) return;

        Events.OnUnlock?.Invoke();
        transform.DOKill();

        // If TurnManager is still doing its delayed setup, this is a startup unlock.
        bool isStartup = TurnManager.Instance == null || TurnManager.Instance.IsInitializing;

        if (!isStartup)
        {
            Vector3 scale = transform.localScale;
            Vector3 rotation = transform.localRotation.eulerAngles;

            transform.localScale = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0, 0, -15f);

            transform.DOScale(scale, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
            transform.DORotate(rotation, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        if (glowBackground)
        {
            glowBackground.SetActive(true);
            StartPulse();
        }
    }

    public void SetUnlocked()
    {
        isAcknowledged = true;
        if (glowBackground) glowBackground.SetActive(false);
        pulseTween?.Kill();
    }

    private void StartPulse()
    {
        pulseTween?.Kill();

        if (glowImage)
        {
            Color c = glowImage.color;
            c.a = maxAlpha;
            glowImage.color = c;
            pulseTween = glowImage.DOFade(minAlpha, pulseDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
        }
    }

    public void OnClickAction()
    {
        if (isAcknowledged || !Button.interactable) return;

        isAcknowledged = true;
        Events.OnFirstClick?.Invoke();

        pulseTween?.Kill(); 

        if (glowImage && glowBackground)
        {
            glowImage.DOFade(0f, fadeOutDuration).SetUpdate(true).OnComplete(() => glowBackground.SetActive(false));
        }
    }

    private void OnDestroy()
    {
        Button.onClick.RemoveListener(OnClickAction);
        pulseTween?.Kill();

        if (glowImage) glowImage.DOKill();
    }
}
