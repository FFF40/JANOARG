using System.Collections;
using System.Collections.Generic;

public class ChartmakerRearrangeLaneAction: IChartmakerAction
{
    public Lane Target;

    public Lane BeforeAdjacent;
    public string BeforeGroup;
    public Lane AfterAdjacent;
    public string AfterGroup;

    public string GetName()
    {
        return "Rearrange Lane";
    }

    public void Do(Lane adjacent, string group) 
    {
        List<Lane> list = Chartmaker.main.CurrentChart.Lanes;
        Target.Group = group;
        list.Remove(Target);
        list.Insert(list.IndexOf(adjacent) + 1, Target);
    }

    public void Redo()
    {
        Do(AfterAdjacent, AfterGroup);
    }

    public void Undo()
    {
        Do(BeforeAdjacent, BeforeGroup);
    }
}