using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FileModalItem : MonoBehaviour
{
    public Button Button;
    public TMP_Text Text;
    public Image Icon;
    public FileModal Parent;
    public FileModalEntry Entry;

    public void Select()
    {
        Parent.SelectItem(Entry);
    }
}
