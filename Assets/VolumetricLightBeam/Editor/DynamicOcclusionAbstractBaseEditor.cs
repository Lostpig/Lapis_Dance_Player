#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VLB
{
    public class DynamicOcclusionAbstractBaseEditor<T> : EditorCommon where T : DynamicOcclusionAbstractBase
    {
        SerializedProperty updateRate, waitXFrames;
        protected TargetList<T> m_Targets;

        protected override void OnEnable()
        {
            base.OnEnable();

            updateRate = FindProperty((DynamicOcclusionDepthBuffer x) => x.updateRate);
            waitXFrames = FindProperty((DynamicOcclusionDepthBuffer x) => x.waitXFrames);

            m_Targets = new TargetList<T>(targets);
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
