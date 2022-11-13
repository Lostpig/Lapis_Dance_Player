#define LOAD_NOISE3D_ON_STARTUP // prevent loading spike from happening when the 1st beam is instantiated

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

        const HideFlags kHideFlags = HideFlags.HideAndDontSave; // hide the noise texture
        const int kMinShaderLevel = 35; // Shader Model 3.5 / OpenGL ES 3.0 to handle sampler3D -> https://docs.unity3d.com/ScriptReference/SystemInfo-graphicsShaderLevel.html

#if LOAD_NOISE3D_ON_STARTUP
        [RuntimeInitializeOnLoadMethod]
        static void OnStartUp()
        {
            LoadIfNeeded();
        }
#endif

#if UNITY_EDITOR
        public static void _EditorForceReloadData()
        {
            if (ms_NoiseTexture)
            {
                Object.DestroyImmediate(ms_NoiseTexture);
                ms_NoiseTexture = null;
            }
            LoadIfNeeded();
        }
#endif

        public static void LoadIfNeeded()
        {
            if (!isSupported) return;

            if (ms_NoiseTexture == null)
            {
                ms_NoiseTexture = LoadTexture3D(Config.Instance.noise3DData, Config.Instance.noise3DSize);
                if(ms_NoiseTexture)
                    ms_NoiseTexture.hideFlags = kHideFlags;

                Shader.SetGlobalTexture(ShaderProperties.GlobalNoiseTex3D, ms_NoiseTexture);
                Shader.SetGlobalFloat(ShaderProperties.GlobalNoiseCustomTime, -1.0f);
            }
        }

        static Texture3D LoadTexture3D(TextAsset textData, int size)
        {
            if (textData == null)
            {
                Debug.LogError("Fail to open Noise 3D Data");
                return null;
            }

            var bytes = textData.bytes;
            Debug.Assert(bytes != null);

            int dataLen = Mathf.Max(0, size * size * size);
            if (bytes.Length != dataLen)
            {
                Debug.LogErrorFormat("Noise 3D Data file has not the proper size {0}x{0}x{0}", size);
                return null;
            }

            var tex = new Texture3D(size, size, size, TextureFormat.Alpha8, false);

            var colors = new Color[dataLen];
            for (int i = 0; i < dataLen; ++i)
                colors[i] = new Color32(0, 0, 0, bytes[i]);

            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }
    }
}