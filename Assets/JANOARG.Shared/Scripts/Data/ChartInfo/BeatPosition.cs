using System;
using System.Globalization;

namespace JANOARG.Shared.Data.ChartInfo
{
    /// <summary>
    /// Represents time position in a song in beats. expressed as a compounding fraction format.
    /// </summary>
    [Serializable]
    public struct BeatPosition : IComparable<BeatPosition>
    {
        #region Fields and work constants
        public int Number;
        public int Numerator;
        public int Denominator;

        public const double PRECISION = 1e-5;
        #endregion

        #region Constructors
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
        #endregion

        #region String handling

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
        #endregion


        #region Conversion operators
        public static implicit operator double(BeatPosition a)
        {
            return a.Number + (double)a.Numerator / a.Denominator;
        }

        public static implicit operator float(BeatPosition a)
        {
            return a.Number + (float)a.Numerator / a.Denominator;
        }

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

                if (medianNumerator > medianDenominator * (a + PRECISION))
                {
                    maxNumerator = medianNumerator;
                    maxDenominator = medianDenominator;
                }
                else if (medianDenominator * (a - PRECISION) > medianNumerator)
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
        #endregion

        #region Operators
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

        public static bool operator <(BeatPosition a, BeatPosition b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator <=(BeatPosition a, BeatPosition b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator >(BeatPosition a, BeatPosition b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator >=(BeatPosition a, BeatPosition b)
        {
            return a.CompareTo(b) >= 0;
        }
        #endregion
        
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

            // Simplify fraction
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


        #region Constants and checks
        // ReSharper disable once InconsistentNaming
        public static readonly BeatPosition NaN = new() { Number = 0, Numerator = 0, Denominator = 0 };

        public static bool IsNaN(BeatPosition a)
        {
            return a.Denominator <= 0;
        }
        #endregion
        
        #region Math functions
        public readonly int CompareTo(BeatPosition other)
        {
            return ((float)this).CompareTo((float)other);
        }
        
        public static BeatPosition Min(BeatPosition a, BeatPosition b)
        {
            return a < b ? a : b;
        }

        public static BeatPosition Max(BeatPosition a, BeatPosition b)
        {
            return a > b ? a : b;
        }
        #endregion
    }
}