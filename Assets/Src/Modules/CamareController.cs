using UnityEngine;

namespace LapisPlayer
{
    public class CameraController : MonoBehaviour
    {
        private Transform CamLookPos;//围绕的物体
        private Vector3 Rotion_Transform;

        readonly Vector3 defaultPos1 = new Vector3(0f, 0.7f, 12f);

        public void Initialize(Transform lookPos)
        {
            CamLookPos = lookPos;
            Rotion_Transform = CamLookPos.position;
        }

        void Update()
        {
            Cam_Ctrl_Rotate();
            Cam_Ctrl_Move();

            // distance = (transform.position - CamLookPos.position).magnitude;
            //Mathf.Clamp(distance,0.944f,1.397f);
            //Debug.Log(distance);
        }

        private void Cam_Ctrl_Move()
        {
            transform.Translate(Input.GetAxis("Mouse ScrollWheel") * Vector3.forward * 2);
            // if (distance <= 0.6f || distance >= 1.49f)
            // {
            //     moveSpeed = 0f;
            // }
        }

        public void Cam_Ctrl_Rotate()
        {
            var mouse_x = Input.GetAxis("Mouse X");//获取鼠标X轴移动
            var mouse_y = -Input.GetAxis("Mouse Y");//获取鼠标Y轴移动
            if (Input.GetKey(KeyCode.Mouse1))
            {
                transform.RotateAround(Rotion_Transform, Vector3.up, mouse_x * 2);
                transform.RotateAround(Rotion_Transform, transform.right, mouse_y * 2);
            }
            else if (Input.GetKey(KeyCode.Mouse2))
            {
                transform.Translate(Vector3.up * mouse_y * 0.1f);
                transform.Translate(Vector3.left * mouse_x * 0.1f);
            }
        }
    }
}
