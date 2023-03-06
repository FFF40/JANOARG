using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneGroupPlayer : MonoBehaviour
{
    public LaneGroup CurrentGroup;
    public LaneGroupPlayer ParentGroup;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetGroup (LaneGroup group) 
    {
        foreach (Timestamp ts in group.Storyboard.Timestamps)
        {
            ts.Duration = ChartPlayer.main.Song.Timing.ToSeconds(ts.Time + ts.Duration);
            ts.Time = ChartPlayer.main.Song.Timing.ToSeconds(ts.Time);
            ts.Duration -= ts.Time;
        }

        CurrentGroup = group;
    }

    // Update is called once per frame
    void Update()
    {
        if (ChartPlayer.main.IsPlaying)
        {
            CurrentGroup.Advance(ChartPlayer.main.CurrentTime);

            transform.localPosition = CurrentGroup.Position;
            transform.localEulerAngles = CurrentGroup.Rotation;
        }
    }
}
