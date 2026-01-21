using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class UIGradient : BaseMeshEffect
{
    public Color colorTop = Color.white;
    public Color colorBottom = Color.black;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        int count = vh.currentVertCount;
        UIVertex vertex = new UIVertex();

        for (int i = 0; i < count; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            // Tính toán màu dựa trên vị trí Y của vertex
            vertex.color = (i == 0 || i == 1) ? colorTop : colorBottom;
            vh.SetUIVertex(vertex, i);
        }
    }
}