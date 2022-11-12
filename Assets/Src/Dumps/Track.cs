using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;

namespace Timeline
{
    public class Mixer : Behaviour
    {
        // Fields
        public Context clipA; // 0x18
        public Context clipB; // 0x20
        public int lastInputCount; // 0x28

        // Properties

        // Methods
        // RVA: 0x144dbf4 VA: 0x6e8999dbf4
        public void PrepareFrame(Playable playable, FrameData info) { }
        // RVA: 0x144dc8c VA: 0x6e8999dc8c
        // public bool IsTimeOut() { }
        // RVA: 0x144dc94 VA: 0x6e8999dc94
        public void OnBehaviourPause(Playable playable, FrameData info) { }
        // RVA: 0x144dcb4 VA: 0x6e8999dcb4
        public void ProcessFrame(Playable playable, FrameData info, UnityEngine.Object playerData) { }
        // RVA: 0x144decc VA: 0x6e8999decc
        public virtual void ProcessZero(Playable playable, FrameData info, UnityEngine.Object playerData) { }
        // RVA: 0x144ded0 VA: 0x6e8999ded0
        public virtual void ProcessOne(Playable playable, FrameData info, UnityEngine.Object playerData) { }
        // RVA: 0x144ded4 VA: 0x6e8999ded4
        public virtual void ProcessTwo(Playable playable, FrameData info, UnityEngine.Object playerData) { }
        // RVA: 0x144ded8 VA: 0x6e8999ded8
        public void OnPlayableDestroy(Playable playable) { }
        // RVA: 0x144defc VA: 0x6e8999defc
        public virtual void InputChanged(Int32 from, Int32 to) { }
        // RVA: 0x144d298 VA: 0x6e8999d298
        public void SetMixContext(Context context) { }
        // RVA: 0x144df00 VA: 0x6e8999df00
        // public Context MaxWeight() { }
    }

    public abstract class Track : TrackAsset
    {
        // public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, Int32 inputCount) { }
        public virtual void SetContext(Mixer mixer, GameObject go) { }
        // public virtual UnityEngine.Object GetBindingObject(PlayableDirector director) {}
    }
}
