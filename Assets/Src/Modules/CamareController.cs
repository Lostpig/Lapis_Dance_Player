using UnityEngine;
using UnityEngine.EventSystems;

namespace LapisPlayer
{
    public class CameraController : MonoBehaviour
    {
        private Transform MovePos;//围绕的物体
        private Vector3 Rotion_Transform = new Vector3(0, 0, 0);

        public void SetCharactersPos(Transform pos)
        {
            MovePos = pos;
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
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (Input.GetKey(KeyCode.LeftControl)) wheel *= 5;
            else wheel *= 0.5f;

            transform.Translate(wheel * Vector3.forward);
        }

        public void Cam_Ctrl_Rotate()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            var mouse_x = Input.GetAxis("Mouse X");//获取鼠标X轴移动
            var mouse_y = -Input.GetAxis("Mouse Y");//获取鼠标Y轴移动

            if (Input.GetKey(KeyCode.Mouse0) && MovePos != null)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    MovePos.Translate(Vector3.back * mouse_y * 0.05f);
                }
                else
                {
                    MovePos.Translate(Vector3.down * mouse_y * 0.05f);
                }

                MovePos.Translate(Vector3.left * mouse_x * 0.05f);
            }
            else if (Input.GetKey(KeyCode.Mouse1))
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
