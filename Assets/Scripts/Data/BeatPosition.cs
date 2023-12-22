
using System;
using System.Globalization;
using System.Numerics;
using Unity.VisualScripting;

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
        else return (Numerator < 0 ? "-" : "") + Math.Abs(Number).ToString(cultureInfo) + "b" + Math.Abs(Numerator).ToString(cultureInfo) + "/" + Denominator.ToString(cultureInfo);
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

    public static implicit operator double(BeatPosition a) => a.Number + (double)a.Numerator / a.Denominator;
    public static implicit operator float(BeatPosition a) => a.Number + (float)a.Numerator / a.Denominator;

    public static explicit operator BeatPosition(double a) 
    {
        int num = (int)Math.Floor(a);
        a -= num;
        if (a == 0) return new BeatPosition(num);

        int minN = 0, minD = 1, maxN = 1, maxD = 1;
        while (true) 
        {
            int midN = minN + maxN;
            int midD = minD + maxD;
            if (midN > midD * (a + Precision))
            {
                maxN = midN; maxD = midD;
            }
            else if (midD * (a - Precision) > midN)
            {
                minN = midN; minD = midD;
            }
            else 
            {
                return new BeatPosition(num, midN, midD);
            }
            
        }
    }

    public static BeatPosition operator +(BeatPosition a, BeatPosition b)
    {
        int gcd = GCD(a.Denominator, b.Denominator);
        return new BeatPosition(
            a.Number + b.Number,
            b.Denominator / gcd * a.Numerator + a.Denominator / gcd * b.Numerator,
            a.Denominator / gcd * b.Denominator
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
        int gcd = GCD(a.Denominator, b.Denominator);
        return new BeatPosition(
            a.Number - b.Number,
            b.Denominator / gcd * a.Numerator - a.Denominator / gcd * b.Numerator,
            a.Denominator / gcd * b.Denominator
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

    void Normalize()
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

        int gcd = GCD(Math.Abs(Numerator), Denominator);
        Numerator /= gcd;
        Denominator /= gcd;
    }

    static int GCD(int a, int b)
    {
        while (a != 0 && b != 0)
        {
            if (a > b) a %= b;
            else b %= a;
        }
        return a | b;
    }

    public static readonly BeatPosition NaN = new() { Number = 0, Numerator = 0, Denominator = 0 };
    public static bool IsNaN(BeatPosition a)
    {
        return a.Denominator <= 0;
    }

    public readonly int CompareTo(BeatPosition other)
    {
        return ((float)this).CompareTo((float)other);
    }
}