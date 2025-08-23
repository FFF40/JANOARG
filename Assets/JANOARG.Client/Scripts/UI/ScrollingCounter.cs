using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Client.UI
{
    public class ScrollingCounter : MonoBehaviour
    {
        public List<ScrollingCounterDigit> Digits;

        public void SetNumber(int number)
        {
            string str = number.ToString()
                .PadLeft(Digits.Count, '0');

            var forced = false;

            for (var a = 0; a < Digits.Count; a++)
                if (forced ||
                    str[a] !=
                    Digits[a]
                        .CurrentDigit[0])
                {
                    Digits[a]
                        .SetDigit(
                            str[a]
                                .ToString());

                    forced = true;
                }
        }
    }
}