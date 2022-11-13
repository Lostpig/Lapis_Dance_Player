using UnityEngine;

namespace VLB
{
    public static class ShaderProperties
    {
        public static readonly int FadeOutFactor                = Shader.PropertyToID("_FadeOutFactor");
        public static readonly int ConeSlopeCosSin              = Shader.PropertyToID("_ConeSlopeCosSin");
        public static readonly int ConeRadius                   = Shader.PropertyToID("_ConeRadius");
        public static readonly int ConeApexOffsetZ              = Shader.PropertyToID("_ConeApexOffsetZ");
        public static readonly int ColorFlat                    = Shader.PropertyToID("_ColorFlat");
        public static readonly int AlphaInside                  = Shader.PropertyToID("_AlphaInside");
        public static readonly int AlphaOutside                 = Shader.PropertyToID("_AlphaOutside");
        public static readonly int AttenuationLerpLinearQuad    = Shader.PropertyToID("_AttenuationLerpLinearQuad");
        public static readonly int DistanceFallOff              = Shader.PropertyToID("_DistanceFallOff");
        public static readonly int DistanceCamClipping          = Shader.PropertyToID("_DistanceCamClipping");
        public static readonly int FresnelPow                   = Shader.PropertyToID("_FresnelPow");
        public static readonly int GlareBehind                  = Shader.PropertyToID("_GlareBehind");
        public static readonly int GlareFrontal                 = Shader.PropertyToID("_GlareFrontal");
        public static readonly int DrawCap                      = Shader.PropertyToID("_DrawCap");
        public static readonly int DepthBlendDistance           = Shader.PropertyToID("_DepthBlendDistance");
        public static readonly int NoiseVelocityAndScale        = Shader.PropertyToID("_NoiseVelocityAndScale");
        public static readonly int NoiseParam                   = Shader.PropertyToID("_NoiseParam");
        public static readonly int CameraParams                 = Shader.PropertyToID("_CameraParams");
        public static readonly int ColorGradientMatrix          = Shader.PropertyToID("_ColorGradientMatrix");
        public static readonly int LocalToWorldMatrix           = Shader.PropertyToID("_LocalToWorldMatrix");
        public static readonly int WorldToLocalMatrix           = Shader.PropertyToID("_WorldToLocalMatrix");
        public static readonly int BlendSrcFactor               = Shader.PropertyToID("_BlendSrcFactor");
        public static readonly int BlendDstFactor               = Shader.PropertyToID("_BlendDstFactor");
        public static readonly int DynamicOcclusionClippingPlaneWS = Shader.PropertyToID("_DynamicOcclusionClippingPlaneWS");
        public static readonly int DynamicOcclusionClippingPlaneProps = Shader.PropertyToID("_DynamicOcclusionClippingPlaneProps");
        public static readonly int DynamicOcclusionDepthTexture = Shader.PropertyToID("_DynamicOcclusionDepthTexture");
        public static readonly int DynamicOcclusionDepthProps   = Shader.PropertyToID("_DynamicOcclusionDepthProps");
        public static readonly int LocalForwardDirection        = Shader.PropertyToID("_LocalForwardDirection");
        public static readonly int TiltVector                   = Shader.PropertyToID("_TiltVector");
        public static readonly int AdditionalClippingPlaneWS    = Shader.PropertyToID("_AdditionalClippingPlaneWS");

        public static readonly int GlobalNoiseTex3D             = Shader.PropertyToID("_VLB_NoiseTex3D");
        public static readonly int GlobalNoiseCustomTime        = Shader.PropertyToID("_VLB_NoiseCustomTime");
        public static readonly int GlobalDitheringFactor        = Shader.PropertyToID("_VLB_DitheringFactor");
        public static readonly int GlobalDitheringNoiseTex      = Shader.PropertyToID("_VLB_DitheringNoiseTex");

        public const string KeywordSRP = "VLB_SRP_API";
    }
}

