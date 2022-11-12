using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SeanPostProcessSetting
{
    // Fields
    private bool _changed; // 0x10
    public PiplineSetting DataSource; // 0x18
    public BloomSetting Bloom; // 0x20
    public BlurSetting Blur; // 0x28
    public ColorSetting Color; // 0x30
    public VignetteSetting Vignette; // 0x38
    public GlowSetting Glow; // 0x40
    public ToneMappingSetting ToneMapping; // 0x48
    public List<PostEffectSetting> PostEffectSettings; // 0x50

	public bool MarkAllSettingChanged { get; set; }

    // public SeanPostProcessSetting Clone() { }
    public void Interp(SeanPostProcessSetting from, SeanPostProcessSetting to, float t) { }
}

[Serializable]
public class PiplineSetting : EffectSetting<PiplineSetting>
{
    // Fields
    public float ratio; // 0x18
    public InterpFloat resolutionScale; // 0x20
    private PiplineType _piplineType; // 0x28
    public InterpVector2 CameraClipping; // 0x30
    public InterpBool AllowHDR; // 0x38
    public InterpBool AllowMSAA; // 0x40
    private AntiAilasing _AntiAilas; // 0x48
    public DepthTextureMode depthTextureMode; // 0x4c
    public MRTDataTexture MRTTexture; // 0x50

    // Properties
    public PiplineType piplineType { get; set; }
    public AntiAilasing AntiAilas { get; set; }
    public override int DebugStepCount { get; }
    public override string EffectName { get; }
}

[Serializable]
public class BloomSetting : EffectSetting<BloomSetting>
{
    // Fields
    public static bool GlobleEnable; // 0x0
    public InterpFloat threshold; // 0x18
    public InterpColor Tint; // 0x20
    public InterpFloat softKnee; // 0x28
    public InterpFloat radius; // 0x30
    public InterpFloat intensity; // 0x38
    public InterpBool highQuality; // 0x40
    public InterpBool antiFlicker; // 0x48
    public Texture dirtTexture; // 0x50
    public InterpFloat dirtIntensity; // 0x58

    // Properties
    public float thresholdGamma { get; set; }
    public float thresholdLinear { get; set; }
    public override string EffectName { get; }
    public override int DebugStepCount { get; }

    // public override BloomSetting Clone() { }
    internal void Interp(BloomSetting from, BloomSetting to, float t) { }
}

[Serializable]
public class BlurSetting : EffectSetting<BlurSetting>
{
    // Fields
    public BlurType blurType; // 0x18
    public InterpFloat BlurSize; // 0x20
    public InterpFloat BlurLevel; // 0x28
    public InterpFloat BlurDistance; // 0x30
    public InterpFloat focalLength; // 0x38
    public SceneReference DofFocal; // 0x40
    public InterpFloat focalSize; // 0x48
    public InterpFloat aperture; // 0x50

    public override int DebugStepCount { get; }
    public override string EffectName { get; }

    internal void Interp(BlurSetting from, BlurSetting to, Single t) { }
    // public override BlurSetting Clone() { }
}

[Serializable]
public class ColorSetting : EffectSetting<ColorSetting>
{
    // Fields
    public InterpFloat Brightness; // 0x18
    public InterpFloat Contrast; // 0x20
    public InterpVector3 ContrastCoeff; // 0x28
    public InterpFloat Gamma; // 0x30

    public override string EffectName { get; }
    public override int DebugStepCount { get; }

    // public override ColorSetting Clone() { }
    internal void Interp(ColorSetting from, ColorSetting to, float t) { }
}

[Serializable]
public class VignetteSetting : EffectSetting<VignetteSetting>
{
    public InterpFloat vignetteBeginRadius; // 0x18
    public InterpFloat vignetteExpandRadius; // 0x20
    public InterpColor vignetteColor; // 0x28

    public override string EffectName { get; }
    public override int DebugStepCount { get; }

    // public override VignetteSetting Clone() { }
    internal void Interp(VignetteSetting from, VignetteSetting to, float t) { }
}

[Serializable]
public class GlowSetting : EffectSetting<GlowSetting>
{
    public GlowType glowType; // 0x18
    public InterpFloat radius; // 0x20
    public InterpFloat intensity; // 0x28

    public override string EffectName { get; }
    public override PiplineType Available { get; }
    public override int DebugStepCount { get; }

    // public override GlowSetting Clone() { }
    internal void Interp(GlowSetting from, GlowSetting to, float t) { }
}

[Serializable]
public class ToneMappingSetting : EffectSetting<ToneMappingSetting>
{
    // Fields
    public InterpVector4 ABCD; // 0x18
    public InterpFloat E; // 0x20

    // Properties
    public override string EffectName { get; }
    public override int DebugStepCount { get; }
}