using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecisionItemHandler : MonoBehaviour
{
    public string StoryFlag;
    public string Value;

    public TMP_Text DecisionText;
    // public Storage Storage = new Storage("save");

    public void Setup(DecisionItem item)
    {
        StoryFlag = item.StoryFlag;
        Value = item.Value;
        DecisionText.text = item.Dialog;
    }


    public void SaveDecision()
    {
        // string ValueType = Parser.GetDetectedDataType(Value);
        // if (ValueType == "null") throw new NullReferenceException("Value is null: " + Value);

        // switch (ValueType)
        // {
        //     case "bool":
        //         int count = Storage.Get("STAT:Count", 0) + 1;
        //         Storage.Set("STAT:Count", count);
        //         Debug.Log(count);
        //         Storage.Save();
        //         break;
        //     case "int":
        //         break;
        //     case "float":
        //         break;
        //     case "string":
        //         break;
        // }

        // Storage.Set(StoryFlag, Value);
        // Storage.Save();

    }
}

// public class Parser : MonoBehaviour
// {
//     // Existing helper methods
//     public static bool IsBool(string value)
//     {
//         if (string.IsNullOrWhiteSpace(value)) return false;
//         return bool.TryParse(value, out _);
//     }

//     public static bool IsInt(string value)
//     {
//         if (string.IsNullOrWhiteSpace(value)) return false;
//         return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
//     }

//     public static bool IsFloat(string value)
//     {
//         if (string.IsNullOrWhiteSpace(value)) return false;
//         return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
//     }

//     public static bool ParseBool(string value)
//     {
//         return bool.Parse(value);
//     }

//      public static string GetDetectedDataType(string value)
//     {
//         if (value == null)  return "null"; 
//         if (IsBool(value))  return "bool";
//         if (IsInt(value))   return "int";
//         if (IsFloat(value)) return "float";
        
//         return "string";
//     }
    
//     public static object ParseAutomatic(string value)
//     {
//         if (value == null) return null;

//         if (IsBool(value)) return bool.Parse(value);
//         if (IsInt(value)) return int.Parse(value, CultureInfo.InvariantCulture);
//         if (IsFloat(value)) return float.Parse(value, CultureInfo.InvariantCulture);

//         return value;
//     }
// }