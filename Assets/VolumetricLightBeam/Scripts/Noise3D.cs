using UnityEngine;

#pragma warning disable 0429, 0162 // Unreachable expression code detected (because of Noise3D.isSupported on mobile)

namespace VLB
{
    public static class Noise3D
    {
        /// <summary>
        /// Returns if the 3D Noise feature is supported on the current platform or not.
        /// 3D Noise feature requires a graphicsShaderLevel 35 or higher (which is basically Shader Model 3.5 / OpenGL ES 3.0 or above)
        /// If not supported, the beams will look like the 3D Noise has been disabled.
        /// </summary>
        public static bool isSupported {
            get {
                if (!ms_IsSupportedChecked)
                {
                    ms_IsSupported = SystemInfo.graphicsShaderLevel >= kMinShaderLevel;
                    if (!ms_IsSupported)
                        Debug.LogWarning(isNotSupportedString);
                    ms_IsSupportedChecked = true;
                }
                return ms_IsSupported;
            }
        }

        /// <summary>
        /// Returns if the 3D Noise Texture has been successfully loaded or not.
        /// If the feature is not supported (isSupported == false), isProperlyLoaded is also false.
        /// </summary>
        public static bool isProperlyLoaded { get { return ms_NoiseTexture != null; } }

        public static string isNotSupportedString { get {
                var str = string.Format("3D Noise requires higher shader capabilities (Shader Model 3.5 / OpenGL ES 3.0), which are not available on the current platform: graphicsShaderLevel (current/required) = {0} / {1}",
                    SystemInfo.graphicsShaderLevel,
                    kMinShaderLevel);
#if UNITY_EDITOR
                str += "\nPlease change the editor's graphics emulation for a more capable one via \"Edit/Graphics Emulation\" and press Play to force the light beams to be recomputed.";
#endif
                return str;
            }
        }

        static bool ms_IsSupportedChecked = false;
        static bool ms_IsSupported = false;
        static Texture3D ms_NoiseTexture = null;

        const int kMinShaderLevel = 35; // Shader Model 3.5 / OpenGL ES 3.0 to handle sampler3D -> https://docs.unity3d.com/ScriptReference/SystemInfo-graphicsShaderLevel.html

        [RuntimeInitializeOnLoadMethod]
        static void OnStartUp()
        {
            LoadIfNeeded();
        }

#if UNITY_EDITOR
        public static void _EditorForceReloadData()
        {
            ms_NoiseTexture = null;
            LoadIfNeeded();
        }
#endif

        public static void LoadIfNeeded()
        {
            if (!isSupported) return;

            if (ms_NoiseTexture == null)
            {
                ms_NoiseTexture = Config.Instance.noiseTexture3D;

                Shader.SetGlobalTexture(ShaderProperties.GlobalNoiseTex3D, ms_NoiseTexture);
                Shader.SetGlobalFloat(ShaderProperties.GlobalNoiseCustomTime, -1.0f);
            }
        }
    }
}