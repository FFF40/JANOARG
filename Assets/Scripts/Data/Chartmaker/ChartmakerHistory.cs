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

    public void AddAction(IChartmakerAction action)
    {
        ActionsBehind.Push(action);
        ActionsAhead.Clear();
    }

    public void SetItem(object target, string field, object value) 
    {
        FieldInfo fieldInfo = target.GetType().GetField(field);
        object oldValue = fieldInfo?.GetValue(target);
        if (oldValue?.Equals(value) ?? value?.Equals(oldValue) ?? true) return;

        fieldInfo.SetValue(target, value);
        
        if (ActionsBehind.Count > 0 &&
            ActionsBehind.Peek() is ChartmakerModifyAction modAction && 
            modAction.Item == target && modAction.Keyword == field
        ){
            modAction.To = value;
        }
        else 
        {
            ChartmakerModifyAction action = new()
            {
                Item = target,
                Keyword = field,
                From = oldValue,
                To = value,
            };
            ActionsBehind.Push(action);
        }
        ActionsAhead.Clear();
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
        Chartmaker.SortList(Target);
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
            if (ts.ID == "StartPos_X" || ts.ID == "EndPos_X")
            {
                ts.From += offset.x;
                ts.Target += offset.x;
            }
            if (ts.ID == "StartPos_Y" || ts.ID == "EndPos_Y")
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

public class ChartmakerMoveHitObjectAction : ChartmakerMoveAction<HitObject>
{

    public override string GetName()
    {
        return "Move Hit Object";
    }

    public override void Do(Vector3 offset) 
    {
        Item.Position += offset.x;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
        {
            if (ts.ID == "Position")
            {
                ts.From += offset.x;
                ts.Target += offset.x;
            }
        }
    }
}

public class ChartmakerMoveHitObjectStartAction : ChartmakerMoveAction<HitObject>
{

    public override string GetName()
    {
        return "Move Hit Object Start";
    }

    public override void Do(Vector3 offset) 
    {
        Item.Position += offset.x;
        Item.Length -= offset.x;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
        {
            if (ts.ID == "Position")
            {
                ts.From += offset.x;
                ts.Target += offset.x;
            }
            else if (ts.ID == "Length")
            {
                ts.From -= offset.x;
                ts.Target -= offset.x;
            }
        }
    }
}

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