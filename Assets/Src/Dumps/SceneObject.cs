using System;
using UnityEngine;

[Serializable]
public class SceneObject
{
    // Fields
    [SerializeField] public string Name; // 0x10
    [SerializeField] public string ChildPath; // 0x18
    [SerializeField] public string Root; // 0x20
    [SerializeField] public int Depth; // 0x28
    [SerializeField] public SceneObjectState state; // 0x2c
    [SerializeField] public string RefType; // 0x30
    [SerializeField] public string AssetPath; // 0x38
    [SerializeField] public string GUID; // 0x40
    [SerializeField] public Vector3 LocalPostion; // 0x48
    [SerializeField] public Quaternion LocalRotation; // 0x54
    [SerializeField] public Vector3 LocalScale; // 0x64
    [SerializeField] public Vector3 Postion; // 0x70
    [SerializeField] public Quaternion Rotation; // 0x7c
    [SerializeField] public Vector3 Scale; // 0x8c
    [SerializeField] public bool Active; // 0x98
    [SerializeField] public bool OverrideTransform; // 0x99
    [SerializeField] public bool RuntimeBind; // 0x9a
}
