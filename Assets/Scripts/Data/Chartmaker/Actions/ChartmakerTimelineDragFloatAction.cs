using System.Collections;
using System.Collections.Generic;

public class ChartmakerTimelineDragFloatAction: IChartmakerAction
{
    public IList Targets = new List<object>();
    public string Keyword;
    public float Value;

    public string GetName()
    {
        return "Drag " + Chartmaker.GetItemName(Targets);
    }

    public void Undo() 
    {
        System.Reflection.FieldInfo field = Targets[0].GetType().GetField("Offset");
        foreach (object item in Targets)
            field.SetValue(item, (float)field.GetValue(item) - Value);
    }
    public void Redo() 
    {
        System.Reflection.FieldInfo field = Targets[0].GetType().GetField("Offset");
        foreach (object item in Targets)
            field.SetValue(item, (float)field.GetValue(item) + Value);
    }
}
