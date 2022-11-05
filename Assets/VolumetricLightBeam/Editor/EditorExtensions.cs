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
        static void Menu_CreateNewBeam()
        {
            OnNewGameObjectCreated(NewBeam());
        }

        [MenuItem("GameObject/Light/Volumetric Beam (3D) and Spotlight", false, 101)]
        static void Menu_CreateSpotLightAndBeam()
        {
            OnNewGameObjectCreated(NewSpotLightAndBeam());
        }

        [MenuItem("GameObject/Light/Volumetric Beam (2D)", false, 102)]
        static void Menu_CreateNewBeam2D()
        {
            OnNewGameObjectCreated(NewBeam2D());
        }

        const string kAddVolumetricBeam = "CONTEXT/Light/Attach a Volumetric Beam";
        static bool CanAddVolumetricBeam(Light light) { return !Application.isPlaying && light != null && light.type == LightType.Spot && light.GetComponent<VolumetricLightBeam>() == null; }

        [MenuItem(kAddVolumetricBeam, false)]
        static void Menu_AttachBeam_Command(MenuCommand menuCommand)
        {
            var light = menuCommand.context as Light;
            if (CanAddVolumetricBeam(light))
                Undo.AddComponent<VolumetricLightBeam>(light.gameObject);
        }

        [MenuItem(kAddVolumetricBeam, true)]
        static bool Menu_AttachBeam_Validate() { return CanAddVolumetricBeam(GetActiveLight()); }


        [MenuItem("CONTEXT/" + VolumetricLightBeam.ClassName + "/Documentation")]
        static void Menu_Beam_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlBeam); }

        [MenuItem("CONTEXT/" + VolumetricDustParticles.ClassName + "/Documentation")]
        static void Menu_DustParticles_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlDustParticles); }

        [MenuItem("CONTEXT/" + DynamicOcclusionRaycasting.ClassName + "/Documentation")]
        static void Menu_DynamicOcclusionRaycasting_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlDynamicOcclusionRaycasting); }

        [MenuItem("CONTEXT/" + DynamicOcclusionDepthBuffer.ClassName + "/Documentation")]
        static void Menu_DynamicOcclusionDepthBuffer_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlDynamicOcclusionDepthBuffer); }

        [MenuItem("CONTEXT/" + TriggerZone.ClassName + "/Documentation")]
        static void Menu_TriggerZone_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlTriggerZone); }

        [MenuItem("CONTEXT/" + EffectFlicker.ClassName + "/Documentation")]
        static void Menu_EffectFlicker_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlEffectFlicker); }

        [MenuItem("CONTEXT/" + EffectPulse.ClassName + "/Documentation")]
        static void Menu_EffectPulse_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlEffectPulse); }

        [MenuItem("CONTEXT/" + Config.ClassName + "/Documentation")]
        static void Menu_Config_Doc(MenuCommand menuCommand) { Application.OpenURL(Consts.Help.UrlConfig); }

        [MenuItem("CONTEXT/" + VolumetricLightBeam.ClassName + "/Open Global Config")]
        [MenuItem("CONTEXT/" + VolumetricDustParticles.ClassName + "/Open Global Config")]
        [MenuItem("CONTEXT/" + DynamicOcclusionAbstractBase.ClassName + "/Open Global Config")]
        [MenuItem("CONTEXT/" + TriggerZone.ClassName + "/Open Global Config")]
        [MenuItem("CONTEXT/" + EffectAbstractBase.ClassName + "/Open Global Config")]
        public static void Menu_Beam_Config(MenuCommand menuCommand) { Config.EditorSelectInstance(); }

        const string kAddParticles = "CONTEXT/" + VolumetricLightBeam.ClassName + "/Add Dust Particles";
        [MenuItem(kAddParticles, false)]                    static void Menu_AddDustParticles_Command(MenuCommand menuCommand) { AddComponentFromEditor<VolumetricDustParticles>(menuCommand.context as VolumetricLightBeam); }
        [MenuItem(kAddParticles, true)]                     static bool Menu_AddDustParticles_Validate() { return CanAddComponentFromEditor<VolumetricDustParticles>(GetActiveVolumetricLightBeam()); }

        const string kAddDynamicOcclusionRaycasting = "CONTEXT/" + VolumetricLightBeam.ClassName + "/Add Dynamic Occlusion (Raycasting)";
        [MenuItem(kAddDynamicOcclusionRaycasting, false)]   static void Menu_AddDynamicOcclusionRaycasting_Command(MenuCommand menuCommand) { AddComponentFromEditor<DynamicOcclusionRaycasting>(menuCommand.context as VolumetricLightBeam); }
        [MenuItem(kAddDynamicOcclusionRaycasting, true)]    static bool Menu_AddDynamicOcclusionRaycasting_Validate() { return Config.Instance.featureEnabledDynamicOcclusion && CanAddComponentFromEditor<DynamicOcclusionAbstractBase>(GetActiveVolumetricLightBeam()); }

        const string kAddDynamicOcclusionDepthBuffer = "CONTEXT/" + VolumetricLightBeam.ClassName + "/Add Dynamic Occlusion (Depth Buffer)";
        [MenuItem(kAddDynamicOcclusionDepthBuffer, false)]  static void Menu_AddDynamicOcclusionDepthBuffer_Command(MenuCommand menuCommand) { AddComponentFromEditor<DynamicOcclusionDepthBuffer>(menuCommand.context as VolumetricLightBeam); }
        [MenuItem(kAddDynamicOcclusionDepthBuffer, true)]   static bool Menu_AddDynamicOcclusionDepthBuffer_Validate() { return Config.Instance.featureEnabledDynamicOcclusion && CanAddComponentFromEditor<DynamicOcclusionAbstractBase>(GetActiveVolumetricLightBeam()); }

        const string kAddTriggerZone = "CONTEXT/" + VolumetricLightBeam.ClassName + "/Add Trigger Zone";
        [MenuItem(kAddTriggerZone, false)]                  static void Menu_AddTriggerZone_Command(MenuCommand menuCommand) { AddComponentFromEditor<TriggerZone>(menuCommand.context as VolumetricLightBeam); }
        [MenuItem(kAddTriggerZone, true)]                   static bool Menu_AddTriggerZone_Validate() { return CanAddComponentFromEditor<TriggerZone>(GetActiveVolumetricLightBeam()); }

        const string kAddEffectFlicker = "CONTEXT/" + VolumetricLightBeam.ClassName + "/Add Effect Flicker";
        [MenuItem(kAddEffectFlicker, false)]                static void Menu_EffectFlicker_Command(MenuCommand menuCommand) { AddComponentFromEditor<EffectFlicker>(menuCommand.context as VolumetricLightBeam); }
        [MenuItem(kAddEffectFlicker, true)]                 static bool Menu_EffectFlicker_Validate() { return CanAddComponentFromEditor<EffectAbstractBase>(GetActiveVolumetricLightBeam()); }

        const string kAddEffectPulse = "CONTEXT/" + VolumetricLightBeam.ClassName + "/Add Effect Pulse";
        [MenuItem(kAddEffectPulse, false)]                  static void Menu_EffectPulse_Command(MenuCommand menuCommand) { AddComponentFromEditor<EffectPulse>(menuCommand.context as VolumetricLightBeam); }
        [MenuItem(kAddEffectPulse, true)]                   static bool Menu_EffectPulse_Validate() { return CanAddComponentFromEditor<EffectAbstractBase>(GetActiveVolumetricLightBeam()); }

        static Light GetActiveLight()                               { return Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<Light>() : null; }
        static VolumetricLightBeam GetActiveVolumetricLightBeam()   { return Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<VolumetricLightBeam>() : null; }

        static bool CanAddComponentFromEditor<TComp>(VolumetricLightBeam self) where TComp : Component
        {
            return !Application.isPlaying && self != null && self.GetComponent<TComp>() == null;
        }

        public static void AddComponentFromEditor<TComp>(VolumetricLightBeam self) where TComp : Component
        {
            if (CanAddComponentFromEditor<TComp>(self))
            {
                /*var comp =*/ Undo.AddComponent<TComp>(self.gameObject);
            }
        }

        [MenuItem("Edit/Volumetric Light Beam Config", false, 20001)]
        static void Menu_EditOpenConfig()
        {
            Config.EditorSelectInstance();
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

        public static void CustomEnum<EnumType>(this SerializedProperty prop, GUIContent content, string[] descriptions = null)
        {
            if(descriptions == null)
                descriptions = System.Enum.GetNames(typeof(EnumType));

            Debug.Assert(System.Enum.GetNames(typeof(EnumType)).Length == descriptions.Length, string.Format("Enum '{0}' and the description array don't have the same size", typeof(EnumType)));

            int enumValueIndex = prop.enumValueIndex;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                {
#if UNITY_2018_1_OR_NEWER
                    enumValueIndex = EditorGUILayout.Popup(content, enumValueIndex, descriptions);
#else
                    enumValueIndex = EditorGUILayout.Popup(content.text, enumValueIndex, descriptions);
#endif
                }
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
                prop.enumValueIndex = enumValueIndex;
        }

        public static void CustomMask<EnumType>(this SerializedProperty prop, GUIContent content, string[] descriptions = null)
        {
            if (descriptions == null)
                descriptions = System.Enum.GetNames(typeof(EnumType));

            Debug.Assert(System.Enum.GetNames(typeof(EnumType)).Length == descriptions.Length, string.Format("Enum '{0}' and the description array don't have the same size", typeof(EnumType)));

            int intValue = prop.intValue;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
                {
                    intValue = EditorGUILayout.MaskField(content, intValue, descriptions);
                }
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
                prop.intValue = intValue;
        }
    }
}
#endif