#if UNITY_5_5_OR_NEWER
#define PARTICLES_SUPPORTED
#endif

using UnityEngine;

namespace VLB
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VolumetricLightBeam))]
    [HelpURL(Consts.HelpUrlDustParticles)]
    public class VolumetricDustParticles : MonoBehaviour
    {
        /// <summary>
        /// Max alpha of the particles
        /// </summary>
        [Range(0f, 1f)]
        public float alpha = 0.5f;

        /// <summary>
        /// Max size of the particles
        /// </summary>
        [Range(0.0001f, 0.1f)]
        public float size = 0.01f;

        /// <summary>
        /// Direction of the particles.
        /// </summary>
        public ParticlesDirection direction = ParticlesDirection.Random;

        /// <summary>
        /// Movement speed of the particles.
        /// </summary>
        public Vector3 velocity = new Vector3(0.0f, 0.0f, 0.03f);

        [System.Obsolete("Use 'velocity' instead")]
        public float speed = 0.03f;

        /// <summary>
        /// Control how many particles are spawned. The higher the density, the more particles are spawned, the higher the performance cost is.
        /// </summary>
        public float density = 5f;

        /// <summary>
        /// The minimum distance (from the light source) where the particles are spawned.
        /// The higher it is, the more the particles are spawned away from the light source.
        /// </summary>
        [Range(0f, 1f)]
        public float spawnMinDistance = 0f;
        
        /// <summary>
        /// The maximum distance (from the light source) where the particles are spawned.
        /// The lower it is, the more the particles are gathered near the light source.
        /// </summary>
        [Range(0f, 1f)]
        public float spawnMaxDistance = 0.7f;

        /// <summary>
        /// Enable particles culling based on the distance to the Main Camera.
        /// We highly recommend to enable this feature to keep good runtime performances.
        /// </summary>
        public bool cullingEnabled = true;

        /// <summary>
        /// If culling is enabled, the particles will not be rendered if they are further than cullingMaxDistance to the Main Camera.
        /// </summary>
        public float cullingMaxDistance = 10f;

        /// <summary>
        /// Is the particle system currently culled (no visible) because too far from the main camera?
        /// </summary>
        public bool isCulled { get; private set; }

#if PARTICLES_SUPPORTED
        public static bool isFeatureSupported = true;
        public bool particlesAreInstantiated { get { return m_Particles; } }
        public int particlesCurrentCount { get { return m_Particles ? m_Particles.particleCount : 0; } }
        public int particlesMaxCount { get { return m_Particles ? m_Particles.main.maxParticles : 0; } }
        ParticleSystem m_Particles = null;
        ParticleSystemRenderer m_Renderer = null;
#else
        public static bool isFeatureSupported = false;
        public bool particlesAreInstantiated { get { return false; } }
        public int particlesCurrentCount { get { return 0; } }
        public int particlesMaxCount { get { return 0; } }
#endif

        /// <summary>Cached version of Camera.main, for performance reasons</summary>
        public Camera mainCamera
        {
            get
            {
                if (!ms_MainCamera)
                {
                    ms_MainCamera = Camera.main;
                    if (!ms_MainCamera && !ms_NoMainCameraLogged)
                    {
                        Debug.LogErrorFormat(gameObject, "In order to use 'VolumetricDustParticles' culling, you must have a MainCamera defined in your scene.");
                        ms_NoMainCameraLogged = true;
                    }
                }

                return ms_MainCamera;
            }
        }

        // Cache the main camera, because accessing Camera.main at each frame is very bad performance-wise
        static bool ms_NoMainCameraLogged = false;
        static Camera ms_MainCamera = null;

#if UNITY_EDITOR
        void OnValidate()
        {
            density = Mathf.Clamp(density, 0f, 1000f);
            cullingMaxDistance = Mathf.Max(cullingMaxDistance, 1f);
#if PARTICLES_SUPPORTED
            // support instant refresh when modifying properties from inspector
            if (m_Particles)
            {
                SetParticleProperties(); // mandatory to update with the latest values
                m_Particles.Simulate(0f);
                m_Particles.Play();
            }
#endif
        }
#endif

#if PARTICLES_SUPPORTED
        VolumetricLightBeam m_Master = null;

        void Start()
        {
            isCulled = false;

            m_Master = GetComponent<VolumetricLightBeam>();
            Debug.Assert(m_Master);
            HandleBackwardCompatibility(m_Master._INTERNAL_pluginVersion, Version.Current);

            InstantiateParticleSystem();

            SetActiveAndPlay();
        }

        void InstantiateParticleSystem()
        {
            // If we duplicate (from Editor and Playmode) the VLB, the children are also duplicated (like the dust particles)
            // we have to make sure to properly destroy them before creating our proper procedural particle instance.
            var slaves = GetComponentsInChildren<ParticleSystem>(true);
            for (int i = slaves.Length - 1; i >= 0; --i)
                DestroyImmediate(slaves[i].gameObject);

            m_Particles = Config.Instance.NewVolumetricDustParticles();

            if (m_Particles)
            {
#if UNITY_EDITOR
                if (m_Master)
                {
                    UnityEditor.GameObjectUtility.SetStaticEditorFlags(m_Particles.gameObject, m_Master.GetStaticEditorFlagsForSubObjects());
                    m_Particles.gameObject.SetSameSceneVisibilityStatesThan(m_Master.gameObject);
                }
#endif
                m_Particles.transform.SetParent(transform, false);
                m_Particles.transform.localRotation = m_Master.beamInternalLocalRotation;
                m_Renderer = m_Particles.GetComponent<ParticleSystemRenderer>();
            }
        }

        void OnEnable() { SetActiveAndPlay(); }

        void SetActiveAndPlay()
        {
            if (m_Particles)
            {
                m_Particles.gameObject.SetActive(true);
                SetParticleProperties();
                m_Particles.Play(true);
            }
        }

        void OnDisable()
        {
            if (m_Particles) m_Particles.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (m_Particles) DestroyImmediate(m_Particles.gameObject); // Make sure to delete the GAO
            m_Particles = null;
        }

        void Update()
        {
#if UNITY_EDITOR
            if (!m_Particles && !Application.isPlaying)
                InstantiateParticleSystem();
#endif
            if(Application.isPlaying)
                UpdateCulling();

            SetParticleProperties();
        }

        void SetParticleProperties()
        {
            if (m_Particles && m_Particles.gameObject.activeSelf)
            {
                var thickness = Mathf.Clamp01(1 - (m_Master.fresnelPow / Consts.FresnelPowMaxValue));

                if (spawnMinDistance > spawnMaxDistance)
                    Utils.Swap(ref spawnMinDistance, ref spawnMaxDistance);
                
                var coneLength = m_Master.fallOffEnd * (spawnMaxDistance - spawnMinDistance);
                var ratePerSec = coneLength * density;
                int maxParticles = (int)(ratePerSec * 4);

                var main = m_Particles.main;

                var startLifetime = main.startLifetime;
                startLifetime.mode = ParticleSystemCurveMode.TwoConstants;
                startLifetime.constantMin = 4f;
                startLifetime.constantMax = 6f;
                main.startLifetime = startLifetime;

                var startSize = main.startSize;
                startSize.mode = ParticleSystemCurveMode.TwoConstants;
                startSize.constantMin = size * 0.9f;
                startSize.constantMax = size * 1.1f;
                main.startSize = startSize;
                
                var startColor = main.startColor;

                if(m_Master.colorMode == ColorMode.Flat)
                {
                    startColor.mode = ParticleSystemGradientMode.Color;
                    var colorMax = m_Master.color;
                    colorMax.a *= alpha;
                    startColor.color = colorMax;
                }
                else
                {
                    startColor.mode = ParticleSystemGradientMode.Gradient;

                    // Duplicate gradient and apply alpha
                    var gradientRef = m_Master.colorGradient;
                    var colorKeys = gradientRef.colorKeys;
                    var alphaKeys = gradientRef.alphaKeys;

                    for(int i=0; i< alphaKeys.Length; ++i)
                        alphaKeys[i].alpha *= alpha;

                    var newGradient = new Gradient();
                    newGradient.SetKeys(colorKeys, alphaKeys);
                    startColor.gradient = newGradient;
                }
                main.startColor = startColor;

                {
                    var startSpeed = main.startSpeed;
                    startSpeed.constant = (direction == ParticlesDirection.Random) ? Mathf.Abs(velocity.z) : 0.0f;
                    main.startSpeed = startSpeed;
                }

                {
                    var velocityOverLifetime = m_Particles.velocityOverLifetime;
                    velocityOverLifetime.enabled = (direction != ParticlesDirection.Random);
                    velocityOverLifetime.space = (direction == ParticlesDirection.LocalSpace) ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
                    velocityOverLifetime.xMultiplier = velocity.x;
                    velocityOverLifetime.yMultiplier = velocity.y;
                    velocityOverLifetime.zMultiplier = velocity.z;
                }

                main.maxParticles = maxParticles;

                {
                    var shape = m_Particles.shape;
                    shape.shapeType = ParticleSystemShapeType.ConeVolume;

                    float coneAngle = m_Master.coneAngle * Mathf.Lerp(0.7f, 1f, thickness);
                    shape.angle = coneAngle * 0.5f;

                    float radiusStart = m_Master.coneRadiusStart * Mathf.Lerp(0.3f, 1.0f, thickness);
                    float radiusEnd = Utils.ComputeConeRadiusEnd(m_Master.fallOffEnd, coneAngle);
                    shape.radius = Mathf.Lerp(radiusStart, radiusEnd, spawnMinDistance);

                    shape.length = coneLength;

                    var localOffset = m_Master.fallOffEnd * spawnMinDistance;
#if UNITY_2017_1_OR_NEWER
                    shape.position = new Vector3(0f, 0f, localOffset);
#else
                    m_Particles.transform.localPosition = m_Master.beamLocalForward * localOffset;
#endif
                    shape.arc = 360f;
                    shape.randomDirectionAmount = (direction == ParticlesDirection.Random) ? 1f : 0f;
                }

                var emission = m_Particles.emission;
                var rate = emission.rateOverTime;
                rate.constant = ratePerSec;
                emission.rateOverTime = rate;

                if(m_Renderer)
                {
                    m_Renderer.sortingLayerID = m_Master.sortingLayerID;
                    m_Renderer.sortingOrder = m_Master.sortingOrder;
                }
            }
        }

        void HandleBackwardCompatibility(int serializedVersion, int newVersion)
        {
            if (serializedVersion == -1) return;            // freshly new spawned entity: nothing to do
            if (serializedVersion == newVersion) return;    // same version: nothing to do

            if (serializedVersion < 1880)
            {
                // Version 1880 changed the order of ParticlesDirection enum and add WorldSpace option
                if ((int)direction == 0)    direction = (ParticlesDirection)1;
                else                        direction = (ParticlesDirection)0;

#pragma warning disable 0618
                // Version 1880 changed from single float speed to 3D velocity vector
                velocity = new Vector3(0.0f, 0.0f, speed);
#pragma warning restore 0618
            }

            Utils.MarkCurrentSceneDirty();
        }

        #region Culling
        void UpdateCulling()
        {
            if (m_Particles)
            {
                bool visible = true;
                if ((cullingEnabled || m_Master.isFadeOutEnabled) && m_Master.hasGeometry)
                {
                    if (mainCamera)
                    {
                        var maxDist = cullingMaxDistance;
                        if (m_Master.isFadeOutEnabled) maxDist = Mathf.Min(maxDist, m_Master.fadeOutEnd);
                        var maxDistSqr = maxDist * maxDist;
                        var distSqr = m_Master.bounds.SqrDistance(mainCamera.transform.position);
                        visible = distSqr <= maxDistSqr;
                    }
                    else
                        cullingEnabled = false;
                }

                if (m_Particles.gameObject.activeSelf != visible)
                {
                    m_Particles.gameObject.SetActive(visible);
                    isCulled = !visible;
                }

                if (visible && !m_Particles.isPlaying)
                    m_Particles.Play();
            }
        } 
#endregion
#endif
    }
    }

