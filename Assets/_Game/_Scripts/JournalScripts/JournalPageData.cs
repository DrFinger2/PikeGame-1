using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[System.Serializable]
public class JournalPageData
{
    [SerializeField, HideInInspector]
    private int pageNumber = -1;
    public int PageNumber
    {
        get { return this.pageNumber; }
        set { pageNumber = value; bookmark.pageNumber = value; }
    }

    [Header("Page Settings")]
    [SerializeField] public Texture2D texture;

    [Header("Bookmark Settings")]
    [SerializeField] public BookMarkData bookmark = new();

    public JournalPageData Clone()
    {
        JournalPageData pageData = (JournalPageData)this.MemberwiseClone();
        pageData.bookmark =  this.bookmark.Clone();
        return pageData;
    }
}
