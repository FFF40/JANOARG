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

    [XmlRoot("ItemList")]
    [XmlInclude(typeof(ScoreStoreEntry))]
    public class ClientSerializeProxyList : SerializeProxyList
    {
    }

    public class Storage : Storage<ClientSerializeProxyList>
    {
        public Storage(string path) : base(path) { }
    }
}
