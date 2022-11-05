using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB_Samples
{
    public class LightGenerator : MonoBehaviour
    {
        [Range(1, 100)]
        [SerializeField] int CountX = 10;
        [Range(1, 100)]
        [SerializeField] int CountY = 10;
        [SerializeField] float OffsetUnits = 1;
        [SerializeField] float PositionY = 1;
        [SerializeField] bool NoiseEnabled = false;
        [SerializeField] bool AddLight = true;

        public void Generate()
        {
            for (int i = 0; i < CountX; ++i)
            {
                for (int j = 0; j < CountY; ++j)
                {
                    GameObject gao = null;
                    if(AddLight)
                        gao = new GameObject("Light_" + i + "_" + j, typeof(Light), typeof(VLB.VolumetricLightBeam), typeof(Rotater));
                    else
                        gao = new GameObject("Light_" + i + "_" + j, typeof(VLB.VolumetricLightBeam), typeof(Rotater));

                    gao.transform.SetPositionAndRotation(   new Vector3(i * OffsetUnits, PositionY, j * OffsetUnits),
                                                            Quaternion.Euler(Random.Range(-45, 45) + 90f, Random.Range(0, 360), 0));

                    var beam = gao.GetComponent<VLB.VolumetricLightBeam>();
               
                    if (AddLight)
                    {
                        var light = gao.GetComponent<Light>();
                        light.type = LightType.Spot;
                        light.color = new Color(Random.value, Random.value, Random.value, 1.0f);
                        light.range = Random.Range(3f, 8f);
                        light.intensity = Random.Range(0.2f, 5f);
                        light.spotAngle = Random.Range(10f, 90f);

                        if(VLB.Config.Instance.geometryOverrideLayer)
                        {
                            // remove the layer of the beams from the light's culling mask to prevent from breaking GPU Instancing
                            light.cullingMask = ~(1 << VLB.Config.Instance.geometryLayerID);
                        }
                    }
                    else
                    {
                        beam.color = new Color(Random.value, Random.value, Random.value, 1.0f);
                        beam.fallOffEnd = Random.Range(3f, 8f);
                        beam.spotAngle = Random.Range(10f, 90f);
                    }

                    beam.coneRadiusStart = Random.Range(0f, 0.1f);
                    beam.geomCustomSides = Random.Range(12, 36);
                    beam.fresnelPow = Random.Range(1, 7.5f);
                    beam.noiseMode = NoiseEnabled ? VLB.NoiseMode.WorldSpace : VLB.NoiseMode.Disabled;

                    var rotater = gao.GetComponent<Rotater>();
                    rotater.EulerSpeed = new Vector3(0, Random.Range(-500, 500), 0);
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LightGenerator))]
    public class LightGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying) return;
            if (GUILayout.Button("Generate"))
            {
                (target as LightGenerator).Generate();
            }
        }
    }
#endif
}

