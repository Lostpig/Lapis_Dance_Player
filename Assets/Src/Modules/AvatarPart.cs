using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Animations.SpringBones;
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
                springBone.radius = bone.radius * 1.25f;
                springBone.springForce = bone.externalForce;
                springBone.dragForce = bone.dragForce * 0.75f;
                springBone.stiffnessForce = Math.Min(bone.stiffnessForce * 16000f, 2500f);
                if (bone.enableRotationLimit)
                {
                    // springBone.angularStiffness = 20f;
                    springBone.pivotNode = node.transform.parent;
                    /*
                    springBone.yAngleLimits = new AngleLimits()
                    {
                        active = true,
                        min = bone.lowRotationLimit.y,
                        max = bone.highRotationLimit.y
                    };
                    springBone.zAngleLimits = new AngleLimits()
                    {
                        active = true,
                        min = bone.lowRotationLimit.z,
                        max = bone.highRotationLimit.z
                    };
                    */
                }


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

}
