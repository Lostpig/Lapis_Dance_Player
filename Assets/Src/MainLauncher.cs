using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LapisPlayer
{
    public class MainLauncher : MonoBehaviour
    {
        MainSceneManager mainScene;
        GameObject bgmListView;
        AudioSource bgmSource;

        private void Start()
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;

            InitButtons();
            InitDanceListView();

            mainScene = new();
            mainScene.Start();
            PlayBGM(MainSceneConfig.Instance.Bgms[0]);
        }

        private void InitButtons()
        {
            var uiCanvas = GameObject.Find("UICanvas");

            var btnDance = Utility.FindObject(uiCanvas, "BtnDance");
            var btnBattleDance = Utility.FindObject(uiCanvas, "BtnBattleDance");
            var btnExit = Utility.FindObject(uiCanvas, "BtnExit");

            btnDance.GetComponent<Button>().onClick.AddListener(ToDance);
            btnBattleDance.GetComponent<Button>().onClick.AddListener(ToBattleDance);
            btnExit.GetComponent<Button>().onClick.AddListener(ToExit);
        }
        private void ToDance()
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

        private void InitDanceListView()
        {
            var uiCanvas = GameObject.Find("UICanvas");
            bgmListView = Utility.FindObject(uiCanvas, "BGMListView");

            var bgms = MainSceneConfig.Instance.Bgms;
            int height = bgms.Length * 40 + 20;

            var content = Utility.FindNodeByRecursion(bgmListView, "Content");
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, height);

            int index = 0;
            foreach (var bgm in bgms)
            {
                string bgmName = Path.GetFileName(bgm);
                var btn = DefaultControls.CreateButton(new DefaultControls.Resources());
                btn.transform.SetParent(content.transform);

                var rect = btn.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(200, 30);
                rect.pivot = new Vector2(0, 1);
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);

                int left = 10;
                float top = index * 40 + 20;
                rect.anchoredPosition = new Vector2(left, -top);

                btn.GetComponentInChildren<Text>().text = bgmName;
                btn.GetComponent<Button>().onClick.AddListener(() => PlayBGM(bgm));

                index++;
            }

            var btnDance = Utility.FindNodeByName(uiCanvas, "BtnBGM");
            btnDance.GetComponent<Button>().onClick.AddListener(ToggleBGMListView);
        }
        private void ToggleBGMListView()
        {
            bgmListView.SetActive(!bgmListView.activeSelf);
        }
        private void PlayBGM(string bgmPath)
        {
            StartCoroutine(SoundBankLoader.Instance.PlayAudioClip(bgmPath, AudioType.OGGVORBIS, bgmSource));
            bgmListView.SetActive(false);
        }
    }
}
