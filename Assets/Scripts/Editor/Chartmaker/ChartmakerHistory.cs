using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class ChartmakerHistory {
    public Stack<IChartmakerAction> ActionsBehind = new Stack<IChartmakerAction>();
    public Stack<IChartmakerAction> ActionsAhead = new Stack<IChartmakerAction>();

    public void Undo(int count = 1)
    {
        while (count > 0)
        {
            if (ActionsBehind.Count <= 0) return;
            ActionsBehind.Peek().Undo();
            ActionsAhead.Push(ActionsBehind.Peek());
            ActionsBehind.Pop();
            count--;
        }
    }
    public void Redo(int count = 1)
    {
        while (count > 0)
        {
            if (ActionsAhead.Count <= 0) return;
            ActionsAhead.Peek().Redo();
            ActionsBehind.Push(ActionsAhead.Peek());
            ActionsAhead.Pop();
            count--;   
        }
    }

    object RecordingItem;

    static string[] IgnoreClasses = {
        "PlayableSong", "Chart", "Pallete", "LaneStyle", "HitStyle", 
        "Lane", "LaneStep", "HitObject", "Storyboard",
    };

    public void StartRecordItem(object item) 
    {
        if (RecordingItem != null) EndRecordItem(item);
        RecordingItem = item;
        MethodInfo deepClone = item.GetType().GetMethod("DeepClone");
        if (deepClone != null) RecordingItem = deepClone.Invoke(item, null);
    }
    public void EndRecordItem(object item) 
    {
        if (RecordingItem == null || item == null || item.GetType() != RecordingItem.GetType()) return;
        FieldInfo[] fields = RecordingItem.GetType().GetFields();

        foreach (FieldInfo field in fields)
        {
            string name = field.FieldType.Name;
            if (field.FieldType.GetInterface("IEnumerable") == null && 
                Array.IndexOf(IgnoreClasses, name) < 0 && 
                (((field.GetValue(item) == null) ^ (field.GetValue(RecordingItem) == null)) ||
                (field.GetValue(item) != null && !field.GetValue(item).Equals(field.GetValue(RecordingItem)))))
            {
                if (ActionsBehind.Count > 0 &&
                    ActionsBehind.Peek() is ChartmakerModifyAction && 
                    ((ChartmakerModifyAction)ActionsBehind.Peek()).Item == item &&
                    ((ChartmakerModifyAction)ActionsBehind.Peek()).Keyword == field.Name)
                {
                    ((ChartmakerModifyAction)ActionsBehind.Peek()).To = field.GetValue(item);
                }
                else {
                    ChartmakerModifyAction action = new ChartmakerModifyAction()
                    {
                        Item = item,
                        Keyword = field.Name,
                        From = field.GetValue(RecordingItem),
                        To = field.GetValue(item),
                    };
                    ActionsBehind.Push(action);
                }
                ActionsAhead.Clear();
            }
        }

        RecordingItem = null;
    }
}

public interface IChartmakerAction {
    public string GetName();
    public void Undo();
    public void Redo();
}

public class ChartmakerAddAction : IChartmakerAction 
{
    public IList Target;
    public object Item;

    public string GetName()
    {
        return "Add " + Chartmaker.GetItemName(Item);
    }

    public void Undo() 
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
    public void Redo() 
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
    }
}

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

public class ChartmakerMoveLaneAction : ChartmakerMoveAction<Lane>
{

    public override string GetName()
    {
        return "Move Lane";
    }

    public override void Do(Vector3 offset) 
    {
        Item.Position += offset;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
        {
            if (ts.ID == "Offset_X")
            {
                ts.From += offset.x;
                ts.Target += offset.x;
            }
            if (ts.ID == "Offset_Y")
            {
                ts.From += offset.y;
                ts.Target += offset.y;
            }
            if (ts.ID == "Offset_Z")
            {
                ts.From += offset.z;
                ts.Target += offset.z;
            }
        }
    }
}

public class ChartmakerMoveLaneStartAction : ChartmakerMoveAction<Lane>
{

    public override string GetName()
    {
        return "Move Lane Start";
    }

    public override void Do(Vector3 offset) 
    {
        foreach (LaneStep step in Item.LaneSteps)
        {
            step.StartPos += (Vector2)offset;
            foreach (Timestamp ts in step.Storyboard.Timestamps)
            {
                if (ts.ID == "StartPos_X")
                {
                    ts.From += offset.x;
                    ts.Target += offset.x;
                }
                if (ts.ID == "StartPos_Y")
                {
                    ts.From += offset.y;
                    ts.Target += offset.y;
                }
            }
        }
    }
}

public class ChartmakerMoveLaneEndAction : ChartmakerMoveAction<Lane>
{

    public override string GetName()
    {
        return "Move Lane End";
    }

    public override void Do(Vector3 offset) 
    {
        foreach (LaneStep step in Item.LaneSteps)
        {
            step.EndPos += (Vector2)offset;
            foreach (Timestamp ts in step.Storyboard.Timestamps)
            {
                if (ts.ID == "EndPos_X")
                {
                    ts.From += offset.x;
                    ts.Target += offset.x;
                }
                if (ts.ID == "EndPos_Y")
                {
                    ts.From += offset.y;
                    ts.Target += offset.y;
                }
            }
        }
    }
}

public class ChartmakerMoveLaneStepAction : ChartmakerMoveAction<LaneStep>
{

    public override string GetName()
    {
        return "Move Lane Step";
    }

    public override void Do(Vector3 offset) 
    {
        Item.StartPos += (Vector2)offset;
        Item.EndPos += (Vector2)offset;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
        {
            if (ts.ID == "StartPos_X" && ts.ID == "EndPos_X")
            {
                ts.From += offset.x;
                ts.Target += offset.x;
            }
            if (ts.ID == "StartPos_Y" && ts.ID == "EndPos_Y")
            {
                ts.From += offset.y;
                ts.Target += offset.y;
            }
        }
    }
}

public class ChartmakerMoveLaneStepStartAction : ChartmakerMoveAction<LaneStep>
{

    public override string GetName()
    {
        return "Move Lane Step Start";
    }

    public override void Do(Vector3 offset) 
    {
        Item.StartPos += (Vector2)offset;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
        {
            if (ts.ID == "StartPos_X")
            {
                ts.From += offset.x;
                ts.Target += offset.x;
            }
            if (ts.ID == "StartPos_Y")
            {
                ts.From += offset.y;
                ts.Target += offset.y;
            }
        }
    }
}

public class ChartmakerMoveLaneStepEndAction : ChartmakerMoveAction<LaneStep>
{

    public override string GetName()
    {
        return "Move Lane Step End";
    }

    public override void Do(Vector3 offset) 
    {
        Item.EndPos += (Vector2)offset;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
        {
            if (ts.ID == "EndPos_X")
            {
                ts.From += offset.x;
                ts.Target += offset.x;
            }
            if (ts.ID == "EndPos_Y")
            {
                ts.From += offset.y;
                ts.Target += offset.y;
            }
        }
    }
}

public class ChartmakerGroupRenameAction: IChartmakerAction
{
    public Chart Target;
    public string From;
    public string To;

    public string GetName()
    {
        return "Rename Group";
    }

    public void Do(string from, string to) 
    {
        foreach (LaneGroup group in Target.Groups)
        {
            if (group.Name == from) group.Name = to;
            if (group.Group == from) group.Group = to;
        }
        foreach (Lane lane in Target.Lanes)
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

public class ChartmakerMultiEditActionItem
{
    public object Target;
    public object From;
    public object To;
}

public class ChartmakerMultiEditAction: IChartmakerAction
{
    public List<ChartmakerMultiEditActionItem> Targets = new List<ChartmakerMultiEditActionItem>();
    public string Keyword;

    public string GetName()
    {
        return "Multi Edit " + Chartmaker.GetItemName(Targets[0].Target) + " " + Keyword;
    }

    public void Undo() 
    {
        foreach (ChartmakerMultiEditActionItem item in Targets)
            item.Target.GetType().GetField(Keyword).SetValue(item.Target, item.From);
    }
    public void Redo() 
    {
        foreach (ChartmakerMultiEditActionItem item in Targets)
            item.Target.GetType().GetField(Keyword).SetValue(item.Target, item.To);
    }
}
