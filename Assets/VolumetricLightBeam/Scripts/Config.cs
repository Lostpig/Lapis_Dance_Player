using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB
{
    [HelpURL(Consts.Help.UrlConfig)]
    public class Config : ScriptableObject
    {
        public const string ClassName = "Config";

        /// <summary>
        /// Override the layer on which the procedural geometry is created or not
        /// </summary>
        public bool geometryOverrideLayer = Consts.Config.GeometryOverrideLayerDefault;

        /// <summary>
        /// The layer the procedural geometry gameObject is in (only if geometryOverrideLayer is enabled)
        /// </summary>
        public int geometryLayerID = Consts.Config.GeometryLayerIDDefault;

        /// <summary>
        /// The tag applied on the procedural geometry gameObject
        /// </summary>
        public string geometryTag = Consts.Config.GeometryTagDefault;

        /// <summary>
        /// Determine in which order beams are rendered compared to other objects.
        /// This way for example transparent objects are rendered after opaque objects, and so on.
        /// </summary>
        public int geometryRenderQueue = (int)Consts.Config.GeometryRenderQueueDefault;

        /// <summary>
        /// Select the Render Pipeline (Built-In or SRP) in use.
        /// </summary>
        public RenderPipeline renderPipeline
        {
            get { return _RenderPipeline; }
            set
            {
#if UNITY_EDITOR
                _RenderPipeline = value;
#else
                Debug.LogError("Modifying the RenderPipeline in standalone builds is not permitted");
#endif
            }
        }
        [FormerlySerializedAs("renderPipeline")]
        [SerializeField] RenderPipeline _RenderPipeline = Consts.Config.GeometryRenderPipelineDefault;

        /// <summary>
        /// MultiPass: Use the 2 pass shader. Will generate 2 drawcalls per beam.
        /// SinglePass: Use the 1 pass shader. Will generate 1 drawcall per beam.
        /// GPUInstancing: Dynamically batch multiple beams to combine and reduce draw calls (Feature only supported in Unity 5.6 or above). More info: https://docs.unity3d.com/Manual/GPUInstancing.html
        /// SRPBatcher: Use the SRP Batcher to automatically batch multiple beams and reduce draw calls. Only available when using SRP.
        /// </summary>
        public RenderingMode renderingMode
        {
            get { return _RenderingMode; }
            set
            {
#if UNITY_EDITOR
                _RenderingMode = value;
#else
                Debug.LogError("Modifying the RenderingMode in standalone builds is not permitted");
#endif
            }
        }
        [FormerlySerializedAs("renderingMode")]
        [SerializeField] RenderingMode _RenderingMode = Consts.Config.GeometryRenderingModeDefault;


        public bool IsSRPBatcherSupported()
        {
            // The SRP Batcher Rendering Mode is only compatible when using a SRP
            if (renderPipeline == RenderPipeline.BuiltIn) return false;

            // SRP Batcher only works with URP and HDRP
            var rp = SRPHelper.renderPipelineType;
            return rp == SRPHelper.RenderPipeline.URP || rp == SRPHelper.RenderPipeline.HDRP;
        }

        /// <summary>
        /// Actual Rendering Mode used on the current platform
        /// </summary>
        public RenderingMode actualRenderingMode
        {
            get
            {
#pragma warning disable 0162 // warning CS0162: Unreachable code detected
                if (renderingMode == RenderingMode.GPUInstancing && !BatchingHelper.isGpuInstancingSupported) return RenderingMode.SinglePass;
#pragma warning restore 0162
                if (renderingMode == RenderingMode.SRPBatcher && !IsSRPBatcherSupported()) return RenderingMode.SinglePass;

                if (renderPipeline != RenderPipeline.BuiltIn)
                {
                    // Using a Scriptable Render Pipeline with 'Multi-Pass' Rendering Mode is not supported
                    if (renderingMode == RenderingMode.MultiPass) return RenderingMode.SinglePass;
                }
                return renderingMode;
            }
        }

        /// <summary>
        /// Depending on the actual Rendering Mode used, returns true if the single pass shader will be used, false otherwise.
        /// </summary>
        public bool useSinglePassShader { get { return actualRenderingMode != RenderingMode.MultiPass; } }

        public bool requiresDoubleSidedMesh { get { return useSinglePassShader; } }

        /// <summary>
        /// Main shader applied to the cone beam geometry
        /// </summary>
        public Shader beamShader
        {
            get
            {
#if UNITY_EDITOR
                if(_BeamShader == null)
                    RefreshShader(RefreshShaderFlags.All);
#endif
                return _BeamShader;
            }
        }

        /// <summary>
        /// Depending on the quality of your screen, you might see some artifacts with high contrast visual (like a white beam over a black background).
        /// These is a very common problem known as color banding.
        /// To help with this issue, the plugin offers a Dithering factor: it smooths the banding by introducing a subtle pattern of noise.
        /// </summary>
        public float ditheringFactor = Consts.Config.DitheringFactor;

        /// <summary>
        /// Number of Sides of the shared cone mesh
        /// </summary>
        public int sharedMeshSides = Consts.Config.SharedMeshSides;

        /// <summary>
        /// Number of Segments of the shared cone mesh
        /// </summary>
        public int sharedMeshSegments = Consts.Config.SharedMeshSegments;

        /// <summary>
        /// Global 3D Noise texture scaling: higher scale make the noise more visible, but potentially less realistic.
        /// </summary>
        [Range(Consts.Beam.NoiseScaleMin, Consts.Beam.NoiseScaleMax)]
        public float globalNoiseScale = Consts.Beam.NoiseScaleDefault;

        /// <summary>
        /// Global World Space direction and speed of the noise scrolling, simulating the fog/smoke movement
        /// </summary>
        public Vector3 globalNoiseVelocity = Consts.Beam.NoiseVelocityDefault;

        /// <summary>
        /// Tag used to retrieve the camera used to compute the fade out factor on beams
        /// </summary>
        public string fadeOutCameraTag = Consts.Config.FadeOutCameraTagDefault;

        public Transform fadeOutCameraTransform
        {
            get
            {
                if (m_CachedFadeOutCamera == null)
                {
                    ForceUpdateFadeOutCamera();
                }

                return m_CachedFadeOutCamera;
            }
        }

        /// <summary>
        /// Call this function if you want to manually change the fadeOutCameraTag property at runtime
        /// </summary>
        public void ForceUpdateFadeOutCamera()
        {
            var gao = GameObject.FindGameObjectWithTag(fadeOutCameraTag);
            if (gao)
                m_CachedFadeOutCamera = gao.transform;
        }

        /// <summary>
        /// 3D Texture storing noise data.
        /// </summary>
        [HighlightNull]
        public Texture3D noiseTexture3D = null;

        /// <summary>
        /// ParticleSystem prefab instantiated for the Volumetric Dust Particles feature (Unity 5.5 or above)
        /// </summary>
        [HighlightNull]
        public ParticleSystem dustParticlesPrefab = null;

        /// <summary>
        /// Noise texture for dithering feature
        /// </summary>
        public Texture2D ditheringNoiseTexture = null;

        /// <summary>
        /// Off: do not support having a gradient as color.
        /// High Only: support gradient color only for devices with Shader Level = 35 or higher.
        /// High and Low: support gradient color for all devices.
        /// </summary>
        public FeatureEnabledColorGradient featureEnabledColorGradient = Consts.Config.FeatureEnabledColorGradientDefault;

        /// <summary>
        /// Support 'Soft Intersection with Opaque Geometry' feature or not.
        /// </summary>
        public bool featureEnabledDepthBlend = Consts.Config.FeatureEnabledDefault;

        /// <summary>
        /// Support 'Noise 3D' feature or not.
        /// </summary>
        public bool featureEnabledNoise3D = Consts.Config.FeatureEnabledDefault;

        /// <summary>
        /// Support 'Dynamic Occlusion' features or not.
        /// </summary>
        public bool featureEnabledDynamicOcclusion = Consts.Config.FeatureEnabledDefault;

        /// <summary>
        /// Support 'Mesh Skewing' feature or not.
        /// </summary>
        public bool featureEnabledMeshSkewing = Consts.Config.FeatureEnabledDefault;

        /// <summary>
        /// Support 'Shader Accuracy' property set to 'High' or not.
        /// </summary>
        public bool featureEnabledShaderAccuracyHigh = Consts.Config.FeatureEnabledDefault;

        // INTERNAL
#pragma warning disable 0414
        [SerializeField] int pluginVersion = -1;
        [SerializeField] Material _DummyMaterial = null;
#pragma warning restore 0414

        [SerializeField] Shader _BeamShader = null;
        Transform m_CachedFadeOutCamera = null;

        public bool hasRenderPipelineMismatch { get { return (SRPHelper.renderPipelineType == SRPHelper.RenderPipeline.BuiltIn) != (_RenderPipeline == RenderPipeline.BuiltIn); } }

        [RuntimeInitializeOnLoadMethod]
        static void OnStartup()
        {
            Instance.m_CachedFadeOutCamera = null;
            Instance.RefreshGlobalShaderProperties();

#if UNITY_EDITOR
            Instance.RefreshShader(RefreshShaderFlags.All);
#endif

            if(Instance.hasRenderPipelineMismatch)
                Debug.LogError("It looks like the 'Render Pipeline' is not correctly set in the config. Please make sure to select the proper value depending on your pipeline in use.", Instance);
        }

        public void Reset()
        {
            geometryOverrideLayer = Consts.Config.GeometryOverrideLayerDefault;
            geometryLayerID = Consts.Config.GeometryLayerIDDefault;
            geometryTag = Consts.Config.GeometryTagDefault;
            geometryRenderQueue = (int)Consts.Config.GeometryRenderQueueDefault;

            sharedMeshSides = Consts.Config.SharedMeshSides;
            sharedMeshSegments = Consts.Config.SharedMeshSegments;

            globalNoiseScale = Consts.Beam.NoiseScaleDefault;
            globalNoiseVelocity = Consts.Beam.NoiseVelocityDefault;

            renderPipeline = Consts.Config.GeometryRenderPipelineDefault;
            renderingMode = Consts.Config.GeometryRenderingModeDefault;
            ditheringFactor = Consts.Config.DitheringFactor;

            fadeOutCameraTag = Consts.Config.FadeOutCameraTagDefault;

            featureEnabledColorGradient = Consts.Config.FeatureEnabledColorGradientDefault;
            featureEnabledDepthBlend = Consts.Config.FeatureEnabledDefault;
            featureEnabledNoise3D = Consts.Config.FeatureEnabledDefault;
            featureEnabledDynamicOcclusion = Consts.Config.FeatureEnabledDefault;
            featureEnabledMeshSkewing = Consts.Config.FeatureEnabledDefault;
            featureEnabledShaderAccuracyHigh = Consts.Config.FeatureEnabledDefault;

            ResetInternalData();

#if UNITY_EDITOR
            GlobalMesh.Destroy();
            VolumetricLightBeam._EditorSetAllMeshesDirty();
#endif
        }

        void RefreshGlobalShaderProperties()
        {
            Shader.SetGlobalFloat(ShaderProperties.GlobalUsesReversedZBuffer, SystemInfo.usesReversedZBuffer ? 1.0f : 0.0f);
            Shader.SetGlobalFloat(ShaderProperties.GlobalDitheringFactor, ditheringFactor);
            Shader.SetGlobalTexture(ShaderProperties.GlobalDitheringNoiseTex, ditheringNoiseTexture);
        }

#if UNITY_EDITOR
        public void _EditorSetRenderingModeAndRefreshShader(RenderingMode mode)
        {
            renderingMode = mode;
            RefreshShader(RefreshShaderFlags.All);
        }

        void OnValidate()
        {
            sharedMeshSides = Mathf.Clamp(sharedMeshSides, Consts.Beam.GeomSidesMin, Consts.Beam.GeomSidesMax);
            sharedMeshSegments = Mathf.Clamp(sharedMeshSegments, Consts.Beam.GeomSegmentsMin, Consts.Beam.GeomSegmentsMax);

            ditheringFactor = Mathf.Clamp01(ditheringFactor);
        }

        void AutoSelectRenderPipeline()
        {
            var newPipeline = renderPipeline;
            switch (SRPHelper.renderPipelineType)
            {
                case SRPHelper.RenderPipeline.BuiltIn:
                    newPipeline = RenderPipeline.BuiltIn;
                    break;
                case SRPHelper.RenderPipeline.HDRP:
                    newPipeline = RenderPipeline.HDRP;
                    break;
                case SRPHelper.RenderPipeline.URP:
                case SRPHelper.RenderPipeline.LWRP:
                    newPipeline = RenderPipeline.URP;
                    break;
            }

            if (newPipeline != renderPipeline)
            {
                renderPipeline = newPipeline;
                EditorUtility.SetDirty(this); // make sure to save this property change
                RefreshShader(RefreshShaderFlags.All);
            }
        }

        public static void EditorSelectInstance()
        {
            Selection.activeObject = Config.Instance; // this will create the instance if it doesn't exist
            if (Selection.activeObject == null)
                Debug.LogError("Cannot find any Config resource");
        }

        [System.Flags]
        public enum RefreshShaderFlags
        {
            Reference = 1 << 1,
            Dummy = 1 << 2,
            All = Reference | Dummy,
        }

        public void RefreshShader(RefreshShaderFlags flags)
        {
            if (flags.HasFlag(RefreshShaderFlags.Reference))
            {
                var prevShader = _BeamShader;

                var enabledFeatures = new ShaderGenerator.EnabledFeatures
                {
                    dithering = ditheringFactor > 0.0f,
                    depthBlend = featureEnabledDepthBlend,
                    noise3D = featureEnabledNoise3D,
                    colorGradient = featureEnabledColorGradient,
                    dynamicOcclusion = featureEnabledDynamicOcclusion,
                    meshSkewing = featureEnabledMeshSkewing,
                    shaderAccuracyHigh = featureEnabledShaderAccuracyHigh
                };

                _BeamShader = ShaderGenerator.Generate(_RenderPipeline, actualRenderingMode, enabledFeatures);
                if (_BeamShader != prevShader)
                {
                    EditorUtility.SetDirty(this);
                }
            }

            if (flags.HasFlag(RefreshShaderFlags.Dummy) && _BeamShader != null)
            {
                bool gpuInstanced = actualRenderingMode == RenderingMode.GPUInstancing;
                _DummyMaterial = DummyMaterial.Create(_BeamShader, gpuInstanced);
            }

            if (_DummyMaterial == null)
            {
                Debug.LogError("No dummy material referenced to VLB config, please try to reset this asset.", this);
            }

            RefreshGlobalShaderProperties();
        }
#endif // UNITY_EDITOR

        public void ResetInternalData()
        {
            noiseTexture3D = Resources.Load("Noise3D_64x64x64") as Texture3D;

            dustParticlesPrefab = Resources.Load("DustParticles", typeof(ParticleSystem)) as ParticleSystem;

            ditheringNoiseTexture = Resources.Load("VLBDitheringNoise", typeof(Texture2D)) as Texture2D;

#if UNITY_EDITOR
            RefreshShader(RefreshShaderFlags.All);
#endif
        }

        public ParticleSystem NewVolumetricDustParticles()
        {
            if (!dustParticlesPrefab)
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("Failed to instantiate VolumetricDustParticles prefab.");
                }
                return null;
            }

            var instance = Instantiate(dustParticlesPrefab);
            instance.useAutoRandomSeed = false;
            instance.name = "Dust Particles";
            instance.gameObject.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
            instance.gameObject.SetActive(true);
            return instance;
        }

        void OnEnable()
        {
            HandleBackwardCompatibility(pluginVersion, Version.Current);
            pluginVersion = Version.Current;
        }

        void HandleBackwardCompatibility(int serializedVersion, int newVersion)
        {
            if (serializedVersion == -1) return;            // freshly new spawned config: nothing to do
            if (serializedVersion == newVersion) return;    // same version: nothing to do

#if UNITY_EDITOR
            if (serializedVersion < 1830)
            {
                AutoSelectRenderPipeline();
            }

            if (serializedVersion < 1950)
            {
                ResetInternalData(); // retrieve Noise3D texture converted from binary data to texture 3D asset in 1950
                EditorUtility.SetDirty(this); // make sure to save this property change
            }

            if (newVersion > serializedVersion)
            {
                // Import to keep, we have to regenerate the shader each time the plugin is updated
                RefreshShader(RefreshShaderFlags.All);
            }
#endif
        }


        // Singleton management
        static Config ms_Instance = null;
        public static Config Instance { get { return GetInstance(true); } }

#if UNITY_EDITOR
        static bool ms_IsCreatingInstance = false;

        public bool IsCurrentlyUsedInstance() { return Instance == this; }

        public bool HasValidAssetName()
        {
            if (name.IndexOf(ConfigOverride.kAssetName) != 0)
                return false;

            return PlatformHelper.IsValidPlatformSuffix(GetAssetSuffix());
        }

        public string GetAssetSuffix()
        {
            var fullname = name;
            var strToFind = ConfigOverride.kAssetName;
            if (fullname.IndexOf(strToFind) == 0)   return fullname.Substring(strToFind.Length);
            else                                    return "";
        }
#endif

        private static Config GetInstance(bool assertIfNotFound)
        {
#if UNITY_EDITOR
            // Do not cache the instance during editing in order to handle new asset created or moved.
            if (!Application.isPlaying || ms_Instance == null)
#else
            if (ms_Instance == null)
#endif
            {
#if UNITY_EDITOR
                if (ms_IsCreatingInstance)
                {
                    Debug.LogError(string.Format("Trying to access Config.Instance while creating it. Breaking before infinite loop."));
                    return null;
                }
#endif
                {
                    var newInstance = Resources.Load<ConfigOverride>(ConfigOverride.kAssetName + PlatformHelper.GetCurrentPlatformSuffix());

                    if (newInstance == null)
                        newInstance = Resources.Load<ConfigOverride>(ConfigOverride.kAssetName);

#if UNITY_EDITOR
                    if (newInstance && newInstance != ms_Instance)
                    {
                        ms_Instance = newInstance;
                        newInstance.RefreshGlobalShaderProperties(); // make sure noise textures are properly loaded as soon as the editor is started
                    }
#endif

                    ms_Instance = newInstance;
                }

                if (ms_Instance == null)
                {
#if UNITY_EDITOR
                    ms_IsCreatingInstance = true;
                    ms_Instance = ConfigOverride.CreateInstanceOverride();
                    ms_IsCreatingInstance = false;

                    ms_Instance.AutoSelectRenderPipeline();

                    if (Application.isPlaying)
                        ms_Instance.Reset(); // Reset is not automatically when instancing a ScriptableObject when in playmode
#endif
                    Debug.Assert(!(assertIfNotFound && ms_Instance == null), string.Format("Can't find any resource of type '{0}'. Make sure you have a ScriptableObject of this type in a 'Resources' folder.", typeof(Config)));
                }
            }

            return ms_Instance;
        }
    }
}
