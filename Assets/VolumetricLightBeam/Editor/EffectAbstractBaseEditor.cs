#if UNITY_EDITOR
using UnityEditor;

namespace VLB
{
    public abstract class EffectAbstractBaseEditor<T> : EditorCommon where T : EffectAbstractBase
    {
        SerializedProperty componentsToChange = null;
        SerializedProperty restoreBaseIntensity = null;

        protected TargetList<T> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Targets = new TargetList<T>(targets);
        }

        protected abstract void DisplayChildProperties();

        public sealed override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (m_Targets.HasAtLeastOneTargetWith((T comp) => { return comp.GetComponent<UnityEngine.Light>() == null && comp.GetComponent<VolumetricLightBeam>() == null; }))
            {
                EditorGUILayout.HelpBox(EditorStrings.Effects.HelpNoValidComponents, MessageType.Error);
                return;
            }

            if (m_Targets.HasAtLeastOneTargetWith((T comp) =>
            {
                if (comp.componentsToChange.HasFlag(EffectAbstractBase.ComponentsToChange.UnityLight))
                {
                    var light = comp.GetComponent<UnityEngine.Light>();
#if UNITY_5_6_OR_NEWER
                    return (light && light.lightmapBakeType == UnityEngine.LightmapBakeType.Baked);
#else
                    return (light && light.lightmappingMode == UnityEngine.LightmappingMode.Baked);
#endif
                }
                return false;
            }))
            {
                EditorGUILayout.HelpBox(EditorStrings.Effects.HelpLightNotChangeable, MessageType.Warning);
            }

            if (m_Targets.HasAtLeastOneTargetWith((T comp) =>
            {
                if (comp.componentsToChange.HasFlag(EffectAbstractBase.ComponentsToChange.VolumetricLightBeam))
                {
                    var beam = comp.GetComponent<VolumetricLightBeam>();
                    return (beam && !beam.trackChangesDuringPlaytime);
                }
                return false;
            }))
            {
                EditorGUILayout.HelpBox(EditorStrings.Effects.HelpBeamNotChangeable, MessageType.Warning);
            }

            DisplayChildProperties();

            if (FoldableHeader.Begin(this, EditorStrings.Effects.HeaderMisc))
            {
                componentsToChange.CustomMask<EffectAbstractBase.ComponentsToChange>(EditorStrings.Effects.ComponentsToChange);
                EditorGUILayout.PropertyField(restoreBaseIntensity, EditorStrings.Effects.RestoreBaseIntensity);
            }
            FoldableHeader.End();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
