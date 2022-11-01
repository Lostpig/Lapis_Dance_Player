using UnityEngine;

namespace LapisPlayer
{
    public class CharacterBehaviour : MonoBehaviour
    {
        public VowelManager Vowel { get; private set; }
        public FacialManager Facial { get; private set; }

        HeelSetting _heel;
        Animator animator;
        Transform leftFoot;
        Transform rightFoot;

        public void ApplySettings (VowelSetting vowelSetting, FacialSettings facialSetting, HeelSetting heelSetting)
        {
            var face = Utility.FindNodeByName(gameObject, "Face");
            Vowel = new(vowelSetting, face);
            Facial = new(facialSetting, face);

            _heel = heelSetting;
            if (_heel == null)
            {
                _heel = new HeelSetting()
                {
                    tweakFootAngle = 0,
                    tweakFootHeight = 0
                };
            }

            var originPos = gameObject.transform.position;
            gameObject.transform.position = new(originPos.x, originPos.y + heelSetting.tweakFootHeight, originPos.z);

            var body = Utility.FindNodeByName(gameObject, "Body");
            leftFoot = Utility.FindNodeByRecursion(body, "Bip001_L_Foot").transform;
            rightFoot = Utility.FindNodeByRecursion(body, "Bip001_R_Foot").transform;
            leftFoot.transform.Rotate(Vector3.back, heelSetting.tweakFootAngle);
            rightFoot.transform.Rotate(Vector3.back, heelSetting.tweakFootAngle);
            animator = body.GetComponent<Animator>();
        }

        private void Update()
        {
            Vowel.Update();
        }

        // FIXME: OnAnimatorIK 不会触发,用LateUpdate模拟解决
        // 如果 OnAnimatorIK 能生效优先使用 OnAnimatorIK
        Quaternion leftFootCache;
        Quaternion rightFootCache;
        private void LateUpdate()
        {
            if (leftFoot.rotation != leftFootCache)
            {
                leftFoot.Rotate(Vector3.back, _heel.tweakFootAngle);
                leftFootCache = leftFoot.rotation;
            }
            if (rightFoot.rotation != rightFootCache)
            {
                rightFoot.Rotate(Vector3.back, _heel.tweakFootAngle);
                rightFootCache = rightFoot.rotation;
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            Debug.Log("OnAnimatorIK Called");

            leftFoot.Rotate(Vector3.back, _heel.tweakFootAngle);
            rightFoot.Rotate(Vector3.back, _heel.tweakFootAngle);

            animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFoot.rotation);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFoot.rotation);
        }
    }
}
