using UnityEngine;
using UnityEngine.Serialization;

namespace VLB_Samples
{
    public class Rotater : MonoBehaviour
    {
        [FormerlySerializedAs("m_EulerSpeed")]
        public Vector3 EulerSpeed = Vector3.zero;

        void Update()
        {
            var euler = transform.rotation.eulerAngles;
            euler += EulerSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(euler);
        }
    }
}
