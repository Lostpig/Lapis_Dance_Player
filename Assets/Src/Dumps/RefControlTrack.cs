using System;
using Timeline;
using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.Timeline;
using System.Collections.Generic;
using LapisPlayer;

namespace BaseTimeLine
{
    public class RefControlTrack : Track
    {
        // public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, Int32 inputCount) { }
    }

    [Serializable]
    public class RefControlClip : Clip
    {
        public List<ControlItem> Items;

        public override ClipCaps clipCaps { get; }
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            RefControlClipPlayableBehaviour behaviour = new()
            {
                Items = this.Items.ToArray()
            };
            return ScriptPlayable<RefControlClipPlayableBehaviour>.Create(graph, behaviour);
        }
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver) { }

    }

    public class RefControlClipPlayableBehaviour : PlayableBehaviour
    {
        public ControlItem[] Items;
        GameObject[] _gos;
        bool[] _readys;
        bool[] _active;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            _gos = new GameObject[Items.Length];
            _readys = new bool[Items.Length];
            _active = new bool[Items.Length];

            for (int i = 0; i < Items.Length; i++)
            {
                var refObj = ReferenceStore.Instance.GetReference(Items[i].Reference.GUID);
                if (refObj != null)
                {
                    _gos[i] = refObj.GetGameObject();
                    _readys[i] = true;
                }
                else
                {
                    _readys[i] = false;
                }
                _active[i] = false;
            }
        }
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var time = playable.GetTime() / playable.GetDuration() + 0.05;
            // Debug.Log(time);

            for (int i = 0; i < Items.Length; i++)
            {
                ApplyItem(i, time);
            }
        }

        private void ApplyItem(int index, double time)
        {
            if (!_readys[index]) return;

            var item = Items[index];
            var go = _gos[index];

            if (time >= item.LifeTime.end)
            {
                Debug.Log("Ref Off:" + go.name);

                go.SetActive(!item.On);
                _readys[index] = false;
            }
            else if (time >= item.LifeTime.start && !_active[index])
            {
                Debug.Log("Ref On:" + go.name);

                go.SetActive(item.On);
                _active[index] = true;
            }
        }
    }
}

[Serializable]
public class ControlItem
{
    // Fields
    [SerializeField] public SceneReference Reference; // 0x10
    [SerializeField] public SceneReference Postion; // 0x18
    [SerializeField] public RangeFloat LifeTime; // 0x20
    [SerializeField] public bool On; // 0x28
    [SerializeField] public bool SetAsChild; // 0x29
    public bool apply = false; // 0x2a
}

