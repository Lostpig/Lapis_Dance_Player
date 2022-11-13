using UnityEngine;

namespace VLB
{
    public static class Consts
    {
        // HELP
        const string HelpUrlBase = "http://saladgamer.com/vlb-doc/";
        public const string HelpUrlBeam = HelpUrlBase + "comp-lightbeam/";
        public const string HelpUrlDustParticles = HelpUrlBase + "comp-dustparticles/";
        public const string HelpUrlDynamicOcclusionRaycasting = HelpUrlBase + "comp-dynocclusion-raycasting/";
        public const string HelpUrlDynamicOcclusionDepthBuffer = HelpUrlBase + "comp-dynocclusion-depthbuffer/";
        public const string HelpUrlTriggerZone = HelpUrlBase + "comp-triggerzone/";
        public const string HelpUrlSkewingHandle = HelpUrlBase + "comp-skewinghandle/";
        public const string HelpUrlConfig = HelpUrlBase + "config/";

        // INTERNAL
        public static readonly bool ProceduralObjectsVisibleInEditor = true;
        public static HideFlags ProceduralObjectsHideFlags { get { return ProceduralObjectsVisibleInEditor ? (HideFlags.NotEditable | HideFlags.DontSave) : (HideFlags.HideAndDontSave); } }

        // BEAM
        public static readonly Color FlatColor = Color.white;
        public const ColorMode ColorModeDefault = ColorMode.Flat;

        public const float IntensityDefault = 1f;
        public const float IntensityMin = 0f;
        public const float IntensityMax = 8f;
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

        // DYNAMIC OCCLUSION
        public static readonly LayerMask DynOcclusionLayerMaskDefault = 1; // Default layer
        public const float DynOcclusionFadeDistanceToSurfaceDefault = 0.25f;
        public const DynamicOcclusionUpdateRate DynamicOcclusionUpdateRateDefault = DynamicOcclusionUpdateRate.EveryXFrames;
        public const int DynOcclusionWaitFramesCountDefault = 3;

        public const Dimensions DynOcclusionRaycastingDimensionsDefault = Dimensions.Dim3D;
        public const bool DynOcclusionRaycastingConsiderTriggersDefault = false;
        public const float DynOcclusionRaycastingMinOccluderAreaDefault = 0.0f;
        public const float DynOcclusionRaycastingMinSurfaceRatioDefault = 0.5f;
        public const float DynOcclusionRaycastingMinSurfaceRatioMin = 50f;
        public const float DynOcclusionRaycastingMinSurfaceRatioMax = 100f;
        public const float DynOcclusionRaycastingMaxSurfaceDotDefault = 0.25f; // around 75 degrees
        public const float DynOcclusionRaycastingMaxSurfaceAngleMin = 45f;
        public const float DynOcclusionRaycastingMaxSurfaceAngleMax = 90f;
        public const PlaneAlignment DynOcclusionRaycastingPlaneAlignmentDefault = PlaneAlignment.Surface;
        public const float DynOcclusionRaycastingPlaneOffsetDefault = 0.1f;

        public const int DynOcclusionDepthBufferDepthMapResolutionDefault = 32;
        public const bool DynOcclusionDepthBufferOcclusionCullingDefault = true;

        // CONFIG
        public const bool ConfigGeometryOverrideLayerDefault = true;
        public const int ConfigGeometryLayerIDDefault = 1;
        public const string ConfigGeometryTagDefault = "Untagged";
        public const string ConfigFadeOutCameraTagDefault = "MainCamera";
        public const RenderQueue ConfigGeometryRenderQueueDefault = RenderQueue.Transparent;
        public const RenderPipeline ConfigGeometryRenderPipelineDefault = RenderPipeline.BuiltIn;
        public const RenderingMode ConfigGeometryRenderingModeDefault = RenderingMode.SinglePass;
        public const int ConfigNoise3DSizeDefault = 64;
        public const int ConfigSharedMeshSides = 24;
        public const int ConfigSharedMeshSegments = 5;
        public const float ConfigDitheringFactor = 0.0f;
    }
}