using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using LapisPlayer;

namespace Oz.Timeline
{
    [Serializable]
    public class EyeBlinkPlayableAsset : PlayableAsset
    {
        public AnimationCurve blinkCurve; // 0x28
        public new double duration => _duration;
        private double _duration = 0;
        public void SetDuration(double clipDuration)
        {
            _duration = clipDuration;
        }
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var characters = owner.GetComponentsInChildren<CharacterBehaviour>();

            EyeBlinkPlayableBehaviour behaviour = new()
            {
                Asset = this,
                BlinkCurve = blinkCurve,
                Characters = characters,
            };
            return ScriptPlayable<EyeBlinkPlayableBehaviour>.Create(graph, behaviour);
        }
    }

    public class EyeBlinkTrack : TrackAsset
    {
        protected override void OnAfterTrackDeserialize()
        {
            foreach (var clip in GetClips())
            {
                if (clip.asset is EyeBlinkPlayableAsset ebpa)
                {
                    ebpa.SetDuration(clip.duration);
                }
            }
        }
    }

    public class EyeBlinkPlayableBehaviour : PlayableBehaviour
    {
        public AnimationCurve BlinkCurve { get; set; }
        public EyeBlinkPlayableAsset Asset { get; set; }
        public CharacterBehaviour[] Characters { get; set; }
        private float elapsed = 0f;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            elapsed = 0f;
        }
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            elapsed += info.deltaTime;
            var elpTime = elapsed / Asset.duration;

            var val = BlinkCurve.Evaluate((float)elpTime);
            var blinkVal = (1f - val) * 100f;

            foreach (var item in Characters)
            {
                item.Facial.UpdateBlink(blinkVal);
            }
            // Debug.Log("time = " + elpTime + ", val = " + val + ", blinkVal = " + blinkVal);
        }
    }
}
