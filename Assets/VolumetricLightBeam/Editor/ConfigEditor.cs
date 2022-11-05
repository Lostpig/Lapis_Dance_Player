#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VLB
{
    [CustomEditor(typeof(Config))]
    public class ConfigEditor : EditorCommon
    {
        SerializedProperty geometryOverrideLayer = null, geometryLayerID = null, geometryTag = null, geometryRenderQueue = null, renderPipeline = null, renderingMode = null;
        SerializedProperty sharedMeshSides = null, sharedMeshSegments = null;
        SerializedProperty globalNoiseScale = null, globalNoiseVelocity = null;
        SerializedProperty fadeOutCameraTag = null;
        SerializedProperty noiseTexture3D = null;
        SerializedProperty dustParticlesPrefab = null;
        SerializedProperty ditheringFactor = null, ditheringNoiseTexture = null;
        SerializedProperty featureEnabledColorGradient = null, featureEnabledDepthBlend = null, featureEnabledNoise3D = null, featureEnabledDynamicOcclusion = null, featureEnabledMeshSkewing = null, featureEnabledShaderAccuracyHigh = null;
        bool isRenderQueueCustom = false;
        Config m_TargetConfig = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            RenderQueueGUIInit();

            Noise3D.LoadIfNeeded(); // Try to load Noise3D, maybe for the 1st time

            m_TargetConfig = this.target as Config;
        }

        void RenderQueueGUIInit()
        {
            isRenderQueueCustom = true;
            foreach (RenderQueue rq in System.Enum.GetValues(typeof(RenderQueue)))
            {
                if (rq != RenderQueue.Custom && geometryRenderQueue.intValue == (int)rq)
                {
                    isRenderQueueCustom = false;
                    break;
                }
            }
        }

        void RenderQueueGUIDraw()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUI.BeginChangeCheck();
                    RenderQueue rq = isRenderQueueCustom ? RenderQueue.Custom : (RenderQueue)geometryRenderQueue.intValue;
                    rq = (RenderQueue)EditorGUILayout.EnumPopup(EditorStrings.Config.GeometryRenderQueue, rq);
                    if (EditorGUI.EndChangeCheck())
                    {
                        isRenderQueueCustom = (rq == RenderQueue.Custom);

                        if (!isRenderQueueCustom)
                            geometryRenderQueue.intValue = (int)rq;
                    }

                    EditorGUI.BeginDisabledGroup(!isRenderQueueCustom);
                    {
                        geometryRenderQueue.intValue = EditorGUILayout.IntField(geometryRenderQueue.intValue, GUILayout.MaxWidth(65.0f));
                    }
                    EditorGUI.EndDisabledGroup();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    SetDirty(DirtyFlags.AllBeamGeom);
                }
            }
        }

        protected override void OnHeaderGUI()
        {
            GUILayout.BeginVertical("In BigTitle");
            EditorGUILayout.Separator();

            var title = string.Format("Volumetric Light Beam - Plugin Configuration");
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(string.Format("Current Version: {0}", Version.Current), EditorStyles.miniBoldLabel);
            EditorGUILayout.Separator();
            GUILayout.EndVertical();
        }

        void OnAddConfigPerPlatform(object platform)
        {
            var p = (RuntimePlatform)platform;
            var clone = Object.Instantiate(m_TargetConfig);
            Debug.Assert(clone);
            var path = AssetDatabase.GetAssetPath(m_TargetConfig).Replace(m_TargetConfig.name + ".asset", "");
            ConfigOverride.CreateAsset(clone, path + ConfigOverride.kAssetName + p.ToString() + ".asset");
            Selection.activeObject = clone;
        }

        [System.Flags]
        enum DirtyFlags
        {
            Target = 1 << 1,
            Shader = 1 << 2,
            Noise = 1 << 3,
            GlobalMesh = 1 << 4,
            AllBeamGeom = 1 << 5,
            AllMeshes = 1 << 6
        }

        void SetDirty(DirtyFlags flags)
        {
            if (m_IsUsedInstance)
            {
                if (flags.HasFlag(DirtyFlags.Target)) EditorUtility.SetDirty(target);
                if (flags.HasFlag(DirtyFlags.Shader)) m_NeedToRefreshShader = true;
                if (flags.HasFlag(DirtyFlags.Noise)) m_NeedToReloadNoise = true;
                if (flags.HasFlag(DirtyFlags.GlobalMesh)) GlobalMesh.Destroy();
                if (flags.HasFlag(DirtyFlags.AllBeamGeom)) VolumetricLightBeam._EditorSetAllBeamGeomDirty();
                if (flags.HasFlag(DirtyFlags.AllMeshes)) VolumetricLightBeam._EditorSetAllMeshesDirty();
            }
        }

        bool m_NeedToReloadNoise = false;
        bool m_NeedToRefreshShader = false;
        bool m_IsUsedInstance = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Debug.Assert(m_TargetConfig != null);

            m_NeedToReloadNoise = false;
            m_NeedToRefreshShader = false;
            m_IsUsedInstance = m_TargetConfig.IsCurrentlyUsedInstance();

            // Config per plaftorm
#if UNITY_2018_1_OR_NEWER
            {
                bool hasValidName = m_TargetConfig.HasValidAssetName();
                bool isCurrentPlatformSuffix = m_TargetConfig.GetAssetSuffix() == PlatformHelper.GetCurrentPlatformSuffix();

                var platformSuffix = m_TargetConfig.GetAssetSuffix();
                string platformStr = "Default Config asset";
                if(!string.IsNullOrEmpty(platformSuffix))
                    platformStr = string.Format("Config asset for platform '{0}'", m_TargetConfig.GetAssetSuffix());
                if (!hasValidName)
                    platformStr += " (INVALID)";
                EditorGUILayout.LabelField(platformStr, EditorStyles.boldLabel);

                if (GUILayout.Button(EditorStrings.Beam.ButtonCreateOverridePerPlatform, EditorStyles.miniButton))
                {
                    var menu = new GenericMenu();
                    foreach (var platform in System.Enum.GetValues(typeof(RuntimePlatform)))
                        menu.AddItem(new GUIContent(platform.ToString()), false, OnAddConfigPerPlatform, platform);
                    menu.ShowAsContext();
                }

                if (!hasValidName)
                {
                    EditorGUILayout.Separator();
                    EditorGUILayout.HelpBox(EditorStrings.Config.InvalidPlatformOverride, MessageType.Error);
                    ButtonOpenConfig();
                }
                else if (!m_IsUsedInstance)
                {
                    EditorGUILayout.Separator();

                    if (isCurrentPlatformSuffix)
                        EditorGUILayout.HelpBox(EditorStrings.Config.WrongAssetLocation, MessageType.Error);
                    else
                        EditorGUILayout.HelpBox(EditorStrings.Config.NotCurrentAssetInUse, MessageType.Warning);

                    ButtonOpenConfig();
                }

                DrawLineSeparator();
            }
#endif

            {
                EditorGUI.BeginChangeCheck();
                {
                    if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderBeamGeometry))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            geometryOverrideLayer.boolValue = EditorGUILayout.Toggle(EditorStrings.Config.GeometryOverrideLayer, geometryOverrideLayer.boolValue);
                            using (new EditorGUI.DisabledGroupScope(!geometryOverrideLayer.boolValue))
                            {
                                geometryLayerID.intValue = EditorGUILayout.LayerField(geometryLayerID.intValue);
                            }
                        }

                        geometryTag.stringValue = EditorGUILayout.TagField(EditorStrings.Config.GeometryTag, geometryTag.stringValue);
                    }
                    FoldableHeader.End();

                    if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderRendering))
                    {
                        RenderQueueGUIDraw();

                        if (BeamGeometry.isCustomRenderPipelineSupported)
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                renderPipeline.CustomEnum<RenderPipeline>(EditorStrings.Config.GeometryRenderPipeline, EditorStrings.Config.GeometryRenderPipelineEnumDescriptions);
                            }
                            if (EditorGUI.EndChangeCheck())
                            {
                                SetDirty(DirtyFlags.AllBeamGeom | DirtyFlags.Shader); // need to fully reset the BeamGeom to update the shader
                            }
                        }

                        if (m_TargetConfig.hasRenderPipelineMismatch)
                            EditorGUILayout.HelpBox(EditorStrings.Config.ErrorRenderPipelineMismatch, MessageType.Error);

                        EditorGUI.BeginChangeCheck();
                        {
                            EditorGUILayout.PropertyField(renderingMode, EditorStrings.Config.GeometryRenderingMode);

                            if (renderPipeline.enumValueIndex == (int)RenderPipeline.BuiltIn)
                            {
                                if (renderingMode.enumValueIndex == (int)RenderingMode.SRPBatcher)
                                    EditorGUILayout.HelpBox(EditorStrings.Config.ErrorSrpBatcherOnlyCompatibleWithSrp, MessageType.Error);
                            }
                            else
                            {
                                if (renderingMode.enumValueIndex == (int)RenderingMode.SRPBatcher && SRPHelper.renderPipelineType == SRPHelper.RenderPipeline.LWRP)
                                    EditorGUILayout.HelpBox(EditorStrings.Config.ErrorSrpBatcherNotCompatibleWithLWRP, MessageType.Error);

                                if (renderingMode.enumValueIndex == (int)RenderingMode.MultiPass)
                                    EditorGUILayout.HelpBox(EditorStrings.Config.ErrorSrpAndMultiPassNotCompatible, MessageType.Error);
                            }

#pragma warning disable 0162 // warning CS0162: Unreachable code detected
                            if (renderingMode.enumValueIndex == (int)RenderingMode.GPUInstancing && !BatchingHelper.isGpuInstancingSupported)
                                EditorGUILayout.HelpBox(EditorStrings.Config.ErrorGeometryGpuInstancingNotSupported, MessageType.Error);
#pragma warning restore 0162
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            SetDirty(DirtyFlags.AllBeamGeom | DirtyFlags.GlobalMesh | DirtyFlags.Shader); // need to fully reset the BeamGeom to update the shader
                        }

                        if (m_TargetConfig.beamShader == null)
                            EditorGUILayout.HelpBox(EditorStrings.Config.GetErrorInvalidShader(), MessageType.Error);

                        if (ditheringFactor.FloatSlider(EditorStrings.Config.DitheringFactor, 0.0f, 1.0f))
                        {
                            SetDirty(DirtyFlags.Shader);
                        }
                    }
                    FoldableHeader.End();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    VolumetricLightBeam._EditorSetAllMeshesDirty();
                }

                if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderSharedMesh))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(sharedMeshSides, EditorStrings.Config.SharedMeshSides);
                    EditorGUILayout.PropertyField(sharedMeshSegments, EditorStrings.Config.SharedMeshSegments);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetDirty(DirtyFlags.GlobalMesh | DirtyFlags.AllMeshes);
                    }

                    var meshInfo = "These properties will change the mesh tessellation of each Volumetric Light Beam with 'Shared' MeshType.\nAdjust them carefully since they could impact performance.";
                    meshInfo += string.Format("\nShared Mesh stats: {0} vertices, {1} triangles", MeshGenerator.GetSharedMeshVertexCount(), MeshGenerator.GetSharedMeshIndicesCount() / 3);
                    EditorGUILayout.HelpBox(meshInfo, MessageType.Info);
                }
                FoldableHeader.End();

                if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderGlobal3DNoise))
                {
                    EditorGUILayout.PropertyField(globalNoiseScale, EditorStrings.Config.GlobalNoiseScale);
                    EditorGUILayout.PropertyField(globalNoiseVelocity, EditorStrings.Config.GlobalNoiseVelocity);
                }
                FoldableHeader.End();

                if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderFadeOutCamera))
                {
                    EditorGUI.BeginChangeCheck();
                    fadeOutCameraTag.stringValue = EditorGUILayout.TagField(EditorStrings.Config.FadeOutCameraTag, fadeOutCameraTag.stringValue);
                    if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                        m_TargetConfig.ForceUpdateFadeOutCamera();
                }
                FoldableHeader.End();

                if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderFeaturesEnabled))
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUILayout.PropertyField(featureEnabledColorGradient, EditorStrings.Config.FeatureEnabledColorGradient);
                        EditorGUILayout.PropertyField(featureEnabledDepthBlend, EditorStrings.Config.FeatureEnabledDepthBlend);
                        EditorGUILayout.PropertyField(featureEnabledNoise3D, EditorStrings.Config.FeatureEnabledNoise3D);
                        EditorGUILayout.PropertyField(featureEnabledDynamicOcclusion, EditorStrings.Config.FeatureEnabledDynamicOcclusion);
                        EditorGUILayout.PropertyField(featureEnabledMeshSkewing, EditorStrings.Config.FeatureEnabledMeshSkewing);
                        EditorGUILayout.PropertyField(featureEnabledShaderAccuracyHigh, EditorStrings.Config.FeatureEnabledShaderAccuracyHigh);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetDirty(DirtyFlags.Shader | DirtyFlags.AllBeamGeom);
                    }
                }
                FoldableHeader.End();

                if (FoldableHeader.Begin(this, EditorStrings.Config.HeaderInternalData))
                {
                    EditorGUILayout.PropertyField(dustParticlesPrefab, EditorStrings.Config.DustParticlesPrefab);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(ditheringNoiseTexture, EditorStrings.Config.DitheringNoiseTexture);
                    if (EditorGUI.EndChangeCheck())
                        SetDirty(DirtyFlags.Shader);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(noiseTexture3D, EditorStrings.Config.NoiseTexture3D);
                    if (EditorGUI.EndChangeCheck())
                        SetDirty(DirtyFlags.Noise);

                    if (Noise3D.isSupported && !Noise3D.isProperlyLoaded)
                        EditorGUILayout.HelpBox(EditorStrings.Common.HelpNoiseLoadingFailed, MessageType.Error);
                }
                FoldableHeader.End();

                if (GUILayout.Button(EditorStrings.Config.OpenDocumentation, EditorStyles.miniButton))
                {
                    UnityEditor.Help.BrowseURL(Consts.Help.UrlConfig);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(EditorStrings.Config.ResetToDefaultButton, EditorStyles.miniButton))
                    {
                        UnityEditor.Undo.RecordObject(target, "Reset Config Properties");
                        m_TargetConfig.Reset();
                        SetDirty(DirtyFlags.Target | DirtyFlags.Noise);
                    }

                    if (GUILayout.Button(EditorStrings.Config.ResetInternalDataButton, EditorStyles.miniButton))
                    {
                        UnityEditor.Undo.RecordObject(target, "Reset Internal Data");
                        m_TargetConfig.ResetInternalData();
                        SetDirty(DirtyFlags.Target | DirtyFlags.Noise);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (m_NeedToRefreshShader)
                m_TargetConfig.RefreshShader(Config.RefreshShaderFlags.All); // need to be done AFTER ApplyModifiedProperties

            if (m_NeedToReloadNoise)
                Noise3D._EditorForceReloadData(); // Should be called AFTER ApplyModifiedProperties so the Config instance has the proper values when reloading data
        }
    }
}
#endif
