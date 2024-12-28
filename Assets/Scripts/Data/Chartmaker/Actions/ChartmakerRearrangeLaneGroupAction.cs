using System.Collections;
using System.Collections.Generic;

public class ChartmakerRearrangeLaneGroupAction: IChartmakerAction
{
    public LaneGroup Target;

    public LaneGroup BeforeAdjacent;
    public string BeforeGroup;
    public LaneGroup AfterAdjacent;
    public string AfterGroup;

    public string GetName()
    {
        return "Rearrange Lane Group";
    }

    public void Do(LaneGroup adjacent, string group) 
    {
        List<LaneGroup> list = Chartmaker.main.CurrentChart.Groups;
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