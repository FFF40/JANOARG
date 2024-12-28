public class ChartmakerModifyAction : IChartmakerAction 
{
    public object Item;
    public string Keyword;
    public object From;
    public object To;

    public string GetName()
    {
        return "Set " + Chartmaker.GetItemName(Item) + " " + Keyword;
    }

    public void Undo() 
    {
        Item.GetType().GetField(Keyword).SetValue(Item, From);
    }
    public void Redo() 
    {
        Item.GetType().GetField(Keyword).SetValue(Item, To);
    }
}

