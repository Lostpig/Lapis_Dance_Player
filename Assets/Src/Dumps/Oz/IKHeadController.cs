using System;
using UnityEngine;

namespace Oz.ActorModule
{
    public class IKHeadController : MonoBehaviour
    {
        // Fields
        private Animator m_Animator; // 0x18
        private Transform m_Head; // 0x20
        public Transform m_Target; // 0x28
        public float m_TargetWeight; // 0x30
        public Vector4 WeightParts; // 0x34
        public float m_Weight; // 0x44
        public float m_WeightVelocity; // 0x48
        public Vector3 m_Position; // 0x4c
        public Vector3 m_PositionVelocity; // 0x58
        private float m_SmoothTime; // 0x64
        private float w; // 0x68
        private Vector3 m_TimelineInputPosition; // 0x6c
        private float m_TimelineInputWeight; // 0x78

        // Properties
        public Vector3 ForwardTarget { get => m_Head.position; }
        public Vector3 TargetPos { get => m_Target.position; }

        // private void Awake() { }
        // RVA: 0x13f1a18 VA: 0x6e89941a18
        public void Init(Animator animator) { }
        // RVA: 0x13f1af8 VA: 0x6e89941af8
        public void ApplyTimelineLookAtTarget(Vector3 position, Single weight) { }
        // RVA: 0x13f1b04 VA: 0x6e89941b04
        public void ReleaseTimelineControl() { }
        // RVA: 0x13f1b0c VA: 0x6e89941b0c
        public void SetLookAtTarget(Transform target, Single weight) { }
        // RVA: 0x13f1be0 VA: 0x6e89941be0
        public void SetLookAtTargetImmediate(Transform target, Single weight) { }
        // RVA: 0x13f1d28 VA: 0x6e89941d28
        // private void OnDrawGizmos() { }
        // RVA: 0x13f1ef8 VA: 0x6e89941ef8
        // private void OnAnimatorIK(Int32 index) { }
    }
}
