using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Options.Input_Types
{
    public class MultiFloatOptionInput : OptionInput<float[]>
    {
        [Space] public float Min;

        public float          Max  = 100;
        public float          Step = 1;
        public string         Unit;
        public MultiValueType ValueType;

        [Space] public TMP_Text ValueHolderSample;

        public TMP_Text       UnitLabelSample;
        public List<TMP_Text> ValueHolders;
        public List<TMP_Text> UnitLabels;

        private Color _FirstFieldColor;
        private Color _StandardColor;

        public new void Start()
        {
            List<MultiValueFieldData> fields = MultiValueFieldData.sInfo[ValueType];
            var index = 0;

            _StandardColor = ValueHolderSample.color;
            _FirstFieldColor = fields[0].Color;

            foreach (MultiValueFieldData field in fields)
            {
                TMP_Text valueLabel, unitLabel;

                if (index <= 0)
                {
                    valueLabel = ValueHolderSample;
                    unitLabel = UnitLabelSample;
                }
                else
                {
                    valueLabel = Instantiate(ValueHolderSample, ValueHolderSample.transform.parent);
                    unitLabel = Instantiate(UnitLabelSample, UnitLabelSample.transform.parent);
                }

                valueLabel.color = unitLabel.color = fields[index].Color;

                ValueHolders.Add(valueLabel);
                UnitLabels.Add(unitLabel);
                index++;
            }

            UpdateValue();
        }

        public new void UpdateValue()
        {
            base.UpdateValue();

            if (CurrentValue.Length > 1)
            {
                ValueHolders[0].color =
                    UnitLabels[0].color = _FirstFieldColor * new Color(1, 1, 1, ValueHolders[0].color.a);

                UnitLabels[0].text = "<alpha=#77>" + Unit + " ";

                for (var a = 0; a < CurrentValue.Length; a++)
                {
                    ValueHolders[a].text = CurrentValue[a]
                        .ToString();

                    UnitLabels[a].text = "<alpha=#77>" + Unit;
                    UnitLabels[a].margin = new Vector4(0, 0, a < CurrentValue.Length - 1 ? 5 : 0, 0);
                }
            }
            else
            {
                ValueHolders[0].color =
                    UnitLabels[0].color = _StandardColor * new Color(1, 1, 1, ValueHolders[0].color.a);

                ValueHolders[0].text = CurrentValue[0]
                    .ToString();

                UnitLabels[0].text = "<alpha=#77>" + Unit;
                if (UnitLabels[0].margin.z != 0) UnitLabels[0].rectTransform.sizeDelta -= new Vector2(5, 0);
                UnitLabels[0].margin = Vector4.zero;

                for (var a = 1; a < ValueHolders.Count; a++)
                {
                    ValueHolders[a].text = UnitLabels[a].text = "";
                    UnitLabels[a].margin = Vector4.zero;
                }
            }
        }

        public void Edit()
        {
            OptionInputHandler.sMain.Edit(this);
        }
    }
}
