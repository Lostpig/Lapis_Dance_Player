using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB
{
    public static class DummyMaterial
    {
#if UNITY_EDITOR
        static string GetPath(Shader shader)
        {
            const string kDummyFilename = "VLBDummyMaterial.mat";
            const string kDummyPathFallback = "Assets/" + Consts.PluginFolder + "/Shaders/" + kDummyFilename;

            if (shader == null)
                return kDummyPathFallback;

            var shaderPath = AssetDatabase.GetAssetPath(shader);
            if (string.IsNullOrEmpty(shaderPath))
                return kDummyPathFallback;

            var shaderFolder = System.IO.Path.GetDirectoryName(shaderPath);
            return System.IO.Path.Combine(shaderFolder, kDummyFilename);
        }

        /// <summary>
        /// Create a dummy material with the proper instancing flag to prevent from stripping away needed shader variants when exporting build
        /// </summary>
        public static Material Create(Shader shader, bool gpuInstanced)
        {
            if (shader == null)
                return null;

            string path = GetPath(shader);
            var dummyMat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (dummyMat == null
             || dummyMat.shader != shader
             || BatchingHelper.IsGpuInstancingEnabled(dummyMat) != gpuInstanced)
            {
                dummyMat = MaterialManager.NewMaterialPersistent(shader, gpuInstanced);
                if (dummyMat)
                    AssetDatabase.CreateAsset(dummyMat, path);
            }

            return dummyMat;
        }
#endif
    }
}