#if UNITY_EDITOR
using UnityEditor;

namespace VLB
{
    [CustomEditor(typeof(EffectPulse))]
    [CanEditMultipleObjects]
    public class EffectPulseEditor : EffectAbstractBaseEditor<EffectPulse>
    {
        SerializedProperty frequency = null;
        SerializedProperty intensityAmplitude = null;

        protected override void DisplayChildProperties()
        {
            if (FoldableHeader.Begin(this, EditorStrings.Effects.HeaderTimings))
            {
                EditorGUILayout.PropertyField(frequency, EditorStrings.Effects.FrequencyPulse);
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Effects.HeaderVisual))
            {
                EditorGUILayout.PropertyField(intensityAmplitude, EditorStrings.Effects.IntensityAmplitude);
            }
            FoldableHeader.End();
        }
    }
}
#endif
