using System;
using Unity.VisualScripting;
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
        float falloff = (getfall(window) - 1) / array.Length;
        for (int i = 0; i < size; i++) array[i] = re[i] * re[i] + im[i] * im[i] * (1 + falloff * i);
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
            float angle = -2 * Mathf.PI / step;
            float reS = Mathf.Cos(angle);
            float imS = Mathf.Sin(angle);   
            for (int k = 0; k < length; k += step) 
            {
                float reW = 1;
                float imW = 0;
                for (int j = 0; j < step / 2; j++) 
                {
                    float reP = re[k + j];
                    float imP = im[k + j];
                    
                    float reQ = re[k + j + step / 2];
                    float imQ = im[k + j + step / 2];
                    cmpmul(reQ, imQ, reW, imW, out reQ, out imQ);

                    re[k + j] = reP + reQ;
                    im[k + j] = imP + imQ;
                    re[k + j + step / 2] = reP - reQ;
                    im[k + j + step / 2] = imP - imQ;

                    cmpmul(reW, imW, reS, imS, out reW, out imW);
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
            FFTWindow.Hamming => cossum(x, 25/46f, 21/46f),
            FFTWindow.Hann => cossum(x, .5f, .5f),
            FFTWindow.Blackman => cossum(x, 7938/18608f, 9240/18608f, 1430/18608f),
            FFTWindow.BlackmanNuttal => cossum(x, 0.3635819f, 0.4891775f, 0.1365995f, 0.0106411f),
            FFTWindow.BlackmanHarris => cossum(x, 0.35875f, 0.48829f, 0.14128f, 0.01168f),
            FFTWindow.FlatTop => cossum(x, 0.21557895f, 0.41663158f, 0.277263158f, 0.083578947f, 0.006947368f),
            _ => 1,
        };
    }

    static float getfall(FFTWindow window) 
    {
        return window switch
        {
            FFTWindow.Rectangular => 1,
            FFTWindow.Triangular => 25,
            _ => 20,
        };
    }

    static float cossum(float x, params float[] k)
    {
        float w = k[0];
        for (int i = 0; i < k.Length; i++) w += k[i] * Mathf.Cos(2 * Mathf.PI * i * x) * (i % 2 == 0 ? 1 : -1);
        return x;
    }

    static void cmpmul(float reX, float imX, float reY, float imY, out float reAns, out float imAns) {
        reAns = reX * reY - imX * imY;
        imAns = reX * imY + imX * reY;
    }
}

public enum FFTWindow
{
    Rectangular,
    Triangular,
    Hamming,
    Hann,
    Blackman,
    BlackmanNuttal,
    BlackmanHarris,
    FlatTop
}