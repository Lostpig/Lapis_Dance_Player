using System;
using UnityEngine;

public interface IEffectSetting
{
    public abstract bool Expend { get; set; }
    public abstract bool Changed { get; set; }
    public abstract bool Enable { get; set; }
    public abstract int DebugShow { get; set; }
    public abstract string EffectName { get; }
    public abstract int DebugStepCount { get; }
    public abstract PiplineType Available { get; }
}

[Serializable]
public abstract class EffectSetting : IEffectSetting
{
    // Fields
    public bool expend; // 0x10
    private bool changed; // 0x11
    public bool enable; // 0x12
    public int debugShow; // 0x14

    // Properties
    public bool Expend { get => expend; set => expend = value; }
    public bool Changed { get => changed; set => changed = value; }
    public bool Enable { get => enable; set => enable = value; }
    public int DebugShow { get => debugShow; set => debugShow = value; }
    public abstract string EffectName { get; }
    public virtual int DebugStepCount { get; }
    public virtual PiplineType Available { get; }

    // internal virtual T Clone() { }
    // internal virtual void Interp(EffectSetting from, EffectSetting to, Single t) { }
}

[Serializable]
public abstract class EffectSetting<T> : EffectSetting
{
    // internal virtual void Interp(T from, T to, Single t) { }
    // public virtual T Clone() { }
}

public abstract class PostEffectSetting : ScriptableObject, IEffectSetting
{
    // Fields
    public bool expend; // 0x18
    public bool changed; // 0x19
    public bool enable; // 0x1a
    public int debugShow; // 0x1c

    // Properties
    public virtual PiplineType Available { get; }
    public bool Expend { get => expend; set => expend = value; }
    public bool Changed { get => changed; set => changed = value; }
    public bool Enable { get => enable; set => enable = value; }
    public int DebugShow { get => debugShow; set => debugShow = value; }
    public virtual int DebugStepCount { get; }
    public abstract string EffectName { get; }
    internal virtual void Interp(PostEffectSetting from, PostEffectSetting to, float t) { }
}
