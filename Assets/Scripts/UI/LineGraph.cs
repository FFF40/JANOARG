using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

[ExecuteAlways][RequireComponent(typeof(CanvasRenderer))]
public class LineGraph : MaskableGraphic
{
    private float[] m_Values = new float[64];
    public float[] Values {
        get { return m_Values; }
        set { Array.Copy(value, m_Values, 64); SetAllDirty(); }
    }

    protected override void Start() 
    {
        base.Start();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        if (base.gameObject.activeInHierarchy) SetMaterialDirty();
        base.OnRectTransformDimensionsChange();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Vector2 min = rectTransform.rect.min;
        Vector2 max = rectTransform.rect.max;
        float height = max.x - min.x;

        float minValue = Mathf.Min(m_Values) - 1 / height;
        float maxValue = Mathf.Max(m_Values) + 1 / height;


        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;
        vert.position = new Vector3(max.x, Mathf.LerpUnclamped(min.y, max.y, minValue));
        vert.uv0 = new Vector2(1, 0);
        vh.AddVert(vert);
        vert.position = new Vector3(max.x, Mathf.LerpUnclamped(min.y, max.y, maxValue));
        vert.uv0 = new Vector2(1, 1);
        vh.AddVert(vert);
        vert.position = new Vector3(min.x, vert.position.y);
        vert.uv0 = new Vector2(0, 1);
        vh.AddVert(vert);
        vert.position = new Vector3(min.x, Mathf.LerpUnclamped(min.y, max.y, minValue));
        vert.uv0 = new Vector2(0, 0);
        vh.AddVert(vert);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);
    }

    Material materialCache, actualMaterial;

    protected override void UpdateMaterial()
    {
        if (IsActive())
        {
            Vector2 min = rectTransform.rect.min;
            Vector2 max = rectTransform.rect.max;
            float height = max.x - min.x;

            float minValue = Mathf.Min(m_Values) - 1 / height;
            float maxValue = Mathf.Max(m_Values) + 1 / height;
        
            float[] realValues = new float[Values.Length];
            for (int i = 0; i < realValues.Length; i++) realValues[i] = Mathf.InverseLerp(minValue, maxValue, m_Values[i]);

            Material mat = materialForRendering;
            if (materialCache != mat)
            {
                DestroyImmediate(actualMaterial);
                actualMaterial = Instantiate(mat);
                materialCache = mat;
            }
            actualMaterial.SetFloatArray("_Values", realValues);
            actualMaterial.SetVector("_Resolution", rectTransform.rect.size * new Vector2(1, maxValue - minValue));

            canvasRenderer.materialCount = 1;
            canvasRenderer.SetMaterial(actualMaterial, 0);
            canvasRenderer.SetTexture(mainTexture);
        }
    }

    protected override void OnDestroy() 
    {
        DestroyImmediate(actualMaterial);
        base.OnDestroy();
    }
}
