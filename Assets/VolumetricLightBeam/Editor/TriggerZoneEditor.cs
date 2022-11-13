#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VLB
{
    [CustomEditor(typeof(TriggerZone))]
    [CanEditMultipleObjects]
    public class TriggerZoneEditor : EditorCommon
    {
        SerializedProperty setIsTrigger, rangeMultiplier;
        TargetList<VolumetricLightBeam> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();

            setIsTrigger = FindProperty((TriggerZone x) => x.setIsTrigger);
            rangeMultiplier = FindProperty((TriggerZone x) => x.rangeMultiplier);

            m_Targets = new TargetList<VolumetricLightBeam>(targets);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(setIsTrigger, EditorStrings.TriggerZone.SetIsTrigger);
            EditorGUILayout.PropertyField(rangeMultiplier, EditorStrings.TriggerZone.RangeMultiplier);

            if (FoldableHeader.Begin(this, EditorStrings.TriggerZone.HeaderInfos))
            {
                EditorGUILayout.HelpBox(
                    m_Targets.m_Targets[0].dimensions == Dimensions.Dim3D ? EditorStrings.TriggerZone.HelpDescription3D : EditorStrings.TriggerZone.HelpDescription2D
                    , MessageType.Info);

                if(m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.trackChangesDuringPlaytime; }))
                {
                    EditorGUILayout.HelpBox(EditorStrings.TriggerZone.HelpTrackChangesDuringPlaytimeEnabled, MessageType.Warning);
                }
            }
            FoldableHeader.End();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
