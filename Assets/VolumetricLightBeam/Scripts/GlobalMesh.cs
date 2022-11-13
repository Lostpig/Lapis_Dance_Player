using UnityEngine;
using System.Collections;

namespace VLB
{
    public static class GlobalMesh
    {
        public static Mesh Get()
        {
            var needDoubleSided = Config.Instance.requiresDoubleSidedMesh;

            if (ms_Mesh == null
             || ms_DoubleSided != needDoubleSided)
            {
                Destroy();

                ms_Mesh = MeshGenerator.GenerateConeZ_Radius(
                    lengthZ: 1f,
                    radiusStart: 1f,
                    radiusEnd: 1f,
                    numSides: Config.Instance.sharedMeshSides,
                    numSegments: Config.Instance.sharedMeshSegments,
                    cap: true,
                    doubleSided: needDoubleSided);

                ms_Mesh.hideFlags = Consts.ProceduralObjectsHideFlags;
                ms_DoubleSided = needDoubleSided;
            }

            return ms_Mesh;
        }

        public static void Destroy()
        {
            if (ms_Mesh != null)
            {
                GameObject.DestroyImmediate(ms_Mesh);
                ms_Mesh = null;
            }
        }

        static Mesh ms_Mesh = null;
        static bool ms_DoubleSided = false;
    }
}
