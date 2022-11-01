
using UnityEngine;

namespace LapisPlayer
{
    public class Launcher : MonoBehaviour
    {
        DanceManager _danceManager;

        private async void Start()
        {
            var dance = DanceStore.Instance.GetDance("MUSIC_0025");
            _danceManager = new(dance);
            await _danceManager.Initialize();

            var camare = GameObject.Find("Main Camera");
            var camCtrl = camare.AddComponent<CameraController>();
            camCtrl.Initialize(_danceManager.Root.transform);

            var uiManager = new UIManager();
            uiManager.Initialize();

            uiManager.OnUiAction += UiManager_OnAction;
        }

        private void UiManager_OnAction(UIActions action ,string param)
        {
            switch (action) {
                case UIActions.ActorChange:
                    ChangeActor(param); break;
                case UIActions.PlayDance:
                    _danceManager.Play(); break;
                case UIActions.StopDance:
                    _danceManager.Stop(); break;
                default:
                    break;
            }
        }

        private void ChangeActor (string actorKey)
        {
            _danceManager.ClearCharacters();
            var chara = CharactersStore.Instance.LoadActor(actorKey);
            _danceManager.AddCharacter(chara);
            _danceManager.SetCharacterPosition();
        }

        private void Update()
        {
            _danceManager.Update();
        }
    }
}
