using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LapisPlayer
{
    public class HeightControlBehaviour : MonoBehaviour
    {
        Transform _target;

        public void Initialize(Transform target)
        {
            _target = target;
        }
        private void Update()
        {
            var originPos = _target.position;
            _target.position = new Vector3(originPos.x, transform.position.y, originPos.z);
        }
    }
}
