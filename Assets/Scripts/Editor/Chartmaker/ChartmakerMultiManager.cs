using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class ChartmakerMultiManager
{
    public Type target;

    public List<FieldInfo> AvailableFields;

    public int CurrentFieldIndex;

    public IChartmakerMultiHandler Handler;

    public ChartmakerMultiManager(Type type)
    {
        AvailableFields = new List<FieldInfo>();

        foreach (FieldInfo field in type.GetFields()) 
        {
            if (typeof(IEnumerable).IsAssignableFrom(field.FieldType)
                || typeof(Storyboard) == field.FieldType) 
            {
                continue;
            }
            AvailableFields.Add(field);
        }

        target = type;
        SetTarget(0);
    }

    public void SetTarget(int target)
    {
        CurrentFieldIndex = target;

        FieldInfo currentField = AvailableFields[target];

        if (currentField.FieldType == typeof(bool)) Handler = new ChartmakerMultiHandlerBoolean();
        else Handler = Activator.CreateInstance(typeof(ChartmakerMultiHandler<>).MakeGenericType(currentField.FieldType)) as IChartmakerMultiHandler;
    }

    public void Execute(IList items, ChartmakerHistory history) {
        FieldInfo currentField = AvailableFields[CurrentFieldIndex];

        ChartmakerMultiEditAction action = new ChartmakerMultiEditAction() 
        { 
            Keyword = currentField.Name 
        };

        foreach(object obj in items) {
            ChartmakerMultiEditActionItem item = new ChartmakerMultiEditActionItem
            {
                Target = obj,
                From = obj.GetType().GetField(currentField.Name).GetValue(obj),
            };
            item.To = Handler.Get(item.From);
            action.Targets.Add(item);
        }

        action.Redo();
        history.ActionsBehind.Add(action);
        history.ActionsAhead.Clear();
    }
}

public interface IChartmakerMultiHandler
{
    public object Get(object from) {
        throw new NotSupportedException();
    }
}

public class ChartmakerMultiHandler<T>: IChartmakerMultiHandler
{
    public T To;
    
    public object Get(object from) {
        return Get((T)from);
    }
    public virtual T Get(T from) {
        return To;
    }
}

public class ChartmakerMultiHandlerBoolean: ChartmakerMultiHandler<bool>
{
    public new bool? To;
    
    public override bool Get(bool from) {
        return To == null ? !from : (bool)To;
    }
}