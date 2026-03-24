using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

[System.Serializable]
public class BookMarkData
{

    [SerializeField] public int pageNumber;
    [SerializeField] public bool enabled = false;
    [SerializeField, Range(0, 1)] public float depthPosOffset = 0.85f;
    [SerializeField, Range(0, 1)] public float widthPosOffset = 0.5f;
    public BookMarkData Clone()
    {
        return (BookMarkData)this.MemberwiseClone();
    }

    public void Apply(BookMarkData data)
    {
        this.pageNumber = data.pageNumber;
        this.enabled = data.enabled;
        this.depthPosOffset = data.depthPosOffset;
        this.widthPosOffset = data.widthPosOffset;
    }

}
