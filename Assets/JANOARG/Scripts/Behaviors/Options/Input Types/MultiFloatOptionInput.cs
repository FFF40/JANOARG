using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MultiFloatOptionInput : OptionInput<float[]>
{
    [Space]
    public float Min = 0;
    public float Max = 100;
    public float Step = 1;
    public string Unit;
    public MultiValueType ValueType;
    [Space]
    public TMP_Text ValueHolderSample;
    public TMP_Text UnitLabelSample;
    public List<TMP_Text> ValueHolders;
    public List<TMP_Text> UnitLabels;

    Color firstFieldColor;
    Color standardColor;

    public new void Start() 
    {
        var fields = MultiValueFieldData.Info[ValueType];
        int index = 0;

        standardColor = ValueHolderSample.color;
        firstFieldColor = fields[0].Color;

        foreach (var field in fields) {
            TMP_Text valueLabel, unitLabel;
            if (index <= 0) 
            {   
                valueLabel = ValueHolderSample;
                unitLabel = UnitLabelSample;
            }
            else 
            {
                valueLabel = Instantiate(ValueHolderSample, ValueHolderSample.transform.parent);
                unitLabel = Instantiate(UnitLabelSample, UnitLabelSample.transform.parent);
            }

            valueLabel.color = unitLabel.color = fields[index].Color;
            
            ValueHolders.Add(valueLabel);
            UnitLabels.Add(unitLabel);
            index++;
        }
        
        UpdateValue();
    }

    public new void UpdateValue() 
    {
        base.UpdateValue();
        if (CurrentValue.Length > 1)
        {
            ValueHolders[0].color = UnitLabels[0].color = firstFieldColor * new Color (1, 1, 1, ValueHolders[0].color.a);
            UnitLabels[0].text = "<alpha=#77>" + Unit + " ";
            for (int a = 0; a < CurrentValue.Length; a++) 
            {
                ValueHolders[a].text = CurrentValue[a].ToString();
                UnitLabels[a].text = "<alpha=#77>" + Unit;
                UnitLabels[a].margin = new(0, 0, a < CurrentValue.Length - 1 ? 5 : 0, 0);
            }
        }
        else 
        {
            ValueHolders[0].color = UnitLabels[0].color = standardColor * new Color (1, 1, 1, ValueHolders[0].color.a);
            ValueHolders[0].text = CurrentValue[0].ToString();
            UnitLabels[0].text = "<alpha=#77>" + Unit;
            if (UnitLabels[0].margin.z != 0) UnitLabels[0].rectTransform.sizeDelta -= new Vector2 (5, 0);
            UnitLabels[0].margin = Vector4.zero;
            for (int a = 1; a < ValueHolders.Count; a++) 
            {
                ValueHolders[a].text = UnitLabels[a].text = "";
                UnitLabels[a].margin = Vector4.zero;
            }
        }
    }

    public void Edit()
    {
        OptionInputHandler.main.Edit(this);
    }
}
