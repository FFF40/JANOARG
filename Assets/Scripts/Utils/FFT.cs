using System;
using UnityEngine;

public class FFT 
{
    public static void Transform (float[] array, FFTWindow window = FFTWindow.Hann)
    {
        if (Mathf.ClosestPowerOfTwo(array.Length) != array.Length) throw new Exception("Arrays' lengths must be a power of 2");
        int size = array.Length;
        float[] re = array, im = new float[size];
        for (int a = 0; a < size; a++) re[a] *= getwin(window, (a + .5f) / size);
        ditfft(re, im);
        for (int i = 0; i < size; i++) array[i] = re[i] * re[i] + im[i] * im[i];
    }

    // Cooley–Tukey FFT algorithm
    static void ditfft(float[] re, float[] im)
    {
        if (re.Length != im.Length)
            throw new Exception("Arrays must be equal in length");
        if (Mathf.ClosestPowerOfTwo(re.Length) != re.Length)
            throw new Exception("Arrays' lengths must be a power of 2");

        // Bit-reversal permutation
        int length = re.Length;
        int bits = (int)Mathf.Log(re.Length, 2);
        for (int pos = 0; pos < length; pos++) 
        {
            int rev = bitrev(pos, bits);
            if (pos < rev)
            {
                float reTemp = re[rev];
                float imTemp = im[rev];
                re[rev] = re[pos];
                im[rev] = im[pos];
                re[pos] = reTemp;
                im[pos] = imTemp;
            }
        }

        // Cooley–Tukey FFT algorithm
        for (int s = 0; s < bits; s++) 
        {
            int step = 2 << s;
            for (int k = 0; k < length; k += step) 
            {
                for (int j = 0; j < step / 2; j++) 
                {
                    float angle = -2 * Mathf.PI / step * j;
                    float reP = re[k + j];
                    float imP = im[k + j];
                    float reQ = re[k + j + step / 2] * Mathf.Cos(angle);
                    float imQ = im[k + j + step / 2] * Mathf.Sin(angle);
                    re[k + j] = reP + reQ;
                    im[k + j] = imP + imQ;
                    re[k + j + step / 2] = reP - reQ;
                    im[k + j + step / 2] = imP - imQ;
                }
            }
        }
    }

    static int bitrev(int number, int bits) 
    {
        int ans = 0;
        for (int a = 0; a < bits; a++) {
            ans <<= 1;
            ans |= number & 1;
            number >>= 1;
        }
        return ans;
    }

    static float getwin(FFTWindow window, float x) 
    {
        return window switch
        {
            FFTWindow.Rectangular => 1,
            FFTWindow.Triangular => 1 - Mathf.Abs(x * 2 - 1),
            FFTWindow.Hann => .5f * (1 - Mathf.Cos(2 * Mathf.PI * x)),
            _ => 1,
        };
    }
}

public enum FFTWindow
{
    Rectangular,
    Triangular,
    Hann
}