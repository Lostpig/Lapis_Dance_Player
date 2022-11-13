#if UNITY_EDITOR
using UnityEngine;

namespace VLB
{
    public static class EditorStrings
    {
        public static class Common
        {
            public static readonly GUIContent ButtonOpenGlobalConfig = new GUIContent("Open Config Asset in use");
            public const string HelpNoiseLoadingFailed = "Fail to load 3D noise texture. Please check your Config.";

            public static readonly GUIContent[] DimensionsEnumDescriptions = new GUIContent[]
            {
                new GUIContent("3D"),
                new GUIContent("2D"),
            };
        }

        public static class Beam
        {
            public static readonly GUIContent HeaderBasic = new GUIContent("Basic", "Basic beam's properties (color, angle, thickness...)");
            public static readonly GUIContent HeaderAttenuation = new GUIContent("Fall-Off Attenuation", "Control the beam's range distance and the light fall-off behaviour");
            public static readonly GUIContent Header3DNoise = new GUIContent("3D Noise", "Simulate animated volumetric fog / mist / smoke effects.\nIt makes the volumetric lights look less 'perfect' and so much more realistic.\nTo achieve that, a tiled 3D noise texture is internally loaded and used by the beam shader.");
            public static readonly GUIContent HeaderBlendingDistances = new GUIContent("Soft Intersections Blending Distances", "Because the volumetric beams are rendered using cone geometry, it is possible that it intersects with the camera's near plane or with the world's geometry, which could produce unwanted artifacts.\nThese properties are designed to fix this issue.");
            public static readonly GUIContent HeaderGeometry = new GUIContent("Cone Geometry", "Control how the beam's geometry is generated.");
            public static readonly GUIContent HeaderFadeOut = new GUIContent("Fade Out");
            public static readonly GUIContent Header2D = new GUIContent("2D", "Tweak and combine the order when beams are rendered with 2D objects (such as 2D sprites)");
            public static readonly GUIContent HeaderInfos = new GUIContent("Infos");

            public static readonly GUIContent FromSpotLight = new GUIContent("From Spot", "Get the value from the Light Spot");

            public static readonly GUIContent SideThickness = new GUIContent(
                "Side Thickness",
                "Thickness of the beam when looking at it from the side.\n1 = the beam is fully visible (no difference between the center and the edges), but produces hard edges.\nLower values produce softer transition at beam edges.");

            public static readonly GUIContent ColorMode = new GUIContent("Color", "Apply a flat/plain/single color, or a gradient.");
            public static readonly GUIContent ColorGradient = new GUIContent("", "Use the gradient editor to set color and alpha variations along the beam.");
            public static readonly GUIContent ColorFlat = new GUIContent("", "Use the color picker to set a plain RGBA color (takes account of the alpha value).");

            public static readonly GUIContent IntensityModeAdvanced = new GUIContent("Adv", "Advanced Mode: control inside and outside intensity values independently.");
            public static readonly GUIContent IntensityGlobal = new GUIContent("Intensity", "Global beam intensity. If you want to control values for inside and outside independently, use the advanced mode.");
            public static readonly GUIContent IntensityOutside = new GUIContent("Intensity (outside)", "Beam outside intensity (when looking at the beam from behind).");
            public static readonly GUIContent IntensityInside = new GUIContent("Intensity (inside)", "Beam inside intensity (when looking at the beam from the inside directly at the source).");

            public static readonly GUIContent BlendingMode = new GUIContent("Blending Mode", "Additive: highly recommended blending mode\nSoftAdditive: softer additive\nTraditional Transparency: support dark/black colors");
            public static readonly GUIContent ShaderAccuracy = new GUIContent("Shader Accuracy", "- Fast: a lot of computation are done on the vertex shader to maximize performance.\n- High: most of the computation are done on the pixel shader to maximize graphical quality at some performance cost.\n\nWe recommend to keep the default 'Fast' shader accuracy to ensure best performance, except when using the 'Tilt Factor' feature or when using a very rich 'Gradient Color'.");

            public static readonly GUIContent SpotAngle = new GUIContent("Spot Angle", "Define the angle (in degrees) at the base of the beam's cone");

            public static readonly GUIContent GlareFrontal = new GUIContent("Glare (frontal)", "Boost intensity factor when looking at the beam from the inside directly at the source.");
            public static readonly GUIContent GlareBehind = new GUIContent("Glare (from behind)", "Boost intensity factor when looking at the beam from behind.");

            public static readonly GUIContent TrackChanges = new GUIContent(
                " Track changes during Playtime",
                "Check this box to be able to modify properties during Playtime via Script, Animator and/or Timeline.\nEnabling this feature is at very minor performance cost. So keep it disabled if you don't plan to modify this light beam during playtime.");

            public static readonly GUIContent AttenuationEquation = new GUIContent("Equation", "Attenuation equation used to compute fading between 'Fade Start Distance' and 'Range Distance'.\n- Linear: Simple linear attenuation\n- Quadratic: Quadratic attenuation, which usually gives more realistic results\n- Blend: Custom blending mix between linear (0.0) and quadratic attenuation (1.0)");
            public static readonly GUIContent AttenuationCustomBlending = new GUIContent("", "Blending value between Linear (0.0) and Quadratic (1.0) attenuation equations.");

            public static readonly GUIContent FallOffStart = new GUIContent("Start Distance", "Distance from the light source (in units) the beam intensity will start to fall-off.");
            public static readonly GUIContent FallOffEnd = new GUIContent("Range Limit", "Distance from the light source (in units) the beam is entirely faded out");

            public static readonly GUIContent NoiseMode = new GUIContent("Enabled", "Enable 3D Noise effect and choose the mode:\n- World Space: the noise will look 'grounded' in the world\n- Local Space: the noise will look 'tied' to the beam");
            public static readonly GUIContent NoiseIntensity = new GUIContent("Intensity", "Higher intensity means the noise contribution is stronger and more visible");
            public static readonly GUIContent NoiseScale = new GUIContent("Scale", "3D Noise texture scaling: higher scale make the noise more visible, but potentially less realistic");
            public static readonly GUIContent NoiseVelocity = new GUIContent("Velocity", "Direction and speed of the noise scrolling, simulating the fog/smoke movement");
            public static readonly GUIContent[] NoiseModeEnumDescriptions = new GUIContent[]
            {
                new GUIContent("Disabled"),
                new GUIContent("Enabled (World Space)"),
                new GUIContent("Enabled (Local Space)"),
            };

            public static readonly GUIContent CameraClippingDistance = new GUIContent("Camera", "Distance from the camera the beam will fade with.\n- 0.0: hard intersection\n- Higher values produce soft intersection when the camera is near the cone triangles.");
            public static readonly GUIContent DepthBlendDistance = new GUIContent("Opaque geometry", "Distance from the world geometry the beam will fade with.\n- 0.0 (feature disabled): hard intersection but faster (doesn't require to update the depth texture).\n- Higher values produce soft intersection when the beam intersects world's geometry, but require to update the camera's depth texture.");

            public static readonly GUIContent ConeRadiusStart = new GUIContent("Truncated Radius", "Radius (in units) at the beam's source (the top of the cone).\n0 will generate a perfect cone geometry.\nHigher values will generate truncated cones.");

            public static readonly GUIContent GeomMeshType = new GUIContent("Mesh Type", "");
            public static readonly GUIContent GeomCap = new GUIContent("Cap", "Show Cap Geometry (only visible from inside)");
            public static readonly GUIContent GeomSides = new GUIContent("Sides", "Number of Sides of the cone.\nHigher values make the beam looks more 'round', but require more memory and graphic performance.\nA recommended value for a decent quality while keeping the poly count low is 18.");
            public static readonly GUIContent GeomSegments = new GUIContent("Segments", "Number of Segments of the cone.\nHigher values give better looking results but require more performance. We recommend at least 3 segments, specially regarding Attenuation and Gradient, otherwise the approximation could become inaccurate.\nThe longer the beam, the more segments we recommend to set.\nA recommended value is 4.");

            public static readonly GUIContent FadeOutEnabled = new GUIContent("Enabled", "Enable the fade out of the beam according to the distance to the camera.");
            public static readonly GUIContent FadeOutBegin = new GUIContent("Begin Distance", "Fade out starting distance. Beyond this distance, the beam intensity will start to be dimmed.");
            public static readonly GUIContent FadeOutEnd = new GUIContent("End Distance", "Fade out ending distance. Beyond this distance, the beam will be culled off to save on performance.");

            public static readonly GUIContent SkewingLocalForwardDirection = new GUIContent("Skewing Factor", "Distort the shape of the beam horizontally and vertically while keeping its circular slice unchanged.");
            public static readonly GUIContent ClippingPlane = new GUIContent("Clipping Plane", "Additional clipping plane transform.");
            public static readonly GUIContent EditorShowClippingPlane = new GUIContent("Show clipping plane", "Display the additional clipping plane.");

            public static readonly GUIContent TiltFactor = new GUIContent("Tilt Factor", "Tilt the color and attenuation gradient compared to the global beam's direction.\nShould be used with 'High' shader accuracy mode.");
            public static readonly GUIContent EditorShowTiltDirection = new GUIContent("Show Tilt Direction", "Display the direction of the tilt factor in editor.");

            public static readonly GUIContent Dimensions = new GUIContent("Dimensions", "- 3D: beam along the Z axis.\n- 2D: beam along the X axis, so you won't have to rotate it to see it in 2D.");
            public const string SortingLayer = "Sorting Layer";
            public static readonly GUIContent SortingOrder = new GUIContent("Order in Layer", "The overlay priority within its layer. Lower numbers are rendered first and subsequent numbers overlay those below.");

            // BUTTONS
            public static readonly GUIContent ButtonResetProperties = new GUIContent("Default values", "Reset properties to their default values.");
            public static readonly GUIContent ButtonGenerateGeometry = new GUIContent("Regenerate geometry", "Force to re-create the Beam Geometry GameObject.");
            public static readonly GUIContent ButtonAddDustParticles = new GUIContent("+ Dust Particles", "Add highly detailed dustlight / mote particles on your beam");
            public static readonly GUIContent ButtonAddDynamicOcclusion = new GUIContent("+ Dynamic Occlusion", "Gives awareness to your beam so it reacts to changes in the world: it could be occluded by environment geometry.");
            public static readonly GUIContent ButtonAddTriggerZone = new GUIContent("+ Trigger Zone", "Track objects passing through the light beam and track when the beam is passing over them.");

            // HELP BOXES
            public const string HelpNoSpotlight = "To bind properties from the Light and the Beam together, this component must be attached to a Light of type 'Spot'";
            public const string HelpAnimatorWarning = "If you want to animate your light beam in real-time, you should enable the 'trackChangesDuringPlaytime' property.";
            public const string HelpTrackChangesEnabled = "This beam will keep track of the changes of its own properties and the spotlight attached to it (if any) during playtime. You can modify every properties except 'geomSides'.";
            public const string HelpDepthTextureMode = "To support 'Soft Intersection with Opaque Geometry', your camera must use 'DepthTextureMode.Depth'.";
            public const string HelpDepthMobile = "On mobile platforms, the depth buffer precision can be pretty low. Try to keep a small depth range on your cameras: the difference between the near and far clip planes should stay as low as possible.";
            public const string HelpFadeOutNoMainCamera = "Fail to retrieve the main camera specified in the config.";
            public const string HelpTiltedWithShaderAccuracyFast = "We highly recommend to set the 'Shader Accuracy' property to 'High' when using 'Tilt Factor'.";
        }

        public static class DustParticles
        {
            public static readonly GUIContent HeaderRendering = new GUIContent("Rendering");
            public static readonly GUIContent HeaderDirectionAndVelocity = new GUIContent("Direction & Velocity");
            public static readonly GUIContent HeaderCulling = new GUIContent("Culling");
            public static readonly GUIContent HeaderSpawning = new GUIContent("Spawning");
            public static readonly GUIContent HeaderInfos = new GUIContent("Infos");

            public static readonly GUIContent Alpha = new GUIContent("Alpha", "Max alpha of the particles");
            public static readonly GUIContent Size = new GUIContent("Size", "Max size of the particles");

            public static readonly GUIContent Direction = new GUIContent("Direction", "Direction of the particles\n- Random: random direction.\n- Local Space: particles follow the velicity direction in local space (Z is along the beam).\n- World Space: particles follow the velicity direction in world space.");
            public static readonly GUIContent Velocity = new GUIContent("Velocity", "Movement speed of the particles along the chosen direction");

            public static readonly GUIContent CullingEnabled = new GUIContent("Enabled", "Enable particles culling based on the distance to the Main Camera.\nWe highly recommend to enable this feature to keep good runtime performances.");
            public static readonly GUIContent CullingMaxDistance = new GUIContent("Max Distance", "The particles will not be rendered if they are further than this distance to the Main Camera");

            public static readonly GUIContent Density = new GUIContent("Density", "Control how many particles are spawned. The higher the density, the more particles are spawned, the higher the performance cost is");
            public static readonly GUIContent SpawnMinDistance = new GUIContent("Min Distance", "The minimum distance (from the light source) where the particles are spawned.\nThe higher it is, the more the particles are spawned away from the light source.");
            public static readonly GUIContent SpawnMaxDistance = new GUIContent("Max Distance", "The maximum distance (from the light source) where the particles are spawned.\nThe lower it is, the more the particles are gathered near the light source.");

            // HELP BOXES
            public const string HelpFeatureNotSupported = "Volumetric Dust Particles feature is only supported in Unity 5.5 or above";
            public const string HelpFailToInstantiate = "Fail to instantiate the Particles. Please check your Config.";
            public const string HelpRecommendation = "We do not recommend to use this feature if you plan to move or change properties of the beam during playtime.";
        }

        public static class DynOcclusion
        {
            public static readonly GUIContent HeaderUpdateRate = new GUIContent("Update Rate");
            public static readonly GUIContent HeaderRaycasting = new GUIContent("Raycasting");
            public static readonly GUIContent HeaderOccluderSurface = new GUIContent("Occluder Surface");
            public static readonly GUIContent HeaderClippingPlane = new GUIContent("Clipping Plane");
            public static readonly GUIContent HeaderCamera = new GUIContent("Camera");
            public static readonly GUIContent HeaderEditorDebug = new GUIContent("Editor Debug");

            public static readonly GUIContent Dimensions = new GUIContent("Dimensions", "Should it interact with 2D or 3D occluders?\n- 3D: the beam will react against 3D Occluders.\n- 2D: the beam will react against 2D Occluders. This is useful when using the beams with 2D objects (such as 2D Sprites).");

            public static readonly GUIContent LayerMask = new GUIContent("Layer Mask",
                "The beam can only be occluded by objects located on the layers matching this mask.\nIt's very important to set it as restrictive as possible (checking only the layers which are necessary) to perform a more efficient process in order to increase the performance.");
            public static readonly GUIContent ConsiderTriggers = new GUIContent("Consider Triggers",
                "Should this beam be occluded by triggers or not?");
            public const string ConsiderTriggersNoPossible = "In order to be able to consider triggers as 2D occluders, you should tick the 'Queries Hit Triggers' checkbox under the 'Physics 2D' settings menu.";
            public static readonly GUIContent MinOccluderArea = new GUIContent("Min Occluder Area",
                "Minimum 'area' of the collider to become an occluder.\nColliders smaller than this value will not block the beam.");
            public static readonly GUIContent UpdateRate = new GUIContent("Update Rate", "How often will the occlusion be processed?\nTry to update the occlusion as rarely as possible to keep good performance.");
            public static readonly GUIContent[] UpdateRateDescriptions = new GUIContent[]
            {
                new GUIContent("Never"),
                new GUIContent("On Enable (only once)"),
                new GUIContent("On Beam Move"),
                new GUIContent("Every X Frames"),
                new GUIContent("On Beam Move and Every X Frames"),
            };

            public static readonly GUIContent WaitXFrames = new GUIContent("X frames to wait",
                "How many frames we wait between 2 occlusion tests?\nIf you want your beam to be super responsive to the changes of your environment, update it every frame by setting 1.\nIf you want to save on performance, we recommend to wait few frames between each update by setting a higher value.");
            public static readonly GUIContent MinSurfaceRatio = new GUIContent("Min Occluded %", "Approximated percentage of the beam to collide with the surface in order to be considered as occluder.");
            public static readonly GUIContent MaxSurfaceDot = new GUIContent("Max Angle", "Max angle (in degrees) between the beam and the surface in order to be considered as occluder.");
            public static readonly GUIContent PlaneAlignment = new GUIContent("Alignment", "Alignment of the computed clipping plane:\n- Surface: align to the surface normal which blocks the beam. Works better for large occluders such as floors and walls.\n- Beam: keep the plane aligned with the beam direction. Works better with more complex occluders or with corners.");
            public static readonly GUIContent PlaneOffset = new GUIContent("Offset Units", "Translate the plane. We recommend to set a small positive offset in order to handle non-flat surface better.");
            public static readonly GUIContent FadeDistanceToSurface = new GUIContent("Fade Distance Units", "Fade out the beam before the occlusion surface in order to soften the transition.");
            public static readonly GUIContent EditorShowDebugPlane = new GUIContent("Show Debug Plane", "Draw debug plane on the scene view.");
            public static readonly GUIContent EditorRaycastAtEachFrame = new GUIContent("Update in Editor", "Perform occlusion tests and raycasts in Editor.");

            public static string GetUpdateRateAdvice<T>(DynamicOcclusionUpdateRate value)
            {
                switch (value)
                {
                    case DynamicOcclusionUpdateRate.Never: return string.Format("The occlusion will never be updated.\nThe only way to update it is to manually call '{0}.ProcessOcclusionManually()' from script whenever you need.", typeof(T).Name);
                    case DynamicOcclusionUpdateRate.OnEnable: return "The occlusion will only be updated once on start, and each time the beam is enabled/activated (after being disabled/deactivated).\nIt's suitable for static beams located in static environment.";
                    case DynamicOcclusionUpdateRate.OnBeamMove: return "The occlusion will only be updated when the beam will move.\nIt's suitable for moving beams located in static environment.";
                    case DynamicOcclusionUpdateRate.EveryXFrames: return "The occlusion will be updated every {0} frame(s).\nIt's suitable for static beams located in moving environment.";
                    case DynamicOcclusionUpdateRate.OnBeamMoveAndEveryXFrames: return "The occlusion will be updated when the beam will move in addition to every {0} frame(s).\nIt's suitable for moving beams located in moving environment.";
                    default: return null;
                }
            }

            public static readonly GUIContent DepthBufferOcclusionCulling = new GUIContent("Occlusion Culling", "Whether or not the virtual camera will use occlusion culling during rendering from the beam's POV.");
            public static readonly GUIContent DepthBufferDepthMapResolution = new GUIContent("Depth Map Resolution", "Controls how large the depth texture captured by the virtual camera is.\nThe lower the resolution, the better the performance, but the less accurate the rendering.");

            public const string HelpDepthBufferAndBeam2D = "'Dynamic Occlusion (Depth Buffer)' doesn't work with 2D sprites nor 2D colliders. It will only track 3D objects.";

            public const string HelpOverrideLayer = "To keep good performance, it's highly recommended to set an 'Override Layer' in the Config when using this feature, to prevent from having a LayerMark including any Volumetric Beam.";
            public static string HelpLayerMaskIssues { get { return string.Format("The beams are generated on the layer '{0}' (set in the Config), but this LayerMask includes this layer.\nTo keep good performance, it's highly recommended to set a LayerMask which doesn't include this layer!", UnityEngine.LayerMask.LayerToName(VLB.Config.Instance.geometryLayerID)); } }
        }

        public static class TriggerZone
        {
            public static readonly GUIContent HeaderInfos = new GUIContent("Infos");

            public static readonly GUIContent SetIsTrigger = new GUIContent("Set Is Trigger", "Define if the Collider will be created as a convex trigger (not physical, most common behavior) or as a regular collider (physical).");
            public static readonly GUIContent RangeMultiplier = new GUIContent("Range Multiplier", "Change the length of the Collider.\nFor example, set 2.0 to make the Collider 2x longer than the beam.");

            public const string HelpDescription2D = "Generate a 2D Polygon Collider with the same shape than the beam, supporting dynamic occlusion.";
            public const string HelpDescription3D = "Generate a 3D Mesh Collider with the same shape than the beam. The collider doesn't support occlusion though.";
            public const string HelpTrackChangesDuringPlaytimeEnabled = "The TriggerZone collider cannot be changed in realtime.\nIf you animate a property which change the shape of the beam, the collider shape won't fit anymore.";
        }

        public static class SkewingHandle
        {
            public static readonly GUIContent Beam = new GUIContent("Beam", "The Volumetric Light Beam you want to modify.");
            public static readonly GUIContent ShouldUpdateEachFrame = new GUIContent("Should Update Each Frame", "Should the beam's skewing property be updated each frame or only once?");

            public const string ErrorAttachedToSelf = "You should attach the 'SkewingHandle' component to another GameObject than the beam itself.";
            public const string ErrorCannotSkew = "This beam can't be skewed because it doesn't use 'Custom' mesh type.";
            public const string ErrorCannotUpdate = "This beam can't be updated each frame since its property 'trackChangesDuringPlaytime' is disabled.";
        }

        public static class Config
        {
            public static readonly GUIContent GeometryOverrideLayer = new GUIContent("Override Layer", "The layer the GameObjects holding the procedural cone meshes are created on");
            public static readonly GUIContent GeometryTag = new GUIContent("Tag", "The tag applied on the procedural geometry GameObjects");
            public static readonly GUIContent GeometryRenderQueue = new GUIContent("Render Queue", "Determine in which order beams are rendered compared to other objects.\nThis way for example transparent objects are rendered after opaque objects, and so on.");
            public static readonly GUIContent GeometryRenderPipeline = new GUIContent("Render Pipeline", "Select the Render Pipeline (Built-In or SRP) in use.");
            public static readonly GUIContent GeometryRenderingMode = new GUIContent("Rendering Mode",
@"- Multi-Pass: Use the 2 pass shader. Will generate 2 drawcalls per beam (Not compatible with custom Render Pipeline such as HDRP and LWRP).
- Single-Pass: Use the 1 pass shader. Will generate 1 drawcall per beam.
- GPU Instancing: Dynamically batch multiple beams to combine and reduce draw calls.
- SRP Batcher: Use the SRP Batcher to automatically batch multiple beams and reduce draw calls. Only available when using SRP.");

            public static string ErrorSrpAndMultiPassNotCompatible { get { return string.Format("Using a Scriptable Render Pipeline with 'Multi-Pass' Rendering Mode is not supported: please choose another Rendering Mode, or '{0}' will be used.", VLB.Config.Instance.actualRenderingMode); } }
            public static string ErrorSrpBatcherOnlyCompatibleWithSrp { get { return string.Format("The 'SRP Batcher' Rendering Mode is only compatible when using a SRP: please choose another Rendering Mode, or '{0}' will be used.", VLB.Config.Instance.actualRenderingMode); } }
            public static string ErrorSrpBatcherNotCompatibleWithLWRP { get { return string.Format("The 'SRP Batcher' Rendering Mode is not compatible with LWRP: please choose another Rendering Mode, or '{0}' will be used.", VLB.Config.Instance.actualRenderingMode); } }
            public static string ErrorGeometryGpuInstancingNotSupported { get { return string.Format("'GPU Instancing' Rendering Mode is only supported on Unity 5.6 or above! '{0}' will be used.", VLB.Config.Instance.actualRenderingMode); } }
            public const string ErrorRenderPipelineMismatch = "It looks like the 'Render Pipeline' correctly is not set.\nPlease make sure to select the proper value depending on your pipeline in use.";

            public static readonly GUIContent FadeOutCameraTag = new GUIContent("Fade Out Camera Tag", "Tag used to retrieve the camera used to compute the fade out factor on beams");

            public static readonly GUIContent BeamShader1Pass = new GUIContent("Shader (single-pass)", "Main shader (1 pass version) applied to the cone beam geometry");
            public static readonly GUIContent BeamShader2Pass = new GUIContent("Shader (multi-pass)", "Main shader (multi-pass version) applied to the cone beam geometry");
            public static readonly GUIContent BeamShaderSRP = new GUIContent("Shader (SRP)", "Main shader (SRP version) applied to the cone beam geometry");
            public static readonly GUIContent SharedMeshSides = new GUIContent("Sides", "Number of Sides of the cone.\nHigher values make the beam looks more 'round', but require more memory and graphic performance.\nA recommended value for a decent quality while keeping the poly count low is 18.");
            public static readonly GUIContent SharedMeshSegments = new GUIContent("Segments", "Number of Segments of the cone.\nHigher values give better looking results but require more performance. We recommend at least 3 segments, specially regarding Attenuation and Gradient, otherwise the approximation could become inaccurate.\nThe longer the beam, the more segments we recommend to set.\nA recommended value is 4.");
            public static readonly GUIContent GlobalNoiseScale = new GUIContent("Scale", "Global 3D Noise texture scaling: higher scale make the noise more visible, but potentially less realistic");
            public static readonly GUIContent GlobalNoiseVelocity = new GUIContent("Velocity", "Global World Space direction and speed of the noise scrolling, simulating the fog/smoke movement");
            public static readonly GUIContent Noise3DData = new GUIContent("3D Noise Data binary file", "Binary file holding the 3D Noise texture data (a 3D array). Must be exactly Size * Size * Size bytes long.");
            public static readonly GUIContent Noise3DSize = new GUIContent("3D Noise Data dimension", "Size (of one dimension) of the 3D Noise data. Must be power of 2. So if the binary file holds a 32x32x32 texture, this value must be 32.");
            public static readonly GUIContent DustParticlesPrefab = new GUIContent("Dust Particles Prefab", "ParticleSystem prefab instantiated for the Volumetric Dust Particles feature (Unity 5.5 or above)");
            public static readonly GUIContent DitheringFactor = new GUIContent("Dithering", "Depending on the quality of your screen, you might see some artifacts with high contrast visual (like a white beam over a black background).\nThese is a very common problem known as color banding.\nTo help with this issue, the plugin offers a Dithering factor: it smooths the banding by introducing a subtle pattern of noise.");
            public static readonly GUIContent DitheringNoiseTexture = new GUIContent("Dithering Noise Texture", "Noise texture for dithering feature.");
            public static readonly GUIContent OpenDocumentation = new GUIContent("Documentation", "Open the online documentation.");
            public static readonly GUIContent ResetToDefaultButton = new GUIContent("Default values", "Reset properties to their default values.");
            public static readonly GUIContent ResetInternalDataButton = new GUIContent("Reset internal data", "Reset internal data to their default values.");

            public static readonly GUIContent[] GeometryRenderPipelineEnumDescriptions = new GUIContent[]
            {
                new GUIContent("Built-In"),
                new GUIContent("URP"),
                new GUIContent("HDRP"),
            };

            public static string GetErrorInvalidShader() { return string.Format("Fail to generate shader asset. Please try to reset the Config asset or reinstall the plugin."); }

            public static readonly string MultipleAssets = string.Format(
                "This overridden Config asset is not the one in use, please make sure:\n- This asset is directly located under a 'Resources' folder.\n- This asset is named {0}.asset.\n- You only have 1 '{1}' asset in your project."
                , ConfigOverride.kAssetName
                , typeof(ConfigOverride).ToString()
                );
        }
    }
}
#endif
