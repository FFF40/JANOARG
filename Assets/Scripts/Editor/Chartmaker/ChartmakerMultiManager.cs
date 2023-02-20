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

    public ChartmakerMultiHandler Handler;

    public Dictionary<Type, ChartmakerMultiHandler> Handlers = new Dictionary<Type, ChartmakerMultiHandler>();

    public ChartmakerMultiManager(Type type)
    {
        AvailableFields = new List<FieldInfo>();

        foreach (FieldInfo field in type.GetFields()) 
        {
            if (typeof(IEnumerable).IsAssignableFrom(field.FieldType)
                || typeof(Storyboard) == field.FieldType
                || field.IsStatic || field.IsLiteral || !field.IsPublic) 
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

        if (currentField.FieldType != Handler?.TargetType)
        {
            if (currentField.FieldType ==  typeof(bool)) 
            {
                Handler = Handlers.ContainsKey(currentField.FieldType) ? Handlers[currentField.FieldType] : new ChartmakerMultiHandlerBoolean();
            }
            else if (currentField.FieldType == typeof(float)) 
            {
                ChartmakerMultiHandlerFloat handler = Handlers.ContainsKey(currentField.FieldType)
                    ? Handlers[currentField.FieldType] as ChartmakerMultiHandlerFloat 
                    : new ChartmakerMultiHandlerFloat();
                handler.SetLerp(Chartmaker.current.TargetThing as IList);
                Handler = handler;
            }
            else 
            {
                Handler = Handlers.ContainsKey(currentField.FieldType) 
                    ? Handlers[currentField.FieldType] 
                    : Activator.CreateInstance(typeof(ChartmakerMultiHandler<>).MakeGenericType(currentField.FieldType)) as ChartmakerMultiHandler;
            }
        }
        Handlers[currentField.FieldType] = Handler;
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
                From = currentField.GetValue(obj),
            };
            item.To = Handler.Get(item.From, obj);
            action.Targets.Add(item);
        }
        action.Redo();
        history.ActionsBehind.Add(action);
        history.ActionsAhead.Clear();
    }
}

public class ChartmakerMultiHandler
{
    public object To;
    
    public virtual object Get(object from, object src) {
        return To;
    }

    public virtual Type TargetType { get; }

}

public class ChartmakerMultiHandler<T>: ChartmakerMultiHandler
{
    
    public override object Get(object from, object src) {
        return Get((T)from, src);
    }
    
    public virtual T Get(T from, object src) {
        return (T)To;
    }
    
    public override Type TargetType { get { return typeof(T); } }
}

public class ChartmakerMultiHandlerBoolean: ChartmakerMultiHandler<bool>
{
    public new bool? To;
    
    public override bool Get(bool from, object src) {
        return To == null ? !from : (bool)To;
    }
}

public class ChartmakerMultiHandlerFloat: ChartmakerMultiHandler<float>
{
    public float From;
    public new float To;

    public FloatOperation Operation;

    public string LerpSource = "Offset";
    FieldInfo LerpField;
    public string LerpEasing = "Linear";
    public EaseMode LerpEaseMode = EaseMode.In;

    public float LerpFrom;
    public float LerpTo;

    public void SetLerp(IList list)
    {
        LerpFrom = float.PositiveInfinity;
        LerpTo = float.NegativeInfinity;
        LerpField = list.GetType().GetGenericArguments()[0].GetField(LerpSource);
        foreach (object item in list)
        {
            float value = (float)LerpField.GetValue(item);
            LerpFrom = Mathf.Min(LerpFrom, value);
            LerpTo = Mathf.Max(LerpTo, value);
        }
    }

    public override float Get(float from, object src) {
        float to = Mathf.InverseLerp(LerpFrom, LerpTo, (float)LerpField.GetValue(src));
        to = Mathf.Lerp(From, To, Ease.Get(to, LerpEasing, LerpEaseMode));
        return FloatOperations[Operation](from, to);
    }

    public enum FloatOperation {
        Set, Add, Multiply, Min, Max
    }

    public Dictionary<FloatOperation, Func<float, float, float>> FloatOperations = new Dictionary<FloatOperation, Func<float, float, float>> {
        { FloatOperation.Set,      (from, to) => to },
        { FloatOperation.Add,      (from, to) => from + to },
        { FloatOperation.Multiply, (from, to) => from * to },
        { FloatOperation.Min,      (from, to) => Mathf.Min(from, to) },
        { FloatOperation.Max,      (from, to) => Mathf.Max(from, to) },
    };
}