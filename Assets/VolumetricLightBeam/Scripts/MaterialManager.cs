using UnityEngine;
using System.Collections;

namespace VLB
{
    public static class MaterialManager
    {
        public static MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

        public enum BlendingMode
        {
            Additive,
            SoftAdditive,
            TraditionalTransparency,
            Count
        }

        public enum Noise3D
        {
            Off,
            On,
            Count
        }

        public enum DepthBlend
        {
            Off,
            On,
            Count
        }

        public enum ColorGradient
        {
            Off,
            MatrixLow,
            MatrixHigh,
            Count
        }

        public enum DynamicOcclusion
        {
            Off,
            ClippingPlane,
            DepthTexture,
            Count
        }

        public enum MeshSkewing
        {
            Off,
            On,
            Count
        }

        public enum ShaderAccuracy
        {
            Fast,
            High,
            Count
        }

        static readonly UnityEngine.Rendering.BlendMode[] BlendingMode_SrcFactor = new UnityEngine.Rendering.BlendMode[(int)BlendingMode.Count]
        {
            UnityEngine.Rendering.BlendMode.One,                // Additive
            UnityEngine.Rendering.BlendMode.OneMinusDstColor,   // SoftAdditive
            UnityEngine.Rendering.BlendMode.SrcAlpha,           // TraditionalTransparency
        };

        static readonly UnityEngine.Rendering.BlendMode[] BlendingMode_DstFactor = new UnityEngine.Rendering.BlendMode[(int)BlendingMode.Count]
        {
            UnityEngine.Rendering.BlendMode.One,                // Additive
            UnityEngine.Rendering.BlendMode.One,                // SoftAdditive
            UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,   // TraditionalTransparency
        };

        static readonly bool[] BlendingMode_AlphaAsBlack = new bool[(int)BlendingMode.Count]
        {
            true,   // Additive
            true,   // SoftAdditive
            false,  // TraditionalTransparency
        };

        static int kStaticPropertiesCount = (int)BlendingMode.Count * (int)Noise3D.Count * (int)DepthBlend.Count * (int)ColorGradient.Count * (int)DynamicOcclusion.Count * (int)MeshSkewing.Count * (int)ShaderAccuracy.Count;

        public struct StaticProperties
        {
            public BlendingMode blendingMode;
            public Noise3D noise3D;
            public DepthBlend depthBlend;
            public ColorGradient colorGradient;
            public DynamicOcclusion dynamicOcclusion;
            public MeshSkewing meshSkewing;
            public ShaderAccuracy shaderAccuracy;

            int blendingModeID      { get { return (int)blendingMode; } }
            int noise3DID           { get { return Config.Instance.featureEnabledNoise3D ? (int)noise3D : 0; } }
            int depthBlendID        { get { return Config.Instance.featureEnabledDepthBlend ? (int)depthBlend : 0; } }
            int colorGradientID     { get { return Config.Instance.featureEnabledColorGradient != FeatureEnabledColorGradient.Off ? (int)colorGradient : 0; } }
            int dynamicOcclusionID  { get { return Config.Instance.featureEnabledDynamicOcclusion ? (int)dynamicOcclusion : 0; } }
            int meshSkewingID       { get { return Config.Instance.featureEnabledMeshSkewing ? (int)meshSkewing : 0; } }
            int shaderAccuracyID    { get { return Config.Instance.featureEnabledShaderAccuracyHigh ? (int)shaderAccuracy : 0; } }

            public int materialID
            {
                get
                {
                    return (((((((blendingModeID)
                            * (int)Noise3D.Count + noise3DID)
                            * (int)DepthBlend.Count + depthBlendID)
                            * (int)ColorGradient.Count + colorGradientID)
                            * (int)DynamicOcclusion.Count + dynamicOcclusionID)
                            * (int)MeshSkewing.Count + meshSkewingID)
                            * (int)ShaderAccuracy.Count + shaderAccuracyID)
                            ;
                }
            }

            public void ApplyToMaterial(Material mat)
            {
                mat.SetKeywordEnabled(ShaderKeywords.AlphaAsBlack, BlendingMode_AlphaAsBlack[(int)blendingMode]);
                mat.SetKeywordEnabled(ShaderKeywords.ColorGradientMatrixLow,  colorGradient == ColorGradient.MatrixLow);
                mat.SetKeywordEnabled(ShaderKeywords.ColorGradientMatrixHigh, colorGradient == ColorGradient.MatrixHigh);
                mat.SetKeywordEnabled(ShaderKeywords.DepthBlend, depthBlend == DepthBlend.On);
                mat.SetKeywordEnabled(ShaderKeywords.Noise3D, noise3D == Noise3D.On);
                mat.SetKeywordEnabled(ShaderKeywords.OcclusionClippingPlane, dynamicOcclusion == DynamicOcclusion.ClippingPlane);
                mat.SetKeywordEnabled(ShaderKeywords.OcclusionDepthTexture, dynamicOcclusion == DynamicOcclusion.DepthTexture);
                mat.SetKeywordEnabled(ShaderKeywords.MeshSkewing, meshSkewing == MeshSkewing.On);
                mat.SetKeywordEnabled(ShaderKeywords.ShaderAccuracyHigh, shaderAccuracy == ShaderAccuracy.High);

                mat.SetInt(ShaderProperties.BlendSrcFactor, (int)BlendingMode_SrcFactor[(int)blendingMode]);
                mat.SetInt(ShaderProperties.BlendDstFactor, (int)BlendingMode_DstFactor[(int)blendingMode]);
            }
        }

        public static Material NewMaterialTransient(bool gpuInstanced)
        {
            var material = NewMaterialPersistent(Config.Instance.beamShader, gpuInstanced);
            if (material)
            {
                material.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
                material.renderQueue = Config.Instance.geometryRenderQueue;
            }
            return material;
        }

        public static Material NewMaterialPersistent(Shader shader, bool gpuInstanced)
        {
            if (!shader)
            {
                Debug.LogError("Invalid VLB Shader. Please try to reset the VLB Config asset or reinstall the plugin.");
                return null;
            }

            var material = new Material(shader);
            BatchingHelper.SetMaterialProperties(material, gpuInstanced);
            return material;
        }

        class MaterialsGroup
        {
            public Material[] materials = new Material[kStaticPropertiesCount];
        }

        static Hashtable ms_MaterialsGroup = new Hashtable(1);

        public static Material GetInstancedMaterial(uint groupID, ref StaticProperties staticProps) // pass StaticProperties by ref to avoid per value arg copy
        {
            MaterialsGroup group = (MaterialsGroup)ms_MaterialsGroup[groupID];
            if (group == null)
            {
                group = new MaterialsGroup();
                ms_MaterialsGroup[groupID] = group;
            }

            int matID = staticProps.materialID;
            Debug.Assert(matID < kStaticPropertiesCount);
            var mat = group.materials[matID];
            if (mat == null)
            {
                mat = NewMaterialTransient(gpuInstanced:true);
                if(mat)
                {
                    group.materials[matID] = mat;
                    staticProps.ApplyToMaterial(mat);
                }
            }

            return mat;
        }
    }
}
