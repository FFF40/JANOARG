using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneGroupPlayer : MonoBehaviour
{
    public LaneGroup Original; 
    public LaneGroup Current;

    public LaneGroupPlayer Parent;
    
    public void UpdateSelf(float time, float beat)
    {
        if (Current != null) 
            Current.Advance(beat);
        else 
            Current = (LaneGroup)Original.GetStoryboardableObject(beat);
        
        transform.localPosition    = Current.Position;
        transform.localEulerAngles = Current.Rotation;
    }
}
