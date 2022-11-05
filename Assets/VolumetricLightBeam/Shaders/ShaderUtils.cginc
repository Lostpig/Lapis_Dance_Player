// The following comment prevents Unity from auto upgrading the shader. Please keep it to keep backward compatibility.
// UNITY_SHADER_NO_UPGRADE

#ifndef _VLB_SHADER_UTILS_INCLUDED_
#define _VLB_SHADER_UTILS_INCLUDED_

#include "ShaderMaths.cginc"

// https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
#define VLB_CAMERA_NEAR_PLANE _ProjectionParams.y
#define VLB_CAMERA_FAR_PLANE _ProjectionParams.z
#define VLB_CAMERA_ORTHO unity_OrthoParams.w // w is 1.0 when camera is orthographic, 0.0 when perspective

// Z buffer to linear depth
float VLB_ZBufferToLinear(float depth, float near, float far)
{
    float x = 1 - far / near;
    float y = far / near;

    float z = x / far;
    float w = y / far;

    return 1.0 / (z * depth + w);
}

#if VLB_DEPTH_BLEND || VLB_DITHERING

inline float SampleSceneZ_Eye(float4 uv)
{
    float rawDepth = VLBSampleDepthTexture(uv);
    float linearDepthPersp = VLBLinearEyeDepth(rawDepth);

    rawDepth = lerp(rawDepth, 1.0f - rawDepth, _VLB_UsesReversedZBuffer);
    float linearDepthOrtho = (VLB_CAMERA_FAR_PLANE - VLB_CAMERA_NEAR_PLANE) * rawDepth + VLB_CAMERA_NEAR_PLANE;

    return lerp(linearDepthPersp, linearDepthOrtho, VLB_CAMERA_ORTHO);
}

inline float4 DepthFade_VS_ComputeProjPos(float3 vertexViewSpace, float4 vertexClipSpace)
{
    float4 projPos = ComputeScreenPos(vertexClipSpace);
    projPos.z = -vertexViewSpace.z; // = COMPUTE_EYEDEPTH
    return projPos;
}

inline float DepthFade_PS_BlendDistance(float4 projPos, float distance)
{
    float sceneZ = max(0, SampleSceneZ_Eye(projPos) - VLB_CAMERA_NEAR_PLANE);
    float partZ = max(0, projPos.z - VLB_CAMERA_NEAR_PLANE);
    return saturate((sceneZ - partZ) / distance);
}
#endif // VLB_DEPTH_BLEND || VLB_DITHERING


#if VLB_NOISE_3D
uniform sampler3D _VLB_NoiseTex3D;
uniform float _VLB_NoiseCustomTime;

float3 Noise3D_GetUVW(float3 posWorldSpace, float3 posLocalSpace)
{
    float4 noiseVelocityAndScale = VLB_GET_PROP(_NoiseVelocityAndScale);
    float2 noiseParam = VLB_GET_PROP(_NoiseParam);
    float3 velocity = noiseVelocityAndScale.xyz;
    float scale = noiseVelocityAndScale.w;

    float3 posRef = lerp(posWorldSpace, posLocalSpace, noiseParam.y); // 0 -> World Space ; 1 -> Local Space

    // use _VLB_NoiseCustomTime if it's equal or higher than 0.0
    float currentTime = lerp(_Time.y, _VLB_NoiseCustomTime, isEqualOrGreater(_VLB_NoiseCustomTime, 0.0f));

	//return frac(posRef.xyz * scale + (currentTime * velocity)); // frac doesn't give good results on VS
	return (posRef.xyz * scale + (currentTime * velocity));
}

float Noise3D_GetFactorFromUVW(float3 uvw)
{
    float2 noiseParam = VLB_GET_PROP(_NoiseParam);
    float intensity = noiseParam.x;
	float noise = tex3D(_VLB_NoiseTex3D, uvw).a;
    return lerp(1, noise, intensity);
}
#endif // VLB_NOISE_3D


inline float ComputeAttenuation(float pixDistZ, float fallOffStart, float fallOffEnd, float lerpLinearQuad)
{
    float distFromSourceNormalized = invLerpClamped(fallOffStart, fallOffEnd, pixDistZ);

    // Almost simple linear attenuation between Fade Start and Fade End: Use smoothstep for a better fall to zero rendering
    float attLinear = smoothstep(0, 1, 1 - distFromSourceNormalized);

    // Unity's custom quadratic attenuation https://forum.unity.com/threads/light-attentuation-equation.16006/
    float attQuad = 1.0 / (1.0 + 25.0 * distFromSourceNormalized * distFromSourceNormalized);

    const float kAttQuadStartToFallToZero = 0.8;
    attQuad *= saturate(smoothstep(1.0, kAttQuadStartToFallToZero, distFromSourceNormalized)); // Near the light's range (fade end) we fade to 0 (because quadratic formula never falls to 0)

    return lerp(attLinear, attQuad, lerpLinearQuad);
}



#if VLB_COLOR_GRADIENT_MATRIX_HIGH || VLB_COLOR_GRADIENT_MATRIX_LOW
#if VLB_COLOR_GRADIENT_MATRIX_HIGH
#define FLOAT_PACKING_PRECISION 64
#else
#define FLOAT_PACKING_PRECISION 8
#endif
inline half4 UnpackToColor(float packedFloat)
{
    half4 color;

    color.a = packedFloat % FLOAT_PACKING_PRECISION;
    packedFloat = floor(packedFloat / FLOAT_PACKING_PRECISION);

    color.b = packedFloat % FLOAT_PACKING_PRECISION;
    packedFloat = floor(packedFloat / FLOAT_PACKING_PRECISION);

    color.g = packedFloat % FLOAT_PACKING_PRECISION;
    packedFloat = floor(packedFloat / FLOAT_PACKING_PRECISION);

    color.r = packedFloat;

    return color / (FLOAT_PACKING_PRECISION - 1);
}

inline float GetAtMatrixIndex(float4x4 mat, uint idx) { return mat[idx % 4][floor(idx / 4)]; }

inline half4 DecodeGradient(float t, float4x4 colorMatrix)
{
#define kColorGradientMatrixSize 16
    float sampleIndexFloat = t * (kColorGradientMatrixSize - 1);
    float ratioPerSample = sampleIndexFloat - (int)sampleIndexFloat;
    uint sampleIndexInt = min((uint)sampleIndexFloat, kColorGradientMatrixSize - 2);
    half4 colorA = UnpackToColor(GetAtMatrixIndex(colorMatrix, sampleIndexInt + 0));
    half4 colorB = UnpackToColor(GetAtMatrixIndex(colorMatrix, sampleIndexInt + 1));
    return lerp(colorA, colorB, ratioPerSample);
}
#elif VLB_COLOR_GRADIENT_ARRAY
inline half4 DecodeGradient(float t, float4 colorArray[kColorGradientArraySize])
{
    uint arraySize = kColorGradientArraySize;
    float sampleIndexFloat = t * (arraySize - 1);
    float ratioPerSample = sampleIndexFloat - (int)sampleIndexFloat;
    uint sampleIndexInt = min((uint)sampleIndexFloat, arraySize - 2);
    half4 colorA = colorArray[sampleIndexInt + 0];
    half4 colorB = colorArray[sampleIndexInt + 1];
    return lerp(colorA, colorB, ratioPerSample);
}
#endif // VLB_COLOR_GRADIENT_*

#endif // _VLB_SHADER_UTILS_INCLUDED_
