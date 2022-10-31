using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.UI;

namespace LapisPlayer
{
    enum DanceState
    {
        Stop,
        Playing,
    }

    public class DanceManager
    {
        List<CharacterActor> _characters = new();
        DanceState _state = DanceState.Stop;
        DanceSetting _setting;
        GameObject _timelineRoot;
        PlayableDirector _director;
        AudioSource _audio;
        AnimationTrack[] _animationTracks;
        double _lastClipStart = 0.0;
        double _lastClipEnd = 0.0;

        public DanceManager(DanceSetting setting)
        {
            _setting = setting;
        }
        public async Task Initialize()
        {
            GameObject timelinePrefab = AssetBundleLoader.Instance.LoadAsset<GameObject>($"ar/song/{_setting.ID}/CM_Timeline_AR");
            _timelineRoot = GameObject.Instantiate(timelinePrefab);
            _director = _timelineRoot.GetComponent<PlayableDirector>();
            _director.extrapolationMode = DirectorWrapMode.Hold;

            var audioClip = await SoundBankLoader.Instance.LoadAudioClip(_setting.ID, _setting.Music);

            var audioSource = _timelineRoot.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            _audio = audioSource;

            var timeline = _director.playableAsset as TimelineAsset;
            List<AnimationTrack> animTracks = new();
            foreach (var rootTrack in timeline.GetRootTracks())
            {
                foreach (var track in rootTrack.GetChildTracks())
                {
                    if (track is AnimationTrack aniTrack)
                    {
                        animTracks.Add(aniTrack);
                    }
                }
            }
            _animationTracks = animTracks.ToArray();

            var lysTexts = _director.GetComponentsInChildren<Text>(true);
            var runner = GameObject.Find("Launcher");
            var runnerText = runner.GetComponent<Text>();
            foreach (var text in lysTexts)
            {
                text.font = runnerText.font;
            }
        }

        public void AddCharacter(CharacterActor character)
        {
            var timeline = _director.playableAsset as TimelineAsset;

            character.Parts.ForEach((part) => {
                foreach (var track in _animationTracks)
                {
                    var newTrack = CloneAnimationTrack(timeline, track.parent as TrackAsset, track);
                    var animator = part.Model.GetComponent<Animator>();
                    _director.SetGenericBinding(newTrack, animator);
                }
            });

            character.Root.transform.SetParent(_timelineRoot.transform);
            _characters.Add(character);
        }
        private AnimationTrack CloneAnimationTrack(TimelineAsset parent, TrackAsset rootTrack, AnimationTrack source)
        {
            Debug.Log("AnimationTrack Clone:" + source.name);

            var newTrack = parent.CreateTrack<AnimationTrack>(rootTrack, source.name);
            foreach (var clip in source.GetClips())
            {
                newTrack.CreateClip(clip.animationClip);
                if (clip.end > _lastClipEnd)
                {
                    _lastClipStart = clip.start;
                    _lastClipEnd = clip.end;
                }
            }
            return newTrack;
        }

        public void SetCharacterPosition()
        {
            for (int i = 0; i < _characters.Count; i++)
            {
                var chara = _characters[i];
                var n = (i % 2) == 0 ? 1 : -1;
                var x = (float)Math.Ceiling(i / 2.0);
                chara.Root.transform.position = new Vector3(x * n * 1.4f, 0, -x * 0.6f);
                chara.Root.transform.Rotate(0, x * n * -10f, 0);
            }
        }

        public void Update()
        {
            if (_state == DanceState.Playing)
            {
                double time = _director.time;
                bool finished = _lastClipEnd - time < 0.01667;
                if (finished)
                {
                    _director.time = _lastClipStart;
                    _audio.time = (float)_lastClipStart;
                }
            }
        }
        public void Play()
        {
            if (_characters.Count == 0)
            {
                Debug.LogWarning("Dance have no binding character");
                return;
            }
            if (_state != DanceState.Stop)
            {
                Debug.Log("Dance already playing");
                return;
            }

            _director.Play();
            _audio.Play();

            _state = DanceState.Playing;
        }
        public void Stop()
        {
            _director.Stop();
            _director.time = 0;
            _audio.Stop();
            _audio.time = 0;

            _state = DanceState.Stop;
        }
    }
}
