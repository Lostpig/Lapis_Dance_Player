using System;
using Timeline;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;

public class PostProcessTrack : Track
{
    // public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, Int32 inputCount) { }
}

[Serializable]
public class PostProcessClip : Clip
{
    public SeanPostProcessSetting State; // 0x38

    public override ClipCaps clipCaps { get; }

    public void PasteFromDefault() { }

    // public override Playable CreatePlayable(PlayableGraph graph, GameObject go) { }
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver) { }
}

