using UnityEngine;

public class ChartmakerMoveLaneStepEndAction : ChartmakerMoveAction<LaneStep>
{

    public override string GetName()
    {
        return "Move Lane Step End";
    }

    public override void Do(Vector3 offset) 
    {
        Item.EndPos += (Vector2)offset;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
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

