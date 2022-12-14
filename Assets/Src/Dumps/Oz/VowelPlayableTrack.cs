using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;
using LapisPlayer;
using System;

namespace Oz.Timeline
{
    [TrackClipType(typeof(VowelPlayableAsset), false)]
    [TrackBindingType(typeof(GameObject))]
    public class VowelPlayableTrack : TrackAsset
    {
        protected override void OnAfterTrackDeserialize()
        {
            foreach (var clip in GetClips())
            {
                if (clip.asset is VowelPlayableAsset ebpa)
                {
                    ebpa.parent = this;
                }
            }
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            PlayableDirector dicector = go.GetComponent<PlayableDirector>();
            var model = dicector.GetGenericBinding(this) as GameObject;
            var updateBehaviour = new VowelUpdateBehaviour();
            if (model != null)
            {
                var actor = model.transform.parent.gameObject.GetComponent<ActorBehaviour>();
                var danceManager = PlayerGlobal.Instance.GetSingleton<IDanceManager>();
                var facial = danceManager.GetCharacter(actor.Postion).Facial;
                updateBehaviour.Facials = new FacialBehaviour[] { facial };
            }
            else
            {
                updateBehaviour.Facials = Array.Empty<FacialBehaviour>();
            }

            return ScriptPlayable<VowelUpdateBehaviour>.Create(graph, updateBehaviour, inputCount);
        }
    }

    public class VowelPlayableAsset : PlayableAsset
    {
        public VowelScriptObject sobj; // 0x18
        public VowelPlayableTrack parent;
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
#if UNITY_EDITOR
            Debug.Log("VowelPlayable created");
#endif
            PlayableDirector dicector = owner.GetComponent<PlayableDirector>();
            var model = dicector.GetGenericBinding(parent) as GameObject;
            var actor = model.transform.parent.gameObject.GetComponent<ActorBehaviour>();
            var danceManager = PlayerGlobal.Instance.GetSingleton<IDanceManager>();
            var facial = danceManager.GetCharacter(actor.Postion).Facial;

            facial.Vowel.BindWithVowelCilipInfo(sobj.ClipInfos.ToArray());

            VowelPlayableBehaviour behaviour = new();
            return ScriptPlayable<VowelPlayableBehaviour>.Create(graph, behaviour);
        }
    }

    public class VowelPlayableBehaviour : PlayableBehaviour { }
}
