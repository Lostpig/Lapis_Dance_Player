using LapisPlayer;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Oz.Timeline
{
    // TODO 目前没有用到,后续有需要补
    public class ExpressionTrack : TrackAsset
    {
        // public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, Int32 inputCount)
        // {
        // 
        // }
    }

    public class ExpressionPlayableAsset : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ExpressionPlayableBehaviour behaviour = new();
            return ScriptPlayable<ExpressionPlayableBehaviour>.Create(graph, behaviour);
        }
    }

    public class ExpressionPlayableBehaviour : PlayableBehaviour
    {

    }
}
