#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VLB
{
    [CustomEditor(typeof(VolumetricDustParticles))]
    [CanEditMultipleObjects]
    public class VolumetricDustParticlesEditor : EditorCommon
    {
        SerializedProperty alpha, size, direction, velocity, density, spawnMinDistance, spawnMaxDistance, cullingEnabled, cullingMaxDistance;

        static bool AreParticlesInfosUpdated() { return VolumetricDustParticles.isFeatureSupported && Application.isPlaying; }
        public override bool RequiresConstantRepaint() { return AreParticlesInfosUpdated(); }

        protected override void OnEnable()
        {
            base.OnEnable();

            alpha = FindProperty((VolumetricDustParticles x) => x.alpha);
            size = FindProperty((VolumetricDustParticles x) => x.size);
            direction = FindProperty((VolumetricDustParticles x) => x.direction);
            velocity = FindProperty((VolumetricDustParticles x) => x.velocity);
            density = FindProperty((VolumetricDustParticles x) => x.density);
            spawnMinDistance = FindProperty((VolumetricDustParticles x) => x.spawnMinDistance);
            spawnMaxDistance = FindProperty((VolumetricDustParticles x) => x.spawnMaxDistance);
            cullingEnabled = FindProperty((VolumetricDustParticles x) => x.cullingEnabled);
            cullingMaxDistance = FindProperty((VolumetricDustParticles x) => x.cullingMaxDistance);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var particles = target as VolumetricDustParticles;

            if (!VolumetricDustParticles.isFeatureSupported)
            {
                EditorGUILayout.HelpBox(EditorStrings.DustParticles.HelpFeatureNotSupported, MessageType.Warning);
            }
            else if (particles.gameObject.activeSelf && particles.enabled && !particles.particlesAreInstantiated)
            {
                EditorGUILayout.HelpBox(EditorStrings.DustParticles.HelpFailToInstantiate, MessageType.Error);
                ButtonOpenConfig();
            }

            if (FoldableHeader.Begin(this, EditorStrings.DustParticles.HeaderRendering))
            {
                EditorGUILayout.PropertyField(alpha, EditorStrings.DustParticles.Alpha);
                EditorGUILayout.PropertyField(size, EditorStrings.DustParticles.Size);
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.DustParticles.HeaderDirectionAndVelocity))
            {
                EditorGUILayout.PropertyField(direction, EditorStrings.DustParticles.Direction);

                if (particles.direction == ParticlesDirection.Random)
                {
                    var vec = velocity.vector3Value;
                    vec.z = EditorGUILayout.FloatField(EditorStrings.DustParticles.Velocity, vec.z);
                    velocity.vector3Value = vec;
                }
                else
                {
                    EditorGUILayout.PropertyField(velocity, EditorStrings.DustParticles.Velocity);
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.DustParticles.HeaderCulling))
            {
                EditorGUILayout.PropertyField(cullingEnabled, EditorStrings.DustParticles.CullingEnabled);
                if (cullingEnabled.boolValue)
                    EditorGUILayout.PropertyField(cullingMaxDistance, EditorStrings.DustParticles.CullingMaxDistance);
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.DustParticles.HeaderSpawning))
            {
                EditorGUILayout.PropertyField(density, EditorStrings.DustParticles.Density);
                EditorGUILayout.PropertyField(spawnMinDistance, EditorStrings.DustParticles.SpawnMinDistance);
                EditorGUILayout.PropertyField(spawnMaxDistance, EditorStrings.DustParticles.SpawnMaxDistance);

                if (VolumetricDustParticles.isFeatureSupported)
                {
                    var infos = "Particles count:\nCurrent: ";
                    if (AreParticlesInfosUpdated()) infos += particles.particlesCurrentCount;
                    else infos += "(playtime only)";
                    if (particles.isCulled)
                        infos += string.Format(" (culled by '{0}')", particles.mainCamera.name);
                    infos += string.Format("\nMax: {0}", particles.particlesMaxCount);
                    EditorGUILayout.HelpBox(infos, MessageType.Info);
                }
            }
            FoldableHeader.End();

            if (FoldableHeader.Begin(this, EditorStrings.DustParticles.HeaderInfos))
            {
                EditorGUILayout.HelpBox(EditorStrings.DustParticles.HelpRecommendation, MessageType.Info);
            }
            FoldableHeader.End();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
