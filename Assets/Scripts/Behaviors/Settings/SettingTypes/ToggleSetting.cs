using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSetting : Setting
{
    public Toggle Field;
    public bool DefaultValue;

    public void Start()
    {
        Field.isOn = Common.main.Storage.Get(ID, DefaultValue);
    }

    public void OnChange()
    {
        Common.main.Storage.Set(ID, Field.isOn);
    }
}
