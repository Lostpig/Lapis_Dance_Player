#if DEBUG
//#define DEBUG_SHOW_RAYCAST_LINES
#endif

using UnityEngine;
using UnityEngine.Serialization;

namespace VLB
{
    [ExecuteInEditMode]
    [HelpURL(Consts.Help.UrlDynamicOcclusionRaycasting)]
    public class DynamicOcclusionRaycasting : DynamicOcclusionAbstractBase
    {
        public new const string ClassName = "DynamicOcclusionRaycasting";

        /// <summary>
        /// Should it interact with 2D or 3D occluders?
        /// </summary>
        public Dimensions dimensions = Consts.DynOcclusion.RaycastingDimensionsDefault;

        /// <summary>
        /// The beam can only be occluded by objects located on the layers matching this mask.
        /// It's very important to set it as restrictive as possible (checking only the layers which are necessary)
        /// to perform a more efficient process in order to increase the performance.
        /// </summary>
        public LayerMask layerMask = Consts.DynOcclusion.LayerMaskDefault;

        /// <summary>
        /// Should this beam be occluded by triggers or not?
        /// </summary>
        public bool considerTriggers = Consts.DynOcclusion.RaycastingConsiderTriggersDefault;

        /// <summary>
        /// Minimum 'area' of the collider to become an occluder.
        /// Colliders smaller than this value will not block the beam.
        /// </summary>
        public float minOccluderArea = Consts.DynOcclusion.RaycastingMinOccluderAreaDefault;

        /// <summary>
        /// Approximated percentage of the beam to collide with the surface in order to be considered as occluder
        /// </summary>
        public float minSurfaceRatio = Consts.DynOcclusion.RaycastingMinSurfaceRatioDefault;

        /// <summary>
        /// Max angle (in degrees) between the beam and the surface in order to be considered as occluder
        /// </summary>
        public float maxSurfaceDot = Consts.DynOcclusion.RaycastingMaxSurfaceDotDefault;

        /// <summary>
        /// Alignment of the computed clipping plane:
        /// </summary>
        public PlaneAlignment planeAlignment = Consts.DynOcclusion.RaycastingPlaneAlignmentDefault;

        /// <summary>
        /// Translate the plane. We recommend to set a small positive offset in order to handle non-flat surface better.
        /// </summary>
        public float planeOffset = Consts.DynOcclusion.RaycastingPlaneOffsetDefault;

        /// <summary>
        /// Fade out the beam before the computed clipping plane in order to soften the transition.
        /// </summary>
        [FormerlySerializedAs("fadeDistanceToPlane")]
        public float fadeDistanceToSurface = Consts.DynOcclusion.FadeDistanceToSurfaceDefault;

        [System.Obsolete("Use 'fadeDistanceToSurface' instead")]
        public float fadeDistanceToPlane { get { return fadeDistanceToSurface; } set { fadeDistanceToSurface = value; } }


        public bool IsColliderHiddenByDynamicOccluder(Collider collider)
        {
            Debug.Assert(collider, "You should pass a valid Collider to VLB.DynamicOcclusion.IsColliderHiddenByDynamicOccluder");

            if (!planeEquationWS.IsValid())
                return false;

            var isInside = GeometryUtility.TestPlanesAABB(new Plane[] { planeEquationWS }, collider.bounds);
            return !isInside;
        }

        public struct HitResult
        {
            public HitResult(ref RaycastHit hit3D)
            {
                point = hit3D.point;
                normal = hit3D.normal;
                distance = hit3D.distance;
                collider3D = hit3D.collider;
                collider2D = null;
            }

            public HitResult(ref RaycastHit2D hit2D)
            {
                point = hit2D.point;
                normal = hit2D.normal;
                distance = hit2D.distance;
                collider2D = hit2D.collider;
                collider3D = null;
            }

            public Vector3 point;
            public Vector3 normal;
            public float distance;

            Collider2D collider2D;
            Collider collider3D;

            public bool hasCollider { get { return collider2D || collider3D; } }

            public string name
            {
                get
                {
                    if (collider3D) return collider3D.name;
                    else if (collider2D) return collider2D.name;
                    else return "null collider";
                }
            }

            public Bounds bounds
            {
                get
                {
                    if (collider3D) return collider3D.bounds;
                    else if (collider2D) return collider2D.bounds;
                    else return new Bounds();
                }
            }

            public void SetNull() { collider2D = null; collider3D = null; }
        }

        /// <summary>
        /// Get information about the current occluder hit by the beam.
        /// Can be null if the beam is not occluded.
        /// </summary>
        HitResult m_CurrentHit;

        protected override string GetShaderKeyword() { return "VLB_OCCLUSION_CLIPPING_PLANE"; }
        protected override MaterialManager.DynamicOcclusion GetDynamicOcclusionMode() { return MaterialManager.DynamicOcclusion.ClippingPlane; }

        float m_RangeMultiplier = 1f;
        public Plane planeEquationWS { get; private set; }

#if UNITY_EDITOR
        public HitResult editorCurrentHitResult { get { return m_CurrentHit; } }

        public struct EditorDebugData
        {
            public int lastFrameUpdate;
        }
        public EditorDebugData editorDebugData;

        public static bool editorShowDebugPlane = true;
        public static bool editorRaycastAtEachFrame = true;
        private static bool editorPrefsLoaded = false;

        public static void EditorLoadPrefs()
        {
            if (!editorPrefsLoaded)
            {
                editorShowDebugPlane = UnityEditor.EditorPrefs.GetBool(EditorPrefsStrings.DynOcclusion.PrefShowDebugPlane, true);
                editorRaycastAtEachFrame = UnityEditor.EditorPrefs.GetBool(EditorPrefsStrings.DynOcclusion.PrefRaycastingEditor, true);
                editorPrefsLoaded = true;
            }
        }
#endif

        protected override void OnValidateProperties()
        {
            base.OnValidateProperties();
            minOccluderArea = Mathf.Max(minOccluderArea, 0f);
            fadeDistanceToSurface = Mathf.Max(fadeDistanceToSurface, 0f);
        }

        protected override void OnEnablePostValidate()
        {
            m_CurrentHit.SetNull();

#if UNITY_EDITOR
            EditorLoadPrefs();
            editorDebugData.lastFrameUpdate = 0;
#endif
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SetHitNull();
        }

        void Start()
        {
            if (Application.isPlaying)
            {
                var triggerZone = GetComponent<TriggerZone>();
                if (triggerZone)
                {
                    m_RangeMultiplier = Mathf.Max(1f, triggerZone.rangeMultiplier);
                }
            }
        }
        
        Vector3 GetRandomVectorAround(Vector3 direction, float angleDiff)
        {
            var halfAngle = angleDiff * 0.5f;
            return Quaternion.Euler(Random.Range(-halfAngle, halfAngle), Random.Range(-halfAngle, halfAngle), Random.Range(-halfAngle, halfAngle)) * direction;
        }

        QueryTriggerInteraction queryTriggerInteraction { get { return considerTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore; } }

        float raycastMaxDistance { get { return m_Master.raycastDistance * m_RangeMultiplier * m_Master.lossyScale.z; } }

        HitResult GetBestHit(Vector3 rayPos, Vector3 rayDir)
        {
            return dimensions == Dimensions.Dim2D ? GetBestHit2D(rayPos, rayDir) : GetBestHit3D(rayPos, rayDir);
        }

        HitResult GetBestHit3D(Vector3 rayPos, Vector3 rayDir)
        {
            var hits = Physics.RaycastAll(rayPos, rayDir, raycastMaxDistance, layerMask.value, queryTriggerInteraction);

            int bestHit = -1;
            float bestLength = float.MaxValue;
            for (int i = 0; i < hits.Length; ++i)
            {
                if (hits[i].collider.gameObject != m_Master.gameObject) // skip collider from TriggerZone
                {
                    if (hits[i].collider.bounds.GetMaxArea2D() >= minOccluderArea)
                    {
                        if (hits[i].distance < bestLength)
                        {
                            bestLength = hits[i].distance;
                            bestHit = i;
                        }
                    }
                }
            }

#if DEBUG_SHOW_RAYCAST_LINES
            Debug.DrawLine(rayPos, rayPos + rayDir * raycastMaxDistance, bestHit != -1 ? Color.green : Color.red);
#endif
            if (bestHit != -1)
                return new HitResult(ref hits[bestHit]);
            else
                return new HitResult();
        }

        HitResult GetBestHit2D(Vector3 rayPos, Vector3 rayDir)
        {
            var hits = Physics2D.RaycastAll(new Vector2(rayPos.x, rayPos.y), new Vector2(rayDir.x, rayDir.y), raycastMaxDistance, layerMask.value);

            int bestHit = -1;
            float bestLength = float.MaxValue;
            for (int i = 0; i < hits.Length; ++i)
            {
                if (!considerTriggers && hits[i].collider.isTrigger) // do not query triggers if considerTriggers is disabled
                    continue;

                if (hits[i].collider.gameObject != m_Master.gameObject) // skip collider from TriggerZone
                {
                    if (hits[i].collider.bounds.GetMaxArea2D() >= minOccluderArea)
                    {
                        if (hits[i].distance < bestLength)
                        {
                            bestLength = hits[i].distance;
                            bestHit = i;
                        }
                    }
                }
            }

#if DEBUG_SHOW_RAYCAST_LINES
            Debug.DrawLine(rayPos, rayPos + rayDir * raycastMaxDistance, bestHit != -1 ? Color.green : Color.red);
#endif
            if (bestHit != -1)
                return new HitResult(ref hits[bestHit]);
            else
                return new HitResult();
        }

        enum Direction {
            Up,
            Down,
            Left,
            Right,
            Max2D = Down,
            Max3D = Right,
        };
        uint m_PrevNonSubHitDirectionId = 0;

        uint GetDirectionCount() { return dimensions == Dimensions.Dim2D ? ((uint)Direction.Max2D + 1) : ((uint)Direction.Max3D + 1); }

        Vector3 GetDirection(uint dirInt)
        {
            dirInt = dirInt % GetDirectionCount();
            switch (dirInt)
            {
                case (uint)Direction.Up:    return  m_Master.raycastGlobalUp;
                case (uint)Direction.Right: return  m_Master.raycastGlobalRight;
                case (uint)Direction.Down:  return -m_Master.raycastGlobalUp;
                case (uint)Direction.Left:  return -m_Master.raycastGlobalRight;
            }
            return Vector3.zero;
        }


        bool IsHitValid(ref HitResult hit, Vector3 forwardVec)
        {
            if (hit.hasCollider)
            {
                float dot = Vector3.Dot(hit.normal, -forwardVec);
                return dot >= maxSurfaceDot;
            }
            return false;
        }

        protected override bool OnProcessOcclusion(ProcessOcclusionSource source)
        {
#if UNITY_EDITOR
            editorDebugData.lastFrameUpdate = Time.frameCount;
#endif
            var raycastGlobalForward = m_Master.raycastGlobalForward;
            var bestHit = GetBestHit(transform.position, raycastGlobalForward);

            if (IsHitValid(ref bestHit, raycastGlobalForward))
            {
                if (minSurfaceRatio > 0.5f)
                {
                    var raycastDistance = m_Master.raycastDistance;
                    for (uint i = 0; i < GetDirectionCount(); i++)
                    {
                        var dir3 = GetDirection(i + m_PrevNonSubHitDirectionId) * (minSurfaceRatio * 2 - 1);
                        dir3.Scale(transform.localScale);
                        var startPt = transform.position + dir3 * m_Master.coneRadiusStart;
                        var newPt   = transform.position + dir3 * m_Master.coneRadiusEnd + raycastGlobalForward * raycastDistance;

                        var bestHitSub = GetBestHit(startPt, (newPt - startPt).normalized);
                        if (IsHitValid(ref bestHitSub, raycastGlobalForward))
                        {
                            if (bestHitSub.distance > bestHit.distance)
                            {
                                bestHit = bestHitSub;
                            }
                        }
                        else
                        {
                            m_PrevNonSubHitDirectionId = i;
                            bestHit.SetNull();
                            break;
                        }
                    }
                }
            }
            else
            {
                bestHit.SetNull();
            }

            SetHit(ref bestHit);
            return bestHit.hasCollider;
        }

        void SetHit(ref HitResult hit)
        {
            if (!hit.hasCollider)
            {
                SetHitNull();
            }
            else
            {
                switch (planeAlignment)
                {
                    case PlaneAlignment.Beam:
                        SetClippingPlane(new Plane(-m_Master.raycastGlobalForward, hit.point));
                        break;
                    case PlaneAlignment.Surface:
                    default:
                        SetClippingPlane(new Plane(hit.normal, hit.point));
                        break;
                }

                m_CurrentHit = hit;
            }
        }

        void SetHitNull()
        {
            SetClippingPlaneOff();
            m_CurrentHit.SetNull();
        }

        protected override void OnModifyMaterialCallback(MaterialModifier.Interface owner)
        {
            Debug.Assert(owner != null);
            var planeWS = planeEquationWS;
            owner.SetMaterialProp(ShaderProperties.DynamicOcclusionClippingPlaneWS, new Vector4(planeWS.normal.x, planeWS.normal.y, planeWS.normal.z, planeWS.distance));
            owner.SetMaterialProp(ShaderProperties.DynamicOcclusionClippingPlaneProps, fadeDistanceToSurface);
        }

        void SetClippingPlane(Plane planeWS)
        {
            planeWS = planeWS.TranslateCustom(planeWS.normal * planeOffset);
            SetPlaneWS(planeWS);
            Debug.Assert(m_MaterialModifierCallbackCached != null);
            m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), m_MaterialModifierCallbackCached);
        }

        void SetClippingPlaneOff()
        {
            SetPlaneWS(new Plane());
            m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), null);
        }

        void SetPlaneWS(Plane planeWS)
        {
            planeEquationWS = planeWS;

#if UNITY_EDITOR
            m_DebugPlaneLocal = planeWS;
            if (m_DebugPlaneLocal.IsValid())
            {
                float dist;
                if (m_DebugPlaneLocal.Raycast(new Ray(transform.position, m_Master.raycastGlobalForward), out dist))
                    m_DebugPlaneLocal.distance = dist; // compute local distance
            }
#endif
        }

#if UNITY_EDITOR
        void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                // In Editor, process raycasts at each frame update
                if (!editorRaycastAtEachFrame)
                    SetHitNull();
                else
                    ProcessOcclusion(ProcessOcclusionSource.EditorUpdate);
            }
        }

        Plane m_DebugPlaneLocal;

        void OnDrawGizmos()
        {
            if (!editorShowDebugPlane)
                return;

            if (m_DebugPlaneLocal.IsValid())
            {
                var planePos = transform.position + m_DebugPlaneLocal.distance * m_Master.raycastGlobalForward;
                float planeDistNormalized = Mathf.Clamp01(Mathf.InverseLerp(0f, m_Master.raycastDistance, m_DebugPlaneLocal.distance));
                float planeSize = Mathf.Lerp(m_Master.coneRadiusStart, m_Master.coneRadiusEnd, planeDistNormalized);

                var color = m_Master.ComputeColorAtDepth(planeDistNormalized).ComputeComplementaryColor(false);
                Utils.GizmosDrawPlane(
                    m_DebugPlaneLocal.normal,
                    planePos,
                    color,
                    Matrix4x4.identity,
                    planeSize,
                    planeSize * 0.5f);

                UnityEditor.Handles.color = color;
                UnityEditor.Handles.DrawWireDisc(planePos,
                                                m_DebugPlaneLocal.normal,
                                                planeSize * (minSurfaceRatio * 2 - 1));
            }
        }
#endif
    }
}
