using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class Storage 
{
    public Dictionary<string, object> values = new();
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

    public UnityEvent OnSave = new();

    public UnityEvent OnLoad = new();

    [XmlInclude(typeof(ScoreStoreEntry))]
    public class SerializeProxy
    {
        [XmlAttribute]
        public string Key;
        public object Value;
        public static implicit operator SerializeProxy(KeyValuePair<string, object> item)
        {
            SerializeProxy proxy = new()
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
    public class SerializeProxyList
    {
        [XmlElement("Item")]
        public List<SerializeProxy> Items = new();
    }

    public void Save()
    {
        SerializeProxyList list = new();
        foreach (KeyValuePair<string, object> pair in values) if (pair.Value != null) list.Items.Add(pair);

        TrySaveToFile(SaveName + ".jas", list);
        TrySaveToFile(SaveName + ".backup.jas", list);

        OnSave.Invoke();
    }

    public void Load()
    {
        SerializeProxyList list = new();

        if (File.Exists(SaveName + ".jas")) {
            list = TryLoadFromFile<SerializeProxyList>(SaveName + ".jas", new());
        } else if (File.Exists(SaveName + ".backup.jas")) {
            list = TryLoadFromFile<SerializeProxyList>(SaveName + ".backup.jas", new());
        }

        foreach (SerializeProxy pair in list.Items) pair.AddPair(values);

        OnLoad.Invoke();
    }

    /// <summary>
    /// Let's not use try catch anymore to know if a file exists or not. Loads an XML file into a C# class. Also, I hate XML
    /// </summary>
    /// <typeparam name="T">The C# representation of the XML Structure</typeparam>
    /// <param name="path">The file path</param>
    /// <param name="defaultValue">
    /// This will be returned if anything goes wrong, 
    /// such as if the file doesn't exist, or if there's no data yet, 
    /// or if the deserializer failed to load the XML. By default it's probably "null"
    /// </param>
    /// <returns>Either the XML data as T or the default value</returns>
    private T TryLoadFromFile<T>(string path, T defaultValue = default) where T : class
    {
        if (!File.Exists(path)) return defaultValue;

        try
        {
            using FileStream fs = new(path, FileMode.Open);
            if (fs.Length == 0) return defaultValue;
            
            XmlSerializer serializer = new(typeof(T));
            var result = serializer.Deserialize(fs);
            return result == null ? defaultValue : (T)result;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return defaultValue;
        }
    }

    /// <summary>
    /// Not sure why I made this, just use it if you want to, just in case something goes wrong when saving.
    /// I don't wanna risk modifying code too much even though I am lol
    /// </summary>
    /// <typeparam name="T">The C# representation of the XML Structure</typeparam>
    /// <param name="path">The file path</param>
    /// <param name="data">The C# data you want to put on the file</param>
    private void TrySaveToFile<T>(string path, T data) where T : class {
        try 
        {
            using FileStream fs = new(path, FileMode.Create);
            XmlSerializer serializer = new(typeof(T));
            serializer.Serialize(fs, data);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }
}
