
using System;
using System.Numerics;
using Unity.VisualScripting;

[Serializable]
public struct BeatPosition 
{
    public int Number;
    public int Numerator;
    public int Denominator;

    public BeatPosition(int a)
    {
        Number = a;
        Numerator = 0;
        Denominator = 1;
        Normalize();
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

    public static implicit operator double(BeatPosition a) => a.Number + (double)a.Numerator / a.Denominator;
    public static implicit operator float(BeatPosition a) => a.Number + (float)a.Numerator / a.Denominator;

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

    void Normalize()
    {
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
}