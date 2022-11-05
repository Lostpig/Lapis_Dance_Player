//#define DEBUG_SHOW_CUSTOM_MATERIAL_INFO

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 0429, 0162 // Unreachable expression code detected (because of Noise3D.isSupported on mobile)

namespace VLB
{
    [CustomEditor(typeof(VolumetricLightBeam))]
    [CanEditMultipleObjects]
    public class VolumetricLightBeamEditor : EditorCommon
    {
        SerializedProperty trackChangesDuringPlaytime = null;
        SerializedProperty colorFromLight = null, colorMode = null, color = null, colorGradient = null;
        SerializedProperty intensityFromLight = null, intensityModeAdvanced = null, intensityInside = null, intensityOutside = null;
        SerializedProperty blendingMode = null, shaderAccuracy = null;
        SerializedProperty fresnelPow = null, glareFrontal = null, glareBehind = null;
        SerializedProperty spotAngleFromLight = null, spotAngle = null;
        SerializedProperty coneRadiusStart = null, geomMeshType = null, geomCustomSides = null, geomCustomSegments = null, geomCap = null;
        SerializedProperty fallOffEndFromLight = null, fallOffStart = null, fallOffEnd = null;
        SerializedProperty attenuationEquation = null, attenuationCustomBlending = null;
        SerializedProperty depthBlendDistance = null, cameraClippingDistance = null;
        SerializedProperty noiseMode = null, noiseIntensity = null, noiseScaleUseGlobal = null, noiseScaleLocal = null, noiseVelocityUseGlobal = null, noiseVelocityLocal = null;
        SerializedProperty fadeOutBegin = null, fadeOutEnd = null;
        SerializedProperty dimensions = null, sortingLayerID = null, sortingOrder = null;
        SerializedProperty skewingLocalForwardDirection = null, clippingPlaneTransform = null, tiltFactor = null;

        TargetList<VolumetricLightBeam> m_Targets;
        string[] m_SortingLayerNames;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_SortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();
            m_Targets = new TargetList<VolumetricLightBeam>(targets);
        }

        static void PropertyThickness(SerializedProperty sp)
        {
            sp.FloatSlider(
                EditorStrings.Beam.SideThickness,
                0, 1,
                (value) => Mathf.Clamp01(1 - (value / Consts.Beam.FresnelPowMaxValue)),    // conversion value to slider
                (value) => (1 - value) * Consts.Beam.FresnelPowMaxValue                    // conversion slider to value
                );
        }


        class ButtonToggleScope : System.IDisposable
        {
            SerializedProperty m_Property;
            bool m_DisableGroup = false;
            GUIContent m_Content = null;

            void Enable()
            {
                EditorGUILayout.BeginHorizontal();
                if(m_DisableGroup)
                    EditorGUI.BeginDisabledGroup(m_Property.HasAtLeastOneValue(true));
            }

            void Disable()
            {
                if(m_DisableGroup)
                    EditorGUI.EndDisabledGroup();
                DrawToggleButton();
                EditorGUILayout.EndHorizontal();
            }

            public ButtonToggleScope(SerializedProperty prop, bool disableGroup, GUIContent content)
            {
                m_Property = prop;
                m_DisableGroup = disableGroup;
                m_Content = content;
                Enable();
            }

            public void Dispose() { Disable(); }

            static GUIStyle ms_ToggleButtonStyleNormal = null;
            static GUIStyle ms_ToggleButtonStyleToggled = null;
            static GUIStyle ms_ToggleButtonStyleMixedValue = null;

            void DrawToggleButton()
            {
                if (ms_ToggleButtonStyleNormal == null)
                {
                    ms_ToggleButtonStyleNormal = new GUIStyle(EditorStyles.miniButton);
                    ms_ToggleButtonStyleToggled = new GUIStyle(ms_ToggleButtonStyleNormal);
                    ms_ToggleButtonStyleToggled.normal.background = ms_ToggleButtonStyleToggled.active.background;
                    ms_ToggleButtonStyleMixedValue = new GUIStyle(ms_ToggleButtonStyleToggled);
                    ms_ToggleButtonStyleMixedValue.fontStyle = FontStyle.Italic;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = m_Property.hasMultipleDifferentValues;

                var style = EditorGUI.showMixedValue ? ms_ToggleButtonStyleMixedValue : (m_Property.boolValue ? ms_ToggleButtonStyleToggled : ms_ToggleButtonStyleNormal);
                var calcSize = style.CalcSize(m_Content);

            #if UNITY_2019_3_OR_NEWER
                var defaultColor = GUI.backgroundColor;
                if(m_Property.boolValue)
                    GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f);
            #endif

                GUILayout.Button(
                    m_Content,
                    style,
                    GUILayout.MaxWidth(calcSize.x));

            #if UNITY_2019_3_OR_NEWER
                GUI.backgroundColor = defaultColor;
            #endif

                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                    m_Property.boolValue = !m_Property.boolValue;
            }
        }

        static ButtonToggleScope ButtonToggleScopeFromLight(SerializedProperty prop, bool visible)
        {
            if (!visible) return null;

            return new ButtonToggleScope(prop,
                true,   // disableGroup
                EditorData.Instance.contentFromSpotLight);
        }

        static ButtonToggleScope ButtonToggleScopeAdvanced(SerializedProperty prop, bool visible)
        {
            if (!visible) return null;

            return new ButtonToggleScope(prop,
                false,  // disableGroup
                EditorStrings.Beam.IntensityModeAdvanced);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Debug.Assert(m_Targets.Count > 0);

#if DEBUG_SHOW_CUSTOM_MATERIAL_INFO
            if (m_Targets.Count == 1)
            {
                string msg = "";
                var geom = m_Targets[0].GetComponentInChildren<BeamGeometry>();
                if (geom == null)
                    msg = "No BeamGeometry";
                else
                    msg = geom._EDITOR_IsUsingCustomMaterial ? "Custom Material" : "GPU Instanced Shared Material";
                EditorGUILayout.HelpBox(msg, MessageType.Info);
            }
#endif

            VolumetricLightBeam.AttachedLightType lightType;
            bool hasLightSpot = m_Targets[0].GetLightSpotAttachedSlow(out lightType) != null;
            if (lightType == VolumetricLightBeam.AttachedLightType.OtherLight)
            {
                EditorGUILayout.HelpBox(EditorStrings.Beam.HelpNoSpotlight, MessageType.Warning);
            }

            if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderBasic))
            {
                // Color
                using (ButtonToggleScopeFromLight(colorFromLight, hasLightSpot))
                {
                    if (!hasLightSpot) EditorGUILayout.BeginHorizontal();    // mandatory to have the color picker on the same line (when the button "from light" is not here)
                    {
                        if (Config.Instance.featureEnabledColorGradient == FeatureEnabledColorGradient.Off)
                        {
                            EditorGUILayout.PropertyField(color, EditorStrings.Beam.ColorMode);
                        }
                        else
                        {
                            using (new LabelWidth(65f))
                            {
                                EditorGUILayout.PropertyField(colorMode, EditorStrings.Beam.ColorMode);
                            }

                            if (colorMode.enumValueIndex == (int)ColorMode.Gradient)
                                EditorGUILayout.PropertyField(colorGradient, EditorStrings.Beam.ColorGradient);
                            else
                                EditorGUILayout.PropertyField(color, EditorStrings.Beam.ColorFlat);
                        }
                    }
                    if (!hasLightSpot) EditorGUILayout.EndHorizontal();
                }
                
                // Blending Mode
                EditorGUILayout.PropertyField(blendingMode, EditorStrings.Beam.BlendingMode);

                EditorGUILayout.Separator();

                // Intensity
                bool advancedModeEnabled = false;
                using (ButtonToggleScopeFromLight(intensityFromLight, hasLightSpot))
                {
                    bool advancedModeButton = !hasLightSpot || intensityFromLight.HasAtLeastOneValue(false);
                    using (ButtonToggleScopeAdvanced(intensityModeAdvanced, advancedModeButton))
                    {
                        advancedModeEnabled = intensityModeAdvanced.HasAtLeastOneValue(true);
                        EditorGUILayout.PropertyField(intensityOutside, advancedModeEnabled ? EditorStrings.Beam.IntensityOutside : EditorStrings.Beam.IntensityGlobal);
                    }
                }

                if (advancedModeEnabled)
                    EditorGUILayout.PropertyField(intensityInside, EditorStrings.Beam.IntensityInside);
                else
                    intensityInside.floatValue = intensityOutside.floatValue;

                EditorGUILayout.Separator();

                // Spot Angle
                using (ButtonToggleScopeFromLight(spotAngleFromLight, hasLightSpot))
                {
                    EditorGUILayout.PropertyField(spotAngle, EditorStrings.Beam.SpotAngle);
                }

                PropertyThickness(fresnelPow);

                EditorGUILayout.Separator();

                EditorGUILayout.PropertyField(glareFrontal, EditorStrings.Beam.GlareFrontal);
                EditorGUILayout.PropertyField(glareBehind, EditorStrings.Beam.GlareBehind);

                EditorGUILayout.Separator();

                if (Config.Instance.featureEnabledShaderAccuracyHigh)
                {
                    EditorGUILayout.PropertyField(shaderAccuracy, EditorStrings.Beam.ShaderAccuracy);
                    EditorGUILayout.Separator();
                }

                trackChangesDuringPlaytime.ToggleLeft(EditorStrings.Beam.TrackChanges);
                DrawAnimatorWarning();
            }
            FoldableHeader.End();
            
            if(FoldableHeader.Begin(this, EditorStrings.Beam.HeaderAttenuation))
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(attenuationEquation, EditorStrings.Beam.AttenuationEquation);
                    if (attenuationEquation.enumValueIndex == (int)AttenuationEquation.Blend)
                        EditorGUILayout.PropertyField(attenuationCustomBlending, EditorStrings.Beam.AttenuationCustomBlending);
                }
                EditorGUILayout.EndHorizontal();

                // Fade End
                using (ButtonToggleScopeFromLight(fallOffEndFromLight, hasLightSpot))
                {
                    EditorGUILayout.PropertyField(fallOffEnd, EditorStrings.Beam.FallOffEnd);
                }

                if (fallOffEnd.hasMultipleDifferentValues)
                    EditorGUILayout.PropertyField(fallOffStart, EditorStrings.Beam.FallOffStart);
                else
                    fallOffStart.FloatSlider(EditorStrings.Beam.FallOffStart, 0f, fallOffEnd.floatValue - Consts.Beam.FallOffDistancesMinThreshold);

                EditorGUILayout.Separator();

                // Tilt
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(tiltFactor, EditorStrings.Beam.TiltFactor);
                    GlobalToggleButton(ref VolumetricLightBeam.editorShowTiltFactor, EditorStrings.Beam.EditorShowTiltDirection, EditorPrefsStrings.Beam.PrefShowTiltDir, 50f);
                }

                if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.isTilted && beam.shaderAccuracy != ShaderAccuracy.High; }))
                    EditorGUILayout.HelpBox(EditorStrings.Beam.HelpTiltedWithShaderAccuracyFast, MessageType.Warning);
            }
            FoldableHeader.End();

            if (Config.Instance.featureEnabledNoise3D)
            {
                if (FoldableHeader.Begin(this, EditorStrings.Beam.Header3DNoise))
                {
                    noiseMode.CustomEnum<NoiseMode>(EditorStrings.Beam.NoiseMode, EditorStrings.Beam.NoiseModeEnumDescriptions);

                    bool showNoiseProps = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.isNoiseEnabled; });
                    if (showNoiseProps)
                    {
                        EditorGUILayout.PropertyField(noiseIntensity, EditorStrings.Beam.NoiseIntensity);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.DisabledGroupScope(noiseScaleUseGlobal.boolValue))
                            {
                                EditorGUILayout.PropertyField(noiseScaleLocal, EditorStrings.Beam.NoiseScale);
                            }
                            noiseScaleUseGlobal.ToggleUseGlobalNoise();
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.DisabledGroupScope(noiseVelocityUseGlobal.boolValue))
                            {
                                EditorGUILayout.PropertyField(noiseVelocityLocal, EditorStrings.Beam.NoiseVelocity);
                            }
                            noiseVelocityUseGlobal.ToggleUseGlobalNoise();
                        }

                        if (Noise3D.isSupported && !Noise3D.isProperlyLoaded)
                            EditorGUILayout.HelpBox(EditorStrings.Common.HelpNoiseLoadingFailed, MessageType.Error);

                        if (!Noise3D.isSupported)
                            EditorGUILayout.HelpBox(Noise3D.isNotSupportedString, MessageType.Info);
                    }
                }
                FoldableHeader.End();
            }

            if(FoldableHeader.Begin(this, EditorStrings.Beam.HeaderBlendingDistances))
            {
                {
                    var content = AddEnabledStatusToContentText(EditorStrings.Beam.CameraClippingDistance, cameraClippingDistance);
                    EditorGUILayout.PropertyField(cameraClippingDistance, content);
                }

                {
                    var content = AddEnabledStatusToContentText(EditorStrings.Beam.DepthBlendDistance, depthBlendDistance);
                    EditorGUILayout.PropertyField(depthBlendDistance, content);
                }
            }
            FoldableHeader.End();

            if(FoldableHeader.Begin(this, EditorStrings.Beam.HeaderGeometry))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(coneRadiusStart, EditorStrings.Beam.ConeRadiusStart);
                    EditorGUI.BeginChangeCheck();
                    {
                        geomCap.ToggleLeft(EditorStrings.Beam.GeomCap, GUILayout.MaxWidth(42.0f));
                    }
                    if (EditorGUI.EndChangeCheck()) { SetMeshesDirty(); }
                }

                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(geomMeshType, EditorStrings.Beam.GeomMeshType);
                }
                if (EditorGUI.EndChangeCheck()) { SetMeshesDirty(); }

                if (geomMeshType.intValue == (int)MeshType.Custom)
                {
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUILayout.PropertyField(geomCustomSides, EditorStrings.Beam.GeomSides);
                        EditorGUILayout.PropertyField(geomCustomSegments, EditorStrings.Beam.GeomSegments);
                    }
                    if (EditorGUI.EndChangeCheck()) { SetMeshesDirty(); }

                    if (Config.Instance.featureEnabledMeshSkewing)
                    {
                        var vec3 = skewingLocalForwardDirection.vector3Value;
                        var vec2 = Vector2.zero;
                        EditorGUI.BeginChangeCheck();
                        {
                            vec2 = EditorGUILayout.Vector2Field(EditorStrings.Beam.SkewingLocalForwardDirection, vec3.xy());
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            vec3 = new Vector3(vec2.x, vec2.y, 1.0f);
                            skewingLocalForwardDirection.vector3Value = vec3;
                            SetMeshesDirty();
                        }
                    }

                    if (m_Targets.Count == 1)
                    {
                        EditorGUILayout.HelpBox(m_Targets[0].meshStats, MessageType.Info);
                    }

                    EditorGUI.indentLevel--;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(clippingPlaneTransform, EditorStrings.Beam.ClippingPlane);

                    if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.clippingPlaneTransform != null; }))
                    {
                        GlobalToggleButton(ref VolumetricLightBeam.editorShowClippingPlane, EditorStrings.Beam.EditorShowClippingPlane, EditorStrings.Beam.PrefShowAddClippingPlane, 50f);
                    }
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderFadeOut))
            {
                bool wasEnabled = fadeOutBegin.floatValue <= fadeOutEnd.floatValue;

                if(m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return (beam.fadeOutBegin <= beam.fadeOutEnd) != wasEnabled; }))
                {
                    wasEnabled = true;
                    EditorGUI.showMixedValue = true;
                }

                System.Action<float> setFadeOutBegin = value =>
                {
                    fadeOutBegin.floatValue = value;
                    m_Targets.RecordUndoAction("Change Fade Out Begin Distance",
                        (VolumetricLightBeam beam) => { beam.fadeOutBegin = value; });
                };

                System.Action<float> setFadeOutEnd = value =>
                {
                    fadeOutEnd.floatValue = value;
                    m_Targets.RecordUndoAction("Change Fade Out End Distance",
                        (VolumetricLightBeam beam) => { beam.fadeOutEnd = value; });
                };

                EditorGUI.BeginChangeCheck();
                bool isEnabled = EditorGUILayout.Toggle(EditorStrings.Beam.FadeOutEnabled, wasEnabled);
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                {
                    float invValue = isEnabled ? 1 : -1;
                    float valueA = Mathf.Abs(fadeOutBegin.floatValue);
                    float valueB = Mathf.Abs(fadeOutEnd.floatValue);
                    setFadeOutBegin(invValue * Mathf.Min(valueA, valueB));
                    setFadeOutEnd  (invValue * Mathf.Max(valueA, valueB));
                }

                if (isEnabled)
                {
                    const float kEpsilon = 0.1f;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(fadeOutBegin, EditorStrings.Beam.FadeOutBegin);
                        if (EditorGUI.EndChangeCheck())
                        {
                            setFadeOutBegin(Mathf.Clamp(fadeOutBegin.floatValue, 0, fadeOutEnd.floatValue - kEpsilon));
                        }

                        using (new LabelWidth(30f))
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(fadeOutEnd, EditorStrings.Beam.FadeOutEnd);
                            if (EditorGUI.EndChangeCheck())
                            {
                                setFadeOutEnd(Mathf.Max(fadeOutBegin.floatValue + kEpsilon, fadeOutEnd.floatValue));
                            }
                        }
                    }
                    if (Application.isPlaying)
                    {
                        if(Config.Instance.fadeOutCameraTransform == null)
                        {
                            EditorGUILayout.HelpBox(EditorStrings.Beam.HelpFadeOutNoMainCamera, MessageType.Error);
                        }
                    }
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.Beam.Header2D))
            {
                dimensions.CustomEnum<Dimensions>(EditorStrings.Beam.Dimensions, EditorStrings.Common.DimensionsEnumDescriptions);
                DrawSortingLayerAndOrder();
            }
            FoldableHeader.End();

            DrawInfos();
            DrawEditInSceneButton();
            DrawCustomActionButtons();
            DrawAdditionalFeatures();
            
            serializedObject.ApplyModifiedProperties();
        }

        GUIContent AddEnabledStatusToContentText(GUIContent inContent, SerializedProperty prop)
        {
            Debug.Assert(prop.propertyType == SerializedPropertyType.Float);

            var content = new GUIContent(inContent);
            if (prop.hasMultipleDifferentValues)
                content.text += " (-)";
            else
                content.text += prop.floatValue > 0.0 ? " (on)" : " (off)";
            return content;
        }

        void SetMeshesDirty()
        {
            foreach (var entity in m_Targets) entity._EditorSetMeshDirty();
        }

        void DrawSortingLayerAndOrder()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = sortingLayerID.hasMultipleDifferentValues;
                int layerIndex = System.Array.IndexOf(m_SortingLayerNames, SortingLayer.IDToName(sortingLayerID.intValue));
                layerIndex = EditorGUILayout.Popup(EditorStrings.Beam.SortingLayer, layerIndex, m_SortingLayerNames);
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                {
                    sortingLayerID.intValue = SortingLayer.NameToID(m_SortingLayerNames[layerIndex]);

                    m_Targets.RecordUndoAction("Edit Sorting Layer",
                        (VolumetricLightBeam beam) => beam.sortingLayerID = sortingLayerID.intValue); // call setters
                }

                using (new LabelWidth(40f))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(sortingOrder, EditorStrings.Beam.SortingOrder, GUILayout.MaxWidth(90f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_Targets.RecordUndoAction("Edit Sorting Order",
                            (VolumetricLightBeam beam) => beam.sortingOrder = sortingOrder.intValue); // call setters
                    }
                }
            }
        }

        void DrawAnimatorWarning()
        {
            var showAnimatorWarning = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.GetComponent<Animator>() != null && beam.trackChangesDuringPlaytime == false; });

            if (showAnimatorWarning)
                EditorGUILayout.HelpBox(EditorStrings.Beam.HelpAnimatorWarning, MessageType.Warning);
        }

        void DrawEditInSceneButton()
        {
            EditorGUI.BeginChangeCheck();
            bool editInScene = EditorPrefs.GetBool(EditorStrings.Beam.PrefEditInScene, false);
            editInScene = GUILayout.Toggle(editInScene, EditorStrings.Beam.ButtonEditInScene, EditorStyles.miniButton);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(EditorStrings.Beam.PrefEditInScene, editInScene);
                SceneView.RepaintAll();
            }
        }

        void DrawCustomActionButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(EditorStrings.Beam.ButtonResetProperties, EditorStyles.miniButton))
                {
                    m_Targets.RecordUndoAction("Reset Light Beam Properties",
                        (VolumetricLightBeam beam) => { beam.Reset(); beam.GenerateGeometry(); } );
                }

                if(m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.geomMeshType == MeshType.Custom; }))
                {
                    if (GUILayout.Button(EditorStrings.Beam.ButtonGenerateGeometry, EditorStyles.miniButton))
                    {
                        foreach (var entity in m_Targets) entity.GenerateGeometry();
                    }
                }
            }
        }

        void AddComponentToTargets<T>() where T : MonoBehaviour
        {
            foreach (var target in m_Targets) EditorExtensions.AddComponentFromEditor<T>(target);
        }

        void DrawAdditionalFeatures()
        {
            if (Application.isPlaying) return; // do not support adding additional components at runtime

            bool showButtonDust         = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.GetComponent<VolumetricDustParticles>() == null; });
            bool showButtonOcclusion    = Config.Instance.featureEnabledDynamicOcclusion && m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.GetComponent<DynamicOcclusionAbstractBase>() == null; });
            bool showButtonTriggerZone  = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.GetComponent<TriggerZone>() == null; });
            bool showButtonEffect       = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.GetComponent<EffectAbstractBase>() == null; });

            using (new EditorGUILayout.HorizontalScope())
            {
                GUIStyle kStyle = GUI.skin.button;

                if (showButtonDust && GUILayout.Button(EditorData.Instance.contentAddDustParticles, kStyle))
                    AddComponentToTargets<VolumetricDustParticles>();

                if (showButtonOcclusion && GUILayout.Button(EditorData.Instance.contentAddDynamicOcclusion, kStyle))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(EditorStrings.Beam.ButtonAddDynamicOcclusionRaycasting),  false, AddComponentToTargets<DynamicOcclusionRaycasting>);
                    menu.AddItem(new GUIContent(EditorStrings.Beam.ButtonAddDynamicOcclusionDepthBuffer), false, AddComponentToTargets<DynamicOcclusionDepthBuffer>);
                    menu.ShowAsContext();
                }

                if (showButtonEffect && GUILayout.Button(EditorData.Instance.contentAddEffect, kStyle))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(EditorStrings.Beam.ButtonAddEffectFlicker), false, AddComponentToTargets<EffectFlicker>);
                    menu.AddItem(new GUIContent(EditorStrings.Beam.ButtonAddEffectPulse),   false, AddComponentToTargets<EffectPulse>);
                    menu.ShowAsContext();
                }

                if (showButtonTriggerZone && GUILayout.Button(EditorData.Instance.contentAddTriggerZone, kStyle))
                    AddComponentToTargets<TriggerZone>();
            }
        }

        struct InfoTip
        {
            public MessageType type;
            public string message;
        }

        bool DrawInfos()
        {
            var tips = GetInfoTips();
            var gpuInstancingReport = GetBatchingReport();

            if (tips.Count > 0 || !string.IsNullOrEmpty(gpuInstancingReport))
            {
                if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderInfos))
                {
                    foreach (var tip in tips)
                        EditorGUILayout.HelpBox(tip.message, tip.type);

                    if (!string.IsNullOrEmpty(gpuInstancingReport))
                        EditorGUILayout.HelpBox(gpuInstancingReport, MessageType.Warning);
                }
                FoldableHeader.End();
                return true;
            }
            return false;
        }

        List<InfoTip> GetInfoTips()
        {
            var tips = new List<InfoTip>();
            if (m_Targets.Count == 1)
            {
                if (depthBlendDistance.floatValue > 0f || !Noise3D.isSupported || trackChangesDuringPlaytime.boolValue)
                {
                    if (depthBlendDistance.floatValue > 0f)
                    {
                        tips.Add(new InfoTip { type = MessageType.Info, message = EditorStrings.Beam.HelpDepthTextureMode });
#if UNITY_IPHONE || UNITY_IOS || UNITY_ANDROID
                        tips.Add(new InfoTip { type = MessageType.Info, message = EditorStrings.Beam.HelpDepthMobile });
#endif
                    }

                    if (trackChangesDuringPlaytime.boolValue)
                        tips.Add(new InfoTip { type = MessageType.Info, message = EditorStrings.Beam.HelpTrackChangesEnabled });
                }
            }
            return tips;
        }

        string GetBatchingReport()
        {
            if (m_Targets.Count > 1)
            {
                string reasons = "";
                for (int i = 1; i < m_Targets.Count; ++i)
                {
                    if (!BatchingHelper.CanBeBatched(m_Targets[0], m_Targets[i], ref reasons))
                    {
                        return "Selected beams can't be batched together:\n" + reasons;
                    }
                }
            }
            return null;
        }

        void DrawFallOffSliderHandle(VolumetricLightBeam beam, float lineStartOffset, float lineAlpha, Handles.CapFunction capFunction, string recordName, ref float value)
        {
            Debug.Assert(beam != null);

            var fwd = beam.beamGlobalForward;

            EditorGUI.BeginChangeCheck();
            var sliderPos = beam.transform.position + fwd * value;
            sliderPos = Handles.Slider(sliderPos, fwd, HandleUtility.GetHandleSize(sliderPos) * 0.1f, capFunction, 0.5f);

            var savedCol = Handles.color;
            {
                var col = savedCol;
                col.a = lineAlpha;
                Handles.color = col;
                Handles.DrawLine(beam.transform.position + fwd * lineStartOffset, sliderPos);
                Handles.color = savedCol;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, string.Format("Edit Beam '{0}'", recordName));

                var sliderVec = (sliderPos - beam.transform.position);
                var sliderLength = 0.0f;
                if (Vector3.Dot(sliderVec, fwd) > 0.0f)
                    sliderLength = sliderVec.magnitude;

                value = sliderLength;
                beam.UpdateAfterManualPropertyChange();
            }
        }

        void DrawRadiusHandle(VolumetricLightBeam beam, Vector3 pos, string recordName, float value, System.Action<float> setValue)
        {
            Debug.Assert(beam != null);

            var handleMatrix = Matrix4x4.TRS(
                pos,
                Quaternion.LookRotation(beam.transform.up, beam.beamGlobalForward) * Quaternion.Euler(0.0f, 45.0f, 0.0f), // rotate the circle handle to have both this and the Unity's light handle
                Vector3.one);

            using (new Handles.DrawingScope(handleMatrix))
            {
                var hdl = handleRadius;
                EditorGUI.BeginChangeCheck();
                hdl.radius = value;
                hdl.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(beam, string.Format("Edit Beam '{0}'", recordName));
                    setValue(hdl.radius);
                    beam.UpdateAfterManualPropertyChange();
                }
            }
        }

        protected virtual void OnSceneGUI()
        {
            bool editInScene = EditorPrefs.GetBool(EditorStrings.Beam.PrefEditInScene, false);
            if (!editInScene)
                return;

            var beam = target as VolumetricLightBeam;
            Debug.Assert(beam != null);

            Handles.color = beam.ComputeColorAtDepth(0.0f).ComputeComplementaryColor(true);
            {
                DrawRadiusHandle(beam, beam.transform.position, "Truncated Radius", beam.coneRadiusStart, (float v) => beam.coneRadiusStart = v);
            }

            if (!beam.hasMeshSkewing)
            {
                Handles.color = beam.ComputeColorAtDepth(beam.fallOffStart / beam.fallOffEnd).ComputeComplementaryColor(true);
                {
                    DrawFallOffSliderHandle(beam, 0.0f, 1.0f, Handles.CylinderHandleCap, "Start Distance", ref beam.fallOffStart);
                }

                Handles.color = beam.ComputeColorAtDepth(1.0f).ComputeComplementaryColor(true);
                {
                    VolumetricLightBeam.AttachedLightType lightType;
                    var hasSpotLight = beam.GetLightSpotAttachedSlow(out lightType) != null;

                    if (!hasSpotLight || !beam.fallOffEndFromLight)
                    {
                        DrawFallOffSliderHandle(beam, beam.fallOffStart, 0.5f, Handles.CubeHandleCap, "Range Limit", ref beam.fallOffEnd);
                    }

                    if (!hasSpotLight || !beam.spotAngleFromLight)
                    {
                        var fallOffEndPos = beam.transform.localToWorldMatrix.MultiplyPoint(beam.beamLocalForward * beam.fallOffEnd);
                        DrawRadiusHandle(beam, fallOffEndPos, "Spot Angle", beam.coneRadiusEnd, (float v) => beam.coneRadiusEnd = v);
                    }
                }
            }
        }
    }
}
#endif
