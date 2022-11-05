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
        CharacterActor[] _characters = new CharacterActor[5];
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

        public async Task InitializeDance(DanceSetting setting)
        {
            if (_state == DanceState.Playing) Stop();

            _setting = setting;
            var originDanceObject = _timelineRoot;

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

            _lastClipStart = 0;
            _lastClipEnd = 0;
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

            BindCharacters();
            if (originDanceObject != null)
            {
                GameObject.Destroy(originDanceObject);
            }
        }
        private void BindCharacters ()
        {
            for (int i = 0; i < _characters.Length; i++)
            {
                if (_characters[i] == null) continue;
                BindAnimationTrack(_characters[i]);
                _characters[i].Root.transform.SetParent(_timelineRoot.transform);
            }
        }

        public void SetCharacter(int characterPos, CharacterActor character)
        {
            RemoveCharacter(characterPos);

            BindAnimationTrack(character);
            character.Root.transform.SetParent(_timelineRoot.transform);
            _characters[characterPos] = character;

            if (_state != DanceState.Playing)
            {
                character.PlayBaseAnimation();
            }
        }
        public CharacterActor GetCharacter(int characterPos)
        {
            return _characters[characterPos];
        }
        public void RemoveCharacter(int characterPos)
        {
            if (_characters[characterPos] != null)
            {
                var character = _characters[characterPos];
                _characters[characterPos] = null;
                character.Destroy();
            }
        }
        public void ClearCharacters()
        {
            for (int i = 0; i < _characters.Length; i++)
            {
                RemoveCharacter(i);
            }
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
            for (int i = 0; i < _characters.Length; i++)
            {
                var chara = _characters[i];
                if (chara == null) continue;

                var n = (i % 2) == 0 ? 1 : -1;
                var x = (float)Math.Ceiling(i / 2.0);
                chara.Root.transform.position = new Vector3(x * n * 1.4f, 0, -x * 0.6f);
                // chara.Root.transform.RotateAround(0, x * n * -10f, 0);
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

            for (int i = 0; i < _characters.Length; i++)
            {
                var chara = _characters[i];
                if (chara != null) chara.PlayBaseAnimation();
            }
        }
    }
}
