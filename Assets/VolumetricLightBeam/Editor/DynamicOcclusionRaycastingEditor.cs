#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VLB
{
    [CustomEditor(typeof(DynamicOcclusionRaycasting))]
    [CanEditMultipleObjects]
    public class DynamicOcclusionRaycastingEditor : DynamicOcclusionAbstractBaseEditor<DynamicOcclusionRaycasting>
    {
        SerializedProperty dimensions = null;
        SerializedProperty layerMask = null;
        SerializedProperty considerTriggers = null;
        SerializedProperty minOccluderArea = null;
        SerializedProperty planeAlignment = null;
        SerializedProperty maxSurfaceDot = null;
        SerializedProperty planeOffset = null;
        SerializedProperty fadeDistanceToSurface = null;
        SerializedProperty minSurfaceRatio = null;

        public override bool RequiresConstantRepaint() { return Application.isPlaying || DynamicOcclusionRaycasting.editorRaycastAtEachFrame; }

        protected override void OnEnable()
        {
            base.OnEnable();
            DynamicOcclusionRaycasting.EditorLoadPrefs();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (FoldableHeader.Begin(this, EditorStrings.DynOcclusion.HeaderRaycasting))
            {
                dimensions.CustomEnum<Dimensions>(EditorStrings.DynOcclusion.Dimensions, EditorStrings.Common.DimensionsEnumDescriptions);
                EditorGUILayout.PropertyField(layerMask, EditorStrings.DynOcclusion.LayerMask);
                EditorGUILayout.PropertyField(considerTriggers, EditorStrings.DynOcclusion.ConsiderTriggers);

                if (Physics2D.queriesHitTriggers == false)
                {
                    if(m_Targets.HasAtLeastOneTargetWith((DynamicOcclusionRaycasting instance) => { return instance.dimensions == Dimensions.Dim2D && instance.considerTriggers; }))
                    {
                        EditorGUILayout.HelpBox(EditorStrings.DynOcclusion.ConsiderTriggersNoPossible, MessageType.Error);
                    }
                }

                EditorGUILayout.PropertyField(minOccluderArea, EditorStrings.DynOcclusion.MinOccluderArea);
            }

            FoldableHeader.End();

            DisplayCommonInspector();

            if (FoldableHeader.Begin(this, EditorStrings.DynOcclusion.HeaderOccluderSurface))
            {
                minSurfaceRatio.FloatSlider(
                    EditorStrings.DynOcclusion.MinSurfaceRatio,
                    Consts.DynOcclusion.RaycastingMinSurfaceRatioMin, Consts.DynOcclusion.RaycastingMinSurfaceRatioMax,
                    (value) => value * 100f,  // conversion value to slider
                    (value) => value / 100f   // conversion slider to value
                    );

                maxSurfaceDot.FloatSlider(
                    EditorStrings.DynOcclusion.MaxSurfaceDot,
                    Consts.DynOcclusion.RaycastingMaxSurfaceAngleMin, Consts.DynOcclusion.RaycastingMaxSurfaceAngleMax,
                    (value) => Mathf.Acos(value) * Mathf.Rad2Deg,   // conversion value to slider
                    (value) => Mathf.Cos(value * Mathf.Deg2Rad)     // conversion slider to value
                    );
            }

            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.DynOcclusion.HeaderClippingPlane))
            {
                EditorGUILayout.PropertyField(planeAlignment, EditorStrings.DynOcclusion.PlaneAlignment);
                EditorGUILayout.PropertyField(planeOffset, EditorStrings.DynOcclusion.PlaneOffset);
                EditorGUILayout.PropertyField(fadeDistanceToSurface, EditorStrings.DynOcclusion.FadeDistanceToSurface);
            }

            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.DynOcclusion.HeaderEditorDebug))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GlobalToggleButton(ref DynamicOcclusionRaycasting.editorShowDebugPlane, EditorStrings.DynOcclusion.EditorShowDebugPlane, EditorPrefsStrings.DynOcclusion.PrefShowDebugPlane);
                    GlobalToggleButton(ref DynamicOcclusionRaycasting.editorRaycastAtEachFrame, EditorStrings.DynOcclusion.EditorRaycastAtEachFrame, EditorPrefsStrings.DynOcclusion.PrefRaycastingEditor);
                }

                if (Application.isPlaying || DynamicOcclusionRaycasting.editorRaycastAtEachFrame)
                {
                    if (!serializedObject.isEditingMultipleObjects)
                    {
                        var instance = (target as DynamicOcclusionRaycasting);
                        Debug.Assert(instance);
                        var hit = instance.editorCurrentHitResult;
                        var lastFrameUpdate = instance.editorDebugData.lastFrameUpdate;

                        var occluderInfo = string.Format("Last update {0} frame(s) ago\n", Time.frameCount - lastFrameUpdate);
                        occluderInfo += (hit.hasCollider) ? string.Format("Current occluder: '{0}'\nEstimated occluder area: {1} units²", hit.name, hit.bounds.GetMaxArea2D()) : "No occluder found";
                        EditorGUILayout.HelpBox(occluderInfo, MessageType.Info);
                    }
                }
            }
            FoldableHeader.End();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
