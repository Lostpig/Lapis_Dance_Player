using UnityEngine;

namespace LapisPlayer
{
    public class Actor : MonoBehaviour
    {
        public int Postion { get; private set; }

        public void BindPosition (int position)
        {
            Postion = position;
        }
    }

    public class FacialBehaviour : MonoBehaviour
    {
        public VowelManager Vowel { get; private set; }
        public FacialManager Facial { get; private set; }
        bool isPlayingAnimation = false;
        Animator _animator;
        GameObject _camare;

        public void ApplySettings(VowelSetting vowelSetting, FacialSettings facialSetting)
        {
            Vowel = new(vowelSetting, gameObject);
            Facial = new(facialSetting, gameObject);

            _animator = GetComponent<Animator>();

            // 写死的Main Camera
            _camare = GameObject.Find("Main Camera");
        }
        public void SetPlayingState(bool playing)
        {
            isPlayingAnimation = playing;
        }
        private void Update()
        {
            if (!isPlayingAnimation)
            {
                Facial.AutoBlinkUpdate();
            }
        }

        // TODO 看镜头功能不能生效，不知道原因，注释了
        /* private void OnAnimatorIK(int layerIndex)
        {
            _animator.SetLookAtWeight(weight: 1.0f,
                    bodyWeight: 0,
                    headWeight: 0,
                    eyesWeight: 1.0f,
                    clampWeight: 0
            );
            _animator.SetLookAtPosition(_camare.transform.position);
        } */
    }

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
