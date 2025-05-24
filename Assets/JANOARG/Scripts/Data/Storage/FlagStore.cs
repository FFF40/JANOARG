using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using System.Linq;

public class FlagStore
{
    public HashSet<string> Entries { get; private set; } = new();

    public void Load()
    {
        string[] entryList = Common.main.Storage.Get("FlagStore", new string[0]);
        foreach (string entry in entryList)
        {
            Entries.Add(entry);
        }
    }

    public void Save()
    {
        Common.main.Storage.Set("FlagStore", Entries.ToArray());
    }

    public bool Test(string flag)
    {
        return Entries.Contains(flag);
    }

    public bool Set(string flag)
    {
        return Entries.Add(flag);
    }

    public bool Unset(string flag) 
    {
        return Entries.Remove(flag);
    }
}