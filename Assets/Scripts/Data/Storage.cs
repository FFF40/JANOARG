using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Linq;

public class Storage 
{
    public Dictionary<string, object> values = new Dictionary<string, object>();
    public string SaveName;

    public Storage(string path)
    {
        SaveName = Application.persistentDataPath + "/" + path;
        Load();
    }

    public T Get<T>(string key, T fallback)
    {
        if (values.ContainsKey(key)) 
        {
            return (T)values[key];
        }
        else return fallback;
    }
    public T[] Get<T>(string key, T[] fallback)
    {
        if (values.ContainsKey(key)) 
        {
            if (values[key] is object[]) return ((object[])values[key]).OfType<T>().ToArray();
            return (T[])values[key];
        }
        else return fallback;
    }

    public void Set(string key, object value)
    {
        values[key] = value;
    }

    public UnityEvent OnSave = new UnityEvent();

    public UnityEvent OnLoad = new UnityEvent();

    public class SerializeProxy
    {
        [XmlAttribute]
        public string Key;
        public object Value;
        public static implicit operator SerializeProxy(KeyValuePair<string, object> item)
        {
            SerializeProxy proxy = new SerializeProxy
            {
                Key = item.Key,
                Value = item.Value,
            };
            if (proxy.Value is Array) proxy.Value = new CollectionProxy(proxy.Value as Array);
            return proxy;
        }

        public static implicit operator KeyValuePair<string, object>(SerializeProxy item)
        {
            object value = item.Value;
            if (value is CollectionProxy) value = ((CollectionProxy)value).Value;
            KeyValuePair<string, object> pair = new KeyValuePair<string, object>(item.Key, value);
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
        [XmlElement("Item")]
        public object[] Value;

        public CollectionProxy()
        {
        }

        public CollectionProxy(Array array)
        {
            Value = new object[array.Length];
            for (int a = 0; a < array.Length; a++) Value[a] = array.GetValue(a);
        }
    }

    [XmlRoot("ItemList")]
    [XmlInclude(typeof(CollectionProxy))]
    [XmlInclude(typeof(RecentSong))]
    public class SerializeProxyList
    {
        [XmlElement("Item")]
        public List<SerializeProxy> Items = new List<SerializeProxy>();
    }

    public void Save()
    {
        SerializeProxyList list = new SerializeProxyList();
        foreach (KeyValuePair<string, object> pair in values) if (pair.Value != null) list.Items.Add(pair);

        XmlSerializer serializer = new XmlSerializer(typeof(SerializeProxyList));
        FileStream fs;

        fs = new FileStream(SaveName + ".jas", FileMode.Create);
        serializer.Serialize(fs, list);
        fs.Close();
        fs = new FileStream(SaveName + ".backup.jas", FileMode.Create);
        serializer.Serialize(fs, list);
        fs.Close();

        OnSave.Invoke();
    }

    public void Load()
    {
        SerializeProxyList list = new SerializeProxyList();

        XmlSerializer serializer = new XmlSerializer(typeof(SerializeProxyList));
        FileStream fs = null;
        try 
        {
            fs = new FileStream(SaveName + ".jas", FileMode.OpenOrCreate);
            list = (SerializeProxyList)serializer.Deserialize(fs);
            fs.Close();
        }
        catch (Exception e)
        {
            try 
            {
                fs?.Close();
                fs = new FileStream(SaveName + ".backup.jas", FileMode.OpenOrCreate);
                list = (SerializeProxyList)serializer.Deserialize(fs);
                fs.Close();
            }
            catch (Exception ee)
            {
                fs?.Close();
                Debug.Log(e + "\n" + ee);
            }
        }

        foreach (SerializeProxy pair in list.Items) pair.AddPair(values);

        OnLoad.Invoke();
    }
}
