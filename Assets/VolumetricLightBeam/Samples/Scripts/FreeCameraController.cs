using UnityEngine;

namespace VLB_Samples
{
    public class FreeCameraController : MonoBehaviour
    {
        public float cameraSensitivity = 90;
        public float speedNormal = 10;
        public float speedFactorSlow = 0.25f;
        public float speedFactorFast = 3;
        public float speedClimb = 4;

        float rotationH = 0.0f;
        float rotationV = 0.0f;

        bool m_UseMouseView = true;
        bool useMouseView
        {
            get { return m_UseMouseView; }
            set
            {
                m_UseMouseView = value;
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !value;
            }
        }

        void Start()
        {
            useMouseView = true;

            var euler = transform.rotation.eulerAngles;
            rotationH = euler.y;
            rotationV = euler.x;
            if (rotationV > 180f)
                rotationV -= 360f;
        }

        void Update()
        {
            if (useMouseView)
            {
                rotationH += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
                rotationV -= Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
            }
            rotationV = Mathf.Clamp(rotationV, -90, 90);

            transform.rotation = Quaternion.AngleAxis(rotationH, Vector3.up);
            transform.rotation *= Quaternion.AngleAxis(rotationV, Vector3.right);

            var speed = speedNormal;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))            speed *= speedFactorFast;
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))   speed *= speedFactorSlow;

            transform.position += speed * Input.GetAxis("Vertical") * Time.deltaTime * transform.forward;
            transform.position += speed * Input.GetAxis("Horizontal") * Time.deltaTime * transform.right;

            if (Input.GetKey(KeyCode.Q)) { transform.position += speedClimb * Time.deltaTime * Vector3.up; }
            if (Input.GetKey(KeyCode.E)) { transform.position += speedClimb * Time.deltaTime * Vector3.down; }

            if (
#if !UNITY_EDITOR
                Input.GetMouseButtonDown(0) ||
#endif
                Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                useMouseView = !useMouseView;

            if (Input.GetKeyDown(KeyCode.Escape))
                useMouseView = false;
        }
    }
}
