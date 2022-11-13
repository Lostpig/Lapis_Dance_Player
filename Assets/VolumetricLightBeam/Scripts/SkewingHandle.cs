using UnityEngine;
using System.Collections;

namespace VLB
{
    [ExecuteInEditMode]
    [HelpURL(Consts.HelpUrlSkewingHandle)]
    public class SkewingHandle : MonoBehaviour
    {
        public VolumetricLightBeam volumetricLightBeam = null;
        public bool shouldUpdateEachFrame = false;

#if UNITY_EDITOR
        void Update()
        {
            if (!Application.isPlaying && CanSetSkewingVector())
                SetSkewingVector();
        }
#endif

        public bool IsAttachedToSelf() { return volumetricLightBeam != null && volumetricLightBeam.gameObject == this.gameObject; }
        public bool CanSetSkewingVector() { return volumetricLightBeam != null && volumetricLightBeam.canHaveMeshSkewing; }
        public bool CanUpdateEachFrame() { return CanSetSkewingVector() && volumetricLightBeam.trackChangesDuringPlaytime; }
        bool ShouldUpdateEachFrame() { return shouldUpdateEachFrame && CanUpdateEachFrame(); }

        void OnEnable()
        {
            if(CanSetSkewingVector())
                SetSkewingVector();
        }

        void Start()
        {
            if (Application.isPlaying && ShouldUpdateEachFrame())
            {
                StartCoroutine(CoUpdate());
            }
        }

        IEnumerator CoUpdate()
        {
            while(ShouldUpdateEachFrame())
            {
                SetSkewingVector();
                yield return null;
            }
        }

        void SetSkewingVector()
        {
            Debug.Assert(CanSetSkewingVector());
            var vec = volumetricLightBeam.transform.InverseTransformPoint(transform.position);
            volumetricLightBeam.skewingLocalForwardDirection = vec;
        }
    }
}

