using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways][RequireComponent(typeof(CanvasRenderer))]
public class GraphicCircle : MaskableGraphic
{
    public int Resolution = 90;
    [Range(0, 1)]
    public float FillAmount = 1;
    [Range(0, 1)]
    public float InsideRadius = 0;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        Vector2 center = rectTransform.rect.center;
        Vector2 radius = new Vector2(rectTransform.rect.width / 2, rectTransform.rect.height / 2);

        vh.Clear();

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        if (Resolution > 1000) Resolution = 1000;

        if (InsideRadius == 0)
        {
            vert.position = center;
            vh.AddVert(vert);

            for (int a = 0; a <= Resolution; a++) 
            {
                float angle = Mathf.Min((float)a / Resolution, FillAmount) * Mathf.PI * 2;
                vert.position = new Vector2(Mathf.Sin(angle) * radius.x, Mathf.Cos(angle) * radius.y) + center;
                vh.AddVert(vert);

                if (a > 0) vh.AddTriangle(a, a + 1, 0);

                if (a >= FillAmount * Resolution) break;
            }
        }
        else
        {
            for (int a = 0; a <= Resolution; a++) 
            {
                float angle = Mathf.Min((float)a / Resolution, FillAmount) * Mathf.PI * 2;
                
                vert.position = new Vector2(Mathf.Sin(angle) * radius.x * InsideRadius, Mathf.Cos(angle) * radius.y * InsideRadius) + center;
                vh.AddVert(vert);
                vert.position = new Vector2(Mathf.Sin(angle) * radius.x, Mathf.Cos(angle) * radius.y) + center;
                vh.AddVert(vert);

                if (a > 0) 
                {
                    vh.AddTriangle(a * 2 - 1, a * 2 + 1, a * 2);
                    vh.AddTriangle(a * 2 - 1, a * 2, a * 2 - 2);
                }

                if (a >= FillAmount * Resolution) break;
            }
        }
    }
}
