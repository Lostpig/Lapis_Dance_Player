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
            var characters = owner.GetComponentsInChildren<CharacterBehaviour>();

            LipsyncBehaviour behaviour = new()
            {
                Asset = this,
                Characters = characters,
            };
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
    }

    public class LipsyncBehaviour : PlayableBehaviour
    {
        public LipsyncAsset Asset { get; set; }
        public CharacterBehaviour[] Characters { get; set; }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            foreach (var item in Characters)
            {
                item.Vowel.AppendVowelAnimationIndex(Asset.Index, Asset.Weight, Asset.Clip.duration, Asset.Clip.mixInDuration, Asset.Clip.mixOutDuration);
            }
        }
    }
}
