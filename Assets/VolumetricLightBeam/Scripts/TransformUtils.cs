using UnityEngine;

namespace VLB
{
    public static class TransformUtils
    {
        public struct Packed
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 lossyScale;

            public bool IsSame(Transform transf)
            {
                return transf.position == position && transf.rotation == rotation && transf.lossyScale == lossyScale;
            }
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
    }
}
