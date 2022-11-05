using UnityEngine;

namespace VLB_Samples
{
    [RequireComponent(typeof(Camera))]
    public class CameraToggleBeamVisibility : MonoBehaviour
    {
        [SerializeField] KeyCode m_KeyCode = KeyCode.Space;

        void Update()
        {
            if (Input.GetKeyDown(m_KeyCode))
            {
                var cam = GetComponent<Camera>();

                int layerID = VLB.Config.Instance.geometryLayerID;
                int layerMask = 1 << layerID;
                if ((cam.cullingMask & layerMask) == layerMask)
                {
                    cam.cullingMask &= ~layerMask;
                }
                else
                {
                    cam.cullingMask |= layerMask;
                }
            }
        }
    }
}

