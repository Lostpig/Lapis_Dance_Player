using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.Text;
using static Unity.VisualScripting.Member;

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
        AnimationTrack _animationTrack;
        GroupTrack _animationGroupTrack;

        double _lastClipStart = 0.0;
        double _lastClipEnd = 0.0;

        public GameObject Root => _timelineRoot;

        public DanceManager(DanceSetting setting)
        {
            _setting = setting;
        }
        public async Task Initialize()
        {
            GameObject timelinePrefab = AssetBundleLoader.Instance.LoadAsset<GameObject>($"ar/song/{_setting.ID}/CM_Timeline_AR");
            _timelineRoot = GameObject.Instantiate(timelinePrefab);
            _timelineRoot.transform.position = Vector3.zero;
            _director = _timelineRoot.GetComponent<PlayableDirector>();
            _director.extrapolationMode = DirectorWrapMode.Hold;

            var audioClip = await SoundBankLoader.Instance.LoadAudioClip(_setting.ID, _setting.Music);

            var audioSource = _timelineRoot.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            _audio = audioSource;

            var timeline = _director.playableAsset as TimelineAsset;
            foreach (var rootTrack in timeline.GetRootTracks())
            {
                foreach (var track in rootTrack.GetChildTracks())
                {
                    if (track is AnimationTrack aniTrack)
                    {
                        _animationTrack = aniTrack;
                        _animationGroupTrack = rootTrack as GroupTrack;
                        break;
                    }
                }
                if (_animationTrack != null) break;
            }
            foreach (var clip in _animationTrack.GetClips())
            {
                if (clip.end > _lastClipEnd)
                {
                    _lastClipStart = clip.start;
                    _lastClipEnd = clip.end;
                }
            }

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
            BindAnimationTrack(character);
            character.Root.transform.SetParent(_timelineRoot.transform);
            _characters.Add(character);
        }
        public void RemoveCharacter(CharacterActor character)
        {
            _characters.Remove(character);
            GameObject.Destroy(character.Root);
            // _director.remo
        }
        public void ClearCharacters()
        {
            foreach(var chara in _characters)
            {
                GameObject.Destroy(chara.Root);
            }
            _characters.Clear();
        }
        
        private void BindAnimationTrack(CharacterActor character)
        {
            var timeline = _director.playableAsset as TimelineAsset;
            var count = character.Parts.Count;
            var tracks = new AnimationTrack[count];

            int idx = 0;
            foreach (var track in _animationGroupTrack.GetChildTracks())
            {
                if (track == _animationTrack) continue;
                var bind = _director.GetGenericBinding(track);
                if (bind == null)
                {
                    tracks[idx] = track as AnimationTrack;
                    idx++;
                }
                if (idx >= count) break;
            }
            for (; idx < count; idx++)
            {
                tracks[idx] = CloneAnimationTrack(timeline, _animationGroupTrack, _animationTrack);
            }

            for (int i = 0; i < count; i++)
            {
                var animator = character.Parts[i].Model.GetComponent<Animator>();
                _director.SetGenericBinding(tracks[i], animator);
            }
        }
        private AnimationTrack CloneAnimationTrack(TimelineAsset parent, TrackAsset rootTrack, AnimationTrack source)
        {
            Debug.Log("AnimationTrack Clone:" + source.name);

            var newTrack = parent.CreateTrack<AnimationTrack>(rootTrack, source.name);
            foreach (var clip in source.GetClips())
            {
                newTrack.CreateClip(clip.animationClip);
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
