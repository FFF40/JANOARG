using UnityEngine;

public class ChartmakerMoveLaneAction : ChartmakerMoveAction<Lane>
{

    public override string GetName()
    {
        return "Move Lane";
    }

    public override void Do(Vector3 offset) 
    {
        Item.Position += offset;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
        {
            if (ts.ID == "Offset_X")
            {
                ts.From += offset.x;
                ts.Target += offset.x;
            }
            if (ts.ID == "Offset_Y")
            {
                ts.From += offset.y;
                ts.Target += offset.y;
            }
            if (ts.ID == "Offset_Z")
            {
                ts.From += offset.z;
                ts.Target += offset.z;
            }
        }
    }
}

