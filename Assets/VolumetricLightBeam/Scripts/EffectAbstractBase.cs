using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLB
{
    [AddComponentMenu("")] // hide it from Component search
    [DisallowMultipleComponent]
    public class EffectAbstractBase : MonoBehaviour
    {
        public const string ClassName = "EffectAbstractBase";

        [System.Flags]
        public enum ComponentsToChange
        {
            UnityLight = 1 << 0,
            VolumetricLightBeam = 1 << 1,
            VolumetricDustParticles = 1 << 2,
        }

        /// <summary>
        /// Decide which component to change among:
        /// - Unity's Light
        /// - Volumetric Light Beam
        /// - Volumetric Dust Particles
        /// </summary>
        public ComponentsToChange componentsToChange = Consts.Effects.ComponentsToChangeDefault;

        /// <summary>
        /// Restore the default intensity when this component is disabled.
        /// </summary>
        public bool restoreBaseIntensity = Consts.Effects.RestoreBaseIntensityDefault;


        protected VolumetricLightBeam m_Beam = null;
        protected Light m_Light = null;
        protected VolumetricDustParticles m_Particles = null;
        protected float m_BaseIntensityBeamInside = 0.0f;
        protected float m_BaseIntensityBeamOutside = 0.0f;
        protected float m_BaseIntensityLight = 0.0f;

        protected void SetAdditiveIntensity(float additive)
        {
            if (componentsToChange.HasFlag(ComponentsToChange.VolumetricLightBeam) && m_Beam)
            {
                m_Beam.intensityInside = Mathf.Max(0.0f, m_BaseIntensityBeamInside + additive);
                m_Beam.intensityOutside = Mathf.Max(0.0f, m_BaseIntensityBeamOutside + additive);
            }

            if (componentsToChange.HasFlag(ComponentsToChange.UnityLight) && m_Light)
                m_Light.intensity = Mathf.Max(0.0f, m_BaseIntensityLight + additive);

            if (componentsToChange.HasFlag(ComponentsToChange.VolumetricDustParticles) && m_Particles)
                m_Particles.alphaAdditionalRuntime = 1.0f + additive;
        }

        void Awake()
        {
            m_Beam = GetComponent<VolumetricLightBeam>();
            m_Light = GetComponent<Light>();
            m_Particles = GetComponent<VolumetricDustParticles>();
            m_BaseIntensityBeamInside = m_Beam ? m_Beam.intensityInside : 0.0f;
            m_BaseIntensityBeamOutside = m_Beam ? m_Beam.intensityOutside : 0.0f;
            m_BaseIntensityLight = m_Light ? m_Light.intensity : 0.0f;
        }

        protected virtual void OnEnable()
        {
            StopAllCoroutines();
        }

        void OnDisable()
        {
            StopAllCoroutines();

            if (restoreBaseIntensity)
                SetAdditiveIntensity(0.0f);
        }
    }
}
