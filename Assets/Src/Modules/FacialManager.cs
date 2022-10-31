using System;
using UnityEngine;
using System.Collections.Generic;

namespace LapisPlayer
{
    public class BlendHandle
    {
        private SkinnedMeshRenderer _mesh;
        private int _shapeIndex;

        public BlendHandle(SkinnedMeshRenderer mesh, int shapeIndex)
        {
            _mesh = mesh;
            _shapeIndex = shapeIndex;
        }

        public void setValue(float value)
        {
            _mesh.SetBlendShapeWeight(_shapeIndex, value);
        }
        public float getValue()
        {
            return _mesh.GetBlendShapeWeight(_shapeIndex);
        }
    }

    public class FacialManager
    {
        private FacialSettings _settings;

        private SkinnedMeshRenderer _faceMesh;
        private SkinnedMeshRenderer _browMesh;
        private SkinnedMeshRenderer _eyeMesh;
        private Dictionary<string, BlendHandle> _blends;

        public FacialManager(FacialSettings settings, GameObject face)
        {
            _blends = new();

            var faceGEO = Utility.FindNodeByRecursion(face, "face_GEO");
            _faceMesh = faceGEO.GetComponent<SkinnedMeshRenderer>();
            var eyeGeo = Utility.FindNodeByRecursion(face, "eye_GEO");
            _eyeMesh = eyeGeo.GetComponent<SkinnedMeshRenderer>();
            var browGEO = Utility.FindNodeByRecursion(face, "brow_GEO");
            _browMesh = browGEO.GetComponent<SkinnedMeshRenderer>();

            InitBlends(_faceMesh);
            InitBlends(_eyeMesh);
            InitBlends(_browMesh);
        }
        private void InitBlends(SkinnedMeshRenderer mesh)
        {
            for (var i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
            {
                string name = mesh.sharedMesh.GetBlendShapeName(i);
                _blends.Add(name, new BlendHandle(mesh, i));
            }
        }

        public void UpdateBlink(float val)
        {
            // 没找到控制眨眼的相关配置
            // 看了一下模型,固定都是用eyeE_GEO来控制闭眼的,就写死了
            _blends["face_blendshape.eyeE_GEO"].setValue(val);
        }
    }
}
