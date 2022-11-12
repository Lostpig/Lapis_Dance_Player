using System;

[Serializable]
public class SceneReference
{
    // Fields
    public string GUID; // 0x10
    public string DyamicName; // 0x18
    public bool Dynamic; // 0x20
    public string TypeName; // 0x28
    public ReferenceObject RefObj; // 0x30
    public UnityEngine.Object ResolvedObj; // 0x38

    public string Identity { get; set; }
    public string Type { get; set; }

    public void ClearCache() { }
    public void ClearTypeCache() { }

    // public virtual T Resolve() { }
    // public virtual Object Resolve() { }
    // public virtual SceneReference Clone() { }
}
