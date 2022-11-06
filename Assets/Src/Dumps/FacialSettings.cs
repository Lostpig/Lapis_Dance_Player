using System;
using UnityEngine;
using System.Collections.Generic;

public class FacialSettings : ScriptableObject
{
    [SerializeField]
    public string ActorName; // 0x18
    [SerializeField]
    public float BlinkDuration; // 0x20
    [SerializeField]
    public float BlinkInterval; // 0x24
    [SerializeField]
    public float BlinkThreshold; // 0x28
    [SerializeField]
    public AnimationCurve BlinkCurve; // 0x30
    [SerializeField]
    private List<FacialData> facialData; // 0x38

    public List<FacialData> FacialData
    {
        get
        {
            return facialData;
        }
        set
        {
            facialData = value;
        }
    }
}

[Serializable]
public class FacialData
{
    // Fields
    [SerializeField] private eFaceExpression expression; // 0x10
    [SerializeField] private bool blink; // 0x14
    [SerializeField] private List<BlendData> blendData; // 0x18
    [SerializeField] private float cheekPower; // 0x20
    [SerializeField] private float foreheadShade; // 0x24
    [SerializeField] private float eyeHighlight; // 0x28
    [SerializeField] private float eyeScale; // 0x2c

    // Properties
    public eFaceExpression Expression
    {
        get { return expression; }
        set { expression = value; }
    }
    public bool Blink
    {
        get { return blink; }
        set { blink = value; }
    }
    public List<BlendData> BlendData
    {
        get { return blendData; }
        set { blendData = value; }
    }
    public float CheekPower
    {
        get { return cheekPower; }
        set { cheekPower = value; }
    }
    public float ForeheadShade
    {
        get { return foreheadShade; }
        set { foreheadShade = value; }
    }
    public float EyeHighlight
    {
        get { return eyeHighlight; }
        set { eyeHighlight = value; }
    }
    public float EyeScale
    {
        get { return eyeScale; }
        set { eyeScale = value; }
    }
}
