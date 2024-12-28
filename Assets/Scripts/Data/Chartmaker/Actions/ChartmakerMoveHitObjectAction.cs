using UnityEngine;

public class ChartmakerMoveHitObjectAction : ChartmakerMoveAction<HitObject>
{

    public override string GetName()
    {
        return "Move Hit Object";
    }

    public override void Do(Vector3 offset) 
    {
        Item.Position += offset.x;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
        {
            if (ts.ID == "Position")
            {
                ts.From += offset.x;
                ts.Target += offset.x;
            }
        }
    }
}

