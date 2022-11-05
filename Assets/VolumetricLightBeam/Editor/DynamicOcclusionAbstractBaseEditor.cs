#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VLB
{
    public class DynamicOcclusionAbstractBaseEditor<T> : EditorCommon where T : DynamicOcclusionAbstractBase
    {
        SerializedProperty updateRate = null;
        SerializedProperty waitXFrames = null;
        protected TargetList<T> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Targets = new TargetList<T>(targets);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Config.Instance.featureEnabledDynamicOcclusion)
            {
                EditorGUILayout.HelpBox(EditorStrings.DynOcclusion.HelpFeatureDisabled, MessageType.Warning);
            }
        }

        protected void DisplayCommonInspector()
        {
            if (FoldableHeader.Begin(this, EditorStrings.DynOcclusion.HeaderUpdateRate))
            {
                updateRate.CustomEnum<DynamicOcclusionUpdateRate>(EditorStrings.DynOcclusion.UpdateRate, EditorStrings.DynOcclusion.UpdateRateDescriptions);

                if (m_Targets.HasAtLeastOneTargetWith((T comp) => { return comp.updateRate.HasFlag(DynamicOcclusionUpdateRate.EveryXFrames); }))
                {
                    EditorGUILayout.PropertyField(waitXFrames, EditorStrings.DynOcclusion.WaitXFrames);
                }

                EditorGUILayout.HelpBox(
                    string.Format(EditorStrings.DynOcclusion.GetUpdateRateAdvice<T>(m_Targets[0].updateRate), m_Targets[0].waitXFrames),
                    MessageType.Info);
            }

            FoldableHeader.End();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
