#if DEBUG
//#define DEBUG_SHOW_MESH_NORMALS
#endif
#define FORCE_CURRENT_CAMERA_DEPTH_TEXTURE_MODE

#if UNITY_2018_1_OR_NEWER
#define VLB_SRP_SUPPORT // Comment this to disable SRP support
#endif

using UnityEngine;
using System.Collections;

#pragma warning disable 0429, 0162 // Unreachable expression code detected (because of Noise3D.isSupported on mobile)

namespace VLB
{
    [AddComponentMenu("")] // hide it from Component search
    [ExecuteInEditMode]
    [HelpURL(Consts.HelpUrlBeam)]
    public class BeamGeometry : MonoBehaviour, MaterialModifier.Interface
    {
        VolumetricLightBeam m_Master = null;
        Matrix4x4 m_ColorGradientMatrix;
        MeshType m_CurrentMeshType = MeshType.Shared;
        Material m_CustomMaterial = null;
        MaterialModifier.Callback m_MaterialModifierCallback = null;
        Coroutine m_CoFadeOut = null;

        public MeshRenderer meshRenderer { get; private set; }
        public MeshFilter meshFilter { get; private set; }
        public Mesh coneMesh { get; private set; }

        public bool visible
        {
            get { return meshRenderer.enabled; }
            set { meshRenderer.enabled = value; }
        }

        public int sortingLayerID
        {
            get { return meshRenderer.sortingLayerID; }
            set { meshRenderer.sortingLayerID = value; }
        }

        public int sortingOrder
        {
            get { return meshRenderer.sortingOrder; }
            set { meshRenderer.sortingOrder = value; }
        }

        public bool _INTERNAL_IsFadeOutCoroutineRunning { get { return m_CoFadeOut != null; } }

        float ComputeFadeOutFactor(Transform camTransform)
        {
            if (m_Master.isFadeOutEnabled)
            {
                float distanceCamToBeam = Vector3.SqrMagnitude(meshRenderer.bounds.center - camTransform.position);
                return Mathf.InverseLerp(m_Master.fadeOutEnd * m_Master.fadeOutEnd, m_Master.fadeOutBegin * m_Master.fadeOutBegin, distanceCamToBeam);
            }
            else
            {
                return 1.0f;
            }
        }

        IEnumerator CoUpdateFadeOut()
        {
            while (m_Master.isFadeOutEnabled)
            {
                ComputeFadeOutFactor();
                yield return null;
            }

            SetFadeOutFactorProp(1.0f);
            m_CoFadeOut = null;
        }

        void ComputeFadeOutFactor()
        {
            var camTransform = Config.Instance.fadeOutCameraTransform;
            if (camTransform)
            {
                float fadeOutFactor = ComputeFadeOutFactor(camTransform);
                SetFadeOutFactorProp(fadeOutFactor);
            }
            else
            {
                SetFadeOutFactorProp(1.0f);
            }
        }

        void SetFadeOutFactorProp(float value)
        {
            if (value > 0)
            {
                meshRenderer.enabled = true;

                MaterialChangeStart();
                SetMaterialProp(ShaderProperties.FadeOutFactor, value);
                MaterialChangeStop();
            }
            else
            {
                meshRenderer.enabled = false;
            }
        }

        public void RestartFadeOutCoroutine()
        {
        #if UNITY_EDITOR
            if (Application.isPlaying)
        #endif
            {
                if (m_CoFadeOut != null)
                {
                    StopCoroutine(m_CoFadeOut);
                    m_CoFadeOut = null;
                }

                if (m_Master && m_Master.isFadeOutEnabled)
                {
                    m_CoFadeOut = StartCoroutine(CoUpdateFadeOut());
                }
            }
        }

        void Start()
        {
            // Handle copy / paste the LightBeam in Editor
            if (!m_Master)
                DestroyImmediate(gameObject);
        }

        void OnDestroy()
        {
            if (m_CustomMaterial)
            {
                DestroyImmediate(m_CustomMaterial);
                m_CustomMaterial = null;
            }
        }

#if VLB_SRP_SUPPORT
        Camera m_CurrentCameraRenderingSRP = null;

        void OnDisable()
        {
            SRPHelper.UnregisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
            m_CurrentCameraRenderingSRP = null;
        }

        public static bool isCustomRenderPipelineSupported { get { return true; } }
#else
        public static bool isCustomRenderPipelineSupported { get { return false; } }
#endif

        bool shouldUseGPUInstancedMaterial
        { get {
            return m_Master._INTERNAL_DynamicOcclusionMode != MaterialManager.DynamicOcclusion.DepthTexture // sampler cannot be passed to shader as instanced property
                && Config.Instance.actualRenderingMode == RenderingMode.GPUInstancing;
        }}

        void OnEnable()
        {
            // When a GAO is disabled, all its coroutines are killed, so renable them on OnEnable.
            RestartFadeOutCoroutine();

#if VLB_SRP_SUPPORT
            SRPHelper.RegisterOnBeginCameraRendering(OnBeginCameraRenderingSRP);
#endif
        }

        public void Initialize(VolumetricLightBeam master)
        {
            Debug.Assert(master != null);

            var customHideFlags = Consts.ProceduralObjectsHideFlags;
            m_Master = master;

            transform.SetParent(master.transform, false);

            meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>();
            meshRenderer.hideFlags = customHideFlags;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off; // different reflection probes could break batching with GPU Instancing
#if UNITY_5_4_OR_NEWER
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
#else
            meshRenderer.useLightProbes = false;
#endif

            if (!shouldUseGPUInstancedMaterial)
            {
                m_CustomMaterial = MaterialManager.NewMaterialTransient(gpuInstanced:false);
                ApplyMaterial();
            }

            if (SortingLayer.IsValid(m_Master.sortingLayerID))
                sortingLayerID = m_Master.sortingLayerID;
            else
                Debug.LogError(string.Format("Beam '{0}' has an invalid sortingLayerID ({1}). Please fix it by setting a valid layer.", Utils.GetPath(m_Master.transform), m_Master.sortingLayerID));

            sortingOrder = m_Master.sortingOrder;

            meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
            meshFilter.hideFlags = customHideFlags;

            gameObject.hideFlags = customHideFlags;

#if UNITY_EDITOR
            UnityEditor.GameObjectUtility.SetStaticEditorFlags(gameObject, master.GetStaticEditorFlagsForSubObjects());
            gameObject.SetSameSceneVisibilityStatesThan(master.gameObject);
#endif

            RestartFadeOutCoroutine();
        }

        /// <summary>
        /// Generate the cone mesh and calls UpdateMaterialAndBounds.
        /// Since this process involves recreating a new mesh, make sure to not call it at every frame during playtime.
        /// </summary>
        public void RegenerateMesh()
        {
            Debug.Assert(m_Master);

            if (Config.Instance.geometryOverrideLayer)
                gameObject.layer = Config.Instance.geometryLayerID;
            else
                gameObject.layer = m_Master.gameObject.layer;

            gameObject.tag = Config.Instance.geometryTag;

            if (coneMesh && m_CurrentMeshType == MeshType.Custom)
            {
                DestroyImmediate(coneMesh);
            }

            m_CurrentMeshType = m_Master.geomMeshType;

            switch (m_Master.geomMeshType)
            {
                case MeshType.Custom:
                    {
                        coneMesh = MeshGenerator.GenerateConeZ_Radius(1f, 1f, 1f, m_Master.geomCustomSides, m_Master.geomCustomSegments, m_Master.geomCap, Config.Instance.requiresDoubleSidedMesh);
                        coneMesh.hideFlags = Consts.ProceduralObjectsHideFlags;
                        meshFilter.mesh = coneMesh;
                        break;
                    }
                case MeshType.Shared:
                    {
                        coneMesh = GlobalMesh.Get();
                        meshFilter.sharedMesh = coneMesh;
                        break;
                    }
                default:
                    {
                        Debug.LogError("Unsupported MeshType");
                        break;
                    }
            }

            UpdateMaterialAndBounds();
        }

        Vector3 ComputeLocalMatrix()
        {
            // In the VS, we compute the vertices so the whole beam fits into a fixed 2x2x1 box.
            // We have to apply some scaling to get the proper beam size.
            // This way we have the proper bounds without having to recompute specific bounds foreach beam.
            var maxRadius = Mathf.Max(m_Master.coneRadiusStart, m_Master.coneRadiusEnd);
            transform.localScale = new Vector3(maxRadius, maxRadius, m_Master.maxGeometryDistance);
            transform.localRotation = m_Master.beamInternalLocalRotation;

            return transform.localScale;
        }

        bool isNoiseEnabled { get { return m_Master.isNoiseEnabled && m_Master.noiseIntensity > 0f && Noise3D.isSupported; } } // test Noise3D.isSupported the last

#pragma warning disable 0162
        bool isDepthBlendEnabled { get { return BatchingHelper.forceEnableDepthBlend || m_Master.depthBlendDistance > 0f; } }
#pragma warning restore 0162

        bool ApplyMaterial()
        {
            var colorGradient = MaterialManager.ColorGradient.Off;
            if (m_Master.colorMode == ColorMode.Gradient)
            {
                var precision = Utils.GetFloatPackingPrecision();
                colorGradient = precision == Utils.FloatPackingPrecision.High ? MaterialManager.ColorGradient.MatrixHigh : MaterialManager.ColorGradient.MatrixLow;
            }

            Debug.Assert((int)BlendingMode.Additive == (int)MaterialManager.BlendingMode.Additive);
            Debug.Assert((int)BlendingMode.SoftAdditive == (int)MaterialManager.BlendingMode.SoftAdditive);
            Debug.Assert((int)BlendingMode.TraditionalTransparency == (int)MaterialManager.BlendingMode.TraditionalTransparency);

            var staticProps = new MaterialManager.StaticProperties
            {
                blendingMode = (MaterialManager.BlendingMode)m_Master.blendingMode,
                noise3D = isNoiseEnabled ? MaterialManager.Noise3D.On : MaterialManager.Noise3D.Off,
                depthBlend = isDepthBlendEnabled ? MaterialManager.DepthBlend.On : MaterialManager.DepthBlend.Off,
                colorGradient = colorGradient,
                dynamicOcclusion = m_Master._INTERNAL_EnabledDynamicOcclusionMode,
                meshSkewing = m_Master.hasMeshSkewing ? MaterialManager.MeshSkewing.On : MaterialManager.MeshSkewing.Off,
                shaderAccuracy = (m_Master.shaderAccuracy == ShaderAccuracy.Fast) ? MaterialManager.ShaderAccuracy.Fast : MaterialManager.ShaderAccuracy.High
            };

            Material mat = null;
            if (!shouldUseGPUInstancedMaterial)
            {
                mat = m_CustomMaterial;
                if(mat)
                    staticProps.ApplyToMaterial(mat);
            }
            else
            {
                mat = MaterialManager.GetInstancedMaterial(m_Master._INTERNAL_InstancedMaterialGroupID, staticProps);
            }

            meshRenderer.material = mat;
            return mat != null;
        }

        public void SetMaterialProp(int nameID, float value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetFloat(nameID, value);
            else
                MaterialManager.materialPropertyBlock.SetFloat(nameID, value);
        }

        public void SetMaterialProp(int nameID, Vector4 value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetVector(nameID, value);
            else
                MaterialManager.materialPropertyBlock.SetVector(nameID, value);
        }

        public void SetMaterialProp(int nameID, Color value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetColor(nameID, value);
            else
                MaterialManager.materialPropertyBlock.SetColor(nameID, value);
        }

        public void SetMaterialProp(int nameID, Matrix4x4 value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetMatrix(nameID, value);
            else
                MaterialManager.materialPropertyBlock.SetMatrix(nameID, value);
        }

        public void SetMaterialProp(int nameID, Texture value)
        {
            if (m_CustomMaterial)
                m_CustomMaterial.SetTexture(nameID, value);
            else
                Debug.LogError("Setting a Texture property to a GPU instanced material is not supported");
        }

        void MaterialChangeStart()
        {
            if (m_CustomMaterial == null)
                meshRenderer.GetPropertyBlock(MaterialManager.materialPropertyBlock);
        }

        void MaterialChangeStop()
        {
            if (m_CustomMaterial == null)
                meshRenderer.SetPropertyBlock(MaterialManager.materialPropertyBlock);
        }

        public void SetDynamicOcclusionCallback(string shaderKeyword, MaterialModifier.Callback cb)
        {
            m_MaterialModifierCallback = cb;

            if (m_CustomMaterial)
            {
                m_CustomMaterial.SetKeywordEnabled(shaderKeyword, cb != null);

                if (cb != null)
                    cb(this);
            }
            else
                UpdateMaterialAndBounds();
        }

        public void UpdateMaterialAndBounds()
        {
            Debug.Assert(m_Master);

            if (ApplyMaterial() == false)
            {
                return;
            }

            MaterialChangeStart();
            {
                if (m_CustomMaterial == null)
                {
                    if(m_MaterialModifierCallback != null)
                        m_MaterialModifierCallback(this);
                }

                float slopeRad = (m_Master.coneAngle * Mathf.Deg2Rad) / 2; // use coneAngle (instead of spotAngle) which is more correct with the geometry
                SetMaterialProp(ShaderProperties.ConeSlopeCosSin, new Vector2(Mathf.Cos(slopeRad), Mathf.Sin(slopeRad)));

                // kMinRadius and kMinApexOffset prevents artifacts when fresnel computation is done in the vertex shader
                const float kMinRadius = 0.0001f;
                var coneRadius = new Vector2(Mathf.Max(m_Master.coneRadiusStart, kMinRadius), Mathf.Max(m_Master.coneRadiusEnd, kMinRadius));
                SetMaterialProp(ShaderProperties.ConeRadius, coneRadius);

                const float kMinApexOffset = 0.0001f;
                float nonNullApex = Mathf.Sign(m_Master.coneApexOffsetZ) * Mathf.Max(Mathf.Abs(m_Master.coneApexOffsetZ), kMinApexOffset);
                SetMaterialProp(ShaderProperties.ConeApexOffsetZ, nonNullApex);

                if (m_Master.colorMode == ColorMode.Flat)
                {
                    SetMaterialProp(ShaderProperties.ColorFlat, m_Master.color);
                }
                else
                {
                    var precision = Utils.GetFloatPackingPrecision();
                    m_ColorGradientMatrix = m_Master.colorGradient.SampleInMatrix((int)precision);
                    // pass the gradient matrix in OnWillRenderObject()
                }

                SetMaterialProp(ShaderProperties.AlphaInside, m_Master.intensityInside);
                SetMaterialProp(ShaderProperties.AlphaOutside, m_Master.intensityOutside);
                SetMaterialProp(ShaderProperties.AttenuationLerpLinearQuad, m_Master.attenuationLerpLinearQuad);
                SetMaterialProp(ShaderProperties.DistanceFallOff, new Vector3(m_Master.fallOffStart, m_Master.fallOffEnd, m_Master.maxGeometryDistance));
                SetMaterialProp(ShaderProperties.DistanceCamClipping, m_Master.cameraClippingDistance);
                SetMaterialProp(ShaderProperties.FresnelPow, Mathf.Max(0.001f, m_Master.fresnelPow)); // no pow 0, otherwise will generate inf fresnel and issues on iOS
                SetMaterialProp(ShaderProperties.GlareBehind, m_Master.glareBehind);
                SetMaterialProp(ShaderProperties.GlareFrontal, m_Master.glareFrontal);
                SetMaterialProp(ShaderProperties.DrawCap, m_Master.geomCap ? 1 : 0);
                SetMaterialProp(ShaderProperties.TiltVector, m_Master.tiltFactor);
                SetMaterialProp(ShaderProperties.AdditionalClippingPlaneWS, m_Master.additionalClippingPlane);

                if (isDepthBlendEnabled)
                {
                    SetMaterialProp(ShaderProperties.DepthBlendDistance, m_Master.depthBlendDistance);
                }

                if (isNoiseEnabled)
                {
                    Noise3D.LoadIfNeeded();

                    var noiseVelocity = m_Master.noiseVelocityUseGlobal ? Config.Instance.globalNoiseVelocity : m_Master.noiseVelocityLocal;
                    var noiseScale = m_Master.noiseScaleUseGlobal ? Config.Instance.globalNoiseScale : m_Master.noiseScaleLocal;

                    SetMaterialProp(ShaderProperties.NoiseVelocityAndScale, new Vector4(
                        noiseVelocity.x,
                        noiseVelocity.y,
                        noiseVelocity.z,
                        noiseScale));

                    SetMaterialProp(ShaderProperties.NoiseParam, new Vector2(
                        m_Master.noiseIntensity,
                        m_Master.noiseMode == NoiseMode.WorldSpace ? 0f : 1f));
                }

                var localScale = ComputeLocalMatrix(); // compute matrix before sending it to the shader

                if (m_Master.hasMeshSkewing)
                {
                    var localForwardDirectionNormalized = m_Master.skewingLocalForwardDirectionNormalized;
                    SetMaterialProp(ShaderProperties.LocalForwardDirection, localForwardDirectionNormalized);

                    if (coneMesh != null) // coneMesh can be null few frames with Dynamic Occlusion & GPU Instancing
                    {
                        var localForwardDirectionN = localForwardDirectionNormalized;
                        localForwardDirectionN /= localForwardDirectionN.z;
                        localForwardDirectionN *= m_Master.fallOffEnd;
                        localForwardDirectionN.x /= localScale.x;
                        localForwardDirectionN.y /= localScale.y;

                        var bounds = MeshGenerator.ComputeBounds(1f, 1f, 1f);
                        var min = bounds.min;
                        var max = bounds.max;

                        if (localForwardDirectionN.x > 0.0f) max.x += localForwardDirectionN.x;
                        else min.x += localForwardDirectionN.x;

                        if (localForwardDirectionN.y > 0.0f) max.y += localForwardDirectionN.y;
                        else min.y += localForwardDirectionN.y;

                        bounds.min = min;
                        bounds.max = max;
                        coneMesh.bounds = bounds;
                    }
                }

#if VLB_SRP_SUPPORT
                // This update is to make QA test 'ReflectionObliqueProjection' pass
                UpdateMatricesPropertiesForGPUInstancingSRP();
#endif
            }
            MaterialChangeStop();

#if DEBUG_SHOW_MESH_NORMALS
            for (int vertexInd = 0; vertexInd < coneMesh.vertexCount; vertexInd++)
            {
                var vertex = coneMesh.vertices[vertexInd];

                // apply modification done inside VS
                vertex.x *= Mathf.Lerp(coneRadius.x, coneRadius.y, vertex.z);
                vertex.y *= Mathf.Lerp(coneRadius.x, coneRadius.y, vertex.z);
                vertex.z *= m_Master.fallOffEnd;

                var cosSinFlat = new Vector2(vertex.x, vertex.y).normalized;
                var normal = new Vector3(cosSinFlat.x * Mathf.Cos(slopeRad), cosSinFlat.y * Mathf.Cos(slopeRad), -Mathf.Sin(slopeRad)).normalized;

                vertex = transform.TransformPoint(vertex);
                normal = transform.TransformDirection(normal);
                Debug.DrawRay(vertex, normal * 0.25f);
            }
#endif
        }

#if VLB_SRP_SUPPORT
        void UpdateMatricesPropertiesForGPUInstancingSRP()
        {
            if (SRPHelper.IsUsingCustomRenderPipeline() && Config.Instance.actualRenderingMode == RenderingMode.GPUInstancing)
            {
                SetMaterialProp(ShaderProperties.LocalToWorldMatrix, transform.localToWorldMatrix);
                SetMaterialProp(ShaderProperties.WorldToLocalMatrix, transform.worldToLocalMatrix);
            }
        }

    #if UNITY_2019_1_OR_NEWER
        void OnBeginCameraRenderingSRP(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
    #else
        void OnBeginCameraRenderingSRP(Camera cam)
    #endif
        {
            m_CurrentCameraRenderingSRP = cam;
        }
#endif

        void OnWillRenderObject()
        {
            Camera currentCam = null;

#if VLB_SRP_SUPPORT
            if (SRPHelper.IsUsingCustomRenderPipeline())
            {
                currentCam = m_CurrentCameraRenderingSRP;
            }
            else
#endif
            {
                currentCam = Camera.current;
            }

            OnWillCameraRenderThisBeam(currentCam);
        }

        void OnWillCameraRenderThisBeam(Camera cam)
        {
            if (m_Master && cam)
            {
                if (
#if UNITY_EDITOR
                    Utils.IsEditorCamera(cam) || // make sure to call UpdateCameraRelatedProperties for editor scene camera 
#endif
                    cam.enabled)    // prevent from doing stuff when we render from a previous DynamicOcclusionDepthBuffer's DepthCamera, because the DepthCamera are disabled 
                {
                    Debug.Assert(cam.GetComponentInParent<VolumetricLightBeam>() == null);
                    UpdateCameraRelatedProperties(cam);
                    m_Master._INTERNAL_OnWillCameraRenderThisBeam(cam);
                }
            }
        }

        void UpdateCameraRelatedProperties(Camera cam)
        {
            if (cam && m_Master)
            {
                MaterialChangeStart();
                {
                    var camPosOS = m_Master.transform.InverseTransformPoint(cam.transform.position);

                    var camForwardVectorOSN = transform.InverseTransformDirection(cam.transform.forward).normalized;
                    float camIsInsideBeamFactor = cam.orthographic ? -1f : m_Master.GetInsideBeamFactorFromObjectSpacePos(camPosOS);
                    SetMaterialProp(ShaderProperties.CameraParams, new Vector4(camForwardVectorOSN.x, camForwardVectorOSN.y, camForwardVectorOSN.z, camIsInsideBeamFactor));

#if VLB_SRP_SUPPORT
                    // This update is to be able to move beams without trackChangesDuringPlaytime enabled with SRP & GPU Instancing
                    UpdateMatricesPropertiesForGPUInstancingSRP();
#endif

                    if (m_Master.colorMode == ColorMode.Gradient)
                    {
                        // Send the gradient matrix every frame since it's not a shader's property
                        SetMaterialProp(ShaderProperties.ColorGradientMatrix, m_ColorGradientMatrix);
                    }
                }
                MaterialChangeStop();

#if FORCE_CURRENT_CAMERA_DEPTH_TEXTURE_MODE
                if (m_Master.depthBlendDistance > 0f)
                    cam.depthTextureMode |= DepthTextureMode.Depth;
#endif
            }
        }

#if UNITY_EDITOR
        public bool _EDITOR_IsUsingCustomMaterial { get { return m_CustomMaterial != null; } }
#endif
    }
}
