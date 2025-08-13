using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class OptionInputListItem : MonoBehaviour
{
    public TMP_Text Text;
    public Graphic Background;

    public Action OnSelect;

    public Button Button;
}