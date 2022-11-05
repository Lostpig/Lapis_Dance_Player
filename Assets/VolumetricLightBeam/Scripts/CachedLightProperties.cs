#if UNITY_EDITOR
using UnityEngine;

namespace VLB
{
    struct CachedLightProperties
    {
        Light light;
        float range;
        float spotAngle;
        Color color;

        public CachedLightProperties(Light lightParam)
        {
            light = lightParam;
            range = -1f;
            spotAngle = -1f;
            color = Color.white;

            if (light && light.type == LightType.Spot)
            {
                range = light.range;
                spotAngle = light.spotAngle;
                color = light.color;
            }
        }

        public override int GetHashCode() { return light ? light.GetHashCode() : 0; }
        public bool Equals(CachedLightProperties other) { return Equals(other, this); }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (CachedLightProperties)obj;
            return other.range == range && other.spotAngle == spotAngle && other.color == color;
        }
    }
}
#endif
