using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Storage 
{
    public Dictionary<string, object> values = new Dictionary<string, object>();
    public string SaveName;

    public Storage(string path)
    {
        SaveName = path;
        Load();
    }

    public T Get<T>(string key, T fallback)
    {
        if (values.ContainsKey(key)) return (T)values[key];
        else return fallback;
    }
    public void Set(string key, object value)
    {
        values[key] = value;
    }

    public class SerializeProxy
    {
        [XmlAttribute]
        public string Key;
        public object Value;

        public static implicit operator SerializeProxy(KeyValuePair<string, object> item)
        {
            return new SerializeProxy
            {
                Key = item.Key,
                Value = item.Value,
            };
        }
    }

    [XmlRoot("ItemList")]
    public class SerializeProxyList
    {
        [XmlElement("Item")]
        public List<SerializeProxy> Items = new List<SerializeProxy>();
    }

    public void Save()
    {
        SerializeProxyList list = new SerializeProxyList();
        foreach (KeyValuePair<string, object> pair in values) list.Items.Add(pair);

        XmlSerializer serializer = new XmlSerializer(typeof(SerializeProxyList));
        FileStream fs;

        fs = new FileStream(Application.persistentDataPath + "/" + SaveName + ".jas", FileMode.Create);
        serializer.Serialize(fs, list);
        fs.Close();
        fs = new FileStream(Application.persistentDataPath + "/" + SaveName + ".backup.jas", FileMode.Create);
        serializer.Serialize(fs, list);
        fs.Close();
    }

    public void Load()
    {
        SerializeProxyList list = new SerializeProxyList();

        XmlSerializer serializer = new XmlSerializer(typeof(SerializeProxyList));
        FileStream fs = null;
        try 
        {
            fs = new FileStream(Application.persistentDataPath + "/" + SaveName + ".jas", FileMode.OpenOrCreate);
            list = (SerializeProxyList)serializer.Deserialize(fs);
            fs.Close();
        }
        catch (Exception e)
        {
            try 
            {
                fs?.Close();
                fs = new FileStream(Application.persistentDataPath + "/" + SaveName + ".backup.jas", FileMode.OpenOrCreate);
                list = (SerializeProxyList)serializer.Deserialize(fs);
                fs.Close();
            }
            catch (Exception ee)
            {
                fs?.Close();
                Debug.Log(e.StackTrace);
                Debug.Log(ee.StackTrace);
            }
        }

        foreach (SerializeProxy pair in list.Items) values.TryAdd(pair.Key, pair.Value);
    }
}