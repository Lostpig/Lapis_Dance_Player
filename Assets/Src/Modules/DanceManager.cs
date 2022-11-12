using System.Threading.Tasks;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace LapisPlayer
{
    interface IDanceManager
    {
        public bool IsARMode { get; }
        public CharacterActor GetCharacter(int position);
        public CharacterActor[] GetActiveCharacters();
    }

    public enum DanceState
    {
        Stop,
        Play,
        Pause,
    }

    public class DanceManager: IDanceManager
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
            if (_state != DanceState.Stop) Stop();

            _setting = setting;
            var originDanceObject = _timelineRoot;

            GameObject timelinePrefab = AssetBundleLoader.Instance.LoadAsset<GameObject>($"ar/song/{_setting.ID}/CM_Timeline_AR");
            _timelineRoot = GameObject.Instantiate(timelinePrefab);
            _timelineRoot.transform.position = originDanceObject?.transform.position ?? Vector3.zero;
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
        private void BindCharacters()
        {
            for (int i = 0; i < _characters.Length; i++)
            {
                if (_characters[i] == null) continue;
                BindAnimationTrack(_characters[i]);
                _characters[i].Root.transform.SetParent(_timelineRoot.transform, false);
            }
        }

        public void SetCharacter(int characterPos, CharacterActor character)
        {
            RemoveCharacter(characterPos);

            BindAnimationTrack(character);
            character.Root.transform.SetParent(_timelineRoot.transform);
            _characters[characterPos] = character;
            character.PlayBaseAnimation();
        }
        public CharacterActor GetCharacter(int characterPos)
        {
            return _characters[characterPos];
        }
        public CharacterActor[] GetActiveCharacters()
        {
            return _characters.Where(c => c != null).ToArray();
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
            AnimationTrack bindTrack = null;

            foreach (var track in _animationGroupTrack.GetChildTracks())
            {
                if (track == _animationTrack) continue;
                var bind = _director.GetGenericBinding(track);
                if (bind == null)
                {
                    bindTrack = track as AnimationTrack;
                    break;
                }
            }
            if (bindTrack == null)
            {
                bindTrack = CloneAnimationTrack(timeline, _animationGroupTrack, _animationTrack);
            }

            var sklAnimator = character.SkeletonRoot.GetComponent<Animator>();
            _director.SetGenericBinding(bindTrack, sklAnimator);
        }
        private AnimationTrack CloneAnimationTrack(TimelineAsset parent, TrackAsset rootTrack, AnimationTrack source)
        {
            Debug.Log("AnimationTrack Clone:" + source.name);

            var newTrack = parent.CreateTrack<AnimationTrack>(rootTrack, source.name);
            foreach (var clip in source.GetClips())
            {
                var tclip = newTrack.CreateClip(clip.animationClip);
                tclip.clipIn = clip.clipIn;
                tclip.duration = clip.duration;
            }
            return newTrack;
        }

        public void SetCharacterPosition()
        {
            int avtiveCount = _characters.Where(c => c != null).Count();
            float[] xpos = avtiveCount switch
            {
                2 => new float[] { 0.9f, -0.9f },
                3 => new float[] { 0, 1.5f, -1.5f },
                4 => new float[] { 0.7f, -0.7f, 2.1f, -2.1f },
                5 => new float[] { 0, 1.1f, -1.1f, 2.2f, -2.2f },
                _ => new float[] { 0 },
            };
            float[] zpos = avtiveCount switch
            {
                2 => new float[] { 0, 0 },
                3 => new float[] { 0.3f, -0.4f, -0.4f },
                4 => new float[] { 0, 0, 0, 0 },
                5 => new float[] { 0.6f, 0, 0, -0.6f, -0.6f },
                _ => new float[] { 0 },
            };

            int idx = 0;
            for (int i = 0; i < _characters.Length; i++)
            {
                var chara = _characters[i];
                if (chara == null) continue;

                // float y = (1 - chara.Scales.ScaleRatio) / 4f + chara.Heel.tweakFootHeight;
                float y = chara.Heel.tweakFootHeight;
                chara.Root.transform.localPosition = new Vector3(xpos[idx], y, zpos[idx]);
                idx++;
            }
        }
        public void SetCharacterState(bool playing)
        {
            for (int i = 0; i < _characters.Length; i++)
            {
                var chara = _characters[i];
                if (chara == null) continue;
                chara.SetPlaying(playing);
            }
        }
        public void SetPostion(Vector3 position)
        {
            _timelineRoot.transform.position = position;
        }

        public void Update()
        {
            if (_state == DanceState.Play)
            {
                double time = _director.time;
                bool finished = _lastClipEnd - time < 0.01667; // || _audio.clip.length - time < 0.01667;
                if (finished)
                {
                    _director.time = _lastClipStart;
                    _audio.time = (float)_lastClipStart;
                    if (!_audio.isPlaying) _audio.Play();
                }
            }
        }
        public void Play()
        {
            if (_state == DanceState.Stop)
            {
                _director.Play();
                _audio.Play();
                SetCharacterState(true);
            }
            else if (_state == DanceState.Pause)
            {
                Time.timeScale = 1;
            }

            _state = DanceState.Play;
        }
        public void Pause()
        {
            if (_state == DanceState.Play)
            {
                _audio.Pause();
                Time.timeScale = 0;
                _state = DanceState.Pause;
            }
            else if (_state == DanceState.Pause)
            {
                _audio.UnPause();
                _audio.time = (float)_director.time;
                Time.timeScale = 1;
                _state = DanceState.Play;
            }
        }
        public void Stop()
        {
            if (_state == DanceState.Pause)
            {
                Time.timeScale = 1;
            }

            _director.Stop();
            _director.time = 0;
            _audio.Stop();
            _audio.time = 0;
            SetCharacterState(false);

            _state = DanceState.Stop;
        }

        public bool IsARMode { get => true; }
    }
}
