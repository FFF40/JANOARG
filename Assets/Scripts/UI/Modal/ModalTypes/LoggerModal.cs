using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class LoggerModal : Modal
{
    public static LoggerModal main;

    public LoggerEntry EntrySample;
    public RectTransform EntryHolder;
    public RectTransform EntryViewport;
    public RectTransform EntryScroll;
    public List<LoggerEntry> Entries;

    public Toggle InfoToggle;
    public Sprite InfoIcon;
    public TMP_Text InfoCountLabel; 
    public Toggle WarningToggle;
    public Sprite WarningIcon;
    public TMP_Text WarningCountLabel; 
    public Toggle ErrorToggle;
    public Sprite ErrorIcon;
    public TMP_Text ErrorCountLabel; 

    BorderlessWindow.LoggerEntry activeEntry;
    float activeEntryHeight;

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }

    public new void Start()
    {
        base.Start();
        UpdateLogger();
    }

    float lastPos;
    public void Update()
    {
        if (lastPos != EntryHolder.anchoredPosition.y)
        {
            lastPos = EntryHolder.anchoredPosition.y;
            UpdateLogger();
        }
    }

    public void UpdateLogger() 
    {
        float itemHeight = EntrySample.rectTransform.sizeDelta.y;
        int index = Mathf.Max((int)(EntryHolder.anchoredPosition.y / itemHeight), 0);
        float offset = index * itemHeight;
        float offsetMax = EntryViewport.rect.height + EntryHolder.anchoredPosition.y;

        int count = 0;
        void AddItem(BorderlessWindow.LoggerEntry entry)
        {
            LoggerEntry item;
            if (Entries.Count <= count) Entries.Add(item = Instantiate(EntrySample, EntryHolder));
            else item = Entries[count];

            bool active = activeEntry == entry;
            item.SetItem(entry, offset, active, this);
            if (active) 
            {
                if (item.rectTransform.rect.height == itemHeight) 
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(item.rectTransform);
                    LayoutRebuilder.MarkLayoutForRebuild(EntryScroll);
                }
                activeEntryHeight = item.rectTransform.rect.height;
            }
            offset += active ? activeEntryHeight : itemHeight;

            count++;
        }

        int infoCount = 0, warnCount = 0, errCount = 0;
        var logger = BorderlessWindow.Logger.FindAll(x => {
            // ???????????????
            bool a(int x, bool y) => y;
            return x.LogType == LogType.Log ? a(infoCount++, InfoToggle.isOn) :
                x.LogType == LogType.Warning ? a(warnCount++, WarningToggle.isOn) :
                a(errCount++, ErrorToggle.isOn);
        });
        float listHeight = logger.Count * itemHeight;
        int activeIndex = BorderlessWindow.Logger.IndexOf(activeEntry);
        if (activeIndex >= 0) 
        {
            listHeight += activeEntryHeight - itemHeight;
            if (index > activeIndex)
            {
                index = Mathf.Max((int)((EntryHolder.anchoredPosition.y - activeEntryHeight) / itemHeight + 1), activeIndex);
                offset = index * itemHeight;
                if (index > activeIndex) offset += activeEntryHeight - itemHeight;
            }
        }
        
        EntryHolder.sizeDelta = new (EntryHolder.sizeDelta.x, listHeight);
        while (offset < offsetMax && index < logger.Count) 
        {
            AddItem(BorderlessWindow.Logger[index]);
            index++;
        }
        InfoCountLabel.text = infoCount.ToString();
        WarningCountLabel.text = warnCount.ToString();
        ErrorCountLabel.text = errCount.ToString();

        while (Entries.Count > count)
        {
            Destroy(Entries[count].gameObject);
            Entries.RemoveAt(count);
        }
    }
    
    public void LoadEntry(BorderlessWindow.LoggerEntry entry, LoggerEntry item)
    {
        activeEntry = entry;
        item.SetItem(entry, item.rectTransform.rect.y, true, this);
        LayoutRebuilder.ForceRebuildLayoutImmediate(item.rectTransform);
        LayoutRebuilder.MarkLayoutForRebuild(EntryScroll);
        activeEntryHeight = item.rectTransform.rect.height;
        
        UpdateLogger();
    }

    public void Clear() 
    {
        BorderlessWindow.Logger.Clear();
        UpdateLogger();
    }

    public void OpenFile() 
    {
        Application.OpenURL("file://" + Application.consoleLogPath);
    }
}