using System.Collections.Generic;
using UnityEngine;

public class UIPageManager : MonoBehaviour
{
    public List<CanvasGroup> pages;
    public Camera snapshotCamera;

    private int currentPageIndex = 0;

    private void Start()
    {
        ShowPage(0);
    }

    public void ShowPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= pages.Count)
        {
            Debug.LogWarning("Invalid page index requested.");
            return;
        }

        for (int i = 0; i < pages.Count; i++)
        {
            if (i == pageIndex)
            {
                pages[i].alpha = 1f;
                pages[i].interactable = true;
                pages[i].blocksRaycasts = true;
            }
            else
            {
                pages[i].alpha = 0f;
                pages[i].interactable = false;
                pages[i].blocksRaycasts = false;
            }
        }

        currentPageIndex = pageIndex;
        UpdateRenderTexture();
    }


    public void UpdateRenderTexture()
    {
        if (snapshotCamera != null)
        {
            snapshotCamera.Render();
        }
        else
        {
            Debug.LogWarning("Snapshot Camera is not assigned!");
        }
    }
}