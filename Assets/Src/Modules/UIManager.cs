using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace LapisPlayer
{
    public class UIManager
    {
        GameObject _uiCanvas;
        GameObject _baseContainer;
        GameObject _buttonsContainer;
        GameObject _panelsContainer;
        GameObject _charaListView;
        GameObject _actorListView;
        GameObject _danceListView;
        GameObject _stageListView;
        GameObject _poseListView;
        GameObject _expressionListView;
        GameObject _danceText;
        GameObject _btnPlay;
        GameObject _btnStop;
        GameObject _btnPause;

        int _activeCharacterPos = -1;
        int _activePosePos = -1;
        string _selectedCharacterName = "";
        List<(CharacterSetting, ActorSetting)> actorList = new();
        (eFaceExpression, MouthState)[] _faceExpressions;

        public event Action<DanceState, UIManager> OnDancePlay;
        public event Action<int, CharacterSetting, ActorSetting, UIManager> OnActorChange;
        public event Action<int, UIManager> OnActorRemove;
        public event Action<DanceSetting, UIManager> OnDanceChange;
        public event Action<StageData, UIManager> OnStageChange;
        public event Action<int, string, UIManager> OnPoseChange;
        public event Action<int, UIManager> OnLoadExpression;
        public event Action<int, eFaceExpression, MouthState, UIManager> OnExpressionChange;
        public event Action<UIManager> OnBack;

        public UnityAction BindIndexAction(int index, Action<int> action)
        {
            return () => action(index);
        }

        public void Initialize()
        {
            _uiCanvas = GameObject.Find("UICanvas");
            _baseContainer = Utility.FindObject(_uiCanvas, "BaseContainer");
            _buttonsContainer = Utility.FindObject(_baseContainer, "Buttons");
            _panelsContainer = Utility.FindObject(_baseContainer, "Panels");

            _btnPlay = Utility.FindObject(_buttonsContainer, "BtnPlay");
            _btnStop = Utility.FindObject(_uiCanvas, "BtnStop");
            _btnPause = Utility.FindObject(_uiCanvas, "BtnPause");

            _btnPlay.GetComponent<Button>().onClick.AddListener(StartDance);
            _btnStop.GetComponent<Button>().onClick.AddListener(StopDance);
            _btnPause.GetComponent<Button>().onClick.AddListener(PauseDance);

            InitializeCharaterPositions();
            InitCharaListView();
            InitDanceListView();
            InitStageListView();
            InitPoseListView();
            InitExpressionView();
            InitBackButton();
        }
        private void InitializeCharaterPositions()
        {
            for (int i = 0; i < 5; i++)
            {
                var btn = Utility.FindNodeByName(_buttonsContainer, $"Character{i}");
                var mbtn = btn.GetComponent<Button>();
                mbtn.onClick.AddListener(BindIndexAction(i, ShowCharaListView));
            }
        }
        private void InitCharaListView()
        {
            _charaListView = Utility.FindObject(_panelsContainer, "CharaListView");
            _actorListView = Utility.FindObject(_panelsContainer, "ActorListView");

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
            _danceListView = Utility.FindObject(_panelsContainer, "DanceListView");
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

            var btnDance = Utility.FindNodeByName(_buttonsContainer, "BtnDance");
            btnDance.GetComponent<Button>().onClick.AddListener(ToggleDanceListView);
        }
        private void InitStageListView()
        {
            _stageListView = Utility.FindObject(_panelsContainer, "StageListView");
            if (_stageListView == null) return;

            var stages = StageStore.Instance.GetAllStages();
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

            var btnStage = Utility.FindNodeByName(_buttonsContainer, "BtnStage");
            btnStage.GetComponent<Button>().onClick.AddListener(ToggleStageListView);
        }
        private void InitPoseListView()
        {
            _poseListView = Utility.FindObject(_panelsContainer, "PoseListView");
            if (_poseListView == null) return;

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
                var btnPose = Utility.FindObject(_buttonsContainer, "Pose" + i);
                btnPose.GetComponent<Button>().onClick.AddListener(BindIndexAction(i, TogglePoseListView));
            }
        }
        private void InitExpressionView()
        {
            _expressionListView = Utility.FindObject(_panelsContainer, "ExpressionListView");
            if (_expressionListView == null) return;

            for (int i = 0; i < 5; i++)
            {
                var button = DefaultControls.CreateButton(new DefaultControls.Resources());
                button.name = "Expression" + i;
                button.SetActive(false);
                button.transform.SetParent(_buttonsContainer.transform);

                var rect = button.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(70, 30);
                rect.anchoredPosition = new Vector2(80, -(i * 90 + 50));

                var text = button.GetComponentInChildren<Text>();
                text.text = "表情";
                button.GetComponent<Button>().onClick.AddListener(BindIndexAction(i, ToggleExpressionListView));
            }
        }
        private void InitBackButton()
        {
            var btnExit = Utility.FindObject(_buttonsContainer, "BtnBack");
            btnExit.GetComponent<Button>().onClick.AddListener(TriggerBack);
        }

        private void SelectCharacter(CharacterSetting character)
        {
            if (_selectedCharacterName != character.Name)
            {
                var actors = CharactersStore.Instance.GetActors(character.Name);
                int height = actors.Length * 40 + 20;

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
                _expressionListView.SetActive(false);

                var rect = _poseListView.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -(90 * pos + 10));
            }
        }
        private void ToggleExpressionListView(int pos)
        {
            var visible = _expressionListView.activeSelf;
            if (visible && pos == _activePosePos)
            {
                _activePosePos = -1;
                _expressionListView.SetActive(false);
            }
            else
            {
                _activePosePos = pos;
                OnLoadExpression?.Invoke(pos, this);

                _expressionListView.SetActive(true);
                _poseListView.SetActive(false);

                var rect = _expressionListView.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -(90 * pos + 30));
            }
        }
        public void SetExpressionList(eFaceExpression[] expressions)
        {
            var content = Utility.FindNodeByRecursion(_expressionListView, "Content");
            var contentRect = content.GetComponent<RectTransform>();

            int btnCount = expressions.Length * 3;
            int height = btnCount * 40 + 20;
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, height);

            if (btnCount > content.transform.childCount)
            {
                for (int i = content.transform.childCount; i < btnCount; i++)
                {
                    var button = DefaultControls.CreateButton(new DefaultControls.Resources());
                    button.transform.SetParent(content.transform);

                    var rect = button.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(200, 30);
                    rect.pivot = new Vector2(0, 1);
                    rect.anchorMin = new Vector2(0, 1);
                    rect.anchorMax = new Vector2(0, 1);

                    int left = 10;
                    float top = i * 40 + 20;
                    rect.anchoredPosition = new Vector2(left, -top);

                    button.GetComponent<Button>().onClick.AddListener(BindIndexAction(i, ChangeExpression));
                }
            }

            _faceExpressions = new (eFaceExpression, MouthState)[btnCount];
            MouthState[] mouthStates = new MouthState[] { MouthState.OPEN, MouthState.CLOSE, MouthState.HALFCLOSE };
            int idx = 0;
            for (int i = 0; i < expressions.Length; i++)
            {
                for (int j = 0; j < mouthStates.Length; j++)
                {
                    var button = content.transform.GetChild(idx).gameObject;
                    button.SetActive(true);
                    var text = button.GetComponentInChildren<Text>();
                    var expName = Enum.GetName(typeof(eFaceExpression), expressions[i]);
                    var mouthName = Enum.GetName(typeof(MouthState), mouthStates[j]);
                    text.text = $"{expName} ({mouthName})";

                    _faceExpressions[idx] = (expressions[i], mouthStates[j]);
                    idx++;
                }
            }

            for (int i = idx; i < content.transform.childCount; i++)
            {
                content.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public void ChangeActorSuccess(int pos, CharacterSetting character, ActorSetting actor)
        {
            var btn = Utility.FindNodeByName(_buttonsContainer, $"Character{pos}");
            var image = Utility.FindNodeByName(btn, "Image");
            image.GetComponent<Image>().sprite = UIUtility.LoadCharacterIcon(character.ShortName);

            var text = btn.GetComponentInChildren<Text>();
            text.text = actor.Name;

            var btnPose = Utility.FindObject(_buttonsContainer, "Pose" + pos);
            btnPose?.SetActive(true);

            var btnExpression = Utility.FindObject(_buttonsContainer, "Expression" + pos);
            btnExpression?.SetActive(true);
        }
        public void RemoveActorSuccess(int pos)
        {
            var btn = Utility.FindNodeByName(_buttonsContainer, $"Character{pos}");
            var image = Utility.FindNodeByName(btn, "Image");
            image.GetComponent<Image>().sprite = null;

            var text = btn.GetComponentInChildren<Text>();
            text.text = "Empty";

            var btnPose = Utility.FindObject(_buttonsContainer, "Pose" + pos);
            btnPose?.SetActive(false);

            var btnExpression = Utility.FindObject(_buttonsContainer, "Expression" + pos);
            btnExpression?.SetActive(false);
        }
        public void DanceChangeSuccess(DanceSetting dance)
        {
            _danceText.GetComponent<Text>().text = dance.Name;
        }
        public void DancePlayingChangeSuccess(DanceState state)
        {
            _baseContainer.SetActive(state == DanceState.Stop);

            _btnStop.SetActive(state != DanceState.Stop);
            _btnPause.SetActive(state != DanceState.Stop);
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

        private void ChangeExpression(int exporessionIndex)
        {
            var (expression, mouthState) = _faceExpressions[exporessionIndex];
            OnExpressionChange?.Invoke(_activePosePos, expression, mouthState, this);
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
            OnDancePlay?.Invoke(DanceState.Play, this);
        }
        private void StopDance()
        {
            OnDancePlay?.Invoke(DanceState.Stop, this);
        }
        private void PauseDance()
        {
            OnDancePlay?.Invoke(DanceState.Pause, this);
        }

        private void TriggerBack()
        {
            OnBack?.Invoke(this);
        }
    }
}
