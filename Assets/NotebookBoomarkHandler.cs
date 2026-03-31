using System.Collections.Generic;
using UnityEngine;

public class NotebookBookmarkHandler : MonoBehaviour
{
    [Header("References")]
    public NotebookPageHandler pageHandler; 
    public List<NotebookBookmark> allBookmarks = new List<NotebookBookmark>();

    private NotebookBookmark currentActiveBookmark;

    private void OnEnable()
    {
        if (pageHandler != null)
        {
            // Subscribing to the UnityEvents in code
            pageHandler.OnPageChanged.AddListener(OnPageUpdate);
            pageHandler.OnBookClosed.AddListener(OnBookClosed);
        }
    }

    private void OnDisable()
    {
        if (pageHandler != null)
        {
            // Unsubscribing to prevent memory leaks or errors when disabled
            pageHandler.OnPageChanged.RemoveListener(OnPageUpdate);
            pageHandler.OnBookClosed.RemoveListener(OnBookClosed);
        }
    }

    public void OnPageUpdate(int pageIndex)
    {
        NotebookBookmark foundBookmark = allBookmarks.Find(
            bookmark => bookmark.targetPageIndex == pageIndex
        );

        if (foundBookmark == currentActiveBookmark) 
            return;

        if (currentActiveBookmark != null)
        {
            currentActiveBookmark.Close();
        }

        // Open the new bookmark and update the reference
        if (foundBookmark != null)
        {
            foundBookmark.Open();
            currentActiveBookmark = foundBookmark;
        }
        else
        {
            currentActiveBookmark = null;
        }
    }

    public void OnBookClosed()
    {
        if (currentActiveBookmark != null)
        {
            currentActiveBookmark.Close();
            currentActiveBookmark = null;
        }
    }
}