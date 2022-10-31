
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

            var actor1 = CharactersStore.Instance.LoadActor("Nadeshiko", "UR002");
            var actor2 = CharactersStore.Instance.LoadActor("Tsubaki", "UR002");
            var actor3 = CharactersStore.Instance.LoadActor("Kaede", "UR002");
            _danceManager.AddCharacter(actor1);
            _danceManager.AddCharacter(actor2);
            _danceManager.AddCharacter(actor3);

            _danceManager.SetCharacterPosition();
        }

        private void Update()
        {
            _danceManager.Update();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Start"))
            {
                _danceManager.Play();
            }

            if (GUILayout.Button("Stop"))
            {
                _danceManager.Stop();
            }
        }
    }
}
