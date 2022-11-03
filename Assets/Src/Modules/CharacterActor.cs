using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LapisPlayer
{
    public class AvatarPart
    {
        public readonly GameObject Model;
        public readonly AvatarSetting Setting;

        public AvatarPart(AvatarSetting setting)
        {
            Setting = setting;
            Model = GameObject.Instantiate(setting.Prefab);
        }

        public void BindDynamicBone()
        {
            DynamicBone dynamicBone = Model.AddComponent<DynamicBone>();
            dynamicBone.m_Radius = 0.04f;
            dynamicBone.m_RadiusDistrib = AnimationCurve.Linear(0f, 0.33f, 1f, 1f);
            dynamicBone.m_Damping = 0.33f;
            dynamicBone.m_Elasticity = 0.05f;
            dynamicBone.m_Stiffness = 0.05f;
            dynamicBone.m_Friction = 0.25f;
            dynamicBone.m_Gravity = new Vector3(0, -0.04f, 0);

            dynamicBone.m_Roots = new();
            dynamicBone.m_Colliders = new();

            var springSetting = Setting.SpringSetting;
            if (springSetting == null) return;

            foreach (var bone in springSetting.boneParameters)
            {
                var node = Utility.FindNodeByPath(Model, bone.nodeName);
                if (node == null)
                {
                    Debug.LogWarning("[" + Model.name + "]Bone node not found:" + bone.nodeName);
                    continue;
                }

                dynamicBone.m_Roots.Add(node.transform);
            }

            foreach (var collider in springSetting.colliderParameters)
            {
                var node = Utility.FindNodeByPath(Model, collider.nodeName);
                if (node == null) node = Utility.FindNodeByPath(Model, collider.nodeName);
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
                    var slblingNode = Utility.FindNodeByPath(Model, collider.sibilingPath);
                    if (slblingNode == null) slblingNode = Utility.FindNodeByPath(Model, collider.sibilingPath);
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


        private List<AvatarPart> _parts = new();
        public List<AvatarPart> Parts => _parts;

        public GameObject Root { get; private set; }
        public GameObject SkeletonRoot { get; private set; }

        public CharacterActor (CharacterSetting chara, ActorSetting actor)
        {
            _chara = chara;
            _actor = actor;

            BuildModel();
        }
        public void Destroy()
        {
            GameObject.Destroy(Root);
        }

        private void BuildModel()
        {
            Root = new GameObject($"{_chara.Name}_{_actor.Name}_Root");

            GameObject skl_std = AssetBundleLoader.Instance.LoadAsset<GameObject>(Skeleton);
            SkeletonRoot = GameObject.Instantiate(skl_std);
            SkeletonRoot.transform.SetParent(Root.transform);

            string human = $"Actors/Avatars/{_chara.ShortName}/Human";

            var faceSetting = AssetBundleLoader.Instance.LoadAsset<AvatarSetting>($"{human}/Hum_{_chara.ShortName}_Face_00");
            var hairSetting = AssetBundleLoader.Instance.LoadAsset<AvatarSetting>($"{human}/Hum_{_chara.ShortName}_Hair_00");
            var bodySetting = AssetBundleLoader.Instance.LoadAsset<AvatarSetting>(_actor.Body);
            BuildAvatarPart(faceSetting, "Face");
            BuildAvatarPart(hairSetting, "Hair");
            BuildAvatarPart(bodySetting, "Body");

            foreach(var equip in _actor.Equips)
            {
                var partSetting = AssetBundleLoader.Instance.LoadAsset<AvatarSetting>(equip);
                BuildAvatarPart(partSetting);
            }
            BindDynamicBone();

            var vowelSetting = AssetBundleLoader.Instance.LoadAsset<VowelSetting>($"{human}/VOW_{_chara.ShortName}000");
            var facialSetting = AssetBundleLoader.Instance.LoadAsset<FacialSettings>($"{human}/FAC_{_chara.ShortName}000");
            var behaviour = Root.AddComponent<CharacterBehaviour>();
            behaviour.ApplySettings(vowelSetting, facialSetting, bodySetting.HeelSetting);

            var scaleSetting = AssetBundleLoader.Instance.LoadAsset<ScaleSettings>($"{human}/SCL_{_chara.ShortName}000");
            Root.transform.localScale = new Vector3(scaleSetting.ScaleRatio, scaleSetting.ScaleRatio, scaleSetting.ScaleRatio);
        }
        private void BuildAvatarPart (AvatarSetting setting, string rename = "")
        {
            var part = new AvatarPart(setting);

            part.Model.transform.SetParent(Root.transform);
            if (!string.IsNullOrEmpty(rename))
            {
                part.Model.transform.name = rename;
            }

            if (setting.Type == eAvatarType.Skinning)
            {
                var skeletonAnimator = SkeletonRoot.GetComponent<Animator>();
                Animator animator = part.Model.GetComponent<Animator>();
                if (animator == null) animator = part.Model.AddComponent<Animator>();
                animator.avatar = skeletonAnimator.avatar;
                animator.runtimeAnimatorController = LoadBaseController();
            } 
            else
            {
                // TODO 类型为Attach 的应该是需要添加到某个骨骼节点上,暂时不知道怎么实现
            }

            _parts.Add(part);
        }

        public void PlayBaseAnimation()
        {
            foreach(var part in _parts)
            {
                Animator animator = part.Model.GetComponent<Animator>();
                if (animator != null) animator.Play("Body@Idle");
            }
        }

        private AnimatorOverrideController LoadBaseController()
        {
            var controller = AssetBundleLoader.Instance.LoadAsset<RuntimeAnimatorController>("Actors/AnimationController/Common@AR");
            var ctrlInstance = new AnimatorOverrideController(controller);

            ctrlInstance["Body@Idle"] = AssetBundleLoader.Instance.LoadAsset<AnimationClip>("Actors/Animations/Actor/Favor/Clothes/Ani_Nor_Ash_Idle002");

            return ctrlInstance;
        }

        // TODO
        // DynamicBone 可以实现简单的头发/衣物物理效果了,但是穿模比较严重
        // 看有没有更好的解决方案
        private void BindDynamicBone()
        {
            foreach(var part in _parts)
            {
                part.BindDynamicBone();
            }
        }
    }
}
