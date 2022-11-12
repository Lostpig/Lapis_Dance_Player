using System;
using UnityEngine;

public abstract class InterpValue
{
    public bool Override; // 0x10
    // public T GetValue() { }
    // public abstract bool Interp(InterpValue from, InterpValue to, float t) { }
}

[Serializable]
public abstract class InterpValue<T> : InterpValue
{
    public static T op_Implicit(InterpValue<T> interpValue) { return interpValue.value; }

    public T value; // 0x0
    // public override bool Interp(InterpValue from, InterpValue to, float t) { }
    protected virtual void Interp(T from, T to, float t) { }
    // public virtual T1 Clone() { }
}

[Serializable]
public sealed class InterpFloat : InterpValue<float>
{
    protected override void Interp(float from, float to, float t) { }
}

[Serializable]
public sealed class InterpVector2 : InterpValue<Vector2>
{
    protected override void Interp(Vector2 from, Vector2 to, float t) { }

    // public static Vector3 op_Implicit(InterpVector2 prop) { }
    // public static Vector4 op_Implicit(InterpVector2 prop) { }
}

[Serializable]
public sealed class InterpBool : InterpValue<bool>
{
    // public override bool Interp(InterpValue from, InterpValue to, float t) { }
}

[Serializable]
public sealed class InterpColor : InterpValue<Color>
{
    // public override void Interp(Color from, Color to, float t) { }
}

[Serializable]
public sealed class InterpVector3 : InterpValue<Vector3>
{
    protected override void Interp(Vector3 from, Vector3 to, float t) { }
    // public static Vector2 op_Implicit(InterpVector3 prop) { }
    // public static Vector4 op_Implicit(InterpVector3 prop) { }
}

[Serializable]
public sealed class InterpVector4 : InterpValue<Vector4>
{
    protected override void Interp(Vector4 from, Vector4 to, float t) { }
    // public static Vector2 op_Implicit(InterpVector4 prop) { }
    // public static Vector3 op_Implicit(InterpVector4 prop) { }
}
