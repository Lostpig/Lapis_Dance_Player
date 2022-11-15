using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Oz.Graphics
{
    [Serializable]
    public class SceneEnvironment : MonoBehaviour
    {
        // Fields
        [SerializeField] public static bool ForbidApplyOnFirstEnable; // 0x0
        [SerializeField] private bool m_ApplyOnFirstEnable; // 0x1a
        [SerializeField] private BatchSettings m_BatchSettings; // 0x20
        [SerializeField] private RenderingSettings m_RenderingSettings; // 0x28
        [SerializeField] private ShadowSettings m_ShadowSettings; // 0x30
        [SerializeField] private ShaderSettings m_ShaderSettings; // 0x38
        [SerializeField] private SettingsProfile m_SettingsProfile; // 0x40
        // [SerializeField] private SeanPostProcessLayer m_PostProcessLayer; // 0x48
        [SerializeField]  private LightSettings m_LightSettings; // 0x50

        // Properties
        public bool ApplyOnEnable { get; set; }
        public bool ResetOnDisable { get; set; }
        public BatchSettings BatchSettings { get => m_BatchSettings; }
        public RenderingSettings RenderingSettings { get => m_RenderingSettings; }
        public ShadowSettings ShadowSettings { get => m_ShadowSettings; }
        public ShaderSettings ShaderSettings { get => m_ShaderSettings; }
        public SettingsProfile SettingsProfile { get => m_SettingsProfile; }
        // public SeanPostProcessLayer PostProcessLayer { get; }
        public LightSettings LightSettings { get => m_LightSettings; }


        // private void Awake() { }
        private void Start()
        {
            if (m_ShadowSettings.UseGeometryShadow)
            {
                var shadows = GetGeometricShadowCastInScene();
                foreach (var s in shadows)
                {
                    s.ApplyShadow(m_ShadowSettings, m_BatchSettings.StaticRoot);
                }
            }
        }
        // private void OnDisable() { }
        public void Apply() { }
        public void ApplyPostProcess(SettingsProfile settingsProfile) { }
        private List<GeometricShadowCast> GetGeometricShadowCastInScene()
        {
            return GeometricShadowCast.geometricShadowCasts;
        }
        // private void Update() { }
        // private void LateUpdate() { }
    }

    [Serializable]
    public class BatchSettings
    {
        public bool m_AutoBatching; // 0x10
        public GameObject m_StaticRoot; // 0x18
        public GameObject m_DynamicRoot; // 0x20

        public bool AutoBatching { get; }
        public GameObject StaticRoot { get => m_StaticRoot; }
        public GameObject DynamicRoot { get => m_DynamicRoot; }
        public void ApplyToScene() { }
    }

    [Serializable]
    public class RenderingSettings
    {
        // Fields
        private AmbientMode m_AmbientMode; // 0x10
        private Color m_AmbientGroundColor; // 0x14
        private Color m_AmbientEquatorColor; // 0x24
        private Color m_AmbientSkyColor; // 0x34
        private float m_AmbientIntensity; // 0x44
        private bool m_Fog; // 0x48
        private FogMode m_FogMode; // 0x4c
        private Color m_FogColor; // 0x50
        private float m_FogDensity; // 0x60
        private float m_LinearFogStart; // 0x64
        private float m_LinearFogEnd; // 0x68

        // Properties
        public AmbientMode AmbientMode { get => m_AmbientMode; }
        public Color AmbientGroundColor => m_AmbientGroundColor;
        public Color AmbientEquatorColor => m_AmbientEquatorColor;
        public Color AmbientSkyColor => m_AmbientSkyColor;
        public float AmbientIntensity => m_AmbientIntensity;
        public bool Fog => m_Fog;
        public FogMode FogMode => m_FogMode;
        public Color FogColor => m_FogColor;
        public float FogDensity => m_FogDensity;
        public float LinearFogStart => m_LinearFogStart;
        public float LinearFogEnd => m_LinearFogEnd;

        public void ApplyToScene() { }
        public void Reset() { }
    }

    [Serializable]
    public class ShadowSettings
    {
        // Fields
        [SerializeField] private float m_ShadowDistance; // 0x10
        [SerializeField] private bool m_UseGeometryShadow; // 0x14
        [SerializeField] private bool m_ForceGeometryShadow; // 0x15
        [SerializeField] private Mesh m_ShadowMesh; // 0x18
        [SerializeField] private Material m_ShadowMaterial; // 0x20
        [SerializeField] private bool m_UseRaycast; // 0x28
        [SerializeField] private Transform m_RaycastOrigin; // 0x30
        [SerializeField] private LayerMask m_CullingMask; // 0x38
        [SerializeField] private float m_WorldHeight; // 0x3c
        [SerializeField] private float m_MaxHeight; // 0x40
        [SerializeField] private Vector3 m_Rotation; // 0x44
        [SerializeField] private AnimationCurve m_ScaleCurve; // 0x50
        [SerializeField] private AnimationCurve m_OffsetCurve; // 0x58
        [SerializeField] private AnimationCurve m_AlphaCurve; // 0x60

        // Properties
        public float ShadowDistance { get => m_ShadowDistance; set => m_ShadowDistance = value; }
        public bool UseGeometryShadow { get => m_UseGeometryShadow; }
        public bool ForceGeometryShadow { get => m_ForceGeometryShadow; }
        public Mesh ShadowMesh { get => m_ShadowMesh; }
        public Material ShadowMaterial { get => m_ShadowMaterial; }
        public bool UseRaycast { get => m_UseRaycast; }
        public Transform RaycastOrigin { get => m_RaycastOrigin; }
        public LayerMask CullingMask { get => m_CullingMask; }
        public float WorldHeight { get => m_WorldHeight; }
        public float MaxHeight { get => m_MaxHeight; }
        public Vector3 Rotation { get => m_Rotation; }
        public AnimationCurve ScaleCurve { get => m_ScaleCurve; }
        public AnimationCurve OffsetCurve { get => m_OffsetCurve; }
        public AnimationCurve AlphaCurve { get => m_AlphaCurve; }

        public void ApplyToScene() { }
        public void Reset() { }
    }

    [Serializable]
    public class ShaderSettings
    {
        // Fields
        private float m_SceneRim; // 0x10
        private Color m_RimColor; // 0x14

        public void ApplyRim(float globalRim, Color rimColor) { }
        public void ApplyToScene() { }
        public void Reset() { }
    }

}
