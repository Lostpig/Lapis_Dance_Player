using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LapisPlayer
{
    public class UIManager
    {
        GameObject _uiCanvas;
        GameObject _baseContainer;
        GameObject _charaListView;
        GameObject _actorListView;
        GameObject _danceListView;
        GameObject _stageListView;
        GameObject _poseListView;
        GameObject _danceText;
        GameObject _btnPlay;
        GameObject _btnStop;

        int _activeCharacterPos = -1;
        int _activePosePos = -1;
        string _selectedCharacterName = "";
        List<(CharacterSetting, ActorSetting)> actorList = new();

        public event Action<bool, UIManager> OnDancePlay;
        public event Action<int, CharacterSetting, ActorSetting, UIManager> OnActorChange;
        public event Action<int, UIManager> OnActorRemove;
        public event Action<DanceSetting, UIManager> OnDanceChange;
        public event Action<StageData, UIManager> OnStageChange;
        public event Action<int, string, UIManager> OnPoseChange;

        public UnityAction BindIndexAction(int index, Action<int> action)
        {
            return () => action(index);
        }

        public void Initialize()
        {
            _uiCanvas = GameObject.Find("UICanvas");
            _baseContainer = Utility.FindObject(_uiCanvas, "BaseContainer");

            _btnPlay = Utility.FindObject(_baseContainer, "BtnPlay");
            _btnStop = Utility.FindObject(_uiCanvas, "BtnStop");
            _btnPlay.GetComponent<Button>().onClick.AddListener(StartDance);
            _btnStop.GetComponent<Button>().onClick.AddListener(StopDance);

            InitializeCharaterPositions();
            InitCharaListView();
            InitDanceListView();
            InitStageListView();
            InitPoseListView();
        }
        private void InitializeCharaterPositions()
        {
            for (int i = 0; i < 5; i++)
            {
                var btn = Utility.FindNodeByName(_baseContainer, $"Character{i}");
                var mbtn = btn.GetComponent<Button>();
                mbtn.onClick.AddListener(BindIndexAction(i, ShowCharaListView));
            }
        }
        private void InitCharaListView()
        {
            _charaListView = Utility.FindObject(_baseContainer, "CharaListView");
            _actorListView = Utility.FindObject(_baseContainer, "ActorListView");

            var characters = CharactersStore.Instance.GetCharacters();
            int height = (int)Math.Ceiling(characters.Length / 2.0) * 80 + 20;

            var backBtn = Utility.FindNodeByRecursion(_charaListView, "CharaBack");
            backBtn.GetComponent<Button>().onClick.AddListener(HideCharaListView);

            var removeBtn = Utility.FindNodeByRecursion(_charaListView, "CharaRemove");
            removeBtn.GetComponent<Button>().onClick.AddListener(RemoveActor);

            var content = Utility.FindNodeByRecursion(_charaListView, "Content");
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, height);

            int index = 0;
            foreach (var character in characters)
            {
                var charaBtn = UIUtility.CreateCharacterButton(content, character.ShortName);
                var rect = charaBtn.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(60, 60);
                rect.pivot = new Vector2(0, 1);
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);

                int left = (index % 2) * 80 + 20;
                float top = (float)Math.Floor(index / 2.0) * 80 + 20;
                rect.anchoredPosition = new Vector2(left, -top);
                charaBtn.GetComponent<Button>().onClick.AddListener(() => SelectCharacter(character));
                index++;
            }

            var actorBackBtn = Utility.FindNodeByRecursion(_actorListView, "ActorBack");
            actorBackBtn.GetComponent<Button>().onClick.AddListener(HideActorListView);
        }
        private void InitDanceListView()
        {
            _danceListView = Utility.FindObject(_baseContainer, "DanceListView");
            _danceText = Utility.FindObject(_baseContainer, "DanceText");

            var dances = DanceStore.Instance.GetAllDance();
            int height = dances.Length * 40 + 20;

            var content = Utility.FindNodeByRecursion(_danceListView, "Content");
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, height);

            int index = 0;
            foreach (var dance in dances)
            {
                var danceBtn = DefaultControls.CreateButton(new DefaultControls.Resources());
                danceBtn.transform.SetParent(content.transform);

                var rect = danceBtn.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(330, 30);
                rect.pivot = new Vector2(0, 1);
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);

                int left = 10;
                float top = index * 40 + 20;
                rect.anchoredPosition = new Vector2(left, -top);

                danceBtn.GetComponentInChildren<Text>().text = dance.Name;
                danceBtn.GetComponent<Button>().onClick.AddListener(() => ChangeDance(dance));

                index++;
            }

            var btnDance = Utility.FindNodeByName(_baseContainer, "BtnDance");
            btnDance.GetComponent<Button>().onClick.AddListener(ToggleDanceListView);
        }
        private void InitStageListView()
        {
            _stageListView = Utility.FindObject(_baseContainer, "StageListView");

            var stages = StageManager.GetAllStages();
            int height = stages.Length * 40 + 20;

            var content = Utility.FindNodeByRecursion(_stageListView, "Content");
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, height);

            int index = 0;
            foreach (var stage in stages)
            {
                var danceBtn = DefaultControls.CreateButton(new DefaultControls.Resources());
                danceBtn.transform.SetParent(content.transform);

                var rect = danceBtn.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(190, 30);
                rect.pivot = new Vector2(0, 1);
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);

                int left = 10;
                float top = index * 40 + 20;
                rect.anchoredPosition = new Vector2(left, -top);

                danceBtn.GetComponentInChildren<Text>().text = stage.Name;
                danceBtn.GetComponent<Button>().onClick.AddListener(() => ChangeStagee(stage));

                index++;
            }

            var btnStage = Utility.FindNodeByName(_baseContainer, "BtnStage");
            btnStage.GetComponent<Button>().onClick.AddListener(ToggleStageListView);
        }
        private void InitPoseListView()
        {
            _poseListView = Utility.FindObject(_baseContainer, "PoseListView");
            var poses = PoseStore.Instance.GetAllPose();
            int height = poses.Length * 40 + 20;

            var content = Utility.FindNodeByRecursion(_poseListView, "Content");
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, height);

            int index = 0;
            foreach (var pose in poses)
            {
                var button = DefaultControls.CreateButton(new DefaultControls.Resources());
                button.transform.SetParent(content.transform);

                var rect = button.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(220, 30);
                rect.pivot = new Vector2(0, 1);
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);

                int left = 10;
                float top = index * 40 + 20;
                rect.anchoredPosition = new Vector2(left, -top);

                button.GetComponentInChildren<Text>().text = pose;
                button.GetComponent<Button>().onClick.AddListener(() => ChangePose(pose));

                index++;
            }

            for (int i = 0; i < 5; i++)
            {
                var btnPose = Utility.FindNodeByName(_baseContainer, "Pose" + i);
                btnPose.GetComponent<Button>().onClick.AddListener(BindIndexAction(i, TogglePoseListView));
            }
        }

        private void SelectCharacter(CharacterSetting character)
        {
            if (_selectedCharacterName != character.Name)
            {
                var actors = CharactersStore.Instance.GetActors(character.Name);
                int height = actors.Length * 30 + 20;

                var content = Utility.FindNodeByRecursion(_actorListView, "Content");
                // content.transform.DetachChildren();
                var contentRect = content.GetComponent<RectTransform>();
                contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, height);

                if (actors.Length > content.transform.childCount)
                {
                    for (int i = content.transform.childCount; i < actors.Length; i++)
                    {
                        var charaBtn = DefaultControls.CreateButton(new DefaultControls.Resources());
                        charaBtn.transform.SetParent(content.transform);

                        var rect = charaBtn.GetComponent<RectTransform>();
                        rect.sizeDelta = new Vector2(80, 30);
                        rect.pivot = new Vector2(0, 1);
                        rect.anchorMin = new Vector2(0, 1);
                        rect.anchorMax = new Vector2(0, 1);

                        int left = 10;
                        float top = i * 40 + 20;
                        rect.anchoredPosition = new Vector2(left, -top);

                        charaBtn.GetComponent<Button>().onClick.AddListener(BindIndexAction(i, ChangeActor));
                        actorList.Add((null, null));
                    }
                }

                int btnCount = content.transform.childCount;
                for (int i = 0; i < btnCount; i++)
                {
                    var btn = content.transform.GetChild(i).gameObject;
                    if (i >= actors.Length)
                    {
                        btn.SetActive(false);
                    }
                    else
                    {
                        btn.SetActive(true);
                        var text = btn.GetComponentInChildren<Text>();
                        text.text = actors[i].Name;

                        actorList[i] = (character, actors[i]);
                    }
                }
                _selectedCharacterName = character.Name;
            }

            ShowActorListView();
        }
        private void ShowActorListView()
        {
            _actorListView.SetActive(true);
        }
        private void HideActorListView()
        {
            _actorListView.SetActive(false);
        }
        private void ShowCharaListView(int pos)
        {
            _activeCharacterPos = pos;
            _charaListView.SetActive(true);
        }
        private void HideCharaListView()
        {
            _activeCharacterPos = -1;
            _charaListView.SetActive(false);
        }
        private void ToggleDanceListView()
        {
            _danceListView.SetActive(!_danceListView.activeSelf);
        }
        private void ToggleStageListView()
        {
            _stageListView.SetActive(!_stageListView.activeSelf);
        }
        private void TogglePoseListView(int pos)
        {
            var visible = _poseListView.activeSelf;
            if (visible && pos == _activePosePos)
            {
                _activePosePos = -1;
                _poseListView.SetActive(false);
            }
            else
            {
                _activePosePos = pos;
                _poseListView.SetActive(true);

                var rect = _poseListView.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -(90 * pos + 10));
            }
        }

        public void ChangeActorSuccess(int pos, CharacterSetting character, ActorSetting actor)
        {
            var btn = Utility.FindNodeByName(_baseContainer, $"Character{pos}");
            var image = Utility.FindNodeByName(btn, "Image");
            image.GetComponent<Image>().sprite = UIUtility.LoadCharacterIcon(character.ShortName);

            var text = btn.GetComponentInChildren<Text>();
            text.text = actor.Name;
        }
        public void RemoveActorSuccess(int pos)
        {
            var btn = Utility.FindNodeByName(_baseContainer, $"Character{pos}");
            var image = Utility.FindNodeByName(btn, "Image");
            image.GetComponent<Image>().sprite = null;

            var text = btn.GetComponentInChildren<Text>();
            text.text = "Empty";
        }
        public void DanceChangeSuccess(DanceSetting dance)
        {
            _danceText.GetComponent<Text>().text = dance.Name;
        }
        public void DancePlayingChangeSuccess(bool playing)
        {
            _baseContainer.SetActive(!playing);
            _btnStop.SetActive(playing);
        }

        private void ChangeActor(int index)
        {
            if (_activeCharacterPos < 0 || _activeCharacterPos > 4)
            {
                Debug.LogError("Select charater position is invalid, position = " + _activeCharacterPos);
                return;
            }

            var (character, actor) = actorList[index];
            OnActorChange?.Invoke(_activeCharacterPos, character, actor, this);
            HideActorListView();
            HideCharaListView();
        }
        private void RemoveActor()
        {
            if (_activeCharacterPos < 0 || _activeCharacterPos > 4)
            {
                Debug.LogError("Select charater position is invalid, position = " + _activeCharacterPos);
                return;
            }

            OnActorRemove?.Invoke(_activeCharacterPos, this);
            HideActorListView();
            HideCharaListView();
        }

        private void ChangePose(string pose)
        {
            OnPoseChange?.Invoke(_activePosePos, pose, this);
        }
        private void ChangeStagee(StageData stage)
        {
            OnStageChange?.Invoke(stage, this);
        }
        private void ChangeDance(DanceSetting dance)
        {
            OnDanceChange?.Invoke(dance, this);
        }
        private void StartDance()
        {
            OnDancePlay?.Invoke(true, this);
        }
        private void StopDance()
        {
            OnDancePlay?.Invoke(false, this);
        }
    }
}
