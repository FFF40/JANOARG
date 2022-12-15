using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class CharterHistory {
    public List<ICharterAction> ActionsBehind = new List<ICharterAction>();
    public List<ICharterAction> ActionsAhead = new List<ICharterAction>();

    public void Undo(int count = 1)
    {
        if (ActionsBehind.Count <= 0) return;
        ActionsBehind[ActionsBehind.Count - 1].Undo();
        ActionsAhead.Add(ActionsBehind[ActionsBehind.Count - 1]);
        ActionsBehind.RemoveAt(ActionsBehind.Count - 1);
        count--;
        if (count > 0) Undo(count - 1);
    }
    public void Redo(int count = 1)
    {
        if (ActionsAhead.Count <= 0) return;
        ActionsAhead[ActionsAhead.Count - 1].Redo();
        ActionsBehind.Add(ActionsAhead[ActionsAhead.Count - 1]);
        ActionsAhead.RemoveAt(ActionsAhead.Count - 1);
        count--;
        if (count > 0) Redo(count - 1);
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
                int count = ActionsBehind.Count - 1;
                if (ActionsBehind.Count > 0 &&
                    ActionsBehind[count] is CharterModifyAction && 
                    ((CharterModifyAction)ActionsBehind[count]).Item == item &&
                    ((CharterModifyAction)ActionsBehind[count]).Keyword == field.Name)
                {
                    ((CharterModifyAction)ActionsBehind[count]).To = field.GetValue(item);
                }
                else {
                    CharterModifyAction action = new CharterModifyAction()
                    {
                        Item = item,
                        Keyword = field.Name,
                        From = field.GetValue(RecordingItem),
                        To = field.GetValue(item),
                    };
                    ActionsBehind.Add(action);
                }
                ActionsAhead.Clear();
            }
        }

        RecordingItem = null;
    }
}

public interface ICharterAction {
    public string GetName();
    public void Undo();
    public void Redo();
}

public class CharterAddAction : ICharterAction 
{
    public IList Target;
    public object Item;

    public string GetName()
    {
        return "Add " + Charter.GetItemName(Item);
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

public class CharterDeleteAction : ICharterAction 
{
    public IList Target;
    public object Item;

    public string GetName()
    {
        return "Delete " + Charter.GetItemName(Item);
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

public class CharterModifyAction : ICharterAction 
{
    public object Item;
    public string Keyword;
    public object From;
    public object To;

    public string GetName()
    {
        return "Set " + Charter.GetItemName(Item) + " " + Keyword;
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

public class CharterMoveLaneAction : ICharterAction 
{
    public Lane Item;
    public Vector3 Offset;

    public string GetName()
    {
        return "Move Lane";
    }

    public void Undo() 
    {
        Item.Offset -= Offset;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
        {
            if (ts.ID == "Position_X")
            {
                ts.From -= Offset.x;
                ts.Target -= Offset.x;
            }
            if (ts.ID == "Position_Y")
            {
                ts.From -= Offset.y;
                ts.Target -= Offset.y;
            }
            if (ts.ID == "Position_Z")
            {
                ts.From -= Offset.z;
                ts.Target -= Offset.z;
            }
        }
    }
    public void Redo() 
    {
        Item.Offset += Offset;
        foreach (Timestamp ts in Item.Storyboard.Timestamps)
        {
            if (ts.ID == "Position_X")
            {
                ts.From += Offset.x;
                ts.Target += Offset.x;
            }
            if (ts.ID == "Position_Y")
            {
                ts.From += Offset.y;
                ts.Target += Offset.y;
            }
            if (ts.ID == "Position_Z")
            {
                ts.From += Offset.z;
                ts.Target += Offset.z;
            }
        }
    }
}