using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using LapisPlayer;

namespace Oz.Timeline
{
    public class LipsyncAsset : PlayableAsset
    {
        [SerializeField] private AnimationIndex index; // 0x18
        [SerializeField] private string originWord; // 0x20
        [SerializeField] private int volume; // 0x28
        [SerializeField] private float weight; // 0x2c

        // Properties
        public AnimationIndex Index { get => index; }
        public float Weight { get => weight; }
        public TimelineClip Clip { get; set; }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            LipsyncBehaviour behaviour = new();
            return ScriptPlayable<LipsyncBehaviour>.Create(graph, behaviour);
        }
    }

    public sealed class LipSyncTrack2 : TrackAsset
    {
        protected override void OnAfterTrackDeserialize()
        {
            foreach (var clip in GetClips())
            {
                if (clip.asset is LipsyncAsset lsa)
                {
                    lsa.Clip = clip;
                }
            }
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var facials = go.GetComponentsInChildren<FacialBehaviour>();
            foreach (var facial in facials)
            {
                facial.Vowel.Clear();
            }
            foreach (var clip in GetClips())
            {
                if (clip.asset is LipsyncAsset lsa)
                {
                    foreach (var facial in facials)
                    {
                        facial.Vowel.AppendVowelAnimationIndex(lsa.Index, lsa.Weight, clip.duration, clip.mixInDuration, clip.mixOutDuration, (float)clip.start);
                    }
                }
            }

            var updateBehaviour = new VowelUpdateBehaviour();
            updateBehaviour.Facials = facials;

            return ScriptPlayable<VowelUpdateBehaviour>.Create(graph, updateBehaviour, inputCount);
        }
    }

    public class LipsyncBehaviour : PlayableBehaviour { }

    public class VowelUpdateBehaviour : PlayableBehaviour
    {
        public FacialBehaviour[] Facials { get; set; }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            foreach (var facial in Facials)
            {
                facial.Vowel.Update((float)playable.GetTime());
            }
        }
    }
}
