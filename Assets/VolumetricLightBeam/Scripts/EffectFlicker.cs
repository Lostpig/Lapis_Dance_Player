using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLB
{
    [HelpURL(Consts.Help.UrlEffectFlicker)]
    public class EffectFlicker : EffectAbstractBase
    {
        public new const string ClassName = "EffectFlicker";

        /// <summary>
        /// Frequency of flickering.
        /// Higher value means the flickering will occur faster.
        /// </summary>
        [Range(1.0f, 60.0f)]
        public float frequency = Consts.Effects.FrequencyDefault;

        /// <summary>
        /// If enabled, pauses will be added between 2 flickering sequences.
        /// </summary>
        public bool performPauses = Consts.Effects.PerformPausesDefault;

        /// <summary>
        /// The duration of a flickering sequence.
        /// A random value will be picked each time inside that range.
        /// </summary>
        [MinMaxRange(0.0f, 10.0f)]
        public MinMaxRangeFloat flickeringDuration = Consts.Effects.FlickeringDurationDefault;

        /// <summary>
        /// The duration of a pause sequence.
        /// A random value will be picked each time inside that range.
        /// </summary>
        [MinMaxRange(0.0f, 10.0f)]
        public MinMaxRangeFloat pauseDuration = Consts.Effects.PauseDurationDefault;

        /// <summary>
        /// The amplitude of intensity change which will be applied to the Light and/or Beam.  
        /// A random value will be picked each time inside that range.
        /// </summary>
        [MinMaxRange(-5.0f, 5.0f)]
        public MinMaxRangeFloat intensityAmplitude = Consts.Effects.IntensityAmplitudeDefault;

        /// <summary>
        /// How much intensity change will be smoothed.
        /// Higher value means the more smoothing.
        /// </summary>
        [Range(0.0f, 0.25f)]
        public float smoothing = Consts.Effects.SmoothingDefault;


        float m_CurrentAdditiveIntensity = 0.0f;

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(CoUpdate());
        }

        IEnumerator CoUpdate()
        {
            while(true)
            {
                yield return CoFlicker();

                float remaining = pauseDuration.randomValue;
                do
                {
                    remaining -= Time.deltaTime;
                    yield return null;
                }
                while (performPauses && remaining > 0.0f);
            }
        }

        IEnumerator CoFlicker()
        {
            float remainingDuration = flickeringDuration.randomValue;
            float lastTime = Time.deltaTime;

            while (!performPauses || remainingDuration > 0.0f)
            {
                Debug.Assert(frequency > 0.0f);
                float freqDuration = 1.0f / frequency;
                yield return CoChangeIntensity(freqDuration, intensityAmplitude.randomValue);
                remainingDuration -= freqDuration;
            }
        }

        IEnumerator CoChangeIntensity(float expectedDuration, float nextIntensity)
        {
            float velocity = 0.0f;
            float t = 0.0f;

            while (t < expectedDuration)
            {
                m_CurrentAdditiveIntensity = Mathf.SmoothDamp(m_CurrentAdditiveIntensity, nextIntensity, ref velocity, smoothing);
                SetAdditiveIntensity(m_CurrentAdditiveIntensity);
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}

