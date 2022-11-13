using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB
{
    [HelpURL(Consts.HelpUrlConfig)]
    public class Config : ScriptableObject
    {
        /// <summary>
        /// Override the layer on which the procedural geometry is created or not
        /// </summary>
        public bool geometryOverrideLayer = Consts.ConfigGeometryOverrideLayerDefault;

        /// <summary>
        /// The layer the procedural geometry gameObject is in (only if geometryOverrideLayer is enabled)
        /// </summary>
        public int geometryLayerID = Consts.ConfigGeometryLayerIDDefault;

        /// <summary>
        /// The tag applied on the procedural geometry gameObject
        /// </summary>
        public string geometryTag = Consts.ConfigGeometryTagDefault;

        /// <summary>
        /// Determine in which order beams are rendered compared to other objects.
        /// This way for example transparent objects are rendered after opaque objects, and so on.
        /// </summary>
        public int geometryRenderQueue = (int)Consts.ConfigGeometryRenderQueueDefault;

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
        [SerializeField] RenderPipeline _RenderPipeline = Consts.ConfigGeometryRenderPipelineDefault;

        /// <summary>
        /// MultiPass: Use the 2 pass shader. Will generate 2 drawcalls per beam.
        /// SinglePass: Use the 1 pass shader. Will generate 1 drawcall per beam. Mandatory when using Render Pipeline such as HDRP, URP and LWRP.
        /// GPUInstancing: Dynamically batch multiple beams to combine and reduce draw calls (Feature only supported in Unity 5.6 or above). More info: https://docs.unity3d.com/Manual/GPUInstancing.html
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
        [SerializeField] RenderingMode _RenderingMode = Consts.ConfigGeometryRenderingModeDefault;

        public void SetRenderingModeAndRefreshShader(RenderingMode mode)
        {
            renderingMode = mode;
#if UNITY_EDITOR
            RefreshShader(RefreshShaderFlags.All);
#endif
        }

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
        public float ditheringFactor = Consts.ConfigDitheringFactor;

        /// <summary>
        /// Number of Sides of the shared cone mesh
        /// </summary>
        public int sharedMeshSides = Consts.ConfigSharedMeshSides;

        /// <summary>
        /// Number of Segments of the shared cone mesh
        /// </summary>
        public int sharedMeshSegments = Consts.ConfigSharedMeshSegments;

        /// <summary>
        /// Global 3D Noise texture scaling: higher scale make the noise more visible, but potentially less realistic.
        /// </summary>
        [Range(Consts.NoiseScaleMin, Consts.NoiseScaleMax)]
        public float globalNoiseScale = Consts.NoiseScaleDefault;

        /// <summary>
        /// Global World Space direction and speed of the noise scrolling, simulating the fog/smoke movement
        /// </summary>
        public Vector3 globalNoiseVelocity = Consts.NoiseVelocityDefault;

        /// <summary>
        /// Tag used to retrieve the camera used to compute the fade out factor on beams
        /// </summary>
        public string fadeOutCameraTag = Consts.ConfigFadeOutCameraTagDefault;

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
        /// Binary file holding the 3D Noise texture data (a 3D array). Must be exactly Size * Size * Size bytes long.
        /// </summary>
        [HighlightNull]
        public TextAsset noise3DData = null;

        /// <summary>
        /// Size (of one dimension) of the 3D Noise data. Must be power of 2. So if the binary file holds a 32x32x32 texture, this value must be 32.
        /// </summary>
        public int noise3DSize = Consts.ConfigNoise3DSizeDefault;

        /// <summary>
        /// ParticleSystem prefab instantiated for the Volumetric Dust Particles feature (Unity 5.5 or above)
        /// </summary>
        [HighlightNull]
        public ParticleSystem dustParticlesPrefab = null;

        /// <summary>
        /// Noise texture for dithering feature
        /// </summary>
        public Texture2D ditheringNoiseTexture = null;

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
                Debug.LogError("It looks like the 'Render Pipeline' correctly is not set in the config. Please make sure to select the proper value depending on your pipeline in use.", Instance);
        }

        public void Reset()
        {
            geometryOverrideLayer = Consts.ConfigGeometryOverrideLayerDefault;
            geometryLayerID = Consts.ConfigGeometryLayerIDDefault;
            geometryTag = Consts.ConfigGeometryTagDefault;
            geometryRenderQueue = (int)Consts.ConfigGeometryRenderQueueDefault;

            sharedMeshSides = Consts.ConfigSharedMeshSides;
            sharedMeshSegments = Consts.ConfigSharedMeshSegments;

            globalNoiseScale = Consts.NoiseScaleDefault;
            globalNoiseVelocity = Consts.NoiseVelocityDefault;

            renderPipeline = Consts.ConfigGeometryRenderPipelineDefault;
            renderingMode = Consts.ConfigGeometryRenderingModeDefault;
            ditheringFactor = Consts.ConfigDitheringFactor;

            ResetInternalData();

#if UNITY_EDITOR
            GlobalMesh.Destroy();
            VolumetricLightBeam._EditorSetAllMeshesDirty();
#endif
        }

        void RefreshGlobalShaderProperties()
        {
            Shader.SetGlobalFloat(ShaderProperties.GlobalDitheringFactor, ditheringFactor);
            Shader.SetGlobalTexture(ShaderProperties.GlobalDitheringNoiseTex, ditheringNoiseTexture);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            noise3DSize = Mathf.Max(2, Mathf.NextPowerOfTwo(noise3DSize));

            sharedMeshSides = Mathf.Clamp(sharedMeshSides, Consts.GeomSidesMin, Consts.GeomSidesMax);
            sharedMeshSegments = Mathf.Clamp(sharedMeshSegments, Consts.GeomSegmentsMin, Consts.GeomSegmentsMax);

            ditheringFactor = Mathf.Clamp01(ditheringFactor);
        }
#endif

#if UNITY_EDITOR
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
                _BeamShader = ShaderGenerator.Generate(_RenderPipeline, actualRenderingMode, ditheringFactor > 0.0f);
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
#endif

        public void ResetInternalData()
        {
#if UNITY_EDITOR
            RefreshShader(RefreshShaderFlags.All);
#endif
            noise3DData = Resources.Load("Noise3D_64x64x64") as TextAsset;
            noise3DSize = Consts.ConfigNoise3DSizeDefault;

            dustParticlesPrefab = Resources.Load("DustParticles", typeof(ParticleSystem)) as ParticleSystem;

            ditheringNoiseTexture = Resources.Load("VLBDitheringNoise", typeof(Texture2D)) as Texture2D;
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
#if UNITY_5_4_OR_NEWER
            instance.useAutoRandomSeed = false;
#endif
            instance.name = "Dust Particles";
            instance.gameObject.hideFlags = Consts.ProceduralObjectsHideFlags;
            instance.gameObject.SetActive(true);
            return instance;
        }

#if UNITY_EDITOR
        public static void EditorSelectInstance()
        {
            Selection.activeObject = Config.Instance; // this will create the instance if it doesn't exist
            if(Selection.activeObject == null)
                Debug.LogError("Cannot find any Config resource");
        }
#endif

        void OnEnable()
        {
            HandleBackwardCompatibility(pluginVersion, Version.Current);
            pluginVersion = Version.Current;

#if UNITY_EDITOR
            var instanceNoAssert = m_Instance;
            if (instanceNoAssert != null)
                instanceNoAssert.RefreshShader(RefreshShaderFlags.Dummy);
#endif
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

            if (newVersion > serializedVersion)
            {
                // Import to keep, we have to regenerate the shader each time the plugin is updated
                RefreshShader(RefreshShaderFlags.All);
            }
#endif
        }

#if UNITY_EDITOR
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
#endif

        // Singleton management
        static Config m_Instance = null;
        public static Config Instance { get { return GetInstance(true); } }

#if UNITY_EDITOR
        static bool ms_IsCreatingInstance = false;
#endif

        private static Config GetInstance(bool assertIfNotFound)
        {
#if UNITY_EDITOR
            // Do not cache the instance during editing in order to handle new asset created or moved.
            if (!Application.isPlaying || m_Instance == null)
#else
            if (m_Instance == null)
#endif
            {
#if UNITY_EDITOR
                if (ms_IsCreatingInstance)
                {
                    Debug.LogError(string.Format("Trying to access Config.Instance while creating it. Breaking before infinite loop."));
                    return null;
                }
#endif
                var foundOverride = Resources.Load<ConfigOverride>(ConfigOverride.kAssetName);
                if (foundOverride)
                {
                    m_Instance = foundOverride;
                }
                else
                {
#if UNITY_EDITOR
                    ms_IsCreatingInstance = true;
                    m_Instance = ConfigOverride.CreateInstanceOverride();
                    ms_IsCreatingInstance = false;

                    m_Instance.AutoSelectRenderPipeline();

                    if (Application.isPlaying)
                        m_Instance.Reset(); // Reset is not automatically when instancing a ScriptableObject when in playmode
#endif
                    Debug.Assert(!(assertIfNotFound && m_Instance == null), string.Format("Can't find any resource of type '{0}'. Make sure you have a ScriptableObject of this type in a 'Resources' folder.", typeof(Config)));
                }
            }

            return m_Instance;
        }
    }
}
