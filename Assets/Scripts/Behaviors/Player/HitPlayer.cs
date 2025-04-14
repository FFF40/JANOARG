using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPlayer : MonoBehaviour
{
    public HitObject Original;
    public HitObject Current;
    

    public float Time;
    public float EndTime;
    public List<float> HoldTicks;
    public float CurrentPosition;

    public MeshRenderer Center;
    public MeshRenderer Left;
    public MeshRenderer Right;
    
    public MeshFilter HoldMesh;
    public MeshRenderer HoldRenderer;
    
    public MeshFilter FlickMesh;
    public MeshRenderer FlickRenderer;

    public LanePlayer Lane;
    public HitScreenCoord HitCoord;

    public bool IsQueuedHit;
    public bool IsHit;
    public bool IsTapped;

    public void Init()
    {
        if (Current.StyleIndex >= 0 && Current.StyleIndex < PlayerScreen.main.HitStyles.Count)
        {
            HitStyleManager style = PlayerScreen.main.HitStyles[Current.StyleIndex];
            Left.sharedMaterial = Right.sharedMaterial = style.NormalMaterial;
            Center.sharedMaterial = 
                Current.Type == HitObject.HitType.Catch ? style.CatchMaterial : style.NormalMaterial;

            if (HoldRenderer) HoldRenderer.sharedMaterial = style.HoldTailMaterial;

            if (Current.Flickable)
            {
                FlickMesh.gameObject.SetActive(true);
                FlickMesh.sharedMesh = float.IsFinite(Current.FlickDirection) 
                    ? PlayerScreen.main.ArrowFlickIndicator 
                    : PlayerScreen.main.FreeFlickIndicator;
                FlickRenderer.sharedMaterial = Center.sharedMaterial;
            }
        }
        else 
        {
            Center.enabled = Left.enabled = Right.enabled = false;
        }
        UpdateMesh();
    }

    public void UpdateSelf(float time, float beat, bool forceDirty = false)
    {
        if (Current != null) Current.Advance(beat);
        else Current = (HitObject)Original.Get(beat);

        if (Current.IsDirty || forceDirty || IsHit) 
        {
            UpdateMesh();
            Current.IsDirty = false;
        }

        if (FlickMesh.gameObject.activeSelf)
        {
            Quaternion rot = Common.main.MainCamera.transform.rotation;
            float angle = float.IsFinite(Current.FlickDirection) 
                ? Current.FlickDirection 
                : Vector2.SignedAngle(Vector2.right, 
                    Common.main.MainCamera.WorldToScreenPoint(Left.transform.position) - 
                    Common.main.MainCamera.WorldToScreenPoint(Right.transform.position));
            FlickMesh.transform.rotation = rot * Quaternion.Euler(0, 0, angle);
        }
    }

    public void UpdateMesh() 
    {
        float time = Mathf.Max(Time, PlayerScreen.main.CurrentTime + PlayerScreen.main.Settings.VisualOffset);
        float z;
        try { z = CurrentPosition = Lane.GetZPosition(time); }
        catch { return; }

        Lane.GetStartEndPosition(time, out Vector2 start, out Vector2 end);
        transform.localPosition = Vector3.LerpUnclamped(start, end, Current.Position + Current.Length / 2) + Vector3.forward * z;
        transform.localEulerAngles = Vector3.forward * Vector2.SignedAngle(Vector2.right, end - start);


        float width = Vector2.Distance(start, end) * Current.Length; 
        if (Current.Type == HitObject.HitType.Catch)
        {
            float scale = PlayerScreen.main.Settings.HitObjectScale[1];
            Center.transform.localScale = new (width, .2f * scale, .2f * scale);
            Left.transform.localScale = Right.transform.localScale = new Vector3(.2f, .4f, .4f) * scale;
            Right.transform.localPosition = Vector3.right * (width / 2);
            Left.transform.localPosition = -Right.transform.localPosition;
        }
        else 
        {
            float scale = PlayerScreen.main.Settings.HitObjectScale[0];
            Center.transform.localScale = new (width - .2f * scale, .4f * scale, .4f * scale);
            Left.transform.localScale = Right.transform.localScale = new Vector3(.2f, .4f, .4f) * scale;
            Right.transform.localPosition = Vector3.right * (width / 2 + .2f * scale);
            Left.transform.localPosition = -Right.transform.localPosition;
        }
    }
}


