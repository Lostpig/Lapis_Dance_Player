using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LapisPlayer
{
    public class BlendHandle
    {
        private SkinnedMeshRenderer _mesh;
        private int _shapeIndex;

        public BlendHandle(SkinnedMeshRenderer mesh, int shapeIndex)
        {
            _mesh = mesh;
            _shapeIndex = shapeIndex;
        }

        public void setValue(float value)
        {
            _mesh.SetBlendShapeWeight(_shapeIndex, value);
        }
        public float getValue()
        {
            return _mesh.GetBlendShapeWeight(_shapeIndex);
        }
    }

    public enum MouthState
    {
        OPEN, CLOSE, HALFCLOSE
    }

    public class FacialManager
    {
        private FacialSettings _settings;

        private SkinnedMeshRenderer _faceMesh;
        private SkinnedMeshRenderer _browMesh;
        private SkinnedMeshRenderer _eyeMesh;
        private Dictionary<string, BlendHandle> _blends;
        private Dictionary<eFaceExpression, FacialData> _facials;

        private MouthState _mouthstate;
        private eFaceExpression _expression;

        public FacialManager(FacialSettings settings, GameObject face)
        {
            _mouthstate = MouthState.CLOSE;
            _settings = settings;
            _blends = new();

            var faceGEO = Utility.FindNodeByRecursion(face, "face_GEO");
            _faceMesh = faceGEO.GetComponent<SkinnedMeshRenderer>();
            var eyeGeo = Utility.FindNodeByRecursion(face, "eye_GEO");
            _eyeMesh = eyeGeo.GetComponent<SkinnedMeshRenderer>();
            var browGEO = Utility.FindNodeByRecursion(face, "brow_GEO");
            _browMesh = browGEO.GetComponent<SkinnedMeshRenderer>();

            InitBlends(_faceMesh);
            InitBlends(_eyeMesh);
            InitBlends(_browMesh);

            InitExpression();
            InitializeAutoBlink();
        }
        private void InitBlends(SkinnedMeshRenderer mesh)
        {
            for (var i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
            {
                string name = mesh.sharedMesh.GetBlendShapeName(i);
                _blends.Add(name, new BlendHandle(mesh, i));
            }
        }
        private void InitExpression()
        {
            _facials = new();
            foreach (var facial in _settings.FacialData)
            {
                _facials.Add(facial.Expression, facial);
            }

            SetExpression(eFaceExpression.USUALLY);
        }
        private string GetBlendName(eFacialParts part, string shapeId)
        {
            string blendName;
            switch (part)
            {
                case eFacialParts.EYE:
                    blendName = $"eye{shapeId}_GEO";
                    break;
                case eFacialParts.BROW:
                    blendName = $"brow{shapeId}_GEO";
                    break;
                case eFacialParts.MOUTH:
                case eFacialParts.MOUTH_OPEN:
                case eFacialParts.MOUTH_CLOSE:
                case eFacialParts.MOUTH_HALFCLOSE:
                    blendName = $"mouth{shapeId}_GEO";
                    break;
                default:
                    // 剩下那些不知道怎么处理，忽略
                    blendName = string.Empty;
                    break;
            }

            if (string.IsNullOrEmpty(blendName))
            {
                return blendName;
            }
            return _blends.Keys.FirstOrDefault(f => f.EndsWith(blendName));
        }

        public void SetMouthState(MouthState state)
        {
            SetExpression(_expression, state);
        }
        public eFaceExpression[] GetAllExpressions()
        {
            return _facials.Keys.ToArray();
        }
        public void SetExpression(eFaceExpression expression, MouthState mouthState)
        {
            var data = _facials[expression];
            if (data == null)
            {
                Debug.LogError($"expression: {expression} not found!");
                return;
            }

            _mouthstate = mouthState;
            _expression = expression;
            _autoBlink = data.Blink;
            bool hasEveEPart = false;

            foreach (var blend in _blends)
            {
                blend.Value.setValue(0);
            }
            foreach (var blend in data.BlendData)
            {
                if (blend.Parts == eFacialParts.MOUTH_CLOSE && _mouthstate != MouthState.CLOSE)
                {
                    continue;
                }
                if (blend.Parts == eFacialParts.MOUTH_OPEN && _mouthstate != MouthState.OPEN)
                {
                    continue;
                }
                if (blend.Parts == eFacialParts.MOUTH_HALFCLOSE && _mouthstate != MouthState.HALFCLOSE)
                {
                    continue;
                }
                string blendName = GetBlendName(blend.Parts, blend.ShapeId);
                if (string.IsNullOrEmpty(blendName))
                {
                    Debug.LogWarning($"expression blend not found, part: {blend.Parts}, id: {blend.ShapeId}");
                    continue;
                }

                if (blendName == "face_blendshape.eyeE_GEO")
                {
                    _baseEyeEVal = blend.BlendValue;
                    hasEveEPart = true;
                }
                _blends[blendName].setValue(blend.BlendValue);
            }
            if (!hasEveEPart)
            {
                _baseEyeEVal = 0f;
            }

            _faceMesh.sharedMaterial.SetFloat("_CheekPower", data.CheekPower);
            _faceMesh.sharedMaterial.SetFloat("_ForeheadShade", data.ForeheadShade);
            _eyeMesh.sharedMaterial.SetFloat("_HighLight", 1f - data.EyeHighlight);
        }
        public void SetExpression(eFaceExpression expression)
        {
            SetExpression(expression, _mouthstate);
        }

        bool _autoBlink = false;
        float _nextBlinkTime;
        // 记录当前表情下face_blendshape.eyeE_GEO的基础值
        float _baseEyeEVal = 0f;
        private void InitializeAutoBlink()
        {
            _nextBlinkTime = Time.time;
        }
        public void AutoBlinkUpdate()
        {
            if (!_autoBlink) return;

            var time = Time.time;
            if (time > _nextBlinkTime)
            {
                var t = time - _nextBlinkTime;
                if (t > _settings.BlinkDuration)
                {
                    _nextBlinkTime = time + _settings.BlinkThreshold + (1 + Random.value) * _settings.BlinkInterval;
                }
                else
                {
                    var range = 100f - _baseEyeEVal;
                    var val = (1 - _settings.BlinkCurve.Evaluate(t / _settings.BlinkDuration)) * range;
                    var blinkVal = _baseEyeEVal + val;

                    UpdateBlink(blinkVal);
                }
            }
        }
        public void UpdateBlink(float val)
        {
            // 没找到控制眨眼的相关配置
            // 看了一下模型,固定都是用eyeE_GEO来控制闭眼的,就写死了
            _blends["face_blendshape.eyeE_GEO"].setValue(val);
        }
    }
}
