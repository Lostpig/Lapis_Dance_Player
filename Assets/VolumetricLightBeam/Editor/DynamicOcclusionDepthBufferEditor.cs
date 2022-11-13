#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VLB
{
    [CustomEditor(typeof(DynamicOcclusionDepthBuffer))]
    [CanEditMultipleObjects]
    public class DynamicOcclusionDepthBufferEditor : DynamicOcclusionAbstractBaseEditor<DynamicOcclusionDepthBuffer>
    {
        SerializedProperty depthMapResolution, layerMask, useOcclusionCulling, fadeDistanceToSurface;

        protected override void OnEnable()
        {
            base.OnEnable();

            depthMapResolution      = FindProperty((DynamicOcclusionDepthBuffer x) => x.depthMapResolution);
            layerMask               = FindProperty((DynamicOcclusionDepthBuffer x) => x.layerMask);
            useOcclusionCulling     = FindProperty((DynamicOcclusionDepthBuffer x) => x.useOcclusionCulling);
            fadeDistanceToSurface   = FindProperty((DynamicOcclusionDepthBuffer x) => x.fadeDistanceToSurface);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (m_Targets.HasAtLeastOneTargetWith((DynamicOcclusionDepthBuffer comp) => { return comp.GetComponent<VolumetricLightBeam>().dimensions == Dimensions.Dim2D; }))
            {
                EditorGUILayout.HelpBox(EditorStrings.DynOcclusion.HelpDepthBufferAndBeam2D, MessageType.Warning);
            }

            if (FoldableHeader.Begin(this, EditorStrings.DynOcclusion.HeaderCamera))
            {
                EditorGUILayout.PropertyField(layerMask, EditorStrings.DynOcclusion.LayerMask);

                if(Config.Instance.geometryOverrideLayer == false)
                {
                    EditorGUILayout.HelpBox(EditorStrings.DynOcclusion.HelpOverrideLayer, MessageType.Warning);
                }
                else if (m_Targets.HasAtLeastOneTargetWith((DynamicOcclusionDepthBuffer comp) => { return comp.HasLayerMaskIssues(); }))
                {
                    EditorGUILayout.HelpBox(EditorStrings.DynOcclusion.HelpLayerMaskIssues, MessageType.Warning);
                }

                EditorGUILayout.PropertyField(useOcclusionCulling, EditorStrings.DynOcclusion.DepthBufferOcclusionCulling);
                EditorGUILayout.PropertyField(depthMapResolution, EditorStrings.DynOcclusion.DepthBufferDepthMapResolution);
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.DynOcclusion.HeaderOccluderSurface))
            {
                EditorGUILayout.PropertyField(fadeDistanceToSurface, EditorStrings.DynOcclusion.FadeDistanceToSurface);
            }
            FoldableHeader.End();

            DisplayCommonInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
