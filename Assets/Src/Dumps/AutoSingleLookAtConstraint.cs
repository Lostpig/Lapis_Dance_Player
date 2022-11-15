using System;
using UnityEngine.Animations;
using UnityEngine;
using LapisPlayer;

public class AutoSingleLookAtConstraint : MonoBehaviour
{
    // Fields
    private LookAtConstraint _lookAtConstraint; // 0x18
    public int positionIndex; // 0x20
    public string lookAtName; // 0x28

    // Properties
    public LookAtConstraint LookAtConstraint { get => _lookAtConstraint; set => _lookAtConstraint = value; }

    // private void Awake() { }

    private void Start() {
        _lookAtConstraint = GetComponent<LookAtConstraint>();
        DoLookAt();
    }

    public void DoLookAt() {
        var danceManager = PlayerGlobal.Instance.GetSingleton<IDanceManager>();
        var chara = danceManager.GetCharacter(positionIndex - 1);

        if (chara != null)
        {
            var lookAt = Utility.FindNodeByRecursion(chara.SkeletonRoot, lookAtName);

            var s = _lookAtConstraint.GetSource(0);
            s.sourceTransform = lookAt.transform;
            _lookAtConstraint.SetSource(0, s);
        }
    }
}
