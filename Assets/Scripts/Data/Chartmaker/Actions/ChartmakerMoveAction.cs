using UnityEngine;

public class ChartmakerMoveAction<T> : IChartmakerAction 
{
    public T Item;
    public Vector3 Offset;

    public virtual string GetName() { return ""; }
    public virtual void Do(Vector3 offset) {}

    public void Redo() 
    {
        Do(Offset);
    }
    
    public void Undo() 
    {
        Do(-Offset);
    }

}

