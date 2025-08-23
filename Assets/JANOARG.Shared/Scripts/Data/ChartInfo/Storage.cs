using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using JANOARG.Client.Data.Storage;
using UnityEngine;
using UnityEngine.Events;

namespace JANOARG.Shared.Data.ChartInfo
{
    public class Storage
    {
        public UnityEvent OnLoad = new();

        public UnityEvent                 OnSave = new();
        public string                     SaveName;
        public Dictionary<string, object> Values = new();

        public Storage(string path)
        {
            SaveName = Application.persistentDataPath + "/" + path;
            Load();
        }

        public T Get<T>(string key, T fallback)
        {
            if (Values.ContainsKey(key)) return (T)Values[key];

            return fallback;
        }

        public T[] Get<T>(string key, T[] fallback)
        {
            if (Values.ContainsKey(key))
            {
                if (Values[key] is object[])
                    return ((object[])Values[key]).OfType<T>()
                        .ToArray();

                return (T[])Values[key];
            }

            return fallback;
        }

        public void Set(string key, object value)
        {
            Values[key] = value;
        }

        public void Save()
        {
            SerializeProxyList list = new();

            foreach (KeyValuePair<string, object> pair in Values)
                if (pair.Value != null)
                    list.Items.Add(pair);

            XmlSerializer serializer = new(typeof(SerializeProxyList));
            FileStream fileStream;

            fileStream = new FileStream(SaveName + ".jas", FileMode.Create);
            serializer.Serialize(fileStream, list);
            fileStream.Close();
            fileStream = new FileStream(SaveName + ".backup.jas", FileMode.Create);
            serializer.Serialize(fileStream, list);
            fileStream.Close();

            OnSave.Invoke();
        }

        public void Load()
        {
            SerializeProxyList list = new();

            XmlSerializer serializer = new(typeof(SerializeProxyList));
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(SaveName + ".jas", FileMode.OpenOrCreate);
                list = (SerializeProxyList)serializer.Deserialize(fileStream);
                fileStream.Close();
            }
            catch (Exception e)
            {
                try
                {
                    fileStream?.Close();
                    fileStream = new FileStream(SaveName + ".backup.jas", FileMode.OpenOrCreate);
                    list = (SerializeProxyList)serializer.Deserialize(fileStream);
                    fileStream.Close();
                }
                catch (Exception ee)
                {
                    fileStream?.Close();
                    Debug.Log(e + "\n" + ee);
                }
            }

            foreach (SerializeProxy pair in list.Items) pair.AddPair(Values);

            OnLoad.Invoke();
        }

        [XmlInclude(typeof(ScoreStoreEntry))]
        public class SerializeProxy
        {
            [XmlAttribute] public string Key;

            public object Value;

            public static implicit operator SerializeProxy(KeyValuePair<string, object> item)
            {
                SerializeProxy proxy = new()
                {
                    Key = item.Key,
                    Value = item.Value
                };

                if (proxy.Value is Array) proxy.Value = new CollectionProxy(proxy.Value as Array);

                return proxy;
            }

            public static implicit operator KeyValuePair<string, object>(SerializeProxy item)
            {
                object value = item.Value;
                if (value is CollectionProxy) value = ((CollectionProxy)value).Value;
                KeyValuePair<string, object> pair = new(item.Key, value);

                return pair;
            }

            public void AddPair(Dictionary<string, object> dict)
            {
                KeyValuePair<string, object> pair = this;
                dict.TryAdd(pair.Key, pair.Value);
            }
        }

        public class CollectionProxy
        {
            [XmlElement("Item")] public object[] Value;

            public CollectionProxy()
            {
            }

            public CollectionProxy(Array array)
            {
                Value = new object[array.Length];
                for (var a = 0; a < array.Length; a++) Value[a] = array.GetValue(a);
            }
        }

        [XmlRoot("ItemList")]
        [XmlInclude(typeof(CollectionProxy))]
        public class SerializeProxyList
        {
            [XmlElement("Item")] public List<SerializeProxy> Items = new();
        }
    }
}