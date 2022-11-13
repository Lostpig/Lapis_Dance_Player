using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using LapisPlayer;
using System.Linq;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using static UnityEngine.UI.GridLayoutGroup;

namespace Oz.Timeline
{
    [Serializable]
    public class EyeBlinkPlayableAsset : PlayableAsset
    {
        public AnimationCurve blinkCurve; // 0x28
        public new double duration => _duration;
        private double _duration = 0;
        private EyeBlinkTrack _parentTrack;

        public void Initialize(EyeBlinkTrack parentTrack, double clipDuration)
        {
            _parentTrack = parentTrack;
            _duration = clipDuration;
        }
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            PlayableDirector dicector = owner.GetComponent<PlayableDirector>();

            EyeBlinkPlayableBehaviour behaviour;
            var danceManager = PlayerGlobal.Instance.GetSingleton<IDanceManager>();
            if (danceManager.IsARMode)
            {
                var characters = danceManager.GetActiveCharacters();
                var facials = characters.Select(c => c.Facial).ToArray();
                behaviour = new()
                {
                    Asset = this,
                    BlinkCurve = blinkCurve,
                    Characters = facials,
                };
            }
            else
            {
                var actorObj = dicector.GetGenericBinding(_parentTrack);
                var actor = actorObj.GetComponent<Actor>();
                var facial = danceManager.GetCharacter(actor.Postion).Facial;

                behaviour = new()
                {
                    Asset = this,
                    BlinkCurve = blinkCurve,
                    Characters = new FacialBehaviour[] { facial },
                };
            }

            return ScriptPlayable<EyeBlinkPlayableBehaviour>.Create(graph, behaviour);
        }
    }

    [TrackClipType(typeof(EyeBlinkPlayableAsset), false)]
    [TrackBindingType(typeof(GameObject))]
    public class EyeBlinkTrack : TrackAsset
    {
        protected override void OnAfterTrackDeserialize()
        {
            foreach (var clip in GetClips())
            {
                if (clip.asset is EyeBlinkPlayableAsset ebpa)
                {
                    ebpa.Initialize(this, clip.duration);
                }
            }
        }
    }

    public class EyeBlinkPlayableBehaviour : PlayableBehaviour
    {
        public AnimationCurve BlinkCurve { get; set; }
        public EyeBlinkPlayableAsset Asset { get; set; }
        public FacialBehaviour[] Characters { get; set; }
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
