using UnityEngine;
using System.Collections;

namespace VLB
{
    [ExecuteInEditMode]
    [HelpURL(Consts.Help.UrlDynamicOcclusionDepthBuffer)]
    public class DynamicOcclusionDepthBuffer : DynamicOcclusionAbstractBase
    {
        public new const string ClassName = "DynamicOcclusionDepthBuffer";

        /// <summary>
        /// The beam can only be occluded by objects located on the layers matching this mask.
        /// It's very important to set it as restrictive as possible (checking only the layers which are necessary)
        /// to perform a more efficient process in order to increase the performance.
        /// It should NOT include the layer on which the beams are generated.
        /// </summary>
        public LayerMask layerMask = Consts.DynOcclusion.LayerMaskDefault;

        /// <summary>
        /// Whether or not the virtual camera will use occlusion culling during rendering from the beam's POV.
        /// </summary>
        public bool useOcclusionCulling = Consts.DynOcclusion.DepthBufferOcclusionCullingDefault;

        /// <summary>
        /// Controls how large the depth texture captured by the virtual camera is.
        /// The lower the resolution, the better the performance, but the less accurate the rendering.
        /// </summary>
        public int depthMapResolution = Consts.DynOcclusion.DepthBufferDepthMapResolutionDefault;

        /// <summary>
        /// Fade out the beam before the occlusion surface in order to soften the transition.
        /// </summary>
        public float fadeDistanceToSurface = Consts.DynOcclusion.FadeDistanceToSurfaceDefault;


        protected override string GetShaderKeyword() { return "VLB_OCCLUSION_DEPTH_TEXTURE"; }
        protected override MaterialManager.DynamicOcclusion GetDynamicOcclusionMode() { return MaterialManager.DynamicOcclusion.DepthTexture; }

        Camera m_DepthCamera = null;
        bool m_NeedToUpdateOcclusionNextFrame = false;

        void ProcessOcclusionInternal()
        {
            UpdateDepthCameraPropertiesAccordingToBeam();
            m_DepthCamera.Render();
        }

        protected override bool OnProcessOcclusion(ProcessOcclusionSource source)
        {
            Debug.Assert(m_Master && m_DepthCamera);

            if (source == ProcessOcclusionSource.RenderLoop && SRPHelper.IsUsingCustomRenderPipeline()) // Recursive rendering is not supported on SRP
                m_NeedToUpdateOcclusionNextFrame = true;
            else
                ProcessOcclusionInternal();

            return true;
        }

        void Update()
        {
            if (m_NeedToUpdateOcclusionNextFrame && m_Master && m_DepthCamera)
            {
                ProcessOcclusionInternal();
                m_NeedToUpdateOcclusionNextFrame = false;
            }
        }

        void UpdateDepthCameraPropertiesAccordingToBeam()
        {
            Debug.Assert(m_Master);
            Debug.Assert(m_DepthCamera);

            float apexDist = m_Master.coneApexOffsetZ;
            m_DepthCamera.transform.localPosition = m_Master.beamLocalForward * (-apexDist);
            m_DepthCamera.transform.localRotation = m_Master.beamInternalLocalRotation;

            var scale = m_Master.lossyScale;
            if (!Mathf.Approximately(scale.y * scale.z, 0))
            {
                const float kMinNearClipPlane = 0.1f; // should be the same than in shader
                m_DepthCamera.nearClipPlane = Mathf.Max(apexDist, kMinNearClipPlane) * scale.z;
                m_DepthCamera.farClipPlane = (m_Master.maxGeometryDistance + apexDist) * scale.z;
                m_DepthCamera.aspect = scale.x / scale.y;
                m_DepthCamera.fieldOfView = scale.y * m_Master.coneAngle / scale.z;
            }
        }

        public bool HasLayerMaskIssues()
        {
            if(Config.Instance.geometryOverrideLayer)
            {
                int layerBit = 1 << Config.Instance.geometryLayerID;
                return ((layerMask.value & layerBit) == layerBit);
            }
            return false;
        }

        protected override void OnValidateProperties()
        {
            base.OnValidateProperties();
            depthMapResolution = Mathf.Clamp(Mathf.NextPowerOfTwo(depthMapResolution), 8, 2048);
            fadeDistanceToSurface = Mathf.Max(fadeDistanceToSurface, 0f);
        }

        void InstantiateOrActivateDepthCamera()
        {
            if (m_DepthCamera != null)
            {
                m_DepthCamera.gameObject.SetActive(true); // active it in case it has been disabled by OnDisable()
            }
            else
            {
                m_DepthCamera = new GameObject("Depth Camera").AddComponent<Camera>();

                if (m_DepthCamera && m_Master)
                {
                    m_DepthCamera.enabled = false;
                    m_DepthCamera.cullingMask = layerMask;
                    m_DepthCamera.clearFlags = CameraClearFlags.Depth;
                    m_DepthCamera.depthTextureMode = DepthTextureMode.Depth;
                    m_DepthCamera.renderingPath = RenderingPath.VertexLit; // faster
                    m_DepthCamera.useOcclusionCulling = useOcclusionCulling;
                    m_DepthCamera.gameObject.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
                    m_DepthCamera.transform.SetParent(transform, false);

                    var rt = new RenderTexture(depthMapResolution, depthMapResolution, 16, RenderTextureFormat.Depth);
                    m_DepthCamera.targetTexture = rt;

                    UpdateDepthCameraPropertiesAccordingToBeam();

#if UNITY_EDITOR
                    UnityEditor.GameObjectUtility.SetStaticEditorFlags(m_DepthCamera.gameObject, m_Master.GetStaticEditorFlagsForSubObjects());
                    m_DepthCamera.gameObject.SetSameSceneVisibilityStatesThan(m_Master.gameObject);
#endif
                }
            }
        }

        protected override void OnEnablePostValidate()
        {
            InstantiateOrActivateDepthCamera();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_DepthCamera) m_DepthCamera.gameObject.SetActive(false);
        }

        protected override void Awake()
        {
            base.Awake();

#if UNITY_EDITOR
            MarkMaterialAsDirty();
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DestroyDepthCamera();

#if UNITY_EDITOR
            MarkMaterialAsDirty();
#endif
        }

        void DestroyDepthCamera()
        {
            if (m_DepthCamera)
            {
                if (m_DepthCamera.targetTexture)
                {
                    m_DepthCamera.targetTexture.Release();
                    DestroyImmediate(m_DepthCamera.targetTexture);
                    m_DepthCamera.targetTexture = null;
                }

                DestroyImmediate(m_DepthCamera.gameObject); // Make sure to delete the GAO
                m_DepthCamera = null;
            }
        }

        protected override void OnModifyMaterialCallback(MaterialModifier.Interface owner)
        {
            Debug.Assert(owner != null);
            owner.SetMaterialProp(ShaderProperties.DynamicOcclusionDepthTexture, m_DepthCamera.targetTexture);
            owner.SetMaterialProp(ShaderProperties.DynamicOcclusionDepthProps, fadeDistanceToSurface);
        }

#if UNITY_EDITOR
        bool m_NeedToReinstantiateDepthCamera = false;

        void MarkMaterialAsDirty()
        {
            // when adding/removing this component in editor, we might need to switch from a GPU Instanced material to a custom one,
            // since this feature doesn't support GPU Instancing
            if (!Application.isPlaying)
                m_Master._EditorSetBeamGeomDirty();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            m_NeedToReinstantiateDepthCamera = true;
        }

        void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                if (m_NeedToReinstantiateDepthCamera)
                {
                    DestroyDepthCamera();
                    InstantiateOrActivateDepthCamera();
                    m_NeedToReinstantiateDepthCamera = false;
                }

                if(m_Master && m_Master.enabled)
                    ProcessOcclusion(ProcessOcclusionSource.EditorUpdate);
            }
        }
#endif // UNITY_EDITOR
    }
}
