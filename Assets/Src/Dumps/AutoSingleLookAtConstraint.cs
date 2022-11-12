using System;
using UnityEngine.Animations;
using UnityEngine;

public class AutoSingleLookAtConstraint : MonoBehaviour
{
    // Fields
    private LookAtConstraint _lookAtConstraint; // 0x18
    public int positionIndex; // 0x20
    public string lookAtName; // 0x28

    // Properties
    public LookAtConstraint LookAtConstraint { get => _lookAtConstraint; set => _lookAtConstraint = value; }

    // private void Awake() { }

    // private void Start() { }

    public void DoLookAt() { }
}
