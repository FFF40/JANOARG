using UnityEngine;

public class ChartmakerMoveLaneEndAction : ChartmakerMoveAction<Lane>
{

    public override string GetName()
    {
        return "Move Lane End";
    }

    public override void Do(Vector3 offset) 
    {
        foreach (LaneStep step in Item.LaneSteps)
        {
            step.EndPos += (Vector2)offset;
            foreach (Timestamp ts in step.Storyboard.Timestamps)
            {
                if (ts.ID == "EndPos_X")
                {
                    ts.From += offset.x;
                    ts.Target += offset.x;
                }
                if (ts.ID == "EndPos_Y")
                {
                    ts.From += offset.y;
                    ts.Target += offset.y;
                }
            }
        }
    }
}

