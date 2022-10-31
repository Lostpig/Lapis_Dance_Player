using UnityEngine;
using System.Collections.Generic;

public class VowelSetting : ScriptableObject
{
    // Fields
    [SerializeField]
    private List<VowelData> vowelData; // 0x18

    // Properties
    public List<VowelData> VowelData
    {
        get { return vowelData; }
        set { vowelData = value; }
    }
}

[System.Serializable]
public class VowelData
{
    // Fields
    [SerializeField]
    private int vowel; // 0x10
    [SerializeField]
    private List<VowelBlendData> blendData; // 0x18

    // Properties
    public List<VowelBlendData> BlendData
    {
        get { return blendData; }
        set { blendData = value; }
    }
    public int Vowel
    {
        get { return vowel; }
        set { vowel = value; }
    }
}

[System.Serializable]
public class VowelBlendData
{
    // Fields
    [SerializeField]
    private int shapeIndex; // 0x10
    [SerializeField]
    private float blendValue; // 0x14

    // Properties
    public int ShapeIndex { get => shapeIndex; set => shapeIndex = value; }
    public float BlendValue { get => blendValue; set => blendValue = value; }
}