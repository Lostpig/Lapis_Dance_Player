using UnityEngine;

namespace LapisPlayer
{
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

}
