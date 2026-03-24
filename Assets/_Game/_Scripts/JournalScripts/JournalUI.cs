
using UnityEngine;
using UnityEngine.UI; // Required for the Button component

public class JournalUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Journal journal;

    [Header("UI Controls")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button closeButton;
    

    private void Awake()
    {
        if (journal == null)
            journal = Journal.Instance;
        if (nextButton != null)
            nextButton.onClick.AddListener(OpenNextPage);
        if (previousButton != null)
            previousButton.onClick.AddListener(OpenPreviousPage);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseJournal);
    }

    private void OnDestroy()
    {
        if (nextButton != null) 
            nextButton.onClick.RemoveListener(OpenNextPage);
        if (previousButton != null) 
            previousButton.onClick.RemoveListener(OpenPreviousPage);
        if (closeButton != null) 
            closeButton.onClick.RemoveListener(CloseJournal);
    }

    private void CloseJournal()
    {
        if(journal != null)
        {
            journal.CloseJournal();
            Debug.Log($"Close journal: {journal.CurrentPageNumber}");
        }
    }

    private void OpenNextPage()
    {
        if (journal != null)
        {
            journal.OpenNextPage();
            Debug.Log($"Moved to page: {journal.CurrentPageNumber}");
        }
    }

    private void OpenPreviousPage()
    {
        if (journal != null)
        {
            journal.CloseCurrentPage();
            Debug.Log($"Moved to page: {journal.CurrentPageNumber}");
        }
    }
}