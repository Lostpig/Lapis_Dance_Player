using UnityEngine;

namespace VLB_Samples
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody), typeof(MeshRenderer))]
    public class CheckIfInsideBeam : MonoBehaviour
    {
        bool isInsideBeam = false;
        Material m_Material = null;
        Collider m_Collider = null;

        void Start()
        {
            m_Collider = GetComponent<Collider>();
            Debug.Assert(m_Collider);

            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer)
                m_Material = meshRenderer.material;
            Debug.Assert(m_Material);
        }

        void Update()
        {
            if (m_Material)
            {
                m_Material.SetColor("_Color", isInsideBeam ? Color.green : Color.red);
            }
        }

        void FixedUpdate()
        {
            isInsideBeam = false;
        }

        void OnTriggerStay(Collider trigger)
        {
            var dynamicOcclusion = trigger.GetComponent<VLB.DynamicOcclusionRaycasting>();

            if (dynamicOcclusion)
            {
                // This GameObject is inside the beam's TriggerZone.
                // Make sure it's not hidden by an occluder
                isInsideBeam = !dynamicOcclusion.IsColliderHiddenByDynamicOccluder(m_Collider);
            }
            else
            {
                isInsideBeam = true;
            }
        }
    }
}

