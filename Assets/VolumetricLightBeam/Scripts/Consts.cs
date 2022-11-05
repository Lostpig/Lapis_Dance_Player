using UnityEngine;

namespace VLB
{
    public static class Consts
    {
        public const string PluginFolder = "Plugins/VolumetricLightBeam";

        public static class Help
        {
            const string UrlBase = "http://saladgamer.com/vlb-doc/";
            const string UrlSuffix = ".html";
            public const string UrlBeam = UrlBase + "comp-lightbeam" + UrlSuffix;
            public const string UrlDustParticles = UrlBase + "comp-dustparticles" + UrlSuffix;
            public const string UrlDynamicOcclusionRaycasting = UrlBase + "comp-dynocclusion-raycasting" + UrlSuffix;
            public const string UrlDynamicOcclusionDepthBuffer = UrlBase + "comp-dynocclusion-depthbuffer" + UrlSuffix;
            public const string UrlTriggerZone = UrlBase + "comp-triggerzone" + UrlSuffix;
            public const string UrlSkewingHandle = UrlBase + "comp-skewinghandle" + UrlSuffix;
            public const string UrlEffectFlicker = UrlBase + "comp-effect-flicker" + UrlSuffix;
            public const string UrlEffectPulse = UrlBase + "comp-effect-pulse" + UrlSuffix;
            public const string UrlConfig = UrlBase + "config" + UrlSuffix;
        }

        public static class Internal
        {
            public static readonly bool ProceduralObjectsVisibleInEditor = true;
            public static HideFlags ProceduralObjectsHideFlags { get { return ProceduralObjectsVisibleInEditor ? (HideFlags.NotEditable | HideFlags.DontSave) : (HideFlags.HideAndDontSave); } }
        }

        public static class Beam
        {
            public static readonly Color FlatColor = Color.white;
            public const ColorMode ColorModeDefault = ColorMode.Flat;

            public const float IntensityDefault = 1f;
            public const float IntensityMin = 0f;
            public const float SpotAngleDefault = 35f;
            public const float SpotAngleMin = 0.1f;
            public const float SpotAngleMax = 179.9f;
            public const float ConeRadiusStart = 0.1f;
            public const MeshType GeomMeshType = MeshType.Shared;
            public const int GeomSidesDefault = 18;
            public const int GeomSidesMin = 3;
            public const int GeomSidesMax = 256;
            public const int GeomSegmentsDefault = 5;
            public const int GeomSegmentsMin = 0;
            public const int GeomSegmentsMax = 64;
            public const bool GeomCap = false;

            public const AttenuationEquation AttenuationEquationDefault = AttenuationEquation.Quadratic;
            public const float AttenuationCustomBlending = 0.5f;
            public const float FallOffStart = 0f;
            public const float FallOffEnd = 3f;
            public const float FallOffDistancesMinThreshold = 0.01f;

            public const float DepthBlendDistance = 2f;
            public const float CameraClippingDistance = 0.5f;

            public const float FresnelPowMaxValue = 10f;
            public const float FresnelPow = 8f;

            public const float GlareFrontal = 0.5f;
            public const float GlareBehind = 0.5f;

            public const NoiseMode NoiseModeDefault = NoiseMode.Disabled;
            public const float NoiseIntensityMin = 0.0f;
            public const float NoiseIntensityMax = 1.0f;
            public const float NoiseIntensityDefault = 0.5f;
            public const float NoiseScaleMin = 0.01f;
            public const float NoiseScaleMax = 2f;
            public const float NoiseScaleDefault = 0.5f;

            public static readonly Vector3 NoiseVelocityDefault = new Vector3(0.07f, 0.18f, 0.05f);

            public const BlendingMode BlendingModeDefault = BlendingMode.Additive;
            public const ShaderAccuracy ShaderAccuracyDefault = ShaderAccuracy.Fast;

            public const float FadeOutBeginDefault = -150;
            public const float FadeOutEndDefault = -200;
            public const Dimensions DimensionsDefault = Dimensions.Dim3D;
            public static readonly Vector2 TiltDefault = Vector2.zero;
            public static readonly Vector3 SkewingLocalForwardDirectionDefault = Vector3.forward;
            public const Transform ClippingPlaneTransformDefault = null;
        }

        public static class DustParticles
        {
            public const float AlphaDefault = 0.5f;
            public const float SizeDefault = 0.01f;
            public const ParticlesDirection DirectionDefault = ParticlesDirection.Random;
            public static readonly Vector3 VelocityDefault = new Vector3(0.0f, 0.0f, 0.03f);
            public const float DensityDefault = 5f;
            public const float DensityMin = 0f;
            public const float DensityMax = 1000f;
            public static readonly MinMaxRangeFloat SpawnDistanceRangeDefault = new MinMaxRangeFloat(0.0f, 0.7f);
            public const bool CullingEnabledDefault = false;
            public const float CullingMaxDistanceDefault = 10f;
            public const float CullingMaxDistanceMin = 1f;
        }

        public static class DynOcclusion
        {
            public static readonly LayerMask LayerMaskDefault = 1; // Default layer
            public const float FadeDistanceToSurfaceDefault = 0.25f;
            public const DynamicOcclusionUpdateRate UpdateRateDefault = DynamicOcclusionUpdateRate.EveryXFrames;
            public const int WaitFramesCountDefault = 3;

            public const Dimensions RaycastingDimensionsDefault = Dimensions.Dim3D;
            public const bool RaycastingConsiderTriggersDefault = false;
            public const float RaycastingMinOccluderAreaDefault = 0.0f;
            public const float RaycastingMinSurfaceRatioDefault = 0.5f;
            public const float RaycastingMinSurfaceRatioMin = 50f;
            public const float RaycastingMinSurfaceRatioMax = 100f;
            public const float RaycastingMaxSurfaceDotDefault = 0.25f; // around 75 degrees
            public const float RaycastingMaxSurfaceAngleMin = 45f;
            public const float RaycastingMaxSurfaceAngleMax = 90f;
            public const PlaneAlignment RaycastingPlaneAlignmentDefault = PlaneAlignment.Surface;
            public const float RaycastingPlaneOffsetDefault = 0.1f;

            public const int DepthBufferDepthMapResolutionDefault = 32;
            public const bool DepthBufferOcclusionCullingDefault = true;
        }

        public static class Effects
        {
            public const EffectAbstractBase.ComponentsToChange ComponentsToChangeDefault = (EffectAbstractBase.ComponentsToChange)int.MaxValue;
            public const bool RestoreBaseIntensityDefault = true;
            public const float FrequencyDefault = 10.0f;
            public const bool PerformPausesDefault = false;
            public static readonly MinMaxRangeFloat FlickeringDurationDefault = new MinMaxRangeFloat(1.0f, 4.0f);
            public static readonly MinMaxRangeFloat PauseDurationDefault = new MinMaxRangeFloat(0.0f, 1.0f);
            public static readonly MinMaxRangeFloat IntensityAmplitudeDefault = new MinMaxRangeFloat(-1.0f, 1.0f);
            public const float SmoothingDefault = 0.05f;
        }

        public static class Config
        {
            public const bool GeometryOverrideLayerDefault = true;
            public const int GeometryLayerIDDefault = 1;
            public const string GeometryTagDefault = "Untagged";
            public const string FadeOutCameraTagDefault = "MainCamera";
            public const RenderQueue GeometryRenderQueueDefault = RenderQueue.Transparent;
            public const RenderPipeline GeometryRenderPipelineDefault = RenderPipeline.BuiltIn;
            public const RenderingMode GeometryRenderingModeDefault = RenderingMode.SinglePass;
            public const int Noise3DSizeDefault = 64;
            public const int SharedMeshSides = 24;
            public const int SharedMeshSegments = 5;
            public const float DitheringFactor = 0.0f;
            public const bool FeatureEnabledDefault = true;
            public const FeatureEnabledColorGradient FeatureEnabledColorGradientDefault = FeatureEnabledColorGradient.HighOnly;
        }
    }
}