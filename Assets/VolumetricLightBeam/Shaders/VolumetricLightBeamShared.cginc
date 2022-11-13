// The following comment prevents Unity from auto upgrading the shader. Please keep it to keep backward compatibility
// UNITY_SHADER_NO_UPGRADE

#ifndef _VOLUMETRIC_LIGHT_BEAM_SHARED_INCLUDED_
#define _VOLUMETRIC_LIGHT_BEAM_SHARED_INCLUDED_

/// ****************************************
/// SHADER INPUT / OUTPUT STRUCT
/// ****************************************
struct vlb_appdata
{
    float4 vertex : POSITION;
    float4 texcoord : TEXCOORD0;

#if VLB_INSTANCING_API_AVAILABLE && (VLB_STEREO_INSTANCING || VLB_GPU_INSTANCING)
    UNITY_VERTEX_INPUT_INSTANCE_ID // for GPU Instancing and Single Pass Instanced rendering
#endif
};

struct v2f
{
    float4 posClipSpace : SV_POSITION;
    float3 posObjectSpace : TEXCOORD0;
    float3 posObjectSpaceNonSkewed : TEXCOORD8;
    float4 posWorldSpace : TEXCOORD1;
    float4 posViewSpace_extraData : TEXCOORD2;
    float4 cameraPosObjectSpace_outsideBeam : TEXCOORD3;

#if OPTIM_VS
    half4 color : TEXCOORD4;
#endif

#if VLB_NOISE_3D || OPTIM_VS
    float4 uvwNoise_intensity : TEXCOORD5;
#endif

#if VLB_DEPTH_BLEND || VLB_DITHERING
    float4 projPos : TEXCOORD6;
#endif

#ifdef VLB_FOG_UNITY_BUILTIN_COORDS
    UNITY_FOG_COORDS(7)
#endif

#if VLB_INSTANCING_API_AVAILABLE
#if VLB_GPU_INSTANCING
    UNITY_VERTEX_INPUT_INSTANCE_ID // not sure this one is useful
#endif

#if VLB_STEREO_INSTANCING
    UNITY_VERTEX_OUTPUT_STEREO // for Single Pass Instanced rendering
#endif
#endif // VLB_INSTANCING_API_AVAILABLE
};


#include "ShaderUtils.cginc"


inline float ComputeBoostFactor(float pixDistFromSource, float outsideBeam, float isCap)
{
    pixDistFromSource = max(pixDistFromSource, 0.001); // prevent 1st segment from being boosted when boostFactor is 0
    float glareFrontal = VLB_GET_PROP(_GlareFrontal);
    float insideBoostDistance = glareFrontal * VLB_GET_PROP(_DistanceFallOff).y;
    float boostFactor = 1 - smoothstep(0, 0 + insideBoostDistance + 0.001, pixDistFromSource); // 0 = no boost ; 1 = max boost
    boostFactor = lerp(boostFactor, 0, outsideBeam); // no boost for outside pass

    float4 cameraParams = VLB_GET_PROP(_CameraParams);
    float cameraIsInsideBeamFactor = saturate(cameraParams.w); // _CameraParams.w is (-1 ; 1) 
    boostFactor = cameraIsInsideBeamFactor * boostFactor; // no boost for outside pass

    boostFactor = lerp(boostFactor, 1, isCap); // cap is always at max boost
    return boostFactor;
}

// boostFactor is normalized
float ComputeFresnel(float3 posObjectSpace, float3 vecCamToPixOSN, float outsideBeam, float boostFactor)
{
    // Compute normal
    float2 cosSinFlat = normalize(posObjectSpace.xy);
    float2 coneSlopeCosSin = VLB_GET_PROP(_ConeSlopeCosSin);
    float3 normalObjectSpace = (float3(cosSinFlat.x * coneSlopeCosSin.x, cosSinFlat.y * coneSlopeCosSin.x, -coneSlopeCosSin.y));
    normalObjectSpace *= (outsideBeam * 2 - 1); // = outsideBeam ? 1 : -1;
    
    // real fresnel factor
    float fresnelReal = dot(normalObjectSpace, -vecCamToPixOSN);

    // compute a fresnel factor to support long beams by projecting the viewDir vector
    // on the virtual plane formed by the normal and tangent
    float coneApexOffsetZ = VLB_GET_PROP(_ConeApexOffsetZ);
    float3 tangentPlaneNormal = normalize(posObjectSpace.xyz + float3(0, 0, coneApexOffsetZ));
    float distToPlane = dot(-vecCamToPixOSN, tangentPlaneNormal);
    float3 vec2D = normalize(-vecCamToPixOSN - distToPlane * tangentPlaneNormal);
    float fresnelProjOnTangentPlane = dot(normalObjectSpace, vec2D);

    // blend between the 2 fresnels
    float3 localForwardDirN = VLB_GET_PROP(_LocalForwardDirection);
    float vecCamToPixDotZ = dot(vecCamToPixOSN, localForwardDirN);
    float factorNearAxisZ = abs(vecCamToPixDotZ); // factorNearAxisZ is normalized

    float fresnel = lerp(fresnelProjOnTangentPlane, fresnelReal, factorNearAxisZ);

    float fresnelPow = VLB_GET_PROP(_FresnelPow);

    // Lerp the fresnel pow to the glare factor according to how far we are from the axis Z
    const float kMaxGlarePow = 1.5;
    float glareFrontal = VLB_GET_PROP(_GlareFrontal);
    float glareBehind = VLB_GET_PROP(_GlareBehind);
    float glareFactor = kMaxGlarePow * (1 - lerp(glareFrontal, glareBehind, outsideBeam));
    fresnelPow = lerp(fresnelPow, min(fresnelPow, glareFactor), factorNearAxisZ);
    
    // Pow the fresnel
    fresnel = smoothstep(0, 1, fresnel);
    fresnel = (1 - isEqualOrGreater(-fresnel, 0)) * // fix edges artefacts on android ES2
              (pow(fresnel, fresnelPow));

    // Boost distance inside
    float boostFresnel = lerp(fresnel, 1 + 0.001, boostFactor);
    fresnel = lerp(boostFresnel, fresnel, outsideBeam); // no boosted fresnel if outside

    // We do not have to treat cap a special way, since boostFactor is already set to 1 for cap via ComputeBoostFactor
    
    return fresnel;
}


inline float ComputeFadeWithCamera(float3 posViewSpace, float enabled)
{
    float distCamToPixWS = abs(posViewSpace.z); // only check Z axis (instead of length(posViewSpace.xyz)) to have smoother transition with near plane (which is not curved)
    float camFadeDistStart = _ProjectionParams.y; // cam near place
    float camFadeDistEnd = camFadeDistStart + VLB_GET_PROP(_DistanceCamClipping);
    float fadeWhenTooClose = smoothstep(0, 1, invLerpClamped(camFadeDistStart, camFadeDistEnd, distCamToPixWS));

    // fade out according to camera's near plane
    return lerp(1, fadeWhenTooClose, enabled);
}

half4 ComputeColor(float pixDistFromSource, float outsideBeam)
{
#if VLB_COLOR_GRADIENT_MATRIX_HIGH || VLB_COLOR_GRADIENT_MATRIX_LOW
    float distanceFadeEnd = VLB_GET_PROP(_DistanceFallOff).y;
    float4x4 colorGradientMatrix = VLB_GET_PROP(_ColorGradientMatrix);
    float distFromSourceNormalized = invLerpClamped(0, distanceFadeEnd, pixDistFromSource);
    half4 color = DecodeGradient(distFromSourceNormalized, colorGradientMatrix);
#else
    half4 color = VLB_GET_PROP(_ColorFlat);
#endif

    half alphaInside  = VLB_GET_PROP(_AlphaInside);
    half alphaOutside = VLB_GET_PROP(_AlphaOutside);
    half alpha = lerp(alphaInside, alphaOutside, outsideBeam);
#if VLB_ALPHA_AS_BLACK
    color.rgb *= color.a;
    color.rgb *= alpha;
#else
    color.a *= alpha;
#endif

    return color;
}

inline float ComputeInOutBlending(float vecCamToPixDotZ, float outsideBeam)
{
    // smooth blend between inside and outside geometry depending of View Direction
    const float kFaceLightSmoothingLimit = 1;
    float factorFaceLightSourcePerPixN = saturate(smoothstep(kFaceLightSmoothingLimit, -kFaceLightSmoothingLimit, vecCamToPixDotZ)); // smoother transition

    return lerp(factorFaceLightSourcePerPixN, 1 - factorFaceLightSourcePerPixN, outsideBeam);
}

#if VLB_MESH_SKEWING
inline float4 SkewVectorOS(float4 vec, float3 scaleObjectSpace)
{
    float3 localForwardDirN = VLB_GET_PROP(_LocalForwardDirection);
    vec.xy += vec.z * localForwardDirN.xy * scaleObjectSpace.z / (scaleObjectSpace.xy * localForwardDirN.z);
    return vec;
}

inline float3 UnskewVectorOS(float3 vec)
{
    float3 localForwardDirN = VLB_GET_PROP(_LocalForwardDirection);
    vec.xy -= localForwardDirN.xy * (vec.z / localForwardDirN.z);
    return vec;
}
#endif // VLB_MESH_SKEWING

// Vector Camera to current Pixel, in object space and normalized
inline float3 ComputeVectorCamToPixOSN(float3 pixPosOS, float3 cameraPosOS)
{
    float3 vecCamToPixOSN = normalize(pixPosOS - cameraPosOS);

    // Deal with ortho camera:
    // With ortho camera, we don't want to change the fresnel according to camera position.
    // So instead of computing the proper vector "Camera to Pixel", we take account of the "Camera Forward" vector (which is not dependant on the pixel position)
    float4 cameraParams = VLB_GET_PROP(_CameraParams);
    float3 vecCamForwardOSN = cameraParams.xyz;

#if VLB_MESH_SKEWING
    vecCamForwardOSN = normalize(UnskewVectorOS(vecCamForwardOSN));
#endif // VLB_MESH_SKEWING

    return lerp(vecCamToPixOSN, vecCamForwardOSN, VLB_CAMERA_ORTHO);
}

inline float GetTiltDistanceFactor(float3 posObjectSpace)
{
    float2 tiltVector = VLB_GET_PROP(_TiltVector);
    float pixDistFromSource = abs(posObjectSpace.z);
    return pixDistFromSource + posObjectSpace.x * tiltVector.x + posObjectSpace.y * tiltVector.y;
}

v2f vertShared(vlb_appdata v, float outsideBeam)
{
    v2f o;

#if VLB_INSTANCING_API_AVAILABLE && (VLB_STEREO_INSTANCING || VLB_GPU_INSTANCING)
    UNITY_SETUP_INSTANCE_ID(v);

    #if VLB_STEREO_INSTANCING
      #ifndef VLB_SRP_API // TODO CHECK THAT WE DON'T NEED THIS WITH SRP
        UNITY_INITIALIZE_OUTPUT(v2f, o);
      #endif
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    #endif

    #if VLB_GPU_INSTANCING
        UNITY_TRANSFER_INSTANCE_ID(v, o);
    #endif
#endif

#if VLB_NOISE_3D || OPTIM_VS
    o.uvwNoise_intensity = 1;
#endif

    // compute the proper cone shape, so the whole beam fits into a 2x2x1 box
    // The model matrix (computed via the localScale from BeamGeometry.)
    float4 vertexOS = v.vertex;

    vertexOS.z *= vertexOS.z; // make segment tessellation denser near the source, since beam is usually more visible at start

    float2 coneRadius = VLB_GET_PROP(_ConeRadius);
    float maxRadius = max(coneRadius.x, coneRadius.y);
    float normalizedRadiusStart = coneRadius.x / maxRadius;
    float normalizedRadiusEnd = coneRadius.y / maxRadius;
    vertexOS.xy *= lerp(normalizedRadiusStart, normalizedRadiusEnd, vertexOS.z);

    float3 scaleObjectSpace = float3(maxRadius, maxRadius, VLB_GET_PROP(_DistanceFallOff).z); // maxGeometryDistance

    o.posObjectSpaceNonSkewed = vertexOS.xyz * scaleObjectSpace;

#if VLB_MESH_SKEWING
    // skew the real vertex position
    vertexOS = SkewVectorOS(vertexOS, scaleObjectSpace);
#endif

    o.posWorldSpace = VLBObjectToWorldPos(vertexOS);
    o.posClipSpace = VLBObjectToClipPos(vertexOS.xyz);
    // TODO Should create and use VLBWorldToClipPos instead
    //o.posClipSpace = VLBWorldToClipPos(o.posWorldSpace.xyz);

#if defined(VLBWorldToViewPos)
    float3 posViewSpace = VLBWorldToViewPos(o.posWorldSpace.xyz);
#elif defined(VLBObjectToViewPos)
    float3 posViewSpace = VLBObjectToViewPos(vertexOS);
#else
    You_should_define_either_VLBWorldToViewPos_or_VLBObjectToViewPos
#endif

    // apply the same scaling than we do through the localScale in BeamGeometry.ComputeLocalMatrix
    // to get the proper transformed vertex position in object space
    o.posObjectSpace = vertexOS.xyz * scaleObjectSpace;


#if VLB_DEPTH_BLEND || VLB_DITHERING
    o.projPos = DepthFade_VS_ComputeProjPos(posViewSpace, o.posClipSpace);
#endif

    float isCap = v.texcoord.x;

#if VLB_NOISE_3D
    o.uvwNoise_intensity.rgb = Noise3D_GetUVW(o.posWorldSpace.xyz, o.posObjectSpace);
#endif

    float3 cameraPosObjectSpace = VLBGetCameraPositionObjectSpace(scaleObjectSpace);

#if VLB_MESH_SKEWING
    cameraPosObjectSpace = UnskewVectorOS(cameraPosObjectSpace); // unskew the camera position of object space
#endif // VLB_MESH_SKEWING

    o.cameraPosObjectSpace_outsideBeam = float4(
        cameraPosObjectSpace,
        outsideBeam);

#if OPTIM_VS
    // Treat Cap a special way: cap is only visible from inside
    float intensity = 1 - outsideBeam * isCap; // AKA if (outsideBeam == 1 && isCap == 1) intensity = 0

    float pixDistFromSource = length(o.posObjectSpace.z);

    float pixDistFromSourceTilted = GetTiltDistanceFactor(o.posObjectSpace);

    float3 distancesFallOff = VLB_GET_PROP(_DistanceFallOff);
    float attenuationLerpLinearQuad = VLB_GET_PROP(_AttenuationLerpLinearQuad);
    intensity *= ComputeAttenuation(pixDistFromSourceTilted, distancesFallOff.x, distancesFallOff.y, attenuationLerpLinearQuad);
    float boostFactor = ComputeBoostFactor(pixDistFromSource, outsideBeam, isCap);

    // Vector Camera to current Pixel, in object space and normalized
    float3 vecCamToPixOSN = ComputeVectorCamToPixOSN(o.posObjectSpaceNonSkewed.xyz, cameraPosObjectSpace);
    float vecCamToPixDotZ = dot(vecCamToPixOSN, float3(0, 0, 1));

#if OPTIM_VS_FRESNEL_VS
    // Pass data needed to compute fresnel in fragment shader
    // Computing fresnel on vertex shader give imprecise results
    intensity *= ComputeFresnel(o.posObjectSpaceNonSkewed, vecCamToPixOSN, outsideBeam, boostFactor);
#endif

    // fade out
    intensity *= VLB_GET_PROP(_FadeOutFactor);

    // smooth blend between inside and outside geometry depending of View Direction
    intensity *= ComputeInOutBlending(vecCamToPixDotZ, outsideBeam);

    // no intensity for cap if _DrawCap = 0
    float drawCap = VLB_GET_PROP(_DrawCap);
    intensity *= isEqualOrGreater(drawCap, isCap);

    o.uvwNoise_intensity.a = intensity;

    o.color = ComputeColor(pixDistFromSourceTilted, outsideBeam);

    float extraData = boostFactor;
#else
    float extraData = isCap;
#endif // OPTIM_VS

    o.posViewSpace_extraData = float4(posViewSpace, extraData);

#ifdef VLB_FOG_UNITY_BUILTIN_COORDS
    UNITY_TRANSFER_FOG(o, o.posClipSpace);
#endif
    return o;
}

half4 fragShared(v2f i, float outsideBeam)
{
#if VLB_INSTANCING_API_AVAILABLE && VLB_GPU_INSTANCING
    UNITY_SETUP_INSTANCE_ID(i);
#endif

#if VLB_INSTANCING_API_AVAILABLE && VLB_STEREO_INSTANCING
    // This fix access to depth map on the right eye when using single pass (aka Stereo Rendering Mode Multiview) on Gear VR or Oculus Go/Quest
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); // https://docs.unity3d.com/Manual/SinglePassInstancing.html
#endif

    {
        float4 cameraParams = VLB_GET_PROP(_CameraParams);
        float cameraIsInsideBeamFactor = saturate(cameraParams.w); // _CameraParams.w is (-1 ; 1) 
    }

#if OPTIM_VS
    float intensity = i.uvwNoise_intensity.a;
#else
    float intensity = 1;
#endif

    float pixDistFromSource = length(i.posObjectSpace.z);

    // additional clipping plane
    {
        float4 addClippingPlaneWS = VLB_GET_PROP(_AdditionalClippingPlaneWS);
        clip(DistanceToPlane(i.posWorldSpace.xyz, addClippingPlaneWS.xyz, addClippingPlaneWS.w));
    }

    // DYNAMIC OCCLUSION
#if VLB_OCCLUSION_CLIPPING_PLANE
    {
        float4 clippingPlaneWS = VLB_GET_PROP(_DynamicOcclusionClippingPlaneWS);
        float distToClipPlane = DistanceToPlane(i.posWorldSpace.xyz, clippingPlaneWS.xyz, clippingPlaneWS.w);
        clip(distToClipPlane);
        float fadeDistance = VLB_GET_PROP(_DynamicOcclusionClippingPlaneProps);
        intensity *= smoothstep(0, fadeDistance, distToClipPlane);
    }
#elif VLB_OCCLUSION_DEPTH_TEXTURE
    {
    #if VLB_GPU_INSTANCING
        // Dynamic Occlusion Depth Texture is not supported with GPU Instancing because instanced cbuffers cannot hold samplers
        return float4(1, 0, 0, 1);
    #endif

        const float kMinNearClipPlane = 0.1f; // should be the same than in DynamicOcclusion.cs
        float apexDist = VLB_GET_PROP(_ConeApexOffsetZ);
        float2 uv = normalize(i.posObjectSpace.xy) * 0.5 + 0.5;
        float dynamicOcclusionDepthRaw = tex2D(_DynamicOcclusionDepthTexture, uv).r;
    #if defined(UNITY_REVERSED_Z)
        dynamicOcclusionDepthRaw = 1.0f - dynamicOcclusionDepthRaw;
    #endif

        float fallOffEnd = VLB_GET_PROP(_DistanceFallOff).z; // maxGeometryDistance

        float near = max(apexDist, kMinNearClipPlane);
        float far = near + fallOffEnd;

        float dynamicOcclusionDepthLinear = VLB_ZBufferToLinear(dynamicOcclusionDepthRaw, near, far);
        dynamicOcclusionDepthLinear -= apexDist;

        float fadeDistance = VLB_GET_PROP(_DynamicOcclusionDepthProps);
        float factor = smoothstep(0, fadeDistance, dynamicOcclusionDepthLinear - pixDistFromSource);
        intensity *= lerp(factor, 1, isEqualOrGreater(dynamicOcclusionDepthRaw, 1)); // only apply factor if(dynamicOcclusionDepthRaw < 1), meaning there is an occlusion
    }

#endif

#if DEBUG_SHOW_NOISE3D
    return Noise3D_GetFactorFromUVW(i.uvwNoise_intensity.xyz);
#endif

    float3 cameraPosObjectSpace = i.cameraPosObjectSpace_outsideBeam.xyz;

    // Vector Camera to current Pixel, in object space and normalized
    float3 vecCamToPixOSN = ComputeVectorCamToPixOSN(i.posObjectSpaceNonSkewed.xyz, cameraPosObjectSpace);

#if VLB_NOISE_3D || !OPTIM_VS
    // Blend inside and outside
    float vecCamToPixDotZ = dot(vecCamToPixOSN, float3(0, 0, 1));
    float factorNearAxisZ = abs(vecCamToPixDotZ);
#endif

    // 3D NOISE
#if VLB_NOISE_3D
    {
        float noise3DFactor = Noise3D_GetFactorFromUVW(i.uvwNoise_intensity.rgb);

        // disable noise 3D when looking from behind or from inside because it makes the cone shape too much visible
        noise3DFactor = lerp(noise3DFactor, 1, pow(factorNearAxisZ, 10));

        intensity *= noise3DFactor;
    }
#endif // VLB_NOISE_3D

    // DEPTH BLEND
#if VLB_DEPTH_BLEND
    {
        float depthBlendDistance = VLB_GET_PROP(_DepthBlendDistance);
        
    #if FIX_DISABLE_DEPTH_BLEND_WITH_OBLIQUE_PROJ
        // disable depth sampling with oblique projection
        float3 nearPlaneWS = VLBFrustumPlanes[4].xyz;
        float3 farPlaneWS = VLBFrustumPlanes[5].xyz;
        float dotNearFar = abs(dot(nearPlaneWS, farPlaneWS)); // abs needed on 5.2, but not needed on 2018
        depthBlendDistance *= isEqualOrGreater(dotNearFar, 0.99);
    #endif // FIX_DISABLE_DEPTH_BLEND_WITH_OBLIQUE_PROJ

        // we disable blend factor when the pixel is near the light source,
        // to prevent from blending with the light source model geometry (like the flashlight model).
        float depthBlendStartDistFromSource = depthBlendDistance;
        float pixDistFromSourceNormalized = invLerpClamped(0, depthBlendStartDistFromSource, pixDistFromSource);
        float depthBlendDist = depthBlendDistance * pixDistFromSourceNormalized;
        float depthBlendFactor = DepthFade_PS_BlendDistance(i.projPos, depthBlendDist);
        depthBlendFactor = lerp(depthBlendFactor, 1, isEqualOrGreater(0, depthBlendDistance));
        depthBlendFactor = lerpClamped(1, depthBlendFactor, pixDistFromSourceNormalized * 100); // prevent artifacts when cap geometry is too close from some geometry
        intensity *= depthBlendFactor;

    #if DEBUG_DEPTH_MODE == DEBUG_VALUE_DEPTHBUFFER
        return SampleSceneZ_Eye(i.projPos) * _ProjectionParams.w;
    #elif DEBUG_DEPTH_MODE == DEBUG_VALUE_DEPTHSTEREOEYE
        float depthValue = SampleSceneZ_Eye(i.projPos) * _ProjectionParams.w;
        #if defined(USING_STEREO_MATRICES) && defined(UNITY_STEREO_MULTIVIEW_ENABLED) // used with single pass / multiview on android VR (Oculus Go/Quest, Gear VR)
            return depthValue * lerp(float4(1, 0, 0, 1), float4(0, 1, 0, 1), unity_StereoEyeIndex);
        #elif defined(UNITY_SINGLE_PASS_STEREO)
            return depthValue * lerp(float4(1, 0, 0, 1), float4(0, 0, 1, 1), unity_StereoEyeIndex);
        #elif defined(UNITY_STEREO_INSTANCING_ENABLED)
            return depthValue * lerp(float4(0, 1, 0, 1), float4(0, 0, 1, 1), unity_StereoEyeIndex);
        #elif defined(UNITY_STEREO_MULTIVIEW_ENABLED)
            return depthValue * lerp(float4(1, 1, 0, 1), float4(0, 1, 1, 1), unity_StereoEyeIndex);
        #else
            return depthValue;
        #endif
    #elif DEBUG_DEPTH_MODE == DEBUG_VALUE_DEPTHBLEND
        return depthBlendFactor;
    #endif
    }
#endif // VLB_DEPTH_BLEND

    float3 posViewSpace = i.posViewSpace_extraData.xyz;

#if !OPTIM_VS
    float pixDistFromSourceTilted = GetTiltDistanceFactor(i.posObjectSpace);
    {
        float isCap = i.posViewSpace_extraData.w;

        // no intensity for cap if _DrawCap = 0
        intensity *= isEqualOrGreater(VLB_GET_PROP(_DrawCap), isCap - 0.00001);

        // Treat Cap a special way: cap is only visible from inside
        intensity *= 1 - outsideBeam * isCap; // AKA if (outsideBeam == 1 && isCap == 1) intensity = 0

        // boost factor
        float boostFactor = ComputeBoostFactor(pixDistFromSource, outsideBeam, isCap);

        // fresnel
        intensity *= ComputeFresnel(i.posObjectSpaceNonSkewed, vecCamToPixOSN, outsideBeam, boostFactor);

        // fade out
        intensity *= VLB_GET_PROP(_FadeOutFactor);

        // attenuation
        float3 distancesFallOff = VLB_GET_PROP(_DistanceFallOff);
        intensity *= ComputeAttenuation(pixDistFromSourceTilted, distancesFallOff.x, distancesFallOff.y, VLB_GET_PROP(_AttenuationLerpLinearQuad));

        // fade when too close to camera factor
        float fadeWithCameraEnabled = 1 - max(boostFactor,          // do not fade according to camera when we are in boost zone, to keep boost effect
                                              VLB_CAMERA_ORTHO);    // fading according to camera eye position doesn't make sense with ortho camera
        intensity *= ComputeFadeWithCamera(posViewSpace, fadeWithCameraEnabled);

        // smooth blend between inside and outside geometry depending of View Direction
        intensity *= ComputeInOutBlending(vecCamToPixDotZ, outsideBeam);
    }
#else // OPTIM_VS ON
    {
        float boostFactor = i.posViewSpace_extraData.w;

        // fade when too close to camera factor
        float fadeWithCameraEnabled = 1 - max(boostFactor,          // do not fade according to camera when we are in boost zone, to keep boost effect
                                              VLB_CAMERA_ORTHO);    // fading according to camera eye position doesn't make sense with ortho camera
        intensity *= ComputeFadeWithCamera(posViewSpace, fadeWithCameraEnabled);

#if !OPTIM_VS_FRESNEL_VS
        // compute fresnel in fragment shader to keep good quality even with low tessellation
        intensity *= ComputeFresnel(i.posObjectSpaceNonSkewed, vecCamToPixOSN, outsideBeam, boostFactor);
#endif
    }
#endif // OPTIM_VS


#if DEBUG_BLEND_INSIDE_OUTSIDE
    float DBGvecCamToPixDotZ = dot(vecCamToPixOSN, float3(0, 0, 1));
    return lerp(float4(1, 0, 0, 1), float4(0, 1, 0, 1), ComputeInOutBlending(DBGvecCamToPixDotZ, outsideBeam));
#endif // DEBUG_BLEND_INSIDE_OUTSIDE

    // Do not fill color.rgb only, because of performance drops on android
#if !OPTIM_VS
    half4 color = ComputeColor(pixDistFromSourceTilted, outsideBeam);
#else
    half4 color = i.color;
#endif

#if VLB_DITHERING
    float2 screenPos = i.projPos.xy / i.projPos.w;
    float2 ditherCoord = screenPos * _ScreenParams.xy * _VLB_DitheringNoiseTex_TexelSize.xy;
    float dither = tex2D(_VLB_DitheringNoiseTex, ditherCoord).r - 0.5;
    color += (1 - saturate(intensity)) * _VLB_DitheringFactor * dither;
#endif

#if VLB_ALPHA_AS_BLACK
    color *= intensity;
#else
    color.a *= intensity;
#endif

#ifdef VLB_FOG_APPLY
    VLB_FOG_APPLY(color);
#endif
    return color;
}

#endif