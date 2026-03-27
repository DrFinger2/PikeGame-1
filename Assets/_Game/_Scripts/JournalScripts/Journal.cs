using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Utils;
using UnityEditor.TerrainTools;
using JournalSystem;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class Journal : SingletonInstance<Journal>
{
    [Header("Editor Tools")]
    [Tooltip("Check this box to force destroy and rebuild all pages from the current Prefab.")]
    [SerializeField] private bool forceRebuildPages = false;

    [Header("References")]
    [SerializeField] private GameObject pagePrefab;
    [SerializeField] private GameObject pageContainer;
    [SerializeField] private GameObject pagePivot;

    [Header("Settings")]
    [SerializeField] private Vector3 pagePositionOffset = new Vector3(0, 0, 0);
    [SerializeField] private Quaternion pageRotationOffset = Quaternion.identity;
    [SerializeField] private Texture2D emptyPage;

    [Header("Animation Settings")]
    [SerializeField] private float singlePageDuration = 1f;
    [SerializeField] private float multiPageDuration = 1f;
    [SerializeField] private float multiPageTurnDelay = 0.2f;
    [SerializeField] private float backgroundPageSmoothness = 10f;

    [Header("Materials")]
    [SerializeField] private MaterialFader fader;

    [Header("Content")]
    [SerializeField] private JournalPageData[] pageData;

    private List<JournalPage> journalPages = new();
    private Coroutine updatePagesCoroutine;
    private Dictionary<int, Coroutine> openingAnimations = new();
    private Dictionary<int, Coroutine> closingAnimations = new();
    private Coroutine openPageIndexCoroutine;

    private int currentPageNumber = 0;
    public int CurrentPageNumber => currentPageNumber;

#if UNITY_EDITOR
    [HideInInspector, SerializeField] private GameObject pageContainerSnapshot;
    [HideInInspector, SerializeField] private GameObject pagePrefabSnapshot; // Added to track prefab swaps
    [HideInInspector, SerializeField] private Texture2D emptyPageSnapshot;
    [HideInInspector, SerializeField] private JournalPageData[] pageDataSnapshot;

    private void OnValidate()
    {
        if (pageData != null)
        {
            for (int i = 0; i < pageData.Length; i++)
            {
                if (pageData[i] != null) pageData[i].PageNumber = i;
            }
        }

        if (gameObject.scene.name == null && pageContainer != null) return;
        if (Application.isPlaying) return;

        // --- NEW: Manual Refresh Logic ---
        if (forceRebuildPages)
        {
            forceRebuildPages = false; // Uncheck it immediately so it acts like a button
            ForceRebuildPages();
            return;
        }

        bool pagesChanged = pageDataSnapshot == null || HavePagesChangedEditor();
        bool emptyPageChanged = emptyPage != emptyPageSnapshot;
        bool prefabSwapped = pagePrefab != pagePrefabSnapshot; // Track if the prefab reference changed

        if (pagesChanged || emptyPageChanged || prefabSwapped)
        {
            bool shouldClearPreviousContainer = pageContainer != pageContainerSnapshot && pageContainerSnapshot != null;
            if (shouldClearPreviousContainer || prefabSwapped)
            {
                RemoveChildrenImmediate(pageContainer != null ? pageContainer.transform : pageContainerSnapshot.transform);
            }

            EditorApplication.delayCall += UpdatePages;
            pageContainerSnapshot = pageContainer;
            emptyPageSnapshot = emptyPage;
            pagePrefabSnapshot = pagePrefab;

            if (pageData != null)
            {
                pageDataSnapshot = new JournalPageData[pageData.Length];
                for (int i = 0; i < pageData.Length; i++)
                {
                    if (pageData[i] != null) pageDataSnapshot[i] = pageData[i].Clone();
                }
            }
            else
            {
                pageDataSnapshot = null;
            }
        }
        ApplyAllPageTransformsImmediate();
    }

    // --- NEW: Context menu option for quick access ---
    [ContextMenu("Force Rebuild Pages")]
    private void ForceRebuildPages()
    {
        EditorApplication.delayCall += () =>
        {
            if (pageContainer != null)
            {
                RemoveChildrenImmediate(pageContainer.transform);
            }
            UpdatePages();
            ApplyAllPageTransformsImmediate();
        };
    }

    private void RemoveChildrenImmediate(Transform container)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(container.GetChild(i).gameObject);
        }
    }

    private bool HavePagesChangedEditor()
    {
        if (pageDataSnapshot == null || pageData == null || pageData.Length != pageDataSnapshot.Length || pageContainerSnapshot != pageContainer)
        {
            return true;
        }

        for (int i = 0; i < pageData.Length; i++)
        {
            var oldPage = pageDataSnapshot[i];
            var newPage = pageData[i];

            if ((newPage == null && oldPage != null) || (newPage != null && oldPage == null))
                return true;

            bool pagesExist = (newPage != null && oldPage != null);
            if (pagesExist && (newPage.texture != oldPage.texture || !newPage.bookmark.Equals(oldPage.bookmark)))
                return true;
        }
        return false;
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorSceneManager.sceneOpened -= OnSceneOpened;
    }

    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        EditorApplication.delayCall += () =>
        {
            pageContainerSnapshot = null;
            pageDataSnapshot = null;
            emptyPageSnapshot = null;
            pagePrefabSnapshot = null;
            UpdatePages();
            ApplyAllPageTransformsImmediate();
        };
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (pageContainerSnapshot != pageContainer && pageContainerSnapshot != null)
                RemoveChildrenImmediate(pageContainerSnapshot.transform);
            UpdatePages();
            ApplyAllPageTransformsImmediate();
        }
    }
#endif

    void Start()
    {
        if (Application.isPlaying)
        {
            UpdatePages();
        }
    }

    public JournalPage GetCurrentPage()
    {
        if (currentPageNumber > 0)
        {
            return GetPageByIndex(currentPageNumber - 1);
        }
        return null;
    }

    public JournalPage GetPageByNumber(int pageNumber)
    {
        int pageIndex = pageNumber / 2;
        return GetPageByIndex(pageIndex);
    }

    public JournalPage GetPageByIndex(int index)
    {
        if (index >= 0 && index < journalPages.Count)
        {
            return journalPages[index];
        }
        return null;
    }

    public void CloseJournal(float fadeDuration = 1f)
    {
        fader.FadeOut(fadeDuration);
    }

    public void OpenJournal(float fadeDuration = 1f)
    {
        fader.FadeIn(fadeDuration);
    }

    public void OpenNextPage()
    {
        if (openPageIndexCoroutine != null) return;
        OpenPage(singlePageDuration);
    }

    public void CloseCurrentPage()
    {
        if (openPageIndexCoroutine != null) return;
        ClosePage(singlePageDuration);
    }

    public void OpenPageNumber(int pageNumber)
    {
        if (openPageIndexCoroutine != null)
        {
            StopCoroutine(openPageIndexCoroutine);
        }
        openPageIndexCoroutine = StartCoroutine(OpenPageCoroutine(pageNumber));
    }

    private IEnumerator OpenPageCoroutine(int targetSheetIndex)
    {
        targetSheetIndex = Mathf.Clamp((targetSheetIndex + 1) / 2, 0, journalPages.Count);

        while (currentPageNumber != targetSheetIndex)
        {
            if (currentPageNumber < targetSheetIndex)
            {
                OpenPage(multiPageDuration);
            }
            else if (currentPageNumber > targetSheetIndex)
            {
                ClosePage(multiPageDuration);
            }
            yield return new WaitForSeconds(multiPageTurnDelay);
        }
        openPageIndexCoroutine = null;
    }

    private void OpenPage(float pageTurnDuration)
    {
        if (currentPageNumber >= journalPages.Count) return;

        if (JournalCamera.Instance != null)
        {
            JournalCamera.Instance.AutoFocus(currentPageNumber + 1, journalPages.Count, 0.2f);
        }

        int pageIndexToOpen = currentPageNumber;

        if (closingAnimations.TryGetValue(pageIndexToOpen, out Coroutine closingAnimation))
        {
            if (closingAnimation != null)
            {
                StopCoroutine(closingAnimation);
            }
            closingAnimations.Remove(pageIndexToOpen);
        }

        if (openingAnimations.ContainsKey(pageIndexToOpen)) return;

        currentPageNumber++;
        Coroutine openingAnimation = StartCoroutine(TurnPageCoroutine(journalPages[pageIndexToOpen], pageIndexToOpen, pageTurnDuration, true));
        openingAnimations[pageIndexToOpen] = openingAnimation;

        StartBackgroundPageUpdate();
    }

    private void ClosePage(float pageTurnDuration)
    {
        if (currentPageNumber <= 0) return;

        if (JournalCamera.Instance != null)
        {
            // We are moving back to the previous sheet (currentPageNumber - 1)
            JournalCamera.Instance.AutoFocus(currentPageNumber - 1, journalPages.Count, 0.2f);
        }


        int pageIndexToClose = currentPageNumber - 1;

        if (openingAnimations.TryGetValue(pageIndexToClose, out Coroutine openingAnimation))
        {
            if (openingAnimation != null)
            {
                StopCoroutine(openingAnimation);
            }
            openingAnimations.Remove(pageIndexToClose);
        }

        if (closingAnimations.ContainsKey(pageIndexToClose)) return;

        currentPageNumber--;
        Coroutine closingAnimation = StartCoroutine(TurnPageCoroutine(journalPages[pageIndexToClose], pageIndexToClose, pageTurnDuration, false));
        closingAnimations[pageIndexToClose] = closingAnimation;

        StartBackgroundPageUpdate();
    }

    private IEnumerator TurnPageCoroutine(JournalPage page, int pageNumber, float pageTurnDuration, bool isOpen)
    {
        float startValue = page.time;
        float endValue = isOpen ? 1f : 0f;
        float timeElapsed = 0f;
        float duration = pageTurnDuration * Mathf.Abs(endValue - startValue);

        if (duration > 0)
        {
            while (timeElapsed < duration)
            {
                page.time = Mathf.Lerp(startValue, endValue, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }
        page.time = endValue;

        if (isOpen) openingAnimations.Remove(pageNumber);
        else closingAnimations.Remove(pageNumber);
    }

    private void StartBackgroundPageUpdate()
    {
        if (updatePagesCoroutine == null)
        {
            updatePagesCoroutine = StartCoroutine(UpdateBackgroundPagesCoroutine());
        }
    }

    private IEnumerator UpdateBackgroundPagesCoroutine()
    {
        while (openingAnimations.Count > 0 || closingAnimations.Count > 0)
        {
            for (int i = 0; i < journalPages.Count; i++)
            {
                bool pageIsNotTurning = !openingAnimations.ContainsKey(i) && !closingAnimations.ContainsKey(i);
                if (pageIsNotTurning)
                {
                    ApplyPageTransformations(journalPages[i].transform, i);
                }
            }
            yield return null;
        }
        ApplyAllPageTransformsImmediate();
        updatePagesCoroutine = null;
    }

    private void ApplyPageTransformations(Transform page, int index)
    {
        if (pagePivot == null)
        {
            page.localPosition = index * pagePositionOffset;
            page.localRotation = Quaternion.identity;
            return;
        }

        bool isOpen = index < currentPageNumber;
        int stackIndex = isOpen ? ((currentPageNumber - 1) - index) : (index - currentPageNumber);

        Quaternion targetCumulativeRotation = Quaternion.identity;
        if (stackIndex > 0)
        {
            pageRotationOffset.ToAngleAxis(out float angle, out Vector3 axis);
            targetCumulativeRotation = Quaternion.AngleAxis((isOpen ? -1 : 1) * angle * stackIndex, axis);
        }

        Vector3 baseLocalPosition = index * pagePositionOffset;
        Vector3 pivotLocalPosition = pageContainer.transform.InverseTransformPoint(pagePivot.transform.position);
        Vector3 vectorFromPivot = baseLocalPosition - pivotLocalPosition;
        Vector3 targetPosition = pivotLocalPosition + (targetCumulativeRotation * vectorFromPivot);

        float smoothTime = Time.deltaTime * backgroundPageSmoothness;
        page.localPosition = Vector3.Lerp(page.localPosition, targetPosition, smoothTime);
        page.localRotation = Quaternion.Slerp(page.localRotation, targetCumulativeRotation, smoothTime);
    }

    private void ApplyAllPageTransformsImmediate()
    {
        if (pageContainer == null || pagePivot == null) return;
        for (int i = 0; i < pageContainer.transform.childCount; i++)
        {
            bool isOpen = i < currentPageNumber;
            int stackIndex = isOpen ? ((currentPageNumber - 1) - i) : (i - currentPageNumber);
            Quaternion targetCumulativeRotation = Quaternion.identity;

            if (stackIndex > 0)
            {
                pageRotationOffset.ToAngleAxis(out float angle, out Vector3 axis);
                targetCumulativeRotation = Quaternion.AngleAxis((isOpen ? -1 : 1) * angle * stackIndex, axis);
            }

            Vector3 baseLocalPosition = i * pagePositionOffset;
            Vector3 pivotLocalPosition = pageContainer.transform.InverseTransformPoint(pagePivot.transform.position);
            Vector3 vectorFromPivot = baseLocalPosition - pivotLocalPosition;
            Vector3 targetPosition = pivotLocalPosition + (targetCumulativeRotation * vectorFromPivot);

            Transform child = pageContainer.transform.GetChild(i);
            child.localPosition = targetPosition;
            child.localRotation = targetCumulativeRotation;
        }
    }

    private void UpdatePages()
    {
#if UNITY_EDITOR
        EditorApplication.delayCall -= UpdatePages;
        if (this == null) return;
#endif

        if (pagePrefab == null || emptyPage == null || pageContainer == null || pageData == null)
            return;

        int requiredPageCount = Mathf.Max(0, Mathf.CeilToInt(pageData.Length / 2.0f));

        while (pageContainer.transform.childCount > requiredPageCount)
        {
            Transform child = pageContainer.transform.GetChild(pageContainer.transform.childCount - 1);
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }

        while (pageContainer.transform.childCount < requiredPageCount)
        {
            Instantiate(pagePrefab, pageContainer.transform);
        }

        journalPages.Clear();
        for (int i = 0; i < requiredPageCount; i++)
        {
            GameObject pageObject = pageContainer.transform.GetChild(i).gameObject;
            pageObject.name = $"Page_{i}";
            JournalPage journalPage = pageObject.GetComponent<JournalPage>();
            journalPage.pageNumber = i;

            if (journalPage != null)
            {
                Texture2D frontTexture = GetTextureAtIndex(i * 2);
                Texture2D backTexture = GetTextureAtIndex(i * 2 + 1);
                journalPage.SetupPage(frontTexture, backTexture);

                BookMarkData frontBookmark = GetBookmarkDataAtIndex(i * 2);
                BookMarkData backBookmark = GetBookmarkDataAtIndex(i * 2 + 1);
                journalPage.SetupBookmarks(frontBookmark, backBookmark);
            }
            journalPages.Add(journalPage);
        }
        ApplyAllPageTransformsImmediate();
    }

    private BookMarkData GetBookmarkDataAtIndex(int index)
    {
        if (index < pageData.Length && pageData[index] != null)
        {
            return pageData[index].bookmark;
        }
        return null;
    }

    private Texture2D GetTextureAtIndex(int index)
    {
        return (index < pageData.Length && pageData[index] != null && pageData[index].texture != null)
            ? pageData[index].texture
            : emptyPage;
    }
}
