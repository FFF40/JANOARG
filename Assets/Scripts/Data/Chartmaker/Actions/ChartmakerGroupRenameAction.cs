public class ChartmakerGroupRenameAction: IChartmakerAction
{
    public string From;
    public string To;

    public string GetName()
    {
        return "Rename Group";
    }

    public void Do(string from, string to) 
    {
        Chart target = Chartmaker.main.CurrentChart;
        foreach (LaneGroup group in target.Groups)
        {
            if (group.Name == from) group.Name = to;
            if (group.Group == from) group.Group = to;
        }
        foreach (Lane lane in target.Lanes)
        {
            if (lane.Group == from) lane.Group = to;
        }
    }


    public void Redo() 
    {
        Do(From, To);
    }
    
    public void Undo() 
    {
        Do(To, From);
    }
}

