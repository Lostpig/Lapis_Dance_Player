using System.Collections.Generic;
using UnityEngine;

namespace VLB
{
    public class PolygonHelper : MonoBehaviour
    {
        public struct Plane2D
        {
            public Vector2 normal;
            public float distance;

            public float Distance(Vector2 point) { return Vector2.Dot(normal, point) + distance; }

            public Vector2 ClosestPoint(Vector2 pt) { return pt - normal * Distance(pt); }

            public Vector2 Intersect(Vector2 p1, Vector2 p2)
            {
                float denominator = Vector2.Dot(normal, p1 - p2);

                if (Utils.IsAlmostZero(denominator))
                    return (p1 + p2) * 0.5f;

                float u = (normal.x * p1.x + normal.y * p1.y + distance) / denominator;
                return (p1 + u * (p2 - p1));
            }

            public bool GetSide(Vector2 point) { return Distance(point) > 0.0f; }

            public static Plane2D FromPoints(Vector3 p1, Vector3 p2)
            {
                var v = (p2 - p1).normalized;

                return new Plane2D
                {
                    normal = new Vector2(v.y, -v.x),
                    distance = (-v.y * p1.x + v.x * p1.y)
                };
            }

            public static Plane2D FromNormalAndPoint(Vector3 normalizedNormal, Vector3 p1)
            {
                return new Plane2D
                {
                    normal = normalizedNormal,
                    distance = (-normalizedNormal.x * p1.x - normalizedNormal.y * p1.y)
                };
            }

            public void Flip() { normal = -normal; distance = -distance; }

            public Vector2[] CutConvex(Vector2[] poly)
            {
                Debug.Assert(poly.Length >= 3);
                var polyOut = new List<Vector2>(poly.Length);

                Vector2 startingPoint = poly[poly.Length - 1];
                foreach (var endPoint in poly)
                {
                    var startingSide = GetSide(startingPoint);
                    var endSide = GetSide(endPoint);
                    if (startingSide && endSide)
                    {
                        polyOut.Add(endPoint);
                    }
                    else if (startingSide && !endSide)
                    {
                        polyOut.Add(Intersect(startingPoint, endPoint));
                    }
                    else if (!startingSide && endSide)
                    {
                        polyOut.Add(Intersect(startingPoint, endPoint));
                        polyOut.Add(endPoint);
                    }
                    startingPoint = endPoint;
                }

                return polyOut.ToArray();
            }

            public override string ToString() { return string.Format("{0} x {1} + {2}", normal.x, normal.y, distance); }
        }
    }
}
