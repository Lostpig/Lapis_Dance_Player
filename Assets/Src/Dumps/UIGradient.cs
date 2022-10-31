using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIGradient : UIBehaviour, IMeshModifier
{
    public Gradient Gradient; // 0x18

    public void ModifyMesh(Mesh mesh) { }
    public void ModifyMesh(VertexHelper helper)
    {
        if (!IsActive() || helper.currentVertCount == 0)
        {
            return;
        }

        List<UIVertex> _vertexList = new();
        helper.GetUIVertexStream(_vertexList);

        float left = _vertexList[0].position.x;
        float right = _vertexList[0].position.x;
        float x = 0f;
        int nCount = _vertexList.Count;

        for (int i = nCount - 1; i >= 1; --i)
        {
            x = _vertexList[i].position.x;
            if (x > right) right = x;
            else if (x < left) left = x;
        }
        float width = 1f / (right - left);
        UIVertex vertex = new UIVertex();

        for (int i = 0; i < helper.currentVertCount; i++)
        {
            helper.PopulateUIVertex(ref vertex, i);
            vertex.color = Gradient.Evaluate((vertex.position.x - left) * width);
            helper.SetUIVertex(vertex, i);
        }
    }
}