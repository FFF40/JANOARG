using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

// STILL IN PROGRESS <-------------------------------------------------------------
namespace JANOARG.Shared.Data.Story.Instructions
{

    [Serializable]
    public class DecisionItem
    {
        public string StoryFlag;
        public string Value;
        public string Dialog;

        public DecisionItem(string flag, string value, string text)
        {
            StoryFlag = flag;
            Value = value;
            Dialog = text;
        }
    }


    public class FlagCheck : DecisionItem
    {
        public FlagCheck(string flag, string value, string text) : base(flag, value, text)
        { }
    }
}
