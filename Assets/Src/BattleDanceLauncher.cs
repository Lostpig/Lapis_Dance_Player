using Oz.Timeline;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LapisPlayer
{
    public class BattleDanceLauncher : MonoBehaviour
    {
        string[] characters;
        string dance;
        BattleDanceManager _battleDanceManager;

#if UNITY_EDITOR
        public double loopStart = 0;
        public double loopEnd = 0;
#endif
        private void Start()
        {
            characters = new string[5];

            PreStart();

            _battleDanceManager = new();
            PlayerGlobal.Instance.SetSingleton<IDanceManager>(_battleDanceManager);

            var uiManager = new UIManager();
            uiManager.Initialize();

            uiManager.OnDancePlay += UiManager_OnDancePlay;
            uiManager.OnActorChange += UiManager_OnActorChange;
            uiManager.OnActorRemove += UiManager_OnActorRemove;
            uiManager.OnDanceChange += UiManager_OnDanceChange;
            uiManager.OnBack += UiManager_OnBack;

            var defaultDance = DanceStore.Instance.GetDance("MUSIC_0001");
            dance = defaultDance.ID;
            uiManager.DanceChangeSuccess(defaultDance);

#if UNITY_EDITOR
            var chs = CharactersStore.Instance.GetCharacters();
            UiManager_OnActorChange(0, chs[0], chs[0].actors[0], uiManager);
            UiManager_OnActorChange(1, chs[0], chs[0].actors[0], uiManager);
            UiManager_OnActorChange(2, chs[0], chs[0].actors[0], uiManager);
            UiManager_OnActorChange(3, chs[0], chs[0].actors[0], uiManager);
            UiManager_OnActorChange(4, chs[0], chs[0].actors[0], uiManager);

            var camare = GameObject.Find("DevCamera");
            var testObj = GameObject.Find("TestObj");
            var camCtrl = camare.AddComponent<CameraController>();
            camCtrl.Initialize(testObj.transform);
#endif
        }

        private void UiManager_OnBack(UIManager obj)
        {
            SceneManager.LoadScene("MainScene");
            SceneManager.UnloadSceneAsync("BattleDanceScene");
        }

        private void PreStart()
        {
            GameObject camera = GameObject.Find("Main Camera");
            var camObj = camera.AddComponent<ReferenceObject>();
            camObj.Data = new();
            camObj.Data.GUID = "f0791cab-b453-499e-9aed-f5641c4e8b9c";  // 主摄像机GUID固定这个,直接写死了

            ReferenceStore.Instance.AddReference(camObj);
        }
        private void ApplyLookats()
        {
            var virtualLookats = _battleDanceManager.Timeline.GetComponentsInChildren<VirtualCameraFollowAndLookAt>();
            foreach (var vl in virtualLookats)
            {
                vl.ApplyLookAt(vl.VirtualCamera);
            }
        }

        private void UiManager_OnDanceChange(DanceSetting dance, UIManager sender)
        {
            this.dance = dance.ID;
            sender.DanceChangeSuccess(dance);
        }

        private void UiManager_OnActorRemove(int characterPos, UIManager sender)
        {
            characters[characterPos] = null;
            sender.RemoveActorSuccess(characterPos);
        }

        private void UiManager_OnActorChange(int characterPos, CharacterSetting charaSetting, ActorSetting actorSetting, UIManager sender)
        {
            characters[characterPos] = charaSetting.Name + "/" + actorSetting.Name;
            sender.ChangeActorSuccess(characterPos, charaSetting, actorSetting);
        }

        private async void UiManager_OnDancePlay(DanceState state, UIManager sender)
        {
            if (state == DanceState.Play)
            {
                var activeChara = characters.Where(c => c != null).ToArray();
                await _battleDanceManager.SetBattleDance(dance, activeChara);

                if (_battleDanceManager.Ready)
                {
                    ApplyLookats();
                    sender.DancePlayingChangeSuccess(state);

                    // 载入资源耗时可能导致音画不同步,载入后延迟开始播放可缓解该问题
                    StartCoroutine(DelayStartPlay(1));
                }
            }
            else if (state == DanceState.Stop)
            {
                _battleDanceManager.Stop();
                _battleDanceManager.Clear();
                sender.DancePlayingChangeSuccess(state);
            }
            else
            {
                if (_battleDanceManager.Ready)
                {
                    _battleDanceManager.Pause();
                    sender.DancePlayingChangeSuccess(state);
                }
            }
        }

        private IEnumerator DelayStartPlay(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            _battleDanceManager.Play();
        }

        private void Update()
        {
            _battleDanceManager.Update();
        }

#if UNITY_EDITOR
        [ContextMenu("Set Loop Time")]
        public void LoadBattleSong()
        {
            _battleDanceManager.SetLoopTime(loopStart, loopEnd);
        }
#endif
    }
}
