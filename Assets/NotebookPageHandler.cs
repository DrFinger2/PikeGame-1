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

    // Update is called once per frame
    void Update()
    {
        
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
        audio.pitch = Random.Range(1.02f, 1.06f);

        if (currentPage < bookPages.Length - 1)
        {
            currentPage++;
        }
        else if (currentPage == bookPages.Length - 1)
        {
            currentPage = 0;
        }

        JumpToPage(currentPage);
        
    }

    public void PreviousPage()
    {
        audio.pitch = Random.Range(0.94f, 0.98f); // Subtly lower pitch

        if (currentPage > 0)
        {
            currentPage--;
        }
        else if (currentPage == 0)
        {
            currentPage = bookPages.Length - 1;
        }
        
        JumpToPage(currentPage);
    }

    public void JumpToPage(int page)
    {

        if (page != currentPage)
        {
            // If jumping directly via a bookmark, reset the pitch to normal
            audio.pitch = Random.Range(0.98f, 1.02f); // Baseline with tiny variance
        }
        
        foreach (GameObject pg in newPages)
        {
            pg.SetActive(false);
        }

        if (!audio.isPlaying)
        {
            audio.Play();
        }

        newPages[page].gameObject.SetActive(true);
        currentPage = page;
        OnPageChanged.Invoke(currentPage);
    }

    public void CloseNotebook()
    {
        this.transform.parent.gameObject.SetActive(false);
        OnBookClosed.Invoke();
    }
}
