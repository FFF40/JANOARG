using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using System.Linq;
using JANOARG.Client.Behaviors.Common;

namespace JANOARG.Client.Data.Storage
{
    public class FlagStore
    {
        public HashSet<string> Entries { get; private set; } = new();

        public void Load()
        {
            string[] entryList = CommonSys.sMain.Storage.Get("FlagStore", new string[0]);
            foreach (string entry in entryList)
            {
                Entries.Add(entry);
            }
        }

        public void Save()
        {
            CommonSys.sMain.Storage.Set("FlagStore", Entries.ToArray());
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
}