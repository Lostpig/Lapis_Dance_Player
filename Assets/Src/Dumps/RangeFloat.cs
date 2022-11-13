using System;

[Serializable]
public struct RangeFloat
{
    // Fields
    public float start; // 0x10
    public float length; // 0x14

    // Properties
    public float end { get => start + length; }
}