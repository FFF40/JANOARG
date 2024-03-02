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

    public LanePlayer Lane;

    public bool IsHit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        if (Current.StyleIndex >= 0 && Current.StyleIndex < PlayerScreen.main.HitStyles.Count)
        {
            HitStyleManager style = PlayerScreen.main.HitStyles[Current.StyleIndex];
            Center.sharedMaterial = Left.sharedMaterial = Right.sharedMaterial =
                Current.Type == HitObject.HitType.Catch ? style.CatchMaterial : style.NormalMaterial;
            if (HoldRenderer) HoldRenderer.sharedMaterial = style.HoldTailMaterial;
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
    }

    public void UpdateMesh() 
    {
        float time = Mathf.Max(Time, PlayerScreen.main.CurrentTime);
        float z;
        try { z = CurrentPosition = Lane.GetZPosition(time); }
        catch { return; }

        Lane.GetStartEndPosition(time, out Vector2 start, out Vector2 end);
        transform.localPosition = Vector3.LerpUnclamped(start, end, Current.Position + Current.Length / 2) + Vector3.forward * z;
        transform.localEulerAngles = Vector3.forward * Vector2.SignedAngle(Vector2.right, end - start);

        float width = Vector2.Distance(start, end) * Current.Length; 
        if (Current.Type == HitObject.HitType.Catch)
        {
            Center.transform.localScale = new (width, .1f, .1f);
            Left.transform.localScale = Right.transform.localScale = new (.1f, .2f, .2f);
            Right.transform.localPosition = Vector3.right * (width / 2);
            Left.transform.localPosition = -Right.transform.localPosition;
        }
        else 
        {
            Center.transform.localScale = new (width - .2f, .4f, .4f);
            Left.transform.localScale = Right.transform.localScale = new (.2f, .4f, .4f);
            Right.transform.localPosition = Vector3.right * (width / 2 + .2f);
            Left.transform.localPosition = -Right.transform.localPosition;
        }
    }
}
