using UnityEngine;

public class ChartmakerMoveLaneStepStartAction : ChartmakerMoveAction<LaneStep>
{

    public override string GetName()
    {
        return "Move Lane Step Start";
    }

    public override void Do(Vector3 offset) 
    {
        Item.StartPos += (Vector2)offset;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
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

