using System.Collections;

public class ChartmakerDeleteAction : IChartmakerAction 
{
    public IList Target;
    public object Item;

    public string GetName()
    {
        return "Delete " + Chartmaker.GetItemName(Item);
    }

    public void Undo() 
    {
        if (Item is IList)
        {
            foreach (object i in (IList)Item)
            {
                Target.Add(i);
            }
        }
        else 
        {
            Target.Add(Item);
        }
        Chartmaker.SortList(Target);
    }
    public void Redo() 
    {
        if (Item is IList)
        {
            foreach (object i in (IList)Item)
            {
                Target.Remove(i);
            }
        }
        else 
        {
            Target.Remove(Item);
        }
    }
}

