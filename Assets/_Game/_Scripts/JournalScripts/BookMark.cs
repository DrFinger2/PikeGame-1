using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Deform;
using UnityEngine.UIElements;
using UnityEngine.EventSystems; // Required for the UI Input Module clicks

public enum BookMarkPlacement
{
    Front, Back
}

[ExecuteInEditMode]
public class BookMark : MonoBehaviour, IPointerClickHandler // Added the interface here
{
    [HideInInspector]
    public int PageNumber
    {
        get { return placementData.pageNumber; }
        set { placementData.pageNumber = value; }
    }

    [SerializeField] MeshRenderer pageMesh;
    [SerializeField] MeshRenderer bookMarkMesh;


    [SerializeField]
    BookMarkPlacement placementSide = BookMarkPlacement.Front;
    [SerializeField] BookMarkData placementData = new();

    void Awake()
    {
        FindMissingPageMesh();
    }

    void OnValidate()
    {
        FindMissingPageMesh();
        UpdateBookmark(placementData, placementSide);
    }

    void OnEnable()
    {
        UpdateBookmark(placementData, placementSide);
    }

    void Start()
    {
        UpdateBookmark(placementData, placementSide);
    }

    // --- NEW: Automatically find the page mesh on the parent if missing ---
    private void FindMissingPageMesh()
    {
        // Check if it's missing AND that we actually have a parent object
        if (pageMesh == null && transform.parent != null)
        {
            // Explicitly look at the parent to avoid accidentally grabbing the bookmark's own mesh
            pageMesh = transform.parent.GetComponent<MeshRenderer>();
        }
    }

    public void ApplyBookmarkData(BookMarkData data)
    {
        this.placementData.Apply(data);
        UpdateBookmark(placementData, placementSide);
    }

    public void UpdateBookmark(BookMarkData data, BookMarkPlacement side)
    {
        UpdateVisiblity(data);
        UpdatePosition(data, side);
    }

    void UpdateVisiblity(BookMarkData data)
    {
        bookMarkMesh.gameObject.SetActive(data.enabled);
    }

    void UpdatePosition(BookMarkData data, BookMarkPlacement placement)
    {
        if (pageMesh == null || bookMarkMesh == null)
            return;

        Vector3 pageSize = pageMesh.bounds.size;
        Vector3 markSize = bookMarkMesh.bounds.size;

        float y = CalculateYPosition(pageSize, placement);
        float x = CalculateXPosition(pageSize, markSize, data.widthPosOffset);
        float z = CalculateZPosition(pageSize, markSize, data.depthPosOffset);

        this.transform.localPosition = new Vector3(x, y, z);
    }

    private float CalculateXPosition(Vector3 pageSize, Vector3 markSize, float xPosOffset)
    {
        float baseXPosition = -pageSize.x + (markSize.x * .5f);
        float xOffset = (markSize.x) * xPosOffset;
        float xPosition = baseXPosition - xOffset;
        return xPosition;

    }
    private float CalculateYPosition(Vector3 pageSize, BookMarkPlacement side)
    {
        float pageOffset = 0.0005f;
        int pageSign = (side == BookMarkPlacement.Front ? 1 : -1);
        float yPosition = (pageOffset * pageSign);
        float yBasePosition = pageMesh.transform.localPosition.y;

        return yBasePosition + yPosition;
    }

    private float CalculateZPosition(Vector3 pageSize, Vector3 markSize, float zPosOffset)
    {
        float baseZPosition = pageSize.z - (pageSize.z / 2) - (markSize.z / 2);
        float zOffset = ((pageSize.z - (markSize.z)) * zPosOffset);
        float zPosition = baseZPosition - zOffset;
        return zPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Application.isPlaying) return;

        if (Journal.Instance != null)
        {
            // Added the '$' here so your PageNumber variable prints correctly
            Debug.Log($"Opening page: {PageNumber}");
            Journal.Instance.OpenPageNumber(PageNumber);
        }
        else
        {
            Debug.LogWarning("Journal Instance not found.");
        }
    }
}
