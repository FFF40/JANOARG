using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneGroupPlayer : MonoBehaviour
{
    public LaneGroup Original; 
    public LaneGroup Current;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateSelf(float time, float beat)
    {
        if (Current != null) Current.Advance(beat);
        else Current = (LaneGroup)Original.Get(beat);

        transform.localPosition = Current.Position;
        transform.localEulerAngles = Current.Rotation;
    }
}
