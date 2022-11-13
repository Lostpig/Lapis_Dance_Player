using UnityEngine;

namespace VLB
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VolumetricLightBeam))]
    [HelpURL(Consts.HelpUrlTriggerZone)]
    public class TriggerZone : MonoBehaviour
    {
        /// <summary>
        /// Define if the Collider will be created as a convex trigger (not physical, most common behavior) or as a regular collider (physical).
        /// </summary>
        public bool setIsTrigger = true;

        /// <summary>
        /// Change the length of the Collider. For example, set 2.0 to make the Collider 2x longer than the beam. Default value is 1.0.
        /// </summary>
        public float rangeMultiplier = 1.0f;

        enum TriggerZoneUpdateRate
        {
            /// <summary>Compute the Trigger Zone only once at startup</summary>
            OnEnable,
            /// <summary>Compute the Trigger Zone each time the dynamic occlusion has changed</summary>
            OnOcclusionChange,
        }

        /// <summary>
        /// How often will the Trigger Zone be computed?
        /// </summary>
        TriggerZoneUpdateRate updateRate
        {
            get
            {
                Debug.Assert(m_Beam != null);
                if(m_Beam.dimensions == Dimensions.Dim3D) return TriggerZoneUpdateRate.OnEnable;  // for 3D meshes, do it only once because it's too performance heavy
                return m_DynamicOcclusionRaycasting != null ? TriggerZoneUpdateRate.OnOcclusionChange : TriggerZoneUpdateRate.OnEnable;
            }
        }
        const int kMeshColliderNumSides = 8;

        VolumetricLightBeam m_Beam = null;
        DynamicOcclusionRaycasting m_DynamicOcclusionRaycasting = null;
        PolygonCollider2D m_PolygonCollider2D = null;

        void OnEnable()
        {
            m_Beam = GetComponent<VolumetricLightBeam>();
            Debug.Assert(m_Beam != null);

            m_DynamicOcclusionRaycasting = GetComponent<DynamicOcclusionRaycasting>();

            switch(updateRate)
            {
                case TriggerZoneUpdateRate.OnEnable:
                {
                    ComputeZone();
                    enabled = false;
                    break;
                }
                case TriggerZoneUpdateRate.OnOcclusionChange:
                {
                    if(m_DynamicOcclusionRaycasting)
                        m_DynamicOcclusionRaycasting.onOcclusionProcessed += OnOcclusionProcessed;
                    break;
                }
            }
        }

        void OnOcclusionProcessed()
        {
            ComputeZone();
        }

        void ComputeZone()
        {
            if (m_Beam)
            {
                var rangeEnd = m_Beam.fallOffEnd * rangeMultiplier;
                var lerpedRadiusEnd = Mathf.LerpUnclamped(m_Beam.coneRadiusStart, m_Beam.coneRadiusEnd, rangeMultiplier);

                if (m_Beam.dimensions == Dimensions.Dim3D)
                {
                    var meshCollider = gameObject.GetOrAddComponent<MeshCollider>();
                    Debug.Assert(meshCollider);

                    var mesh = MeshGenerator.GenerateConeZ_Radius(rangeEnd, m_Beam.coneRadiusStart, lerpedRadiusEnd, kMeshColliderNumSides, 0, false, false);
                    mesh.hideFlags = Consts.ProceduralObjectsHideFlags;

                    meshCollider.sharedMesh = mesh;
                    meshCollider.convex = setIsTrigger;
                    meshCollider.isTrigger = setIsTrigger;
                }
                else
                {
                    if (m_PolygonCollider2D == null)
                    {
                        m_PolygonCollider2D = gameObject.GetOrAddComponent<PolygonCollider2D>();
                        Debug.Assert(m_PolygonCollider2D);
                    }
                    
                    var polyCoordsLS = new Vector2[] // polygon coord in local space
                    {
                        new Vector2(0.0f,      -m_Beam.coneRadiusStart),
                        new Vector2(rangeEnd,  -lerpedRadiusEnd),
                        new Vector2(rangeEnd,  lerpedRadiusEnd),
                        new Vector2(0.0f,      m_Beam.coneRadiusStart)
                    };

                    if (m_DynamicOcclusionRaycasting && m_DynamicOcclusionRaycasting.planeEquationWS.IsValid())
                    {
                        var plane3dWS = m_DynamicOcclusionRaycasting.planeEquationWS;

                        if (Utils.IsAlmostZero(plane3dWS.normal.z))
                        {
                            // Compute 2 points on the plane in world space
                            // Use this technique instead of transforming the plane's normal to fully support scaling

                            var ptOnPlane1 = plane3dWS.ClosestPointOnPlaneCustom(Vector3.zero);
                            var ptOnPlane2 = plane3dWS.ClosestPointOnPlaneCustom(Vector3.up);

                            if(Utils.IsAlmostZero(Vector3.SqrMagnitude(ptOnPlane1 - ptOnPlane2)))
                                ptOnPlane1 = plane3dWS.ClosestPointOnPlaneCustom(Vector3.right);

                            // Compute 2 points on the plane in local space
                            ptOnPlane1 = transform.InverseTransformPoint(ptOnPlane1);
                            ptOnPlane2 = transform.InverseTransformPoint(ptOnPlane2);

                            // Compute plane equation in local space
                            var plane2dLS = PolygonHelper.Plane2D.FromPoints(ptOnPlane1, ptOnPlane2);
                            if (plane2dLS.normal.x > 0.0f) plane2dLS.Flip();

                            polyCoordsLS = plane2dLS.CutConvex(polyCoordsLS);
                        }
                    }

                    m_PolygonCollider2D.points = polyCoordsLS;
                    m_PolygonCollider2D.isTrigger = setIsTrigger;
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            rangeMultiplier = Mathf.Max(rangeMultiplier, 0.001f);
        }
#endif
    }
}
