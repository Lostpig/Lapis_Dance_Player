using UnityEngine;
using UnityEngine.Serialization;

namespace VLB
{
    [AddComponentMenu("")] // hide it from Component search
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VolumetricLightBeam))]
    public abstract class DynamicOcclusionAbstractBase : MonoBehaviour
    {
        /// <summary>
        /// How often will the occlusion be processed?
        /// Try to update the occlusion as rarely as possible to keep good performance.
        /// </summary>
        public DynamicOcclusionUpdateRate updateRate = Consts.DynamicOcclusionUpdateRateDefault;

        /// <summary>
        /// How many frames we wait between 2 occlusion tests?
        /// If you want your beam to be super responsive to the changes of your environment, update it every frame by setting 1.
        /// If you want to save on performance, we recommend to wait few frames between each update by setting a higher value.
        /// </summary>
        [FormerlySerializedAs("waitFrameCount")]
        public int waitXFrames = Consts.DynOcclusionWaitFramesCountDefault;

        /// <summary>
        /// Manually process the occlusion.
        /// You have to call this function in order to update the occlusion when using DynamicOcclusionUpdateRate.Never.
        /// </summary>
        public void ProcessOcclusionManually() { ProcessOcclusion(ProcessOcclusionSource.User); }

        public event System.Action onOcclusionProcessed;

        public static bool _INTERNAL_ApplyRandomFrameOffset = true;


        protected enum ProcessOcclusionSource
        {
            RenderLoop,
            OnEnable,
            EditorUpdate,
            User,
        }

        protected void ProcessOcclusion(ProcessOcclusionSource source)
        {
            Debug.Assert(!Application.isPlaying || m_LastFrameRendered != Time.frameCount, "ProcessOcclusion has been called twice on the same frame, which is forbidden");
            Debug.Assert(m_Master);

            bool occlusionSuccess = OnProcessOcclusion(source);

            if(onOcclusionProcessed != null)
                onOcclusionProcessed();

            if (m_Master)
                m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), occlusionSuccess ? OnModifyMaterialCallback : (MaterialModifier.Callback)(null));

            if (updateRate.HasFlag(DynamicOcclusionUpdateRate.OnBeamMove))
                m_TransformPacked = transform.GetWorldPacked();

            bool firstTime = m_LastFrameRendered < 0;
            m_LastFrameRendered = Time.frameCount;

            if (firstTime && _INTERNAL_ApplyRandomFrameOffset)
            {
                m_LastFrameRendered += Random.Range(0, waitXFrames); // add a random offset to prevent from updating texture for all beams having the same wait value
            }
        }

        TransformUtils.Packed m_TransformPacked = null;
        int m_LastFrameRendered = int.MinValue;
        public int _INTERNAL_LastFrameRendered { get { return m_LastFrameRendered; } } // for unit tests
        protected VolumetricLightBeam m_Master = null;


        protected abstract string GetShaderKeyword();
        protected abstract MaterialManager.DynamicOcclusion GetDynamicOcclusionMode();

        protected abstract bool OnProcessOcclusion(ProcessOcclusionSource source);
        protected abstract void OnModifyMaterialCallback(MaterialModifier.Interface owner);
        protected abstract void OnEnablePostValidate();


        protected virtual void OnValidateProperties()
        {
            waitXFrames = Mathf.Clamp(waitXFrames, 1, 60);
        }

        protected virtual void Awake()
        {
            m_Master = GetComponent<VolumetricLightBeam>();
            Debug.Assert(m_Master);

            m_Master._INTERNAL_DynamicOcclusionMode = GetDynamicOcclusionMode();
        }

        protected virtual void OnDestroy()
        {
            m_Master._INTERNAL_DynamicOcclusionMode = MaterialManager.DynamicOcclusion.Off;
            DisableOcclusion();
        }

        protected virtual void OnEnable()
        {
            OnValidateProperties();

            OnEnablePostValidate();

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                m_Master.onWillCameraRenderThisBeam += OnWillCameraRender;

                if (!updateRate.HasFlag(DynamicOcclusionUpdateRate.Never))
                    m_Master.RegisterOnBeamGeometryInitializedCallback(() => ProcessOcclusion(ProcessOcclusionSource.OnEnable));
            }
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                m_Master.onWillCameraRenderThisBeam -= OnWillCameraRender;
            }

            DisableOcclusion();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            OnValidateProperties();
        }
#endif


        void OnWillCameraRender(Camera cam)
        {
            Debug.Assert(Application.isPlaying);

            if (cam != null && cam.enabled
                && Time.frameCount != m_LastFrameRendered) // prevent from updating multiple times if there are more than 1 camera
            {
                bool shouldUpdate = false;

                if (!shouldUpdate && updateRate.HasFlag(DynamicOcclusionUpdateRate.OnBeamMove))
                {
                    if (!transform.IsSame(m_TransformPacked))
                        shouldUpdate = true;
                }

                if (!shouldUpdate && updateRate.HasFlag(DynamicOcclusionUpdateRate.EveryXFrames))
                {
                    if (Time.frameCount >= m_LastFrameRendered + waitXFrames)
                        shouldUpdate = true;
                }

                if (shouldUpdate)
                    ProcessOcclusion(ProcessOcclusionSource.RenderLoop);
            }
        }

        void DisableOcclusion()
        {
            m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), null);
        }
    }
}

