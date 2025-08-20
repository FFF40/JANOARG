using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JANOARG.Shared.Script.Data.ChartInfo
{
    [Serializable]
    public enum EaseMode 
    {
        In, Out, InOut
    }

    [Serializable]
    public enum EaseFunction
    {
        Linear,
        Sine,
        Quadratic,
        Cubic,
        Quartic,
        Quintic,
        Exponential,
        Circle,
        Back,
        Elastic,
        Bounce,
    }

    [Serializable]
    public class Ease 
    {
        public Func<float, float> In;
        public Func<float, float> Out;
        public Func<float, float> InOut;

        public static float Get(float x, EaseFunction ease, EaseMode mode) 
        {
            Ease _ease = Eases[ease];
            var func = _ease.InOut;
            if (mode == EaseMode.In) func = _ease.In;
            if (mode == EaseMode.Out) func = _ease.Out;
            return func(Mathf.Clamp01(x));
        }

        // We don't need DOTween, guys
        public static IEnumerator Animate(float duration, System.Action<float> callback)
        {
            for (float a = 0; a < 1; a += Time.deltaTime / duration)
            {
                callback(a);
                yield return null;
            }
            callback(1);
        }

        public static IEnumerator AnimateText(TMP_Text text, float duration, float xOffset, Action<TMP_CharacterInfo, float> letterCallback)
        {
            float minPos = float.NaN;

            bool finished;
            void update(float x)
            {
                text.ForceMeshUpdate();
                finished = true;
                minPos = float.NaN;
                foreach (var charInfo in text.textInfo.characterInfo) 
                {
                    if (!charInfo.isVisible) continue;
                    if (!float.IsFinite(minPos)) minPos = charInfo.vertex_BL.position.x;
                    float prog = Mathf.Clamp01((x - xOffset * (charInfo.vertex_BL.position.x - minPos)) / duration);
                    letterCallback(charInfo, prog);
                    if (prog < 1) finished = false;
                }
                int index = 0;
                foreach (var meshInfo in text.textInfo.meshInfo) 
                {
                    meshInfo.mesh.vertices = meshInfo.vertices;
                    text.UpdateGeometry(meshInfo.mesh, index);
                    index++;
                }
            }

            float a = 0;
            while (true)
            {
                update(a);
                if (finished) break;
                yield return null;
                a += Time.deltaTime;
            }
        }

        public static Dictionary<EaseFunction, Ease> Eases = new () {
            {EaseFunction.Linear, new Ease {
                In = (x) => x,
                Out = (x) => x,
                InOut = (x) => x,
            }},
            {EaseFunction.Sine, new Ease {
                In = (x) => 1 - Mathf.Cos((x * Mathf.PI) / 2),
                Out = (x) => Mathf.Sin((x * Mathf.PI) / 2),
                InOut = (x) => (1 - Mathf.Cos(x * Mathf.PI)) / 2,
            }},
            {EaseFunction.Quadratic, new Ease {
                In = (x) => x * x,
                Out = (x) => 1 - Mathf.Pow(1 - x, 2),
                InOut = (x) => x < 0.5f ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2,
            }},
            {EaseFunction.Cubic, new Ease {
                In = (x) => x * x * x,
                Out = (x) => 1 - Mathf.Pow(1 - x, 3),
                InOut = (x) => x < 0.5f ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2,
            }},
            {EaseFunction.Quartic, new Ease {
                In = (x) => x * x * x * x,
                Out = (x) => 1 - Mathf.Pow(1 - x, 4),
                InOut = (x) => x < 0.5f ? 8 * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 4) / 2,
            }},
            {EaseFunction.Quintic, new Ease {
                In = (x) => x * x * x * x * x,
                Out = (x) => 1 - Mathf.Pow(1 - x, 5),
                InOut = (x) => x < 0.5f ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2,
            }},
            {EaseFunction.Exponential, new Ease {
                In = (x) => x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10) - 0.0009765625f * (1 - x),
                Out = (x) => x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x) + 0.0009765625f * x,
                InOut = (x) => x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? Mathf.Pow(2, 20 * x - 10) / 2 - 0.0009765625f * (1 - x) : (2 - Mathf.Pow(2, -20 * x + 10)) / 2 + 0.0009765625f * x,
            }},
            {EaseFunction.Circle, new Ease {
                In = (x) => 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2)),
                Out = (x) => Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2)),
                InOut = (x) => x < 0.5 ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2 : (Mathf.Sqrt(1 - Mathf.Pow(-2 * x + 2, 2)) + 1) / 2,
            }},
            {EaseFunction.Back, new Ease {
                In = (x) => 2.70158f * x * x * x - 1.70158f * x * x,
                Out = (x) => 1 + 2.70158f * Mathf.Pow(x - 1, 3) + 1.70158f * Mathf.Pow(x - 1, 2),
                InOut = (x) => {
                    const float c1 = 1.70158f;
                    const float c2 = c1 * 1.525f;

                    return x < 0.5f
                        ? (Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
                        : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
                }
            }},
            {EaseFunction.Elastic, new Ease {
                In = (x) => {
                    const float c3 = Mathf.PI * 2 / 3;
                    return x == 0 ? 0 : x == 1 ? 1
                        : -Mathf.Pow(2, 10 * x - 10) * Mathf.Sin((x * 10 - 10.75f) * c3);
                },
                Out = (x) => {
                    const float c3 = Mathf.PI * 2 / 3;
                    return x == 0 ? 0 : x == 1 ? 1
                        : Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c3) + 1;
                },
                InOut = (x) => {
                    const float c5 = Mathf.PI * 2 / 4.5f;
                    return x == 0 ? 0 : x == 1 ? 1
                        : x < 0.5 ? -(Mathf.Pow(2, 20 * x - 10) * Mathf.Sin((20 * x - 11.125f) * c5)) / 2
                        : Mathf.Pow(2, -20 * x + 10) * Mathf.Sin((20 * x - 11.125f) * c5) / 2 + 1;
                }
            }},
            {EaseFunction.Bounce, new Ease {
                In = (x) => 1 - Get(1 - x, EaseFunction.Bounce, EaseMode.Out),
                Out = (x) => {
                    const float n1 = 7.5625f;
                    const float d1 = 2.75f;

                    if (x < 1 / d1) {
                        return n1 * x * x;
                    } else if (x < 2 / d1) {
                        return n1 * (x -= 1.5f / d1) * x + 0.75f;
                    } else if (x < 2.5 / d1) {
                        return n1 * (x -= 2.25f / d1) * x + 0.9375f;
                    } else {
                        return n1 * (x -= 2.625f / d1) * x + 0.984375f;
                    }
                },
                InOut = (x) => x < 0.5 ? (1 - Get(1 - 2 * x, EaseFunction.Bounce, EaseMode.Out)) / 2 : (1 + Get(2 * x - 1, EaseFunction.Bounce, EaseMode.Out)) / 2,
            }},
        };
    }


    public interface IEaseDirective
    {
        public float Get(float x);
    }

    [Serializable]
    public struct BasicEaseDirective : IEaseDirective
    {
        public EaseFunction Function;
        public EaseMode Mode;

        public BasicEaseDirective(EaseFunction function, EaseMode mode)
        {
            Function = function;
            Mode = mode;
        }

        public float Get(float x) 
        {
            return Ease.Get(x, Function, Mode);
        }

        public override string ToString() 
        {
            if (Function == EaseFunction.Linear) return "Linear";
            return Function + "/" + (Mode == EaseMode.In ? "I" : Mode == EaseMode.Out ? "O" : "IO");
        }
    }

    [Serializable]
    public struct CubicBezierEaseDirective : IEaseDirective
    {
        const int BINARY_SEARCH_MAX_ITERATIONS = 10;
        const float BINARY_SEARCH_PRECISION = 1e-6f;
        const int NEWTON_ITERATIONS = 4;
        const float NEWTON_MIN_SLOPE = 0.001f;

        public readonly Vector2 P1;
        public readonly Vector2 P2;

        readonly float[] samples;

        public CubicBezierEaseDirective(Vector2 p1, Vector2 p2)
        {
            Validate(p1); Validate(p2);
            P1 = p1; P2 = p2;
            samples = new float[11];
            UpdateSamples();
        }

        public CubicBezierEaseDirective(float p1x, float p1y, float p2x, float p2y)
            : this(new (p1x, p1y), new(p2x, p2y)) {}

        static void Validate(Vector2 p) 
        {
            if (p.x is < 0 or > 1) throw new ArgumentException("X value of control points must be within [0, 1] range");
        }

        void UpdateSamples() 
        {   
            float step = 1.0f / (samples.Length - 1);
            for (int a = 0; a < samples.Length - 1; a++) 
            {
                samples[a] = getBezier(a * step, P1.x, P2.x);
            }
        }

        public float Get(float x) 
        {
            if (x == 0 || x == 1) return x;

            int nIndex = Array.FindIndex(samples, n => n > x);
            nIndex = Math.Max(nIndex, 1);

            float step = 1.0f / (samples.Length - 1);
            float t = (nIndex - Mathf.InverseLerp(samples[nIndex], samples[nIndex - 1], x)) * step;
            float slope = getBezierSlope(t, P1.x, P2.x);

            if (slope >= NEWTON_MIN_SLOPE) {
                t = newtonSolveApprox(x, t);
            } else if (slope != 0) {
                t = binarySearchApprox(x, nIndex * step, nIndex * step + step);
            }


            float ans = getBezier(t, P1.y, P2.y);
            return ans; 
        }

        float getBezier(float t, float p1, float p2) 
        {
            float a = 1 - 3 * p2 + 3 * p1;
            float b = 3 * p2 - 6 * p1;
            float c = 3 * p1;
            return ((a * t + b) * t + c) * t;
        }

        float getBezierSlope(float t, float p1, float p2) 
        {
            float a = 1 - 3 * p2 + 3 * p1;
            float b = 3 * p2 - 6 * p1;
            float c = 3 * p1;
            return 3 * a * t * t + 2 * b * t + c;
        }

        float binarySearchApprox(float x, float minBound, float maxBound)
        {
            float t, xDiff, i = 0;
            do
            {
                t = (minBound + maxBound) / 2;
                xDiff = getBezier(t, P1.x, P2.x) - x;
                if (xDiff < 0) minBound = t;
                else maxBound = t;
            } 
            while (Mathf.Abs(xDiff) < BINARY_SEARCH_PRECISION && i < BINARY_SEARCH_MAX_ITERATIONS);
            return t;
        }

        float newtonSolveApprox(float x, float t) {
            int i;
            for (i = 0; i < NEWTON_ITERATIONS; i++) 
            {
                float dx = getBezierSlope(t, P1.x, P2.x);
                if (dx == 0) return t;
                float x2 = getBezier(t, P1.x, P2.x);
                if (x == x2) return t;
                t -= (x2 - x) / dx;
                t = Mathf.Clamp01(t);
            }
            return t;
        }

        public override string ToString() 
        {
            return "CubicBézier";
        }
    }
}