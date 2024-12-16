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
    public GameObject SeparatorSample;
    public GameObject CloseButton;
    
    List<GameObject> Items = new();

    public new void Start() 
    {
        base.Start();
        transform.parent = ModalHolder.main.PriorityModalHolder;
    }

    public void SetDialog(string title, string body, string[] actions, Action<int> onSelect, bool allowX = true)
    {
        TitleLabel.text = title;
        BodyLabel.text = body;
        CloseButton.SetActive(allowX);
        
        foreach (GameObject item in Items) Destroy(item);
        Items.Clear();

        int index = 0;
        foreach (string act in actions) 
        {
            int i = index;
            if (string.IsNullOrEmpty(act))
            {
                Items.Add(Instantiate(SeparatorSample, ActionHolder));
            }
            else 
            {
                DialogModalAction action = Instantiate(ActionSample, ActionHolder);
                action.Text.text = act;
                action.Button.onClick.AddListener(() => { onSelect(i); Close(); });
                Items.Add(action.gameObject);
            }
            index++;
        }
    }
}
