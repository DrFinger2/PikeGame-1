using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class NotebookPageHandler : MonoBehaviour
{

    public GameObject[] bookPages;
    [SerializeField] private List<GameObject> newPages = new List<GameObject>();
    [SerializeField] private int currentPage;
    public UnityEvent<int> OnPageChanged = new();
    public UnityEvent OnBookClosed = new();
    private AudioSource audio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPage = 0;
        audio = GetComponent<AudioSource>();
        GeneratePages();
        JumpToPage(currentPage);
    }


    private void GeneratePages()
    {
        foreach (GameObject page in bookPages)
        {
            var newPage = Instantiate(page, this.transform.position, Quaternion.identity);
            newPage.transform.SetParent(this.transform);
            newPage.transform.localScale = new Vector3(1, 1, 1);
            newPage.gameObject.SetActive(false);
            newPages.Add(newPage);
        }
    }

    public void NextPage()
    {
        
        int page = currentPage;

        if (currentPage < bookPages.Length - 1)
        {
            page++;
        }
        else if (currentPage == bookPages.Length - 1)
        {
            page = 0;
        }

        JumpToPage(page);
        
    }

    public void PreviousPage()
    {
        
        int page = currentPage;

        if (currentPage > 0)
        {
            page--;
        }
        else if (currentPage == 0)
        {
            page = bookPages.Length - 1;
        }
        
        JumpToPage(page);
    }

    public void JumpToPage(int page)
    {

        bool isNextPage = (page > currentPage);
        bool isPreviousPage = (page < currentPage);

         for (int i = 0; i < newPages.Count; i++)
        {
            if (i == page)
            {
                newPages[i].SetActive(true);
                RectTransform pageRect = newPages[i].GetComponent<RectTransform>();
                pageRect.DOKill();

                float timeVariation = Random.Range(-0.05f, 0.05f);
                float scaleVariation = Random.Range(-0.01f, 0.01f);
                float offsetVariation = Random.Range(-5f, 5f);
                int sign = (isNextPage ? 1 : -1);

                float startOffset = (isNextPage || isPreviousPage) ? (20 * sign) + offsetVariation : 0f;
                float startScale = (isNextPage || isPreviousPage) ? 0.98f + scaleVariation : 1f; 
                
                pageRect.anchoredPosition = new Vector2(startOffset, 0);
                pageRect.localScale = Vector3.one * startScale; 

                pageRect.DOAnchorPos(Vector2.zero, 0.2f + timeVariation).SetEase(Ease.OutQuad, 1.2f);
                pageRect.DOScale(Vector3.one, 0.2f + timeVariation).SetEase(Ease.OutQuad);
            }
            else
            {
                newPages[i].SetActive(false);
            }
        }
    

        if (true)
        {
            float pitch = isNextPage ? Random.Range(1.01f, 1.04f)
                        : isPreviousPage ? Random.Range(0.96f, 0.99f)
                        : Random.Range(0.98f, 1.02f); 
            
            audio.pitch = pitch;
            audio.Play();
        }

        currentPage = page;
        OnPageChanged.Invoke(currentPage);
    }

    public void CloseNotebook()
    {
        // Invoke the event FIRST so the PopupHandler can restore buttons
        OnBookClosed.Invoke();
        this.transform.parent.gameObject.SetActive(false);
    }

}
