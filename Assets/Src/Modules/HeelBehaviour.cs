using UnityEngine;

namespace LapisPlayer
{
    public class HeelBehaviour : MonoBehaviour
    {
        HeelSetting _setting;
        Transform leftFoot;
        Transform rightFoot;
        Animator _animator;
        public void ApplySettings(HeelSetting heelSetting)
        {
            _setting = heelSetting;
            if (_setting == null)
            {
                _setting = new HeelSetting()
                {
                    tweakFootAngle = 0,
                    tweakFootHeight = 0
                };
            }

            // var originPos = gameObject.transform.position;
            // gameObject.transform.position = new(originPos.x, originPos.y + heelSetting.tweakFootHeight, originPos.z);

            _animator = GetComponent<Animator>();
            leftFoot = Utility.FindNodeByRecursion(gameObject, "Bip001_L_Foot").transform;
            rightFoot = Utility.FindNodeByRecursion(gameObject, "Bip001_R_Foot").transform;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            leftFoot.Rotate(Vector3.back, _setting.tweakFootAngle);
            rightFoot.Rotate(Vector3.back, _setting.tweakFootAngle);

            _animator.SetBoneLocalRotation(HumanBodyBones.LeftFoot, leftFoot.localRotation);
            _animator.SetBoneLocalRotation(HumanBodyBones.RightFoot, rightFoot.localRotation);

            // _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            // _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
            // _animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFoot.rotation);
            // _animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFoot.rotation);
        }
    }

}
