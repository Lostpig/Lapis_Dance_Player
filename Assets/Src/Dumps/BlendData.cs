using System;
using UnityEngine;

[Serializable]
public class BlendData
{
    // Fields
    [SerializeField] private eFacialParts parts; // 0x10
    [SerializeField] private string shapeId; // 0x18
    [SerializeField] private float blendValue; // 0x20

    // Properties
    public eFacialParts Parts
    {
        get { return parts; }
        set { parts = value; }
    }
    public string ShapeId
    {
        get { return shapeId; }
        set { shapeId = value; }
    }
    public float BlendValue
    {
        get { return blendValue; }
        set { blendValue = value; }
    }
}
