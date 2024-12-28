using System;
using System.Collections;
using System.Collections.Generic;
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

