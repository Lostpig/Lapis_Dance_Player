
using UnityEngine;

namespace LapisPlayer
{
    public class Launcher : MonoBehaviour
    {
        DanceManager _danceManager;
        StageManager _stageManager;

        private async void Start()
        {
            var defaultDance = DanceStore.Instance.GetDance("MUSIC_0001");
            _danceManager = new();
            await _danceManager.InitializeDance(defaultDance);

            var camare = GameObject.Find("Main Camera");
            var camCtrl = camare.AddComponent<CameraController>();
            camCtrl.Initialize(_danceManager.Root.transform);

            _stageManager = new();

            var uiManager = new UIManager();
            uiManager.Initialize();

            uiManager.OnDancePlay += UiManager_OnDancePlay;
            uiManager.OnActorChange += UiManager_OnActorChange;
            uiManager.OnActorRemove += UiManager_OnActorRemove;
            uiManager.OnDanceChange += UiManager_OnDanceChange;
            uiManager.OnStageChange += UiManager_OnStageChange;

            uiManager.DanceChangeSuccess(defaultDance);
        }

        private void UiManager_OnStageChange(string stage, UIManager sender)
        {
            _stageManager.LoadStage(stage);
        }

        private async void UiManager_OnDanceChange(DanceSetting dance, UIManager sender)
        {
            await _danceManager.InitializeDance(dance);
            sender.DanceChangeSuccess(dance);
        }

        private void UiManager_OnActorRemove(int characterPos, UIManager sender)
        {
            _danceManager.RemoveCharacter(characterPos);
            sender.RemoveActorSuccess(characterPos);
        }

        private void UiManager_OnActorChange(int characterPos, CharacterSetting charaSetting, ActorSetting actorSetting, UIManager sender)
        {
            var character = CharactersStore.Instance.LoadActor(charaSetting.Name, actorSetting.Name);
            _danceManager.SetCharacter(characterPos, character);
            _danceManager.SetCharacterPosition();

            sender.ChangeActorSuccess(characterPos, charaSetting, actorSetting);
        }

        private void UiManager_OnDancePlay(bool play, UIManager sender)
        {
            if (play)
            {
                _danceManager.Play();
            }
            else
            {
                _danceManager.Stop();
            }

            sender.DancePlayingChangeSuccess(play);
        }

        private void Update()
        {
            _danceManager.Update();
        }
    }
}
