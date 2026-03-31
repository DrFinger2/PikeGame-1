using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class SafeArea : MonoBehaviour
{
    public static UnityEvent OnResize = new UnityEvent();

    private RectTransform panel;
    private Rect lastSafeArea;

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
    }

    private void Update()
    {
        Rect currentSafeArea = Screen.safeArea;

        if (currentSafeArea != lastSafeArea && Screen.width > 0 && Screen.height > 0)
        {
            lastSafeArea = currentSafeArea;

            if (Application.isPlaying)
            {
                StopAllCoroutines();
                StartCoroutine(ApplySafeArea());
            }
            else
            {
                panel.anchorMin = new Vector2(currentSafeArea.xMin / Screen.width, currentSafeArea.yMin / Screen.height);
                panel.anchorMax = new Vector2(currentSafeArea.xMax / Screen.width, currentSafeArea.yMax / Screen.height);
            }
        }
    }

    private IEnumerator ApplySafeArea()
    {
        yield return new WaitForEndOfFrame();

        panel.anchorMin = new Vector2(lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height);
        panel.anchorMax = new Vector2(lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height);

        Canvas.ForceUpdateCanvases();
        OnResize.Invoke();
    }
}
