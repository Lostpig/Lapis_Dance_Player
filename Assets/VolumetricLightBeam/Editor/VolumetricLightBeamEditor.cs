//#define DEBUG_SHOW_CUSTOM_MATERIAL_INFO

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 0429, 0162 // Unreachable expression code detected (because of Noise3D.isSupported on mobile)

namespace VLB
{
    [CustomEditor(typeof(VolumetricLightBeam))]
    [CanEditMultipleObjects]
    public class VolumetricLightBeamEditor : EditorCommon
    {
        SerializedProperty trackChangesDuringPlaytime;
        SerializedProperty colorFromLight, colorMode, color, colorGradient;
        SerializedProperty intensityFromLight, intensityModeAdvanced, intensityInside, intensityOutside;
        SerializedProperty blendingMode, shaderAccuracy;
        SerializedProperty fresnelPow, glareFrontal, glareBehind;
        SerializedProperty spotAngleFromLight, spotAngle;
        SerializedProperty coneRadiusStart, geomMeshType, geomCustomSides, geomCustomSegments, geomCap;
        SerializedProperty fallOffEndFromLight, fallOffStart, fallOffEnd;
        SerializedProperty attenuationEquation, attenuationCustomBlending;
        SerializedProperty depthBlendDistance, cameraClippingDistance;
        SerializedProperty noiseMode, noiseIntensity, noiseScaleUseGlobal, noiseScaleLocal, noiseVelocityUseGlobal, noiseVelocityLocal;
        SerializedProperty fadeOutBegin, fadeOutEnd;
        SerializedProperty dimensions, sortingLayerID, sortingOrder;
        SerializedProperty skewingLocalForwardDirection, clippingPlaneTransform, tiltFactor;

        TargetList<VolumetricLightBeam> m_Targets;
        string[] m_SortingLayerNames;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Targets = new TargetList<VolumetricLightBeam>(targets);

            colorFromLight = FindProperty((VolumetricLightBeam x) => x.colorFromLight);
            color = FindProperty((VolumetricLightBeam x) => x.color);
            colorGradient = FindProperty((VolumetricLightBeam x) => x.colorGradient);
            colorMode = FindProperty((VolumetricLightBeam x) => x.colorMode);

            intensityFromLight = FindProperty((VolumetricLightBeam x) => x.intensityFromLight);
            intensityModeAdvanced = FindProperty((VolumetricLightBeam x) => x.intensityModeAdvanced);
            intensityInside = FindProperty((VolumetricLightBeam x) => x.intensityInside);
            intensityOutside = FindProperty((VolumetricLightBeam x) => x.intensityOutside);

            blendingMode = FindProperty((VolumetricLightBeam x) => x.blendingMode);
            shaderAccuracy = FindProperty((VolumetricLightBeam x) => x.shaderAccuracy);

            fresnelPow = FindProperty((VolumetricLightBeam x) => x.fresnelPow);

            glareFrontal = FindProperty((VolumetricLightBeam x) => x.glareFrontal);
            glareBehind = FindProperty((VolumetricLightBeam x) => x.glareBehind);

            spotAngleFromLight = FindProperty((VolumetricLightBeam x) => x.spotAngleFromLight);
            spotAngle = FindProperty((VolumetricLightBeam x) => x.spotAngle);

            coneRadiusStart = FindProperty((VolumetricLightBeam x) => x.coneRadiusStart);

            geomMeshType = FindProperty((VolumetricLightBeam x) => x.geomMeshType);
            geomCustomSides = FindProperty((VolumetricLightBeam x) => x.geomCustomSides);
            geomCustomSegments = FindProperty((VolumetricLightBeam x) => x.geomCustomSegments);
            geomCap = FindProperty((VolumetricLightBeam x) => x.geomCap);

            fallOffEndFromLight = FindProperty((VolumetricLightBeam x) => x.fallOffEndFromLight);
            fallOffStart = FindProperty((VolumetricLightBeam x) => x.fallOffStart);
            fallOffEnd = FindProperty((VolumetricLightBeam x) => x.fallOffEnd);

            attenuationEquation = FindProperty((VolumetricLightBeam x) => x.attenuationEquation);
            attenuationCustomBlending = FindProperty((VolumetricLightBeam x) => x.attenuationCustomBlending);

            depthBlendDistance = FindProperty((VolumetricLightBeam x) => x.depthBlendDistance);
            cameraClippingDistance = FindProperty((VolumetricLightBeam x) => x.cameraClippingDistance);

            // NOISE
            noiseMode = FindProperty((VolumetricLightBeam x) => x.noiseMode);
            noiseIntensity = FindProperty((VolumetricLightBeam x) => x.noiseIntensity);
            noiseScaleUseGlobal = FindProperty((VolumetricLightBeam x) => x.noiseScaleUseGlobal);
            noiseScaleLocal = FindProperty((VolumetricLightBeam x) => x.noiseScaleLocal);
            noiseVelocityUseGlobal = FindProperty((VolumetricLightBeam x) => x.noiseVelocityUseGlobal);
            noiseVelocityLocal = FindProperty((VolumetricLightBeam x) => x.noiseVelocityLocal);

            trackChangesDuringPlaytime = serializedObject.FindProperty("_TrackChangesDuringPlaytime");

            fadeOutBegin = serializedObject.FindProperty("_FadeOutBegin");
            fadeOutEnd   = serializedObject.FindProperty("_FadeOutEnd");

            skewingLocalForwardDirection = FindProperty((VolumetricLightBeam x) => x.skewingLocalForwardDirection);
            clippingPlaneTransform = FindProperty((VolumetricLightBeam x) => x.clippingPlaneTransform);
            tiltFactor = FindProperty((VolumetricLightBeam x) => x.tiltFactor);

            // 2D
            sortingLayerID = serializedObject.FindProperty("_SortingLayerID");
            sortingOrder = serializedObject.FindProperty("_SortingOrder");
            dimensions = FindProperty((VolumetricLightBeam x) => x.dimensions);
            m_SortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();
        }

        static void PropertyThickness(SerializedProperty sp)
        {
            sp.FloatSlider(
                EditorStrings.Beam.SideThickness,
                0, 1,
                (value) => Mathf.Clamp01(1 - (value / Consts.FresnelPowMaxValue)),    // conversion value to slider
                (value) => (1 - value) * Consts.FresnelPowMaxValue                    // conversion slider to value
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
                EditorStrings.Beam.FromSpotLight);
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

            bool hasLightSpot = false;
            var light = m_Targets[0].GetComponent<Light>();
            if (light)
            {
                hasLightSpot = light.type == LightType.Spot;
                if (!hasLightSpot)
                {
                    EditorGUILayout.HelpBox(EditorStrings.Beam.HelpNoSpotlight, MessageType.Warning);
                }
            }

            if (FoldableHeader.Begin(this, EditorStrings.Beam.HeaderBasic))
            {
                // Color
                using (ButtonToggleScopeFromLight(colorFromLight, hasLightSpot))
                {
                    if (!hasLightSpot) EditorGUILayout.BeginHorizontal();    // mandatory to have the color picker on the same line (when the button "from light" is not here)
                    {
                        EditorGUIUtility.fieldWidth = 65.0f;
                        EditorGUILayout.PropertyField(colorMode, EditorStrings.Beam.ColorMode);
                        EditorGUIUtility.fieldWidth = 0.0f;

                        if (colorMode.enumValueIndex == (int)ColorMode.Gradient)
                            EditorGUILayout.PropertyField(colorGradient, EditorStrings.Beam.ColorGradient);
                        else
                            EditorGUILayout.PropertyField(color, EditorStrings.Beam.ColorFlat);
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

                EditorGUILayout.PropertyField(shaderAccuracy, EditorStrings.Beam.ShaderAccuracy);

                EditorGUILayout.Separator();

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
                    fallOffStart.FloatSlider(EditorStrings.Beam.FallOffStart, 0f, fallOffEnd.floatValue - Consts.FallOffDistancesMinThreshold);

                EditorGUILayout.Separator();

                // Tilt
                EditorGUILayout.PropertyField(tiltFactor, EditorStrings.Beam.TiltFactor);
                GlobalToggle(ref VolumetricLightBeam.editorShowTiltFactor, EditorStrings.Beam.EditorShowTiltDirection, "VLB_BEAM_SHOWTILTDIR");

                if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.isTilted && beam.shaderAccuracy != ShaderAccuracy.High; }))
                    EditorGUILayout.HelpBox(EditorStrings.Beam.HelpTiltedWithShaderAccuracyFast, MessageType.Warning);
            }
            FoldableHeader.End();

            if(FoldableHeader.Begin(this, EditorStrings.Beam.Header3DNoise))
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

                    ButtonOpenConfig();

                    if (Noise3D.isSupported && !Noise3D.isProperlyLoaded)
                        EditorGUILayout.HelpBox(EditorStrings.Common.HelpNoiseLoadingFailed, MessageType.Error);

                    if (!Noise3D.isSupported)
                        EditorGUILayout.HelpBox(Noise3D.isNotSupportedString, MessageType.Info);
                }
            }
            FoldableHeader.End();

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
                        geomCap.ToggleLeft(EditorStrings.Beam.GeomCap, GUILayout.MaxWidth(40.0f));
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

                    EditorGUI.indentLevel--;
                }

                if (m_Targets.Count == 1)
                {
                    EditorGUILayout.HelpBox(m_Targets[0].meshStats, MessageType.Info);
                }

                EditorGUILayout.Separator();

                EditorGUILayout.PropertyField(clippingPlaneTransform, EditorStrings.Beam.ClippingPlane);

                if (m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.clippingPlaneTransform != null; }))
                {
                    GlobalToggle(ref VolumetricLightBeam.editorShowClippingPlane, EditorStrings.Beam.EditorShowClippingPlane, "VLB_BEAM_SHOWADDCLIPPINGPLANE");
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

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(fadeOutBegin, EditorStrings.Beam.FadeOutBegin);
                    if(EditorGUI.EndChangeCheck())
                    {
                        setFadeOutBegin(Mathf.Clamp(fadeOutBegin.floatValue, 0, fadeOutEnd.floatValue - kEpsilon));
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(fadeOutEnd, EditorStrings.Beam.FadeOutEnd);
                    if(EditorGUI.EndChangeCheck())
                    {
                        setFadeOutEnd(Mathf.Max(fadeOutBegin.floatValue + kEpsilon, fadeOutEnd.floatValue));
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
                DrawSortingLayer();
                DrawSortingOrder();
            }
            FoldableHeader.End();

            if (DrawInfos())
            {
                DrawLineSeparator();
            }

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

        void DrawSortingLayer()
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
                    (VolumetricLightBeam beam) => beam.sortingLayerID = sortingLayerID.intValue ); // call setters
            }
        }

        void DrawSortingOrder()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sortingOrder, EditorStrings.Beam.SortingOrder);
            if (EditorGUI.EndChangeCheck())
            {
                m_Targets.RecordUndoAction("Edit Sorting Order",
                    (VolumetricLightBeam beam) => beam.sortingOrder = sortingOrder.intValue ); // call setters
            }
        }

        void DrawAnimatorWarning()
        {
            var showAnimatorWarning = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.GetComponent<Animator>() != null && beam.trackChangesDuringPlaytime == false; });

            if (showAnimatorWarning)
                EditorGUILayout.HelpBox(EditorStrings.Beam.HelpAnimatorWarning, MessageType.Warning);
        }

        void DrawCustomActionButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(EditorStrings.Beam.ButtonResetProperties, EditorStyles.miniButtonLeft))
                {
                    m_Targets.RecordUndoAction("Reset Light Beam Properties",
                        (VolumetricLightBeam beam) => { beam.Reset(); beam.GenerateGeometry(); } );
                }

                if (geomMeshType.intValue == (int)MeshType.Custom)
                {
                    if (GUILayout.Button(EditorStrings.Beam.ButtonGenerateGeometry, EditorStyles.miniButtonRight))
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
#if UNITY_5_5_OR_NEWER
            bool showButtonDust         = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.GetComponent<VolumetricDustParticles>() == null; });
#else
            bool showButtonDust = false;
#endif
            bool showButtonOcclusion    = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.GetComponent<DynamicOcclusionAbstractBase>() == null; });
            bool showButtonTriggerZone  = m_Targets.HasAtLeastOneTargetWith((VolumetricLightBeam beam) => { return beam.GetComponent<TriggerZone>() == null; });

            if (showButtonDust || showButtonOcclusion || showButtonTriggerZone)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (showButtonDust && GUILayout.Button(EditorStrings.Beam.ButtonAddDustParticles, EditorStyles.miniButton))
                    {
                        AddComponentToTargets<VolumetricDustParticles>();
                    }

                    if (showButtonOcclusion && GUILayout.Button(EditorStrings.Beam.ButtonAddDynamicOcclusion, EditorStyles.miniButton))
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("+ Dynamic Occlusion (Raycasting)"),    false, AddComponentToTargets<DynamicOcclusionRaycasting>);
                        menu.AddItem(new GUIContent("+ Dynamic Occlusion (Depth Texture)"), false, AddComponentToTargets<DynamicOcclusionDepthBuffer>);
                        menu.ShowAsContext();
                    }

                    if (showButtonTriggerZone && GUILayout.Button(EditorStrings.Beam.ButtonAddTriggerZone, EditorStyles.miniButton))
                    {
                        AddComponentToTargets<TriggerZone>();
                    }
                }
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
    }
}
#endif
