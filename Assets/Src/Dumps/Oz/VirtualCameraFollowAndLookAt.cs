using Cinemachine;
using LapisPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Oz.Timeline
{
    public class VirtualCameraFollowAndLookAt : MonoBehaviour
    {
        public int positionIndex; // 0x18
        public string followName; // 0x20
        public string lookAtName; // 0x28
        private CinemachineVirtualCamera _virtualCamera; // 0x30
        public CinemachineVirtualCamera VirtualCamera { get => _virtualCamera; }

        private void Start()
        {
            _virtualCamera = GetComponent<CinemachineVirtualCamera>();

            // TODO HeadFix这个节点在解包的数据中找不到...
            // 可能是硬编码写死的,转为Head_Point做个兼容
            if (followName == "HeadFix") followName = "Head_Point";
            if (lookAtName == "HeadFix") lookAtName = "Head_Point";
        }
        public void ApplyLookAt(ICinemachineCamera cinemachineCamera)
        {
            if (!isActiveAndEnabled) return;
            if (positionIndex > 5)
            {
                Debug.Log("ApplyLookAt Failed: positionIndex = " + positionIndex);
                return;
            }

            var danceManager = PlayerGlobal.Instance.GetSingleton<IDanceManager>();
            var character = danceManager.GetCharacter(positionIndex - 1);



            var follow = Utility.FindNodeByRecursion(character.SkeletonRoot, followName);
            var lookAt = Utility.FindNodeByRecursion(character.SkeletonRoot, lookAtName);

            if (lookAt == null)
            {
                Debug.Log("Lookat not found: positionIndex = " + positionIndex + ", lookAt = " + lookAtName);
            }
            else
            {
                cinemachineCamera.LookAt = lookAt.transform;
            }
            if (follow == null)
            {
                Debug.Log("Follow not found: positionIndex = " + positionIndex + ", Follow = " + followName);
            }
            else
            {
                cinemachineCamera.Follow = follow.transform;
            }
        }
    }
}
