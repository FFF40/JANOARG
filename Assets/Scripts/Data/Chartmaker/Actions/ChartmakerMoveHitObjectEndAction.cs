using UnityEngine;

public class ChartmakerMoveHitObjectEndAction : ChartmakerMoveAction<HitObject>
{

    public override string GetName()
    {
        return "Move Hit Object End";
    }

    public override void Do(Vector3 offset) 
    {
        Item.Length += offset.x;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
        {
            if (ts.ID == "Length")
            {
                ts.From += offset.x;
                ts.Target += offset.x;
            }
        }
    }
}

