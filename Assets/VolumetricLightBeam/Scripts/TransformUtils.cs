using UnityEngine;

namespace VLB
{
    public static class TransformUtils
    {
        public class Packed
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 lossyScale;
        }

        public static Packed GetWorldPacked(this Transform self)
        {
            return new Packed()
            {
                position = self.position,
                rotation = self.rotation,
                lossyScale = self.lossyScale,
            };
        }

        public static bool IsSame(this Transform self, Packed packed)
        {
            return packed != null && self.position == packed.position && self.rotation == packed.rotation && self.lossyScale == packed.lossyScale;
        }
    }
}
