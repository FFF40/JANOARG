using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JANOARG.Shared.Data.ChartInfo
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
        Bounce
    }

    [Serializable]
    public class Ease
    {
        public Func<float, float> In;
        public Func<float, float> Out;
        public Func<float, float> InOut;

        public static float Get(float x, EaseFunction easeFunc, EaseMode mode)
        {
            Ease ease = sEases[(int)easeFunc];
            x = Mathf.Clamp01(x);

            return mode switch
            {
                EaseMode.In => ease.In(x),
                EaseMode.Out => ease.Out(x),
                _ => ease.InOut(x)
            };
        }

        // We don't need DOTween, guys
        public static IEnumerator Animate(float duration, Action<float> callback)
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
            float minPosition;

            bool finished;

            void f_update(float x)
            {
                text.ForceMeshUpdate();
                finished = true;
                minPosition = float.NaN;

                foreach (TMP_CharacterInfo charInfo in text.textInfo.characterInfo)
                {
                    if (!charInfo.isVisible) continue;

                    if (!float.IsFinite(minPosition)) minPosition = charInfo.vertex_BL.position.x;
                    float prog = Mathf.Clamp01((x - xOffset * (charInfo.vertex_BL.position.x - minPosition)) / duration);
                    letterCallback(charInfo, prog);
                    if (prog < 1) finished = false;
                }

                var index = 0;

                foreach (TMP_MeshInfo meshInfo in text.textInfo.meshInfo)
                {
                    meshInfo.mesh.vertices = meshInfo.vertices;
                    text.UpdateGeometry(meshInfo.mesh, index);
                    index++;
                }
            }

            float elapsedTime = 0;

            while (true)
            {
                f_update(elapsedTime);

                if (finished) break;

                yield return null;

                elapsedTime += Time.deltaTime;
            }
        }

        public static Ease[] sEases;

        static Ease()
        {
            sEases = new Ease[Enum.GetValues(typeof(EaseFunction)).Length];

            sEases[(int)EaseFunction.Linear] = new Ease
            {
                In = (x) => x,
                Out = (x) => x,
                InOut = (x) => x
            };

            sEases[(int)EaseFunction.Sine] = new Ease
            {
                In = (x) => 1 - Mathf.Cos(x * Mathf.PI / 2),
                Out = (x) => Mathf.Sin(x * Mathf.PI / 2),
                InOut = (x) => (1 - Mathf.Cos(x * Mathf.PI)) / 2
            };

            sEases[(int)EaseFunction.Quadratic] = new Ease
            {
                In = (x) => x * x,
                Out = (x) => 1 - Mathf.Pow(1 - x, 2),
                InOut = (x) => x < 0.5f
                    ? 2 * x * x
                    : 1 - Mathf.Pow(-2 * x + 2, 2) / 2
            };

            sEases[(int)EaseFunction.Cubic] = new Ease
            {
                In = (x) => x * x * x,
                Out = (x) => 1 - Mathf.Pow(1 - x, 3),
                InOut = (x) => x < 0.5f
                    ? 4 * x * x * x
                    : 1 - Mathf.Pow(-2 * x + 2, 3) / 2
            };

            sEases[(int)EaseFunction.Quartic] = new Ease
            {
                In = (x) => x * x * x * x,
                Out = (x) => 1 - Mathf.Pow(1 - x, 4),
                InOut = (x) => x < 0.5f
                    ? 8 * x * x * x * x
                    : 1 - Mathf.Pow(-2 * x + 2, 4) / 2
            };

            // For fuck's sake, why do C# not have an exponent operator??
            // Maybe exponent is not ALU standard
            sEases[(int)EaseFunction.Quintic] = new Ease
            {
                In = (x) => x * x * x * x * x,
                Out = (x) => 1 - Mathf.Pow(1 - x, 5),
                InOut = (x) => x < 0.5f
                    ? 16 * x * x * x * x * x
                    : 1 - Mathf.Pow(-2 * x + 2, 5) / 2
            };

            sEases[(int)EaseFunction.Exponential] = new Ease
            {
                In = (x) => x == 0
                    ? 0
                    : Mathf.Pow(2, 10 * x - 10) - 0.0009765625f * (1 - x),
                Out = (x) => Mathf.Approximately(x, 1)
                    ? 1
                    : 1 - Mathf.Pow(2, -10 * x) + 0.0009765625f * x,
                InOut = (x) => x == 0
                    ? 0
                    : Mathf.Approximately(x, 1)
                        ? 1
                        : x < 0.5
                            ? Mathf.Pow(2, 20 * x - 10) / 2 - 0.0009765625f * (1 - x)
                            : (2 - Mathf.Pow(2, -20 * x + 10)) / 2 + 0.0009765625f * x
            };

            sEases[(int)EaseFunction.Circle] = new Ease
            {
                In = (x) => 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2)),
                Out = (x) => Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2)),
                InOut = (x) => x < 0.5
                    ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2
                    : (Mathf.Sqrt(1 - Mathf.Pow(-2 * x + 2, 2)) + 1) / 2
            };

            sEases[(int)EaseFunction.Back] = new Ease
            {
                In = (x) =>
                {
                    const float OVERSHOOT = 1.70158f;

                    return 2.70158f * x * x * x - OVERSHOOT * x * x;
                },
                Out = (x) =>
                {
                    const float OVERSHOOT = 1.70158f;

                    return 1 + 2.70158f * Mathf.Pow(x - 1, 3) + OVERSHOOT * Mathf.Pow(x - 1, 2);
                },
                InOut = (x) =>
                {
                    const float OVERSHOOT = 1.70158f;
                    const float SCALED_OVERSHOOT = OVERSHOOT * 1.525f;

                    return x < 0.5f
                        ? Mathf.Pow(2 * x, 2) * ((SCALED_OVERSHOOT + 1) * 2 * x - SCALED_OVERSHOOT) / 2
                        : (Mathf.Pow(2 * x - 2, 2) * ((SCALED_OVERSHOOT + 1) * (x * 2 - 2) + SCALED_OVERSHOOT) + 2) / 2;
                }
            };

            sEases[(int)EaseFunction.Elastic] = new Ease
            {
                In = (x) =>
                {
                    const float PERIOD = Mathf.PI * 2 / 3;

                    if (x == 0) return 0;
                    if (Mathf.Approximately(x, 1)) return 1;

                    return -Mathf.Pow(2, 10 * x - 10) * Mathf.Sin((x * 10 - 10.75f) * PERIOD);
                },
                Out = (x) =>
                {
                    const float PERIOD = Mathf.PI * 2 / 3;

                    if (x == 0)
                        return 0;

                    if (Mathf.Approximately(x, 1))
                        return 1;

                    return Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * PERIOD) + 1;
                },
                InOut = (x) =>
                {
                    const float PERIOD = Mathf.PI * 2 / 4.5f;

                    if (x == 0)
                        return 0;

                    if (Mathf.Approximately(x, 1))
                        return 1;

                    if (x < 0.5)
                        return -(Mathf.Pow(2, 20 * x - 10) * Mathf.Sin((20 * x - 11.125f) * PERIOD)) / 2;

                    return Mathf.Pow(2, -20 * x + 10) * Mathf.Sin((20 * x - 11.125f) * PERIOD) / 2 + 1;
                }
            };

            sEases[(int)EaseFunction.Bounce] = new Ease
            {
                In = (x) => 1 - Get(1 - x, EaseFunction.Bounce, EaseMode.Out),
                Out = (x) =>
                {
                    const float BOUNCE_CONSTANT = 7.5625f;
                    const float BOUNCE_THRESHOLD = 2.75f;

                    if (x < 1 / BOUNCE_THRESHOLD)
                        return BOUNCE_CONSTANT * Mathf.Pow(x, 2);


                    if (x < 2 / BOUNCE_THRESHOLD)
                        return BOUNCE_CONSTANT * (x -= 1.5f / BOUNCE_THRESHOLD) * x + 0.75f;

                    if (x < 2.5 / BOUNCE_THRESHOLD)
                        return BOUNCE_CONSTANT * (x -= 2.25f / BOUNCE_THRESHOLD) * x + 0.9375f;

                    return BOUNCE_CONSTANT * (x -= 2.625f / BOUNCE_THRESHOLD) * x + 0.984375f;
                },
                InOut = (x) => x < 0.5
                    ? (1 - Get(1 - 2 * x, EaseFunction.Bounce, EaseMode.Out)) / 2
                    : (1 + Get(2 * x - 1, EaseFunction.Bounce, EaseMode.Out)) / 2
            };
        }
    }


    public interface IEaseDirective
    {
        public float Get(float x);
    }

    [Serializable]
    public struct BasicEaseDirective : IEaseDirective
    {
        public EaseFunction Function;
        public EaseMode     Mode;

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
        private const int   _BINARY_SEARCH_MAX_ITERATIONS = 10;
        private const float _BINARY_SEARCH_PRECISION      = 1e-6f;
        private const int   _NEWTON_ITERATIONS            = 4;
        private const float _NEWTON_MIN_SLOPE             = 0.001f;

        public readonly Vector2 Point1;
        public readonly Vector2 Point2;

        private readonly float[] _Samples;

        public CubicBezierEaseDirective(Vector2 point1, Vector2 point2)
        {
            Validate(point1);
            Validate(point2);
            Point1 = point1;
            Point2 = point2;
            _Samples = new float[11];
            UpdateSamples();
        }

        public CubicBezierEaseDirective(float point1X, float point1Y, float point2X, float point2Y)
            : this(new Vector2(point1X, point1Y), new Vector2(point2X, point2Y))
        {
        }

        private static void Validate(Vector2 point)
        {
            if (point.x is < 0 or > 1)
                throw new ArgumentException("X value of control points must be within [0, 1] range");
        }

        private void UpdateSamples()
        {
            float step = 1.0f / (_Samples.Length - 1);
            for (var t = 0; t < _Samples.Length - 1; t++) _Samples[t] = GetBezier(t * step, Point1.x, Point2.x);
        }

        public float Get(float x)
        {
            if (x == 0 || Mathf.Approximately(x, 1)) return x;

            int nIndex = Array.FindIndex(_Samples, n => n > x);
            nIndex = Math.Max(nIndex, 1);

            float step = 1.0f / (_Samples.Length - 1);
            float t = (nIndex - Mathf.InverseLerp(_Samples[nIndex], _Samples[nIndex - 1], x)) * step;
            float slope = GetBezierSlope(t, Point1.x, Point2.x);

            if (slope >= _NEWTON_MIN_SLOPE)
                t = NewtonSolveApprox(x, t);
            else if (slope != 0) t = BinarySearchApprox(x, nIndex * step, nIndex * step + step);

            float ans = GetBezier(t, Point1.y, Point2.y);

            return ans;
        }

        private float GetBezier(float t, float point1, float point2)
        {
            float cubicCoefficient = 1 - 3 * point2 + 3 * point1;
            float quadraticCoefficient = 3 * point2 - 6 * point1;
            float linearCoefficient = 3 * point1;

            return ((cubicCoefficient * t + quadraticCoefficient) * t + linearCoefficient) * t;
        }

        private float GetBezierSlope(float t, float point1, float point2)
        {
            float cubicCoefficient = 1 - 3 * point2 + 3 * point1;
            float quadraticCoefficient = 3 * point2 - 6 * point1;
            float linearCoefficient = 3 * point1;

            return 3 * cubicCoefficient * t * t + 2 * quadraticCoefficient * t + linearCoefficient;
        }

        private float BinarySearchApprox(float x, float minBound, float maxBound)
        {
            float t, xDiff, i = 0;

            do
            {
                t = (minBound + maxBound) / 2;
                xDiff = GetBezier(t, Point1.x, Point2.x) - x;

                if (xDiff < 0) minBound = t;
                else maxBound = t;
            }
            while (Mathf.Abs(xDiff) < _BINARY_SEARCH_PRECISION && i < _BINARY_SEARCH_MAX_ITERATIONS);

            return t;
        }

        private float NewtonSolveApprox(float targetX, float initialGuess)
        {
            for (var i = 0; i < _NEWTON_ITERATIONS; i++)
            {
                float slope = GetBezierSlope(initialGuess, Point1.x, Point2.x);

                if (slope == 0)
                    return initialGuess;

                float currentX = GetBezier(initialGuess, Point1.x, Point2.x);

                if (Mathf.Approximately(targetX, currentX))
                    return initialGuess;

                initialGuess -= (currentX - targetX) / slope;
                initialGuess = Mathf.Clamp01(initialGuess);
            }

            return initialGuess;
        }

        public override string ToString()
        {
            return "CubicBézier";
        }
    }
}