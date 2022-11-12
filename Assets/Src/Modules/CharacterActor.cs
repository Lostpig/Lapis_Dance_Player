using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Animations.SpringBones;
using Unity.Animations.SpringBones.GameObjectExtensions;
using UnityEngine;


namespace LapisPlayer
{
    public class AvatarPart
    {
        public readonly AvatarSetting Setting;
        public GameObject Prefab => Setting.Prefab;
        public readonly GameObject Model;

        public AvatarPart(AvatarSetting setting)
        {
            Setting = setting;
            Model = GameObject.Instantiate(Prefab);
        }

        public SpringBone[] BindSpringBone(GameObject SklRoot, Dictionary<string, SpringSphereCollider> sphereList, Dictionary<string, SpringCapsuleCollider> capsuleList)
        {
            var springSetting = Setting.SpringSetting;
            if (springSetting == null) return Array.Empty<SpringBone>();

            List<SpringBone> boneList = new();
            foreach (var bone in springSetting.boneParameters)
            {
                var node = Utility.FindNodeByPath(SklRoot, bone.nodeName);
                if (node == null)
                {
                    Debug.LogWarning("[" + Model.name + "]Bone node not found:" + bone.nodeName);
                    continue;
                }
                var springBone = node.AddComponent<SpringBone>();
                springBone.radius = bone.radius;
                springBone.dragForce = bone.dragForce * 0.8f;
                springBone.stiffnessForce = Math.Min(bone.stiffnessForce * 16000f, 2000f);

                if (!string.IsNullOrEmpty(bone.sibilingPath))
                {
                    var sibilingNode = Utility.FindNodeByPath(SklRoot, bone.sibilingPath);
                    if (sibilingNode != null)
                    {
                        // Transform[] trs = sibilingNode.GetComponentsInChildren<Transform>(true);
                        springBone.lengthLimitTargets = new Transform[] { sibilingNode.transform };
                    }
                }

                List<SpringSphereCollider> spheres = new();
                List<SpringCapsuleCollider> capsules = new();
                foreach (var id in bone.colliderIDs)
                {
                    var name = Enum.GetName(typeof(SpringColliderID), (SpringColliderID)id);
                    var sp = sphereList.GetValueOrDefault(name);
                    var cap = capsuleList.GetValueOrDefault(name);

                    if (sp != null) spheres.Add(sp);
                    if (cap != null) capsules.Add(cap);
                }

                springBone.sphereColliders = spheres.ToArray();
                springBone.capsuleColliders = capsules.ToArray();
                springBone.panelColliders = Array.Empty<SpringPanelCollider>();

                boneList.Add(springBone);
            }

            return boneList.ToArray();
        }
        public void CreateSpringColiier(GameObject SklRoot, ref Dictionary<string, SpringSphereCollider> sphereList, ref Dictionary<string, SpringCapsuleCollider> capsuleList)
        {
            var springSetting = Setting.SpringSetting;
            if (springSetting == null) return;

            foreach (var colliderParam in springSetting.colliderParameters)
            {
                var node = Utility.FindNodeByPath(SklRoot, colliderParam.nodeName);
                if (node == null)
                {
                    Debug.LogWarning("[" + Model.name + "]Collider node not found:" + colliderParam.nodeName);
                    continue;
                }

                var idName = colliderParam.nodeName.Split('/').Last();
                if (!sphereList.ContainsKey(idName))
                {
                    var collider = node.AddComponent<SpringSphereCollider>();
                    collider.radius = colliderParam.radius;
                    sphereList.Add(idName, collider);
                }

                if (!string.IsNullOrEmpty(colliderParam.sibilingPath) && !capsuleList.ContainsKey(idName))
                {
                    var slblingNode = Utility.FindNodeByPath(SklRoot, colliderParam.sibilingPath);
                    if (slblingNode == null)
                    {
                        Debug.LogWarning("[" + Model.name + "]Sibiling collider node not found:" + colliderParam.sibilingPath);
                        continue;
                    }

                    var curr = node;
                    var target = slblingNode;
                    if (Utility.IsParentNode(node.transform, slblingNode.transform))
                    {
                        curr = slblingNode;
                        target = node;
                    }
                    var sibilingObject = new GameObject("Sibiling_" + slblingNode.name);
                    sibilingObject.transform.SetParent(curr.transform);
                    sibilingObject.transform.localPosition = Vector3.zero;
                    sibilingObject.transform.localRotation = Quaternion.identity;

                    var capsule = sibilingObject.AddComponent<SpringCapsuleCollider>();
                    capsule.radius = colliderParam.radius;

                    var relativeVec = curr.transform.InverseTransformPoint(target.transform.position);
                    var length = relativeVec.magnitude;
                    capsule.height = length;

                    Vector3 dif = target.transform.position - curr.transform.position;
                    sibilingObject.transform.rotation = Utility.YLookRotation(dif, Vector3.up);
                    capsuleList.Add(idName, capsule);
                }
            }
        }

        public void BindDynamicBone(GameObject SklRoot, DynamicBone dynamicBone)
        {
            var springSetting = Setting.SpringSetting;
            if (springSetting == null) return;

            foreach (var bone in springSetting.boneParameters)
            {
                var node = Utility.FindNodeByPath(SklRoot, bone.nodeName);
                if (node == null)
                {
                    Debug.LogWarning("[" + Model.name + "]Bone node not found:" + bone.nodeName);
                    continue;
                }

                dynamicBone.m_Roots.Add(node.transform);
            }

            foreach (var collider in springSetting.colliderParameters)
            {
                var node = Utility.FindNodeByPath(SklRoot, collider.nodeName);
                if (node == null)
                {
                    Debug.LogWarning("Collider node not found:" + collider.nodeName);
                    continue;
                }

                var dyCollider = node.AddComponent<DynamicBoneCollider>();
                dyCollider.m_Radius = collider.radius;
                dyCollider.m_Center = collider.offset;
                dynamicBone.m_Colliders.Add(dyCollider);

                if (!string.IsNullOrEmpty(collider.sibilingPath))
                {
                    var slblingNode = Utility.FindNodeByPath(SklRoot, collider.sibilingPath);
                    if (slblingNode == null)
                    {
                        Debug.LogWarning("Collider slbling node not found:" + collider.sibilingPath);
                        continue;
                    }

                    var slblingParam = springSetting.colliderParameters.FirstOrDefault(p => p.nodeName == collider.sibilingPath);
                    var radiusDef = slblingParam == null ? 0 : slblingParam.radius - collider.radius;
                    var mediumRadius = collider.radius + (radiusDef / 2);

                    var curr = node;
                    var currOffset = collider.offset;
                    var currRadius = collider.radius;
                    var target = slblingNode;
                    var targetOffset = slblingParam == null ? Vector3.zero : slblingParam.offset;
                    if (Utility.IsParentNode(node.transform, slblingNode.transform))
                    {
                        curr = slblingNode;
                        currOffset = targetOffset;
                        target = node;
                        targetOffset = collider.offset;

                        currRadius = collider.radius + radiusDef;
                        radiusDef = -radiusDef;
                    }

                    var targetPos = target.transform.TransformPoint(targetOffset);
                    var relativeVec = curr.transform.InverseTransformPoint(targetPos) - currOffset;
                    var length = relativeVec.magnitude;

                    var count = (float)Math.Ceiling(length / (mediumRadius * 2));
                    var perLength = 1f / count;
                    if (perLength > 0.5f) perLength = 0.5f;

                    for (float i = perLength; i < 1; i += perLength)
                    {
                        var pos = (relativeVec * i) + currOffset;

                        var siblingCollider = curr.AddComponent<DynamicBoneCollider>();
                        siblingCollider.m_Radius = currRadius + (radiusDef * i);
                        siblingCollider.m_Center = pos;
                        dynamicBone.m_Colliders.Add(siblingCollider);
                    }
                }
            }
        }
    }

    public class CharacterActor
    {
        const string Skeleton = "actors/skeletons/skl_std";

        CharacterSetting _chara;
        ActorSetting _actor;
        RuntimeAnimatorController _baseController;
        FacialBehaviour _faceBehaviour;
        bool _isCommonActor = false;

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

            Root.transform.localScale = new Vector3(Scales.ScaleRatio, Scales.ScaleRatio, Scales.ScaleRatio);
            // 添加了这段反而显示不正常
            // 不确定 scaleSetting.ListScaleData 是如何生效的
            /* foreach (var scl in scaleSetting.ListScaleData)
            {
                var sclBone = Utility.FindNodeByRecursion(body, scl.BoneName);
                sclBone.transform.localScale = scl.Scale;
            } */

            BindPhysicalBones();
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
        public void PlayBaseAnimation()
        {
            SkeletonRoot.GetComponent<Animator>().Play("Body@Idle");
        }
        public void StopBaseAnimation()
        {
            SkeletonRoot.GetComponent<Animator>().Play("Body@Idle1");
        }

        public void SetBaseAnimationClip(AnimationClip clip)
        {
            var ctrlInstance = new AnimatorOverrideController(_baseController);
            ctrlInstance["Body@Idle"] = clip;
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

        private void BindPhysicalBones()
        {
            if (ConfigManager.Instance.PhysicalType == 1)
            {
                BindDynamicBone();
            }
            else
            {
                BindSpringBone();
            }
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
        private void MovePartItems(Transform skl, Transform partRoot)
        {
            var count = partRoot.childCount;
            Transform[] items = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                items[i] = partRoot.GetChild(i);
            }
            foreach (var item in items)
            {
                if (item.name == "Reference") continue;
                item.SetParent(skl);
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
