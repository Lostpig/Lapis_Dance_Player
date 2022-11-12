using Cinemachine;
using Oz.Timeline;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace LapisPlayer
{
    public class BattleDanceManager : IDanceManager
    {
        public bool Ready { get; private set; }
        DanceState _state = DanceState.Stop;
        GameObject _stage;
        GameObject _timeline;
        PlayableDirector _director;
        CharacterActor[] _characters;
        int[] positions;
        double _lastClipStart = 0.0;
        double _lastClipEnd = 0.0;

        public GameObject Timeline => _timeline;
        public GameObject Stage => _stage;

        public async Task SetBattleDance(string danceKey, string[] actorKeys)
        {
            var danceSetting = DanceStore.Instance.GetDance(danceKey);
            if (danceSetting.CharacterCount > actorKeys.Length)
            {
                Debug.LogError("Create BattleDance failed, character count less than " + danceSetting.CharacterCount);
                return;
            }
            positions = danceSetting.CharacterCount switch
            {
                2 => new int[] { 1, 2 },
                3 => new int[] { 1, 2, 3 },
                4 => new int[] { 0, 1, 2, 3 },
                5 => new int[] { 0, 1, 2, 3, 4 },
                _ => new int[] { 0 },
            };

            LoadTimeline(danceSetting);

            LoadStage(danceSetting.StageKey);
            BindingCharacters(danceSetting, actorKeys);

            await BindingAudio(danceSetting);
            BindingCamare();

            SetClipTimes();
            SolveBugs(danceSetting);

            Ready = true;
        }
        public void Clear()
        {
            GameObject.Destroy(_stage);
            GameObject.Destroy(_timeline);

            _characters = null;
            _director = null;

            Ready = false;
        }


        private void LoadTimeline(DanceSetting setting)
        {
            GameObject timelinePrefab = AssetBundleLoader.Instance.LoadAsset<GameObject>($"battle/song/{setting.ID}/CM Timeline");
            var timeline = GameObject.Instantiate(timelinePrefab);

            _timeline = timeline;
            _director = _timeline.GetComponent<PlayableDirector>();
            _director.extrapolationMode = DirectorWrapMode.Hold;

            var lysTexts = _director.GetComponentsInChildren<Text>(true);
            var runner = GameObject.Find("Launcher");
            var runnerText = runner.GetComponent<Text>();
            foreach (var text in lysTexts)
            {
                text.font = runnerText.font;
            }
        }
        private void LoadStage(string stageId)
        {
            var prefab = StageStore.Instance.LoadStage(stageId);
            if (_stage)
            {
                GameObject.Destroy(_stage);
            }

            _stage = GameObject.Instantiate(prefab);
        }

        private async Task BindingAudio(DanceSetting setting)
        {
            var audioClip = await SoundBankLoader.Instance.LoadAudioClip(setting.ID, setting.Music);
            var audioTrack = FindRootTrack<AudioTrack>();

            foreach (var clip in audioTrack.GetClips())
            {
                (clip.asset as AudioPlayableAsset).clip = audioClip;
            }
        }
        private void BindingCamare()
        {
            var camera = GameObject.Find("Main Camera");
            var brain = camera.GetComponent<CinemachineBrain>();

            var timelineAsset = _director.playableAsset as TimelineAsset;
            var cinemachine = timelineAsset.GetRootTrack(0);
            foreach (var track in cinemachine.GetChildTracks())
            {
                if (track is CinemachineTrack cineTrack)
                {
                    _director.SetGenericBinding(cineTrack, brain);
                }
            }
        }
        private void BindingCharacters(DanceSetting setting, string[] actorKeys)
        {
            _characters = new CharacterActor[5];
            for (int i = 0; i < positions.Length; i++)
            {
                var pos = positions[i];
                var posStr = (pos + 1).ToString();
                var chara = CharactersStore.Instance.LoadActor(actorKeys[i]);

                var posObj = Utility.FindNodeByRecursion(_stage, "TopLight " + posStr);
                Utility.ActiveToTop(posObj);

                chara.Root.transform.SetParent(posObj.transform);
                chara.Root.transform.localPosition = Vector3.zero;
                chara.Root.name = "Actor " + posStr;
                chara.SkeletonRoot.name = "Model " + posStr;

                chara.Root.AddComponent<Actor>().BindPosition(pos);

                chara.SetPlaying(true);
                _characters[pos] = chara;
            }
        }

        private T FindRootTrack<T>() where T : TrackAsset
        {
            var timelineAsset = _director.playableAsset as TimelineAsset;
            foreach (var track in timelineAsset.GetRootTracks())
            {
                if (track is T tTrack)
                {
                    return tTrack;
                }
            }
            return null;
        }
        private T FindRootTrack<T>(string name) where T : TrackAsset
        {
            var timelineAsset = _director.playableAsset as TimelineAsset;
            foreach (var track in timelineAsset.GetRootTracks())
            {
                if (track is T tTrack && tTrack.name == name)
                {
                    return tTrack;
                }
            }
            return null;
        }
        private T FindTrack<T>(GroupTrack group, string name) where T : TrackAsset
        {
            foreach (var track in group.GetChildTracks())
            {
                if (track is T tTrack && tTrack.name == name)
                {
                    return tTrack;
                }
            }
            return null;
        }

        public CharacterActor GetCharacter(int characterPos)
        {
            return _characters[characterPos];
        }
        public CharacterActor[] GetActiveCharacters()
        {
            CharacterActor[] res = new CharacterActor[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                res[i] = _characters[positions[i]];
            }
            return res;
        }
        public bool IsARMode { get => false; }

        private void SetClipTimes()
        {
            _lastClipStart = 0;
            _lastClipEnd = 0;
            var charGoup = FindRootTrack<GroupTrack>("Char Animation Group");
            foreach (var track in charGoup.GetChildTracks())
            {
                foreach (var clip in track.GetClips())
                {
                    if (clip.end > _lastClipEnd)
                    {
                        _lastClipStart = clip.start;
                        _lastClipEnd = clip.end;
                    }
                }
            }
        }

        public void Play()
        {
            if (_state == DanceState.Play) return;

            Time.timeScale = 1;
            _director.Play();
            _state = DanceState.Play;
        }
        public void Stop()
        {
            if (_state == DanceState.Stop) return;

            Time.timeScale = 1;
            _director.Stop();
            _director.time = 0;
            _state = DanceState.Stop;
        }
        public void Pause()
        {
            if (_state == DanceState.Play)
            {
                Time.timeScale = 0;
                _state = DanceState.Pause;
            }
            else if (_state == DanceState.Pause)
            {
                Time.timeScale = 1;
                _state = DanceState.Play;
            }
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
                }
            }
        }



        // 某些问题找不出原因,特殊处理
        private void SolveBugs(DanceSetting danceSetting)
        {
            if (danceSetting.StageKey == "BG2004")
            {
                var toplight = Utility.FindNodeByRecursion(_stage, "TopLight");
                toplight.transform.localPosition = Vector3.zero;
            }
            else if (danceSetting.StageKey == "BG307")
            {
                var toplight = Utility.FindNodeByRecursion(_stage, "TopLight");
                toplight.transform.localPosition = new Vector3(0, 1.072f, 0);
            }
            
            if (danceSetting.ID == "MUSIC_0028")
            {
                _lastClipStart = 0;
                _lastClipEnd = _director.duration;

                var charGoup = FindRootTrack<GroupTrack>("Char Animation Group");
                foreach (var track in charGoup.GetChildTracks())
                {
                    foreach (var clip in track.GetClips())
                    {
                        clip.duration = clip.animationClip.averageDuration;
                        if (clip.end > _lastClipEnd) _lastClipEnd = clip.end;
                    }
                }
            }
        }
    }
}
