using System.Collections.Generic;
using Unity.Animations.SpringBones;
using Unity.Animations.SpringBones.GameObjectExtensions;
using UnityEngine;

namespace LapisPlayer
{
    public class CharacterActor
    {
        const string Skeleton = "actors/skeletons/skl_std";

        CharacterSetting _chara;
        ActorSetting _actor;
        RuntimeAnimatorController _baseController;
        FacialBehaviour _faceBehaviour;
        bool _isCommonActor = false;
        bool _physicalBinding = false;

        private List<AvatarPart> _parts = new();
        public List<AvatarPart> Parts => _parts;

        public GameObject Root { get; private set; }
        public GameObject SkeletonRoot { get; private set; }
        public ScaleSettings Scales { get; private set; }
        public HeelSetting Heel { get; private set; }
        public FacialBehaviour Facial { get => _faceBehaviour; }

        public CharacterActor(CharacterSetting chara, ActorSetting actor, bool isCommonActor = false)
        {
            _chara = chara;
            _actor = actor;
            _isCommonActor = isCommonActor;

            BuildModel();
        }
        public void Destroy()
        {
            GameObject.Destroy(Root);
        }

        private void BuildModel()
        {
            Root = new GameObject($"{_chara.Name}_{_actor.Name}_Root");
            _baseController = AssetBundleLoader.Instance.LoadAsset<RuntimeAnimatorController>("Actors/AnimationController/Common@AR");
            GameObject skl_std = AssetBundleLoader.Instance.LoadAsset<GameObject>(Skeleton);
            SkeletonRoot = GameObject.Instantiate(skl_std);
            SkeletonRoot.transform.SetParent(Root.transform);

            string human = $"Actors/Avatars/{_chara.ShortName}/Human";
            Scales = AssetBundleLoader.Instance.LoadAsset<ScaleSettings>($"{human}/SCL_{_chara.ShortName}000");
            string bodyAvatarPath = _isCommonActor ? (_actor.Body + (Scales.IsLoli ? "_l" : "_r")) : _actor.Body;

            var faceSetting = AssetBundleLoader.Instance.LoadAsset<AvatarSetting>($"{human}/Hum_{_chara.ShortName}_Face_00");
            var hairSetting = AssetBundleLoader.Instance.LoadAsset<AvatarSetting>($"{human}/Hum_{_chara.ShortName}_Hair_00");
            var bodySetting = AssetBundleLoader.Instance.LoadAsset<AvatarSetting>(bodyAvatarPath);
            Heel = bodySetting.HeelSetting;
            BuildAvatarPart(faceSetting, "Face");
            BuildAvatarPart(hairSetting, "Hair");
            BuildAvatarPart(bodySetting, "Body");

            foreach (var equip in _actor.Equips)
            {
                string equipPath = _isCommonActor ? string.Format(equip, _chara.ShortName) : equip;
                var partSetting = AssetBundleLoader.Instance.LoadAsset<AvatarSetting>(equipPath);

                if (partSetting == null)
                {
                    Debug.LogError("Equip avatar part not found: " + equipPath);
                    continue;
                }
                BuildAvatarPart(partSetting);
            }

            var vowelSetting = AssetBundleLoader.Instance.LoadAsset<VowelSetting>($"{human}/VOW_{_chara.ShortName}000");
            var facialSetting = AssetBundleLoader.Instance.LoadAsset<FacialSettings>($"{human}/FAC_{_chara.ShortName}000");

            var face = Utility.FindNodeByName(Root, "Face");
            _faceBehaviour = face.AddComponent<FacialBehaviour>();
            _faceBehaviour.ApplySettings(vowelSetting, facialSetting);

            var _heelBehaviour = SkeletonRoot.AddComponent<HeelBehaviour>();
            _heelBehaviour.ApplySettings(bodySetting.HeelSetting);

            ApplyBreastBlends();
            Root.transform.localScale = new Vector3(Scales.ScaleRatio, Scales.ScaleRatio, Scales.ScaleRatio);

            // 添加了这段反而显示不正常
            // 不确定 scaleSetting.ListScaleData 是如何生效的
            /* foreach (var scl in scaleSetting.ListScaleData)
            {
                var sclBone = Utility.FindNodeByRecursion(body, scl.BoneName);
                sclBone.transform.localScale = scl.Scale;
            } */

            // BindPhysicalBones();
        }
        private void BuildAvatarPart(AvatarSetting setting, string rename = "")
        {
            var part = new AvatarPart(setting);

            if (!string.IsNullOrEmpty(rename))
            {
                part.Model.transform.name = rename;
            }

            if (setting.Type == eAvatarType.Skinning)
            {
                part.Model.transform.SetParent(Root.transform);
                part.Model.transform.localPosition = Vector3.zero;

                MergeBones(part.Model);
            }
            else
            {
                // 类型为Attach 的需要添加到骨骼节点上
                var point = Utility.FindNodeByRecursion(SkeletonRoot, part.Setting.Dummy);
                if (point == null)
                {
                    Debug.LogError("Attach avatar part dummy node not found:" + part.Setting.Dummy);
                }
                part.Model.transform.SetParent(point.transform);
                part.Model.transform.localPosition = Vector3.zero;
            }

            _parts.Add(part);
        }
        private void ApplyBreastBlends()
        {
            GameObject body = Utility.FindNodeByName(Root, "Body");

            var skinMeshes = body.GetComponentsInChildren<SkinnedMeshRenderer>();
            SkinnedMeshRenderer breastMesh = null;

            foreach(var mesh in skinMeshes)
            {
                if (mesh.gameObject.name.ToLower().IndexOf("breast") >= 0)
                {
                    breastMesh = mesh;
                    break;
                }
            }
            if (breastMesh == null) return;

            for (var i = 0; i < breastMesh.sharedMesh.blendShapeCount; i++)
            {
                string name = breastMesh.sharedMesh.GetBlendShapeName(i);
                
                if (name.ToLower().IndexOf("up") >= 0)
                {
                    breastMesh.SetBlendShapeWeight(i, Scales.BreastUp);
                }
                else if (name.ToLower().IndexOf("down") >= 0)
                {
                    breastMesh.SetBlendShapeWeight(i, Scales.BreastDown);
                }
                else if (name.ToLower().IndexOf("larger") >= 0)
                {
                    breastMesh.SetBlendShapeWeight(i, Scales.BreastLarger);
                }
                else if (name.ToLower().IndexOf("smaller") >= 0)
                {
                    breastMesh.SetBlendShapeWeight(i, Scales.BreastSmall);
                }
            }
        }

        public void SetPlaying(bool playing)
        {
            _faceBehaviour.SetPlayingState(playing);

            if (playing)
            {
                StopBaseAnimation();
            }
            else
            {
                PlayBaseAnimation();
                SetExpression(eFaceExpression.USUALLY, MouthState.CLOSE);
            }
        }
        public void PlayBaseAnimation(string stateName = "Body@Idle")
        {
            SkeletonRoot.GetComponent<Animator>().Play(stateName);
        }
        public void StopBaseAnimation()
        {
            SkeletonRoot.GetComponent<Animator>().StopPlayback();
        }

        public void SetBaseAnimationController(AnimatorOverrideController controller)
        {
            SkeletonRoot.GetComponent<Animator>().runtimeAnimatorController = controller;
        }
        public void SetBaseAnimationClip(AnimationClip clip, string stateName = "Body@Idle")
        {
            var ctrlInstance = new AnimatorOverrideController(_baseController);
            ctrlInstance[stateName] = clip;
            SkeletonRoot.GetComponent<Animator>().runtimeAnimatorController = ctrlInstance;
        }
        public void SetPose(string poseName)
        {
            var clip = PoseStore.Instance.LoadPoseClip(poseName);
            SetBaseAnimationClip(clip);
        }
        public void SetExpression(eFaceExpression expression, MouthState mouthstate)
        {
            _faceBehaviour.Facial.SetExpression(expression, mouthstate);
        }
        public void SetExpression(eFaceExpression expression)
        {
            _faceBehaviour.Facial.SetExpression(expression);
        }
        public void SetMouthState(MouthState mouthstate)
        {
            _faceBehaviour.Facial.SetMouthState(mouthstate);
        }

        public eFaceExpression[] GetExpressions()
        {
            return _faceBehaviour.Facial.GetAllExpressions();
        }

        public void SetPosition(Vector3 position)
        {
            Root.transform.position = new Vector3(position.x, position.y + Heel.tweakFootHeight, position.z);
        }
        public void SetLocalPosition(Vector3 localPosition)
        {
            var position = Root.transform.parent.TransformPoint(localPosition);
            SetPosition(position);
            // Root.transform.localPosition = new Vector3(localPosition.x, localPosition.y + Heel.tweakFootHeight, localPosition.z);
        }
        public void BindPhysicalBones()
        {
            if (_physicalBinding) return;

            if (ConfigManager.Instance.PhysicalType == 1)
            {
                BindDynamicBone();
            }
            else
            {
                BindSpringBone();
            }

            _physicalBinding = true;
        }
        // TODO
        // DynamicBone / SpringBone 都会有穿模问题
        // 但是暂时没有更好的解决方案
        private void BindDynamicBone()
        {
            DynamicBone dynamicBone = SkeletonRoot.AddComponent<DynamicBone>();
            dynamicBone.m_Radius = 0.04f;
            dynamicBone.m_RadiusDistrib = AnimationCurve.Linear(0f, 0.33f, 1f, 1f);
            dynamicBone.m_Damping = 0.33f;
            dynamicBone.m_Elasticity = 0.05f;
            dynamicBone.m_Stiffness = 0.05f;
            dynamicBone.m_Friction = 0.25f;
            dynamicBone.m_Gravity = new Vector3(0, -0.04f, 0);

            dynamicBone.m_Roots = new();
            dynamicBone.m_Colliders = new();

            foreach (var part in _parts)
            {
                part.BindDynamicBone(SkeletonRoot, dynamicBone);
            }
        }
        private void BindSpringBone()
        {
            Dictionary<string, SpringSphereCollider> sphereList = new();
            Dictionary<string, SpringCapsuleCollider> capsuleList = new();
            foreach (var part in _parts)
            {
                part.CreateSpringColiier(SkeletonRoot, ref sphereList, ref capsuleList);
            }

            var bontList = new List<SpringBone>();
            foreach (var part in _parts)
            {
                bontList.AddRange(part.BindSpringBone(SkeletonRoot, sphereList, capsuleList));
            }

            var manager = Root.AddComponent<SpringManager>();
            manager.springBones = bontList.ToArray();
        }

        private void MergeBones(GameObject partRoot)
        {
            var sklRef = SkeletonRoot.FindChildByName("Reference");

            var partRef = partRoot.FindChildByName("Reference");
            var skinMeshes = partRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
            Dictionary<Transform, Transform> replaceDict = new();

            MergeReference(sklRef, partRef, ref replaceDict);
            // MovePartItems(SkeletonRoot.transform, partRoot.transform);

            foreach (var sm in skinMeshes)
            {
                ReplaceMeshBones(sm, ref replaceDict);
            }
        }
        private void MergeReference(Transform sklRef, Transform partRef, ref Dictionary<Transform, Transform> dict)
        {
            var count = partRef.childCount;
            for (int i = 0; i < count; i++)
            {
                var item = partRef.GetChild(i);
                var sklItem = sklRef.Find(item.name);
                if (sklItem == null)
                {
                    var newItem = new GameObject();
                    newItem.name = item.name;
                    newItem.transform.SetParent(sklRef);
                    sklItem = newItem.transform;
                }
                sklItem.transform.position = item.position;
                sklItem.transform.rotation = item.rotation;
                sklItem.transform.localScale = item.localScale;

                dict.Add(item, sklItem);
                MergeReference(sklItem, item, ref dict);
            }
        }
        private void ReplaceMeshBones(SkinnedMeshRenderer mesh, ref Dictionary<Transform, Transform> dict)
        {
            mesh.rootBone = dict[mesh.rootBone];
            var replaceBones = new Transform[mesh.bones.Length];
            for (int i = 0; i < mesh.bones.Length; i++)
            {
                replaceBones[i] = dict[mesh.bones[i]];
            }

            mesh.bones = replaceBones;
        }
    }
}
