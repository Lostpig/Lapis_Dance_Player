using UnityEngine;

namespace LapisPlayer
{
    public class LightSpotHorizontal : MonoBehaviour
    {
        public Transform shadowPos;

        private void LateUpdate()
        {
            var originPos = transform.position;
            var horizontal = shadowPos == null ? 0.01f : shadowPos.position.y + 0.01f;

            transform.position = new Vector3(originPos.x, horizontal, originPos.z);
            transform.eulerAngles = new Vector3(0, 90, 0);
        }
    }
}
