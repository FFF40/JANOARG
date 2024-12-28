using UnityEngine;

public class ChartmakerMoveLaneStartAction : ChartmakerMoveAction<Lane>
{

    public override string GetName()
    {
        return "Move Lane Start";
    }

    public override void Do(Vector3 offset) 
    {
        foreach (LaneStep step in Item.LaneSteps)
        {
            step.StartPos += (Vector2)offset;
            foreach (Timestamp ts in step.Storyboard.Timestamps)
            {
                if (ts.ID == "StartPos_X")
                {
                    ts.From += offset.x;
                    ts.Target += offset.x;
                }
                if (ts.ID == "StartPos_Y")
                {
                    ts.From += offset.y;
                    ts.Target += offset.y;
                }
            }
        }
    }
}

