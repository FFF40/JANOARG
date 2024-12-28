using System.Collections;
using System.Collections.Generic;

public class ChartmakerTimelineDragBeatPositionAction: IChartmakerAction
{
    public IList Targets = new List<object>();
    public string Keyword;
    public BeatPosition Value;

    public string GetName()
    {
        return "Drag " + Chartmaker.GetItemName(Targets);
    }

    public void Undo() 
    {
        System.Reflection.FieldInfo field = Targets[0].GetType().GetField("Offset");
        foreach (object item in Targets)
            field.SetValue(item, (BeatPosition)field.GetValue(item) - Value);
    }
    public void Redo() 
    {
        System.Reflection.FieldInfo field = Targets[0].GetType().GetField("Offset");
        foreach (object item in Targets)
            field.SetValue(item, (BeatPosition)field.GetValue(item) + Value);
    }
}