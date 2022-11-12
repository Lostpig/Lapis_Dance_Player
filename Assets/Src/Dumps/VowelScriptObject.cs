using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class VowelScriptObject : ScriptableObject
{
    // Fields
    [SerializeField] public double FrameRate; // 0x18
    [SerializeField] public List<VowelClipInfo> ClipInfos; // 0x20
    [SerializeField] public int[] ClipInfoPos; // 0x28

    public void AddElement(AnimationIndex animationId, double start, double duration, double easeInDuration, double easeOutDuration) { }
    public void Generate()
    {

    }
}

[Serializable]
public class VowelClipInfo
{
    // Fields
    [SerializeField] public AnimationIndex Index; // 0x10
    [SerializeField] public double Start; // 0x18
    [SerializeField] public double Duration; // 0x20
    [SerializeField] public double EaseInDuration; // 0x28
    [SerializeField] public double EaseOutDuration; // 0x30

    public double End { get => Start + Duration; }
}