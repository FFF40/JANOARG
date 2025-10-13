using System;
using System.Globalization;

namespace JANOARG.Shared.Data.ChartInfo
{
    [Serializable]
    public struct BeatPosition : IComparable<BeatPosition>
    {
        public int Number;
        public int Numerator;
        public int Denominator;

        public const double Precision = 1e-5;

        public BeatPosition(int a)
        {
            Number = a;
            Numerator = 0;
            Denominator = 1;
        }

        public BeatPosition(int a, int b, int c)
        {
            if (c <= 0) throw new ArgumentException("Denominator must be positive");

            Number = a;
            Numerator = b;
            Denominator = c;
            Normalize();
        }

        public override string ToString()
        {
            if (Numerator == 0) return Number + "b";
            else return (Numerator < 0 ? "-" : "") + Math.Abs(Number) + "b" + Math.Abs(Numerator) + "/" + Denominator;
        }

        public string ToString(CultureInfo cultureInfo)
        {
            if (Numerator == 0) return Number.ToString(cultureInfo) + "b";
            else
                return (Numerator < 0 ? "-" : "") + Math.Abs(Number)
                    .ToString(cultureInfo) + "b" + Math.Abs(Numerator)
                    .ToString(cultureInfo) + "/" + Denominator.ToString(cultureInfo);
        }

        public static BeatPosition Parse(string number, CultureInfo culture = null)
        {
            int slashPos = number.IndexOf('/');

            if (slashPos >= 0)
            {
                int bPos = number.IndexOf('b');

                return new BeatPosition(
                    int.Parse(number[..bPos], culture),
                    int.Parse(number[(bPos + 1)..slashPos], culture),
                    int.Parse(number[(slashPos + 1)..], culture)
                );
            }
            else
            {
                return (BeatPosition)float.Parse(number.Replace('b', '.'), culture);
            }
        }

        public static bool TryParse(string number, out BeatPosition output, CultureInfo culture = null)
        {
            try
            {
                output = Parse(number, culture);

                return true;
            }
            catch
            {
                output = NaN;

                return false;
            }
        }

        public static implicit operator double(BeatPosition a) => 
            a.Number + (double)a.Numerator / a.Denominator;

        public static implicit operator float(BeatPosition a) => 
            a.Number + (float)a.Numerator / a.Denominator;

        public static explicit operator BeatPosition(double a)
        {
            var num = (int)Math.Floor(a);
            a -= num;

            if (a == 0) return new BeatPosition(num);

            int minNumerator = 0,
                minDenominator = 1,
                maxNumerator = 1,
                maxDenominator = 1;

            while (true)
            {
                int medianNumerator = minNumerator + maxNumerator;
                int medianDenominator = minDenominator + maxDenominator;

                if (medianNumerator > medianDenominator * (a + Precision))
                {
                    maxNumerator = medianNumerator;
                    maxDenominator = medianDenominator;
                }
                else if (medianDenominator * (a - Precision) > medianNumerator)
                {
                    minNumerator = medianNumerator;
                    minDenominator = medianDenominator;
                }
                else
                {
                    return new BeatPosition(num, medianNumerator, medianDenominator);
                }
            }
        }

        public static BeatPosition operator +(BeatPosition a, BeatPosition b)
        {
            int greatestCommonDivisor = GreatestCommonDivisor(a.Denominator, b.Denominator);

            return new BeatPosition(
                a.Number + b.Number,
                b.Denominator / greatestCommonDivisor * a.Numerator + a.Denominator / greatestCommonDivisor * b.Numerator,
                a.Denominator / greatestCommonDivisor * b.Denominator
            );
        }

        public static BeatPosition operator -(BeatPosition a)
        {
            return new BeatPosition(
                -a.Number,
                -a.Numerator,
                a.Denominator
            );
        }

        public static BeatPosition operator -(BeatPosition a, BeatPosition b)
        {
            int greatestCommonDivisor = GreatestCommonDivisor(a.Denominator, b.Denominator);

            return new BeatPosition(
                a.Number - b.Number,
                b.Denominator / greatestCommonDivisor * a.Numerator - a.Denominator / greatestCommonDivisor * b.Numerator,
                a.Denominator / greatestCommonDivisor * b.Denominator
            );
        }

        public static bool operator <(BeatPosition a, BeatPosition b) => 
            a.CompareTo(b) < 0;

        public static bool operator <=(BeatPosition a, BeatPosition b) => 
            a.CompareTo(b) <= 0;

        public static bool operator >(BeatPosition a, BeatPosition b) => 
            a.CompareTo(b) > 0;

        public static bool operator >=(BeatPosition a, BeatPosition b) => 
            a.CompareTo(b) >= 0;

        private void Normalize()
        {
            if (Denominator <= 0) return;

            if (Number < 0)
            {
                if (Numerator <= -Denominator)
                {
                    int offset = -Numerator / Denominator;
                    Number -= offset;
                    Numerator += offset * Denominator;
                }
                else if (Numerator > 0)
                {
                    int offset = Numerator / Denominator + 1;
                    Number += offset;
                    Numerator -= offset * Denominator;

                    if (Number > 0)
                    {
                        Number--;
                        Numerator += Denominator;
                    }
                }
            }
            else if (Number > 0)
            {
                if (Numerator >= Denominator)
                {
                    int offset = Numerator / Denominator;
                    Number += offset;
                    Numerator -= offset * Denominator;
                }
                else if (Numerator < 0)
                {
                    int offset = -Numerator / Denominator + 1;
                    Number -= offset;
                    Numerator += offset * Denominator;

                    if (Number < 0)
                    {
                        Number++;
                        Numerator -= Denominator;
                    }
                }
            }
            else
            {
                if (Numerator <= -Denominator)
                {
                    int offset = -Numerator / Denominator;
                    Number -= offset;
                    Numerator += offset * Denominator;
                }
                else if (Numerator >= Denominator)
                {
                    int offset = Numerator / Denominator;
                    Number += offset;
                    Numerator -= offset * Denominator;
                }
            }

            int greatestCommonDivisor = GreatestCommonDivisor(Math.Abs(Numerator), Denominator);
            Numerator /= greatestCommonDivisor;
            Denominator /= greatestCommonDivisor;
        }

        private static int GreatestCommonDivisor(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b) a %= b;
                else b %= a;
            }

            return a | b;
        }

        // ReSharper disable once InconsistentNaming
        public static readonly BeatPosition NaN = new() { Number = 0, Numerator = 0, Denominator = 0 };

        public static bool IsNaN(BeatPosition a) => 
            a.Denominator <= 0;

        public readonly int CompareTo(BeatPosition other)
        {
            return ((float)this).CompareTo((float)other);
        }

        // -------------------- Math functions
        public static BeatPosition Min(BeatPosition a, BeatPosition b) => 
            a < b ? a : b;

        public static BeatPosition Max(BeatPosition a, BeatPosition b) => 
            a > b ? a : b;
    }
}