using System;
using UnityEngine;
using UnityEngine.UI;

namespace LapisPlayer
{
    public enum UIActions
    {
        ActorChange,
        PlayDance,
        StopDance
    }

    public class UIManager
    {
        GameObject _uiCanvas;
        GameObject _btnChara;
        GameObject _viewChara;

        public event Action<UIActions, string> OnUiAction;

        public void Initialize()
        {
            _uiCanvas = GameObject.Find("UICanvas");

            _btnChara = Utility.FindNodeByName(_uiCanvas, "BtnChara");
            _viewChara = Utility.FindObject(_uiCanvas, "ViewChara");

            var charaButton = _btnChara.GetComponent<Button>();
            charaButton.onClick.AddListener(ToggleCharaView);

            var btnDanceStart = Utility.FindNodeByName(_uiCanvas, "BtnDanceStart");
            var btnDanceStop = Utility.FindNodeByName(_uiCanvas, "BtnDanceStop");
            btnDanceStart.GetComponent<Button>().onClick.AddListener(StartDance);
            btnDanceStop.GetComponent<Button>().onClick.AddListener(StopDance);
        }

        bool _charaViewInited = false;
        private void InitCharaView()
        {
            string[] actors = CharactersStore.Instance.GetActors();
            int height = actors.Length * 40 + 20;

            var content = Utility.FindNodeByRecursion(_viewChara, "Content");
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, height);

            int index = 0;
            foreach (var actor in actors)
            {
                var charaBtn = DefaultControls.CreateButton(new DefaultControls.Resources());
                charaBtn.transform.SetParent(content.transform);
                var text = charaBtn.GetComponentInChildren<Text>();
                text.text = actor;
                var rect = charaBtn.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(150, 30);
                rect.pivot = new Vector2(0.5f, 1);
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.anchoredPosition = new Vector2(0, -(index * 40 + 10));

                charaBtn.GetComponent<Button>().onClick.AddListener(() => ChangeActor(actor));
                index++;
            }

            _charaViewInited = true;
        }

        private void ChangeActor (string actorKey)
        {
            OnUiAction?.Invoke(UIActions.ActorChange, actorKey);
        }
        private void ToggleCharaView()
        {
            _viewChara.SetActive(!_viewChara.activeSelf);

            if (!_charaViewInited)
            {
                InitCharaView();
            }
        }
        private void StartDance()
        {
            OnUiAction?.Invoke(UIActions.PlayDance, null);
        }
        private void StopDance()
        {
            OnUiAction?.Invoke(UIActions.StopDance, null);
        }
    }
}
