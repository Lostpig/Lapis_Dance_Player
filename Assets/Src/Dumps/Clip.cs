using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;

namespace Timeline
{
    [Serializable]
    public class Clip : PlayableAsset, ITimelineClipAsset, IPropertyPreview
    {
        // Fields
        public bool Await; // 0x18
        public TimelineClip clipInfo; // 0x20
        public Mixer mixer; // 0x28
        public Behaviour behaviour; // 0x30

        // Properties
        public virtual ClipCaps clipCaps { get; }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
            return new Playable();
        }
        public virtual void SetContext(Behaviour behaviour) { }
        public virtual void GatherProperties(PlayableDirector director, IPropertyCollector driver) { }
    }
}
