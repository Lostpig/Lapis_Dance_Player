using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;

namespace Oz.Timeline
{
    public interface ITimelineBinding { }

    public interface IMaterialPropertyBlockModifier : ITimelineBinding
    {
        public abstract void SetMaterialProperty(String name, Single value);
        public abstract void SetMaterialProperty(String name, Vector4 value);
        public abstract void SetMaterialProperty(String name, Color value);
        public abstract void ClearMaterialPropertyBlock();
    }

    public class MaterialPropertyBlockTrack : TrackAsset
    {
        // public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, Int32 inputCount) { }
    }

    public abstract class MaterialPropertyBlockPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        // Fields
        public IMaterialPropertyBlockModifier modifier; // 0x18
        public string propertyName; // 0x20

        public ClipCaps clipCaps { get; }
    }
    public class MaterialPropertyBlockFloatPlayableAsset : MaterialPropertyBlockPlayableAsset
    {
        public MaterialPropertyBlockFloatPlayableBehaviour template; // 0x28

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            MaterialPropertyBlockFloatPlayableBehaviour behaviour = new();
            return ScriptPlayable<MaterialPropertyBlockFloatPlayableBehaviour>.Create(graph, behaviour);
        }
    }

    [Serializable]
    public class MaterialPropertyBlockFloatPlayableBehaviour : MaterialPropertyBlockPlayableBehaviour
    {
        public string propertyName; // 0x18
        public float value; // 0x20

        public void ProcessFrame(Playable playable, FrameData info, UnityEngine.Object playerData) { }
    }

    public class MaterialPropertyBlockPlayableBehaviour : PlayableBehaviour
    {
        public IMaterialPropertyBlockModifier modifier; // 0x10
    }
}
