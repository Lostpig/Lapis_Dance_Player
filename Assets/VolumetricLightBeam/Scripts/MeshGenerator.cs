using UnityEngine;

namespace VLB
{
    public static class MeshGenerator
    {
        const float kMinTruncatedRadius = 0.001f;

        static float GetAngleOffset(int numSides)
        {
            // rotate square beams so they are properly oriented to scale them more easily
            return numSides == 4 ? (Mathf.PI * 0.25f) : 0f;
        }

        public static Mesh GenerateConeZ_RadiusAndAngle(float lengthZ, float radiusStart, float coneAngle, int numSides, int numSegments, bool cap, bool doubleSided)
        {
            Debug.Assert(lengthZ > 0f);
            Debug.Assert(coneAngle > 0f && coneAngle < 180f);
            var radiusEnd = lengthZ * Mathf.Tan(coneAngle * Mathf.Deg2Rad * 0.5f);

            return GenerateConeZ_Radius(lengthZ, radiusStart, radiusEnd, numSides, numSegments, cap, doubleSided);
        }

        public static Mesh GenerateConeZ_Angle(float lengthZ, float coneAngle, int numSides, int numSegments, bool cap, bool doubleSided)
        {
            return GenerateConeZ_RadiusAndAngle(lengthZ, 0f, coneAngle, numSides, numSegments, cap, doubleSided);
        }

        public static Mesh GenerateConeZ_Radius(float lengthZ, float radiusStart, float radiusEnd, int numSides, int numSegments, bool cap, bool doubleSided)
        {
            Debug.Assert(lengthZ > 0f);
            Debug.Assert(radiusStart >= 0f);
            Debug.Assert(numSides >= 3);
            Debug.Assert(numSegments >= 0);

            var mesh = new Mesh();
            bool genCap = cap && radiusStart > 0f;

            // We use the XY position of the vertices to compute the cone normal in the shader.
            // With a perfectly sharp cone, we couldn't compute accurate normals at its top.
            radiusStart = Mathf.Max(radiusStart, kMinTruncatedRadius); 

            int vertCountSides = numSides * (numSegments + 2);
            int vertCountTotal = vertCountSides;

            if (genCap)
                vertCountTotal += numSides + 1;

            // VERTICES
            {
                float angleOffset = GetAngleOffset(numSides);

                var vertices = new Vector3[vertCountTotal];

                for (int i = 0; i < numSides; i++)
                {
                    float angle = angleOffset + 2 * Mathf.PI * i / numSides;
                    float angleCos = Mathf.Cos(angle);
                    float angleSin = Mathf.Sin(angle);

                    for (int seg = 0; seg < numSegments + 2; seg++)
                    {
                        float tseg = (float)seg / (numSegments + 1);
                        Debug.Assert(tseg >= 0f && tseg <= 1f);
                        float radius = Mathf.Lerp(radiusStart, radiusEnd, tseg);
                        vertices[i + seg * numSides] = new Vector3(radius * angleCos, radius * angleSin, tseg * lengthZ);
                    }
                }

                if (genCap)
                {
                    int ind = vertCountSides;

                    vertices[ind] = Vector3.zero;
                    ind++;

                    for (int i = 0; i < numSides; i++)
                    {
                        float angle = angleOffset + 2 * Mathf.PI * i / numSides;
                        float angleCos = Mathf.Cos(angle);
                        float angleSin = Mathf.Sin(angle);
                        vertices[ind] = new Vector3(radiusStart * angleCos, radiusStart * angleSin, 0f);
                        ind++;
                    }

                    Debug.Assert(ind == vertices.Length);
                }

                if (!doubleSided)
                {
                    mesh.vertices = vertices;
                }
                else
                {
                    var vertices2 = new Vector3[vertices.Length * 2];
                    vertices.CopyTo(vertices2, 0);
                    vertices.CopyTo(vertices2, vertices.Length);
                    mesh.vertices = vertices2;
                }
            }

            // UV (used to flags vertices as sides or cap)
            // X: 0 = sides ; 1 = cap
            // Y: 0 = front face ; 1 = back face (doubleSided only)
            {
                var uv = new Vector2[vertCountTotal];
                int ind = 0;
                for (int i = 0; i < vertCountSides; i++)
                    uv[ind++] = Vector2.zero;

                if (genCap)
                {
                    for (int i = 0; i < numSides + 1; i++)
                        uv[ind++] = new Vector2(1, 0);
                }

                Debug.Assert(ind == uv.Length);


                if (!doubleSided)
                {
                    mesh.uv = uv;
                }
                else
                {
                    var uv2 = new Vector2[uv.Length * 2];
                    uv.CopyTo(uv2, 0);
                    uv.CopyTo(uv2, uv.Length);

                    for (int i = 0; i < uv.Length; i++)
                    {
                        var value = uv2[i + uv.Length];
                        uv2[i + uv.Length] = new Vector2(value.x, 1);
                    }

                    mesh.uv = uv2;
                }
            }

            // INDICES
            {
                int triCountSides = numSides * 2 * Mathf.Max(numSegments + 1, 1);
                int indCountSides = triCountSides * 3;
                int indCountTotal = indCountSides;

                if (genCap)
                    indCountTotal += numSides * 3;

                var indices = new int[indCountTotal];
                int ind = 0;

                for (int i = 0; i < numSides; i++)
                {
                    int ip1 = i + 1;
                    if (ip1 == numSides)
                        ip1 = 0;

                    for (int k = 0; k < numSegments + 1; ++k)
                    {
                        var offset = k * numSides;

                        indices[ind++] = offset + i;
                        indices[ind++] = offset + ip1;
                        indices[ind++] = offset + i + numSides;

                        indices[ind++] = offset + ip1 + numSides;
                        indices[ind++] = offset + i + numSides;
                        indices[ind++] = offset + ip1;
                    }
                }

                if (genCap)
                {
                    for (int i = 0; i < numSides - 1; i++)
                    {
                        indices[ind++] = vertCountSides;
                        indices[ind++] = vertCountSides + i + 2;
                        indices[ind++] = vertCountSides + i + 1;
                    }

                    indices[ind++] = vertCountSides;
                    indices[ind++] = vertCountSides + 1;
                    indices[ind++] = vertCountSides + numSides;
                }

                Debug.Assert(ind == indices.Length);

                if (!doubleSided)
                {
                    mesh.triangles = indices;
                }
                else
                {
                    var indices2 = new int[indices.Length * 2];
                    indices.CopyTo(indices2, 0);
                    
                    for (int i = 0; i < indices.Length; i += 3)
                    {
                        indices2[indices.Length + i + 0] = indices[i + 0] + vertCountTotal;
                        indices2[indices.Length + i + 1] = indices[i + 2] + vertCountTotal;
                        indices2[indices.Length + i + 2] = indices[i + 1] + vertCountTotal;
                    }
                    
                    mesh.triangles = indices2;
                }
            }

            mesh.bounds = ComputeBounds(lengthZ, radiusStart, radiusEnd);

            Debug.Assert(mesh.vertexCount == GetVertexCount(numSides, numSegments, genCap, doubleSided));
            Debug.Assert(mesh.triangles.Length == GetIndicesCount(numSides, numSegments, genCap, doubleSided));

            return mesh;
        }

        public static Bounds ComputeBounds(float lengthZ, float radiusStart, float radiusEnd)
        {
            float maxDiameter = Mathf.Max(radiusStart, radiusEnd) * 2;
            return new Bounds(
                new Vector3(0, 0, lengthZ * 0.5f),
                new Vector3(maxDiameter, maxDiameter, lengthZ)
                );
        }


        public static int GetVertexCount(int numSides, int numSegments, bool geomCap, bool doubleSided)
        {
            Debug.Assert(numSides >= 2);
            Debug.Assert(numSegments >= 0);

            int count = numSides * (numSegments + 2);
            if (geomCap) count += numSides + 1;
            if (doubleSided) count *= 2;
            return count;
        }

        public static int GetIndicesCount(int numSides, int numSegments, bool geomCap, bool doubleSided)
        {
            Debug.Assert(numSides >= 2);
            Debug.Assert(numSegments >= 0);

            int count = numSides * (numSegments + 1) * 2 * 3;
            if (geomCap) count += numSides * 3;
            if (doubleSided) count *= 2;
            return count;
        }

        public static int GetSharedMeshVertexCount()
        {
            return GetVertexCount(Config.Instance.sharedMeshSides, Config.Instance.sharedMeshSegments, true, Config.Instance.requiresDoubleSidedMesh);
        }

        public static int GetSharedMeshIndicesCount()
        {
            return GetIndicesCount(Config.Instance.sharedMeshSides, Config.Instance.sharedMeshSegments, true, Config.Instance.requiresDoubleSidedMesh);
        }
    }
}
