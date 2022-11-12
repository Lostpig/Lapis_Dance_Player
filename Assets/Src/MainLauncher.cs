using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LapisPlayer
{
    public class MainLauncher : MonoBehaviour
    {
        private void Start()
        {
            QualitySettings.SetQualityLevel(ConfigManager.Instance.QualityLevel);

            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = ConfigManager.Instance.FullScreen;

            InitButtons();
        }

        private void InitButtons ()
        {
            var uiCanvas = GameObject.Find("UICanvas");

            var btnDance = Utility.FindObject(uiCanvas, "BtnDance");
            var btnBattleDance = Utility.FindObject(uiCanvas, "BtnBattleDance");
            var btnExit = Utility.FindObject(uiCanvas, "BtnExit");

            btnDance.GetComponent<Button>().onClick.AddListener(ToDance);
            btnBattleDance.GetComponent<Button>().onClick.AddListener(ToBattleDance);
            btnExit.GetComponent<Button>().onClick.AddListener(ToExit);
        }
        private void ToDance ()
        {
            SceneManager.LoadScene("DanceScene");
            SceneManager.UnloadSceneAsync("MainScene");
        }
        private void ToBattleDance()
        {
            SceneManager.LoadScene("BattleDanceScene");
            SceneManager.UnloadSceneAsync("MainScene");
        }
        private void ToExit()
        {
            Application.Quit();
        }
    }
}
