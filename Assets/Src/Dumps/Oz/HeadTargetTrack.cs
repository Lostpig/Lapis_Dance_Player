using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;
using Oz.ActorModule;

namespace Oz.Timeline
{
    public class HeadTargetTrack : TrackAsset
    {
        // public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, Int32 inputCount) { }
    }

    public class HeadTargetPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        // Fields
        public IKHeadController headDirection; // 0x18
        public ExposedReference<Transform> lookAtTarget; // 0x20
        public AnimationCurve weight; // 0x30

        // Properties
        public ClipCaps clipCaps { get; }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            HeadTargetPlayableBehaviour behaviour = new();
            return ScriptPlayable<HeadTargetPlayableBehaviour>.Create(graph, behaviour);
        }
    }

    public class HeadTargetPlayableBehaviour : PlayableBehaviour
    {
        // Fields
        public Transform lookAtTarget; // 0x10
        public AnimationCurve weightCurve; // 0x18
        public IKHeadController headDirection; // 0x20

        // Properties

        // Methods
        // RVA: 0x13d8d70 VA: 0x6e89928d70
        public void ProcessFrame(Playable playable, FrameData info, UnityEngine.Object playerData) { }
    }
}
