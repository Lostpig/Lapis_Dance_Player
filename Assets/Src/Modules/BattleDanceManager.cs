using Cinemachine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
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
        public double _lastClipStart = 0.0;
        public double _lastClipEnd = 0.0;

        public GameObject Timeline => _timeline;
        public GameObject Stage => _stage;

        DanceSetting _danceSetting;
        StageData _stageSetting;

        public async Task SetBattleDance(string danceKey, string[] actorKeys)
        {
            _danceSetting = DanceStore.Instance.GetDance(danceKey);
            if (_danceSetting.CharacterCount > actorKeys.Length)
            {
                Debug.LogError("Create BattleDance failed, character count less than " + _danceSetting.CharacterCount);
                return;
            }
            positions = _danceSetting.CharacterCount switch
            {
                2 => new int[] { 1, 2 },
                3 => new int[] { 1, 2, 3 },
                4 => new int[] { 0, 1, 2, 3 },
                5 => new int[] { 0, 1, 2, 3, 4 },
                _ => new int[] { 0 },
            };

            LoadTimeline();

            LoadStage();
            BindingCharacters(actorKeys);

            await BindingAudio();
            BindingCamare();

            SetClipTimes();
            // SolveBugs(danceSetting);

            _director.time = 0.005;
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

        private void LoadTimeline()
        {
            GameObject timelinePrefab = AssetBundleLoader.Instance.LoadAsset<GameObject>($"battle/song/{_danceSetting.ID}/CM Timeline");
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

            var refObjs = _timeline.GetComponentsInChildren<ReferenceObject>(true);
            foreach(var refObj in refObjs)
            {
                ReferenceStore.Instance.AddReference(refObj);
            }
        }
        private void LoadStage()
        {
            string stageId = _danceSetting.StageKey;

            _stageSetting = StageStore.Instance.GetStage(stageId);
            var prefab = StageStore.Instance.LoadStage(stageId);
            if (_stage)
            {
                GameObject.Destroy(_stage);
            }

            _stage = GameObject.Instantiate(prefab);

            var refObjs = _stage.GetComponentsInChildren<ReferenceObject>(true);
            foreach (var refObj in refObjs)
            {
                ReferenceStore.Instance.AddReference(refObj);
            }
        }

        private async Task BindingAudio()
        {
            var audioClip = await SoundBankLoader.Instance.LoadAudioClip(_danceSetting.ID, _danceSetting.Music);
            var audioTrack = FindTracks<AudioTrack>()[0];

            if (audioTrack == null)
            {
                var ta = (_director.playableAsset as TimelineAsset);
                audioTrack = ta.CreateTrack<AudioTrack>();
                audioTrack.CreateClip<AudioPlayableAsset>();
            }

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
        private void BindingCharacters(string[] actorKeys)
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
                SetByStage(chara.Root);

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
        private T[] FindTracks<T>() where T : TrackAsset
        {
            List<T> tracks = new();

            var timelineAsset = _director.playableAsset as TimelineAsset;
            foreach (var track in timelineAsset.GetRootTracks())
            {
                if (track is GroupTrack gt)
                {
                    foreach (var childTrack in track.GetChildTracks())
                    {
                        if (childTrack is T tt)
                        {
                            tracks.Add(tt);
                        }
                    }
                }
                else if (track is T tTrack)
                {
                    tracks.Add(tTrack);
                }
            }
            return tracks.ToArray();
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

            Debug.Log("Set dance track loop start: " + _lastClipStart + ", end: " + _lastClipEnd);
        }
        public void SetLoopTime (double start, double end)
        {
            _lastClipStart = start;
            _lastClipEnd = end;
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


        private void SetByStage(GameObject charaGo)
        {
            if (_stageSetting.ID == "BG2004")
            {
                charaGo.transform.localPosition = new Vector3(0, 0, 7.5f);
                charaGo.transform.localRotation = Quaternion.Euler(new Vector3(-90, 90, -90));
            }
            else if (_stageSetting.ID == "BG307")
            {
                charaGo.transform.localPosition = new Vector3(0, 1.055f, 0);
                charaGo.transform.localRotation = Quaternion.identity;
            }
            else
            {
                charaGo.transform.localPosition = Vector3.zero;
                charaGo.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
