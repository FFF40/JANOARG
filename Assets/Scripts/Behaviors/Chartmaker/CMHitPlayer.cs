using System;
using System.Collections.Generic;
using UnityEngine;

public class CMHitPlayer : MonoBehaviour
{
    public HitObjectManager CurrentHit;
    public MeshRenderer Renderer;
    public MeshRenderer[] IndicatohRenderers;

    public MeshRenderer HoldTail;
    public MeshRenderer FlickEmblem;

    public void OnDestroy()
    {
        if (HoldTail) Destroy(HoldTail.gameObject);
    }

    public void UpdateObjects(HitObjectManager hit) 
    {
        CurrentHit = hit;
        transform.localPosition = hit.Position;
        transform.localRotation = hit.Rotation;

        var styles = PlayerView.main.Manager.PalleteManager.HitStyles;
        int index = hit.CurrentHit.StyleIndex;

        Material material = null;

        if (hit.CurrentHit.Type == HitObject.HitType.Normal)
        {
            Renderer.transform.localScale = new (hit.Length - .5f, .5f, .5f);
            IndicatohRenderers[0].transform.localScale = IndicatohRenderers[1].transform.localScale = new (.25f, .5f, .5f);
            IndicatohRenderers[0].transform.localPosition = new (hit.Length / 2 + .125f, 0, 0);
            IndicatohRenderers[1].transform.localPosition = -IndicatohRenderers[0].transform.localPosition;
            material = index >= 0 && index < styles.Count ? styles[index].NormalMaterial : null;
        } else if (hit.CurrentHit.Type == HitObject.HitType.Catch)
        {
            Renderer.transform.localScale = new (hit.Length, .25f, .25f);
            IndicatohRenderers[0].transform.localScale = IndicatohRenderers[1].transform.localScale = new (.25f, .5f, .5f);
            IndicatohRenderers[0].transform.localPosition = new (hit.Length / 2 + .125f, 0, 0);
            IndicatohRenderers[1].transform.localPosition = -IndicatohRenderers[0].transform.localPosition;
            material = index >= 0 && index < styles.Count ? styles[index].CatchMaterial : null;
        }

        Vector2 camStart = PlayerView.main.MainCamera.WorldToScreenPoint(IndicatohRenderers[0].transform.position);
        Vector2 camEnd = PlayerView.main.MainCamera.WorldToScreenPoint(IndicatohRenderers[1].transform.position);

        if (Renderer.sharedMaterial != material) 
        {
            Renderer.enabled = material;
            Renderer.sharedMaterial = material;
            foreach (MeshRenderer ind in IndicatohRenderers) 
            {
                ind.enabled = Renderer.enabled;
                ind.sharedMaterial = material;
            }
        }

        if (hit.HoldMesh && index >= 0 && index < styles.Count) 
        { 
            if (!HoldTail) {
                HoldTail = Instantiate(PlayerView.main.HoldMeshSample, transform.parent);
            } 
            HoldTail.sharedMaterial = styles[index].HoldTailMaterial;
            HoldTail.GetComponent<MeshFilter>().sharedMesh = hit.HoldMesh;
        }
        else 
        {
            if (HoldTail) {
                Destroy(HoldTail.gameObject);
            } 
        }

        if (hit.CurrentHit.Flickable) 
        { 
            if (!FlickEmblem) {
                FlickEmblem = Instantiate(PlayerView.main.HoldMeshSample, transform);
            } 
            FlickEmblem.sharedMaterial = Renderer.material;
            FlickEmblem.transform.eulerAngles = PlayerView.main.MainCamera.transform.eulerAngles;
            bool directional = float.IsFinite(hit.CurrentHit.FlickDirection);
            FlickEmblem.GetComponent<MeshFilter>().sharedMesh = directional ? PlayerView.main.ArrowFlickIndicator : PlayerView.main.FreeFlickIndicator;
            if (directional) FlickEmblem.transform.Rotate(Vector3.back * hit.CurrentHit.FlickDirection);
            else FlickEmblem.transform.Rotate(Vector3.forward * Vector2.SignedAngle(Vector2.right, camEnd - camStart));
        }
        else 
        {
            if (FlickEmblem) {
                Destroy(FlickEmblem.gameObject);
            } 
        }
    }
}