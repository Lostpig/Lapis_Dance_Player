#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VLB
{
    [CustomEditor(typeof(SkewingHandle))]
    public class SkewingHandleEditor : EditorCommon
    {
        SerializedProperty volumetricLightBeam = null;
        SerializedProperty shouldUpdateEachFrame = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var thisTarget = target as SkewingHandle;
            if (thisTarget == null)
                return;

            if(Config.Instance.featureEnabledMeshSkewing == false)
                EditorGUILayout.HelpBox(EditorStrings.SkewingHandle.HelpFeatureDisabled, MessageType.Warning);

            EditorGUILayout.PropertyField(volumetricLightBeam, EditorStrings.SkewingHandle.Beam);
            EditorGUILayout.PropertyField(shouldUpdateEachFrame, EditorStrings.SkewingHandle.ShouldUpdateEachFrame);

            if (thisTarget.volumetricLightBeam)
            {
                if(thisTarget.IsAttachedToSelf())
                {
                    EditorGUILayout.HelpBox(EditorStrings.SkewingHandle.ErrorAttachedToSelf, MessageType.Error);
                }
                else if (!thisTarget.CanSetSkewingVector())
                {
                    EditorGUILayout.HelpBox(EditorStrings.SkewingHandle.ErrorCannotSkew, MessageType.Error);
                }
                else if (thisTarget.shouldUpdateEachFrame && !thisTarget.CanUpdateEachFrame())
                {
                    EditorGUILayout.HelpBox(EditorStrings.SkewingHandle.ErrorCannotUpdate, MessageType.Error);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

