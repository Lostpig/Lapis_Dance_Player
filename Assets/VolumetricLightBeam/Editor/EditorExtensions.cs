#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace VLB
{
    public static class EditorExtensions
    {
        public static GameObject NewBeam()
        {
            return new GameObject("Volumetric Light Beam", typeof(VolumetricLightBeam));
        }

        public static GameObject NewBeam2D()
        {
            var gao = new GameObject("Volumetric Light Beam (2D)", typeof(VolumetricLightBeam));
            gao.GetComponent<VolumetricLightBeam>().dimensions = Dimensions.Dim2D;
            return gao;
        }

        public static GameObject NewBeamAndDust()
        {
            return new GameObject("Volumetric Light Beam + Dust", typeof(VolumetricLightBeam), typeof(VolumetricDustParticles));
        }

        public static GameObject NewSpotLightAndBeam()
        {
            var light = Utils.NewWithComponent<Light>("Spotlight and Beam");
            light.type = LightType.Spot;
            var gao = light.gameObject;
            gao.AddComponent<VolumetricLightBeam>();
            return gao;
        }

        static void OnNewGameObjectCreated(GameObject gao)
        {
            if (Selection.activeGameObject)
                gao.transform.SetParent(Selection.activeGameObject.transform);

            Selection.activeGameObject = gao;
        }

        [MenuItem("GameObject/Light/Volumetric Beam (3D)", false, 100)]
        public static void Menu_CreateNewBeam()
        {
            OnNewGameObjectCreated(NewBeam());
        }

        [MenuItem("GameObject/Light/Volumetric Beam (3D) and Spotlight", false, 101)]
        public static void Menu_CreateSpotLightAndBeam()
        {
            OnNewGameObjectCreated(NewSpotLightAndBeam());
        }

        [MenuItem("GameObject/Light/Volumetric Beam (2D)", false, 102)]
        public static void Menu_CreateNewBeam2D()
        {
            OnNewGameObjectCreated(NewBeam2D());
        }

        [MenuItem("CONTEXT/Light/Attach a Volumetric Beam")]
        public static void Menu_AttachBeam(MenuCommand menuCommand)
        {
            var light = menuCommand.context as Light;
            if (light)
            {
                if (light.GetComponent<VolumetricLightBeam>() == null)
                    Undo.AddComponent<VolumetricLightBeam>(light.gameObject);
            }
        }

        [MenuItem("CONTEXT/VolumetricLightBeam/Documentation")]
        public static void Menu_Beam_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.HelpUrlBeam); }

        [MenuItem("CONTEXT/VolumetricDustParticles/Documentation")]
        public static void Menu_DustParticles_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.HelpUrlDustParticles); }

        [MenuItem("CONTEXT/DynamicOcclusionRaycasting/Documentation")]
        public static void Menu_DynamicOcclusionRaycasting_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.HelpUrlDynamicOcclusionRaycasting); }

        [MenuItem("CONTEXT/DynamicOcclusionDepthBuffer/Documentation")]
        public static void Menu_DynamicOcclusionDepthBuffer_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.HelpUrlDynamicOcclusionDepthBuffer); }

        [MenuItem("CONTEXT/TriggerZone/Documentation")]
        public static void Menu_TriggerZone_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.HelpUrlTriggerZone); }

        [MenuItem("CONTEXT/Config/Documentation")]
        public static void Menu_Config_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.HelpUrlConfig); }

        [MenuItem("CONTEXT/VolumetricLightBeam/Open Global Config")]
        [MenuItem("CONTEXT/VolumetricDustParticles/Open Global Config")]
        [MenuItem("CONTEXT/DynamicOcclusionRaycasting/Open Global Config")]
        [MenuItem("CONTEXT/DynamicOcclusionDepthBuffer/Open Global Config")]
        [MenuItem("CONTEXT/TriggerZone/Open Global Config")]
        public static void Menu_Beam_Config(MenuCommand menuCommand) { Config.EditorSelectInstance(); }

        [MenuItem("CONTEXT/VolumetricLightBeam/Add Dust Particles")]
        public static void Menu_AddDustParticles(MenuCommand menuCommand) { AddComponentFromEditor<VolumetricDustParticles>(menuCommand.context as VolumetricLightBeam); }

        [MenuItem("CONTEXT/VolumetricLightBeam/Add Dynamic Occlusion (Raycasting)")]
        public static void Menu_AddDynamicOcclusion_Raycasting(MenuCommand menuCommand) { AddComponentFromEditor<DynamicOcclusionRaycasting>(menuCommand.context as VolumetricLightBeam); }

        [MenuItem("CONTEXT/VolumetricLightBeam/Add Dynamic Occlusion (Depth Buffer)")]
        public static void Menu_AddDynamicOcclusion_DepthBuffer(MenuCommand menuCommand) { AddComponentFromEditor<DynamicOcclusionDepthBuffer>(menuCommand.context as VolumetricLightBeam); }

        [MenuItem("CONTEXT/VolumetricLightBeam/Add Trigger Zone")]
        public static void Menu_AddTriggerZone(MenuCommand menuCommand) { AddComponentFromEditor<TriggerZone>(menuCommand.context as VolumetricLightBeam); }

        public static void AddComponentFromEditor<TComp>(VolumetricLightBeam self) where TComp : Component
        {
            if (self)
            {
                if (self.GetComponent<TComp>() == null)
                {
                    var comp = Undo.AddComponent<TComp>(self.gameObject);

                    if (comp is DynamicOcclusionRaycasting) (comp as DynamicOcclusionRaycasting).dimensions = self.dimensions;
                }
            }
        }

        [MenuItem("Edit/Volumetric Light Beam Config", false, 20001)]
        public static void Menu_EditOpenConfig()
        {
            Config.EditorSelectInstance();
        }

        public static void HorizontalLineSeparator()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        /// <summary>
        /// Add a EditorGUILayout.ToggleLeft which properly handles multi-object editing
        /// </summary>
        public static void ToggleLeft(this SerializedProperty prop, GUIContent label, params GUILayoutOption[] options)
        {
            ToggleLeft(prop, label, prop.boolValue, options);
        }

        public static void ToggleLeft(this SerializedProperty prop, GUIContent label, bool forcedValue, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            var newValue = EditorGUILayout.ToggleLeft(label, forcedValue, options);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.boolValue = newValue;
        }


        public static bool HasAtLeastOneValue(this SerializedProperty prop, bool value)
        {
            return (prop.boolValue == value) || prop.hasMultipleDifferentValues;
        }

        /// <summary>
        /// Create a EditorGUILayout.Slider which properly handles multi-object editing
        /// We apply the 'convIn' conversion to the SerializedProperty value before exposing it as a Slider.
        /// We apply the 'convOut' conversion to the Slider value to get the SerializedProperty back.
        /// </summary>
        /// <param name="prop">The value the slider shows.</param>
        /// <param name="label">Label in front of the slider.</param>
        /// <param name="leftValue">The value at the left end of the slider.</param>
        /// <param name="rightValue">The value at the right end of the slider.</param>
        /// <param name="convIn">Conversion applied on the SerializedProperty to get the Slider value</param>
        /// <param name="convOut">Conversion applied on the Slider value to get the SerializedProperty</param>
        public static bool FloatSlider(
            this SerializedProperty prop,
            GUIContent label,
            float leftValue, float rightValue,
            System.Func<float, float> convIn,
            System.Func<float, float> convOut,
            params GUILayoutOption[] options)
        {
            var floatValue = convIn(prop.floatValue);
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                {
                    floatValue = EditorGUILayout.Slider(label, floatValue, leftValue, rightValue, options);
                }
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = convOut(floatValue);
                return true;
            }
            return false;
        }

        public static bool FloatSlider(
            this SerializedProperty prop,
            GUIContent label,
            float leftValue, float rightValue,
            params GUILayoutOption[] options)
        {
            var floatValue = prop.floatValue;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                {
                    floatValue = EditorGUILayout.Slider(label, floatValue, leftValue, rightValue, options);
                }
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = floatValue;
                return true;
            }
            return false;
        }
/*
        public static void ToggleFromLight(this SerializedProperty prop)
        {
            ToggleLeft(
                prop,
                new GUIContent("From Spot", "Get the value from the Light Spot"),
                GUILayout.MaxWidth(80.0f));
        }
*/
        public static void ToggleUseGlobalNoise(this SerializedProperty prop)
        {
            ToggleLeft(
                prop,
                new GUIContent("Global", "Get the value from the Global 3D Noise"),
                GUILayout.MaxWidth(55.0f));
        }

        public static void CustomEnum<EnumType>(this SerializedProperty prop, GUIContent content, GUIContent[] descriptions)
        {
            Debug.Assert(System.Enum.GetNames(typeof(EnumType)).Length == descriptions.Length, string.Format("Enum '{0}' and the description array don't have the same size", typeof(EnumType)));

            int enumValueIndex = prop.enumValueIndex;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                {
                    enumValueIndex = EditorGUILayout.Popup(content, enumValueIndex, descriptions);
                }
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
                prop.enumValueIndex = enumValueIndex;
        }
    }
}
#endif