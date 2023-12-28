using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogModal : Modal
{
    public TMP_Text TitleLabel;
    public TMP_Text BodyLabel;
    public RectTransform ActionHolder;
    public DialogModalAction ActionSample;
    public GameObject CloseButton;
    
    List<DialogModalAction> Actions = new();

    public void SetDialog(string title, string body, string[] actions, Action<int> onSelect, bool allowX = true)
    {
        TitleLabel.text = title;
        BodyLabel.text = body;
        CloseButton.SetActive(allowX);
        
        foreach (DialogModalAction act in Actions) Destroy(act.gameObject);
        Actions.Clear();

        int index = 0;
        foreach (string act in actions) 
        {
            int i = index;
            index++;
            DialogModalAction action = Instantiate(ActionSample, ActionHolder);
            action.Text.text = act;
            action.Button.onClick.AddListener(() => { onSelect(i); Close(); });
            Actions.Add(action);
        }
    }
}
