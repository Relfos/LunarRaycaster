#if UNITY_5_3_OR_NEWER
#define UNITY
#endif

#if UNITY
using UnityEngine;
#endif

using System;

namespace LunarLabs.Raycaster
{

#if !UNITY
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float Length => Mathf.Sqrt(X * X + Y * Y);

        public void Normalize()
        {
            var len = this.Length;
            if (len == 0)
            {
                return;
            }

            len = 1.0f / len;
            X *= len;
            Y *= len;
        }
    }

    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Length => Mathf.Sqrt(X * X + Y * Y + Z * Z);

        public void Normalize()
        {
            var len = this.Length;
            if (len == 0)
            {
                return;
            }

            len = 1.0f / len;
            X *= len;
            Y *= len;
            Z *= len;
        }
    }
#endif

    public static class Mathf
    {
#if UNITY
        public const float PI = Mathf.PI;
        public const float Deg2Rad = Mathf.Deg2Rad;
        public const float Rad2Deg = Mathf.Rad2Deg;
#else
        public const float PI = 3.14159265359f;
        public const float Deg2Rad = PI / 180.0f;
        public const float Rad2Deg = 180.0f / PI;
#endif

        public const float PIx2 = 6.28318530717958647692f;
        public const float Epsilon = 0.2f;

        #region CORE
        public static uint NextPowerOfTwo(uint value)
        {
            if (value > 0)
            {
                value--;
                value |= (value >> 1);
                value |= (value >> 2);
                value |= (value >> 4);
                value |= (value >> 8);
                value |= (value >> 16);
            }

            return value + 1;
        }

        public static bool Blink(float time, float duration)
        {
            float mod = duration * 2;
            float delta = time % mod;
            return delta <= duration;
        }

        public static float Clamp(float val, float min, float max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        public static float Clamp01(float val)
        {
            if (val < 0) return 0;
            if (val > 1) return 1;
            return val;
        }

        public static int Sign(float x)
        {
            if (x == 0)
            {
                return 0;
            }

            return x < 0 ? -1 : 1;
        }

        public static float Frac(float x)
        {
#if UNITY
            return x - Mathf.Floor(x);
#else
            return x - (int)(x);
#endif
        }

        public static float Round(float x)
        {
#if UNITY
            return Mathf.Round(x);
#else
            return (float)System.Math.Round(x);
#endif
        }


#if UNITY
        public static Vector4 Frac(Vector4 v)
        {
            return new Vector4(Frac(v.x), Frac(v.y), Frac(v.z), Frac(v.w));
        }
#endif

        public static float Sqr(float x)
        {
            return x * x;
        }

        public static float Sqrt(float x)
        {
#if UNITY
            return Mathf.Sqrt(x);
#else
            return (float)System.Math.Sqrt(x);
#endif
        }

        public static float Pow(float f, float p)
        {
#if UNITY
            return Mathf.Pow(f, p);
#else
            return (float)System.Math.Pow(f, p);
#endif
        }

        public static float Log(float x, float power)
        {
#if UNITY
            return Mathf.Log(x, power);
#else
            return (float)System.Math.Log(x, power);
#endif
        }

        public static float Log(float x)
        {
#if UNITY
            return Mathf.Log(x);
#else
            return (float)System.Math.Log(x);
#endif
        }

        public static float Log10(float x)
        {
#if UNITY
            return Mathf.Log10(x);
#else
            return (float)System.Math.Log10(x);
#endif
        }

        public static float Exp(float x)
        {
#if UNITY
            return Mathf.Exp(x);
#else
            return (float)System.Math.Exp(x);
#endif
        }

        public static float Abs(float x)
        {
#if UNITY
            return Mathf.Abs(x);
#else
            return (float)System.Math.Abs(x);
#endif
        }

        public static int Abs(int x)
        {
#if UNITY
            return Mathf.Abs(x);
#else
            return System.Math.Abs(x);
#endif
        }

        public static int RoundToInt(float x)
        {
            return (int)Round(x);
        }

        public static int FloorToInt(float x)
        {
            return (int)Floor(x);
        }

        public static int CeilToInt(float x)
        {
            return (int)Ceiling(x);
        }

        public static float Floor(float x)
        {
#if UNITY
            return Mathf.Floor(x);
#else
            return (float)System.Math.Floor(x);
#endif
        }

        public static float Ceiling(float x)
        {
#if UNITY
            return Mathf.Floor(x);
#else
            return (float)System.Math.Ceiling(x);
#endif
        }


        public static float Sin(float x)
        {
#if UNITY
            return Mathf.Sin(x);
#else
            return (float)System.Math.Sin(x);
#endif
        }

        public static float Cos(float x)
        {
#if UNITY
            return Mathf.Cos(x);
#else
            return (float)System.Math.Cos(x);
#endif
        }

        public static float Tan(float x)
        {
#if UNITY
            return Mathf.Tan(x);
#else
            return (float)System.Math.Tan(x);
#endif
        }

        public static float Asin(float x)
        {
#if UNITY
            return Mathf.Asin(x);
#else
            return (float)System.Math.Asin(x);
#endif
        }

        public static float Acos(float x)
        {
#if UNITY
            return Mathf.Acos(x);
#else
            return (float)System.Math.Acos(x);
#endif
        }

        public static float Atan(float x)
        {
#if UNITY
            return Mathf.Atan(x);
#else
            return (float)System.Math.Atan(x);
#endif
        }

        public static float Atan2(float dy, float dx)
        {
#if UNITY
            return Mathf.Atan2(dy, dx);
#else
            return (float)System.Math.Atan2(dy, dx);
#endif
        }

        public static float DeltaAngle(float current, float target)
        {
            float num = Mathf.Repeat(target - current, 360f);
            if (num > 180f)
            {
                num -= 360f;
            }
            return num;
        }

        public static float Repeat(float t, float length)
        {
            return Clamp(t - Floor(t / length) * length, 0f, length);
        }

        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            smoothTime = Max(0.0001f, smoothTime);
            float num = 2f / smoothTime;
            float num2 = num * deltaTime;
            float num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
            float value = current - target;
            float num4 = target;
            float num5 = maxSpeed * smoothTime;
            value = Clamp(value, 0f - num5, num5);
            target = current - value;
            float num6 = (currentVelocity + num * value) * deltaTime;
            currentVelocity = (currentVelocity - num * num6) * num3;
            float num7 = target + (value + num6) * num3;
            if (num4 - current > 0f == num7 > num4)
            {
                num7 = num4;
                currentVelocity = (num7 - num4) / deltaTime;
            }
            return num7;
        }

        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            target = current + DeltaAngle(current, target);
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        public static float SmoothStep(float from, float to, float t)
        {
            t = Clamp01(t);
            t = -2f * t * t * t + 3f * t * t;
            return to * t + from * (1f - t);
        }

        public static float Min(float a, float b)
        {
#if UNITY
            return Mathf.Min(a, b);
#else
            return (float)System.Math.Min(a, b);
#endif
        }

        public static float Max(float a, float b)
        {
#if UNITY
            return Mathf.Max(a, b);
#else
            return (float)System.Math.Max(a, b);
#endif
        }

        public static int Min(int a, int b)
        {
#if UNITY
            return Mathf.Min(a, b);
#else
            return System.Math.Min(a, b);
#endif
        }

        public static int Max(int a, int b)
        {
#if UNITY
            return Mathf.Max(a, b);
#else
            return System.Math.Max(a, b);
#endif
        }

        public static void GetDirectionForAngle(float angle, float speed, out float dx, out float dy)
        {
            dx = Cos(angle) * speed;
            dy = Sin(angle) * speed;
        }

        #endregion

        #region DISTANCES
        public static float Distance(float x1, float y1, float x2, float y2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            dx *= dx;
            dy *= dy;
            return Sqrt(dx + dy);
        }

#if UNITY
        public static float Distance(Vector2 A, Vector2 B)
        {
            return Distance(A.x, A.y, B.x, B.y);
        }
#endif

        public static float DotProduct(float x1, float y1, float x2, float y2)
        {
            return x1 * x2 + y1 * y2;
        }

        public static float Angle(float x1, float y1, float x2, float y2)
        {
            return Acos(DotProduct(x1, y1, x2, y2));
        }

        #endregion

        #region CURVES
        public static float SmoothCurveWithOffset(float delta, float offset)
        {
            if (delta < offset)
            {
                delta = (delta / offset);
                return Abs(Sin(delta * PI * 0.5f));
            }
            else
            {
                delta = delta - offset;
                delta = (delta / (1.0f - offset));
                return Abs(Cos(delta * PI * 0.5f));
            }
        }

        public static float SmoothCurve(float delta)
        {
            return Abs(Sin(delta * PI));
        }

        public static float AccelerationCurve(float delta, out bool hasFinished, float accelerationTime, float duration, float deaccelerationTime = 0)
        {
            if (delta < accelerationTime)
            {
                hasFinished = false;
                return delta / accelerationTime;
            }

            if (delta >= duration)
            {
                if (deaccelerationTime > 0)
                {
                    delta -= duration;
                    delta /= deaccelerationTime;

                    hasFinished = delta >= 1.0f;
                    if (hasFinished)
                    {
                        delta = 1.0f;
                    }
                    return 1.0f - delta;
                }
                else
                {
                    hasFinished = false;
                    return 1;
                }
            }

            hasFinished = false;
            return 1.0f;
        }

        public static float CubicInterpolate(float y0, float y1, float y2, float y3, float mu)
        {
            float mu2 = (mu * mu);
            float a0 = y3 - y2 - y0 + y1;
            float a1 = y0 - y1 - a0;
            float a2 = y2 - y0;
            float a3 = y1;
            return (a0 * mu * mu2) + (a1 * mu2) + (a2 * mu) + a3;
        }

        public static float CatmullRomInterpolate(float y0, float y1, float y2, float y3, float mu)
        {
            float mu2 = (mu * mu);
            float a0 = (-0.5f * y0) + (1.5f * y1) - (1.5f * y2) + (0.5f * y3);
            float a1 = y0 - (2.5f * y1) + (2.0f * y2) - (0.5f * y3);
            float a2 = (-0.5f * y0) + (0.5f * y2);
            float a3 = y1;
            return (a0 * mu * mu2) + (a1 * mu2) + (a2 * mu) + a3;
        }

        public static float HermiteInterpolate(float pA, float pB, float vA, float vB, float u)
        {
            float u2 = (u * u);
            float u3 = u2 * u;
            float B0 = 2.0f * u3 - 3.0f * u2 + 1.0f;
            float B1 = -2.0f * u3 + 3.0f * u2;
            float B2 = u3 - 2.0f * u2 + u;
            float B3 = u3 - u;
            return (B0 * pA + B1 * pB + B2 * vA + B3 * vB);

        }

        public static float QuadraticBezierCurve(float y0, float y1, float y2, float mu)
        {

            return Sqr(1 - mu) * y0 + 2 * (1 - mu) * mu * y1 + Sqr(mu) * y2;
        }

        public static float CubicBezierCurve(float y0, float y1, float y2, float y3, float mu)
        {
            return (1 - mu) * Sqr(1 - mu) * y0 + 3 * Sqr(1 - mu) * y1 + 3 * (1 - mu) * Sqr(mu) * y2 + Sqr(mu) * mu * y3; ;
        }
        #endregion

        #region LERPING
        public static float Lerp(float first, float second, float delta)
        {
#if UNITY
            return Mathf.Lerp(first, second, delta);
#else
            delta = delta > 1 ? 1 : delta < 0 ? 0 : delta;
            //return first * (1.0f - delta) + second * delta;
            return first + delta * (second - first);
#endif
        }

        public static float InverseLerp(float min, float max, float value)
        {
#if UNITY
            return Mathf.InverseLerp(min, max, value);
#else
            return (value - min) / (max - min);
#endif
        }


        public static float LerpAngle(float a, float b, float t)
        {
            float num = Repeat(b - a, 360f);
            if (num > 180f)
            {
                num -= 360f;
            }
            return a + num * Clamp01(t);
        }

        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Abs(target - current) <= maxDelta)
            {
                return target;
            }
            return current + Sign(target - current) * maxDelta;
        }

        public static float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            float num = DeltaAngle(current, target);
            if (0f - maxDelta < num && num < maxDelta)
            {
                return target;
            }
            target = current + num;
            return MoveTowards(current, target, maxDelta);
        }

        public static float PingPong(float t, float length)
        {
            t = Repeat(t, length * 2f);
            return length - Abs(t - length);
        }

        // Some quadrilateral with position vectors a, b, c, and d.
        // a---b
        // |     |
        // c---d

        // u = relative position on the "horizontal" axis between a and b, or c and d.
        // v = relative position on the "vertical" axis between a and c, or b and d.

#if UNITY
        public static Vector3 BilinearLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u, float v)
        {
            Vector3 ab = Vector3.Lerp(a, b, u); // interpolation of a and b by u
            Vector3 cd = Vector3.Lerp(c, d, u); // interpolation of c and d by u
            return Vector3.Lerp(ab, cd, v); // interpolation of the interpolation of a and b and c and d by u, by v
        }
#endif

        #endregion

        public static bool Approximately(float value1, float value2)
        {
            var delta = value1 - value2;
            return (delta >= -Epsilon) && (delta <= Epsilon);
        }


        public static bool IsNotZero(float value)
        {
            return (value < -Epsilon) || (value > Epsilon);
        }


        public static bool IsZero(float value)
        {
            return (value >= -Epsilon) && (value <= Epsilon);
        }


        public static bool AbsoluteIsOverThreshold(float value, float threshold)
        {
            return (value < -threshold) || (value > threshold);
        }

        //Normalizes any number to an arbitrary range 
        //by assuming the range wraps around when going below min or above max 
        public static float Normalize(float value, float start, float end)
        {
            var width = end - start;   // 
            var offsetValue = value - start;   // value relative to 0

            return (offsetValue - (Floor(offsetValue / width) * width)) + start;
            // + start to reset back to start of original range
        }

        public static float NormalizeAngle(float angle)
        {
            return Normalize(angle, 0, 360 * Rad2Deg);
        }

        public static float ApplySmoothing(float thisValue, float lastValue, float deltaTime, float sensitivity)
        {
            // 1.0f and above is instant (no smoothing).
            if (Approximately(sensitivity, 1.0f))
            {
                return thisValue;
            }

            // Apply sensitivity (how quickly the value adapts to changes).
            var maxDelta = deltaTime * sensitivity * 100.0f;

            // Snap to zero when changing direction quickly.
            if (Sign(lastValue) != Sign(thisValue))
            {
                lastValue = 0.0f;
            }

            return MoveTowards(lastValue, thisValue, maxDelta);
        }


        public static float ApplySnapping(float value, float threshold)
        {
            if (value < -threshold)
            {
                return -1.0f;
            }

            if (value > threshold)
            {
                return 1.0f;
            }

            return 0.0f;
        }

        public static float Min(float v0, float v1, float v2, float v3)
        {
            var r0 = (v0 >= v1) ? v1 : v0;
            var r1 = (v2 >= v3) ? v3 : v2;
            return (r0 >= r1) ? r1 : r0;
        }


        public static float Max(float v0, float v1, float v2, float v3)
        {
            var r0 = (v0 <= v1) ? v1 : v0;
            var r1 = (v2 <= v3) ? v3 : v2;
            return (r0 <= r1) ? r1 : r0;
        }


        #region COLOR
#if UNITY
        //NOTE: values only valid for 1024 x 32
        private static Vector3 coord_scale = new Vector4(0.0302734375f, 0.96875f, 31.0f);
        private static Vector4 coord_offset = new Vector4(0.00048828125f, 0.015625f, 0.0f, 0.0f);
        private static Vector2 texel_height_X0 = new Vector2(0.03125f, 0.0f);

        private static Color LUTSample(Texture2D LUT, int red0, int green0, int blue, float u, float v)
        {
            int red1 = red0 < 31 ? red0 + 1 : red0;
            int green1 = green0 < 31 ? green0 + 1 : green0;

            Color c00 = LUT.GetPixel(blue * 32 + red0, green0);
            Color c10 = LUT.GetPixel(blue * 32 + red1, green0);
            Color c11 = LUT.GetPixel(blue * 32 + red1, green1);
            Color c01 = LUT.GetPixel(blue * 32 + red0, green1);

            Color ab = Color.Lerp(c00, c10, u); // interpolation of a and b by u
            Color cd = Color.Lerp(c01, c11, u); // interpolation of c and d by u
            return Color.Lerp(ab, cd, v); // interpolation of the interpolation of a and b and c and d by u, by v
        }

        public static Color32 LUTTransform(Color32 color, Texture2D LUT)
        {
            /*Vector4 coord = new Vector4(color.r * coord_scale.x, color.g * coord_scale.y, color.b * coord_scale.z, 0);
            coord += coord_offset;

            Vector4 coord_frac = Frac(coord);
            Vector4 coord_floor = coord - coord_frac;

            Vector2 coord_bot = new Vector2(coord.x + coord_floor.z * texel_height_X0.x, coord.y + coord_floor.z * texel_height_X0.y);
            Vector2 coord_top = coord_bot + texel_height_X0;

            Color lutcol_bot = LUT.GetPixelBilinear(coord_bot.x, coord_bot.y);
            Color lutcol_top = LUT.GetPixelBilinear(coord_top.x, coord_top.y);

            //Color lutcol_bot = LUT.GetPixel((int)(coord_bot.x * LUT.width), (int)(coord_bot.y * LUT.height));
            //Color lutcol_top = LUT.GetPixel((int)(coord_top.x * LUT.width), (int)(coord_top.x * LUT.height));

            return Color.Lerp(lutcol_bot, lutcol_top, coord_frac.z);
            */

            float div = 1.0f / 8.0f;
            float red = (float)color.r * div;
            float green = (float)color.g * div;
            float blue = (float)color.b * div;

            float u = Frac(red);
            float v = Frac(green);
            float w = Frac(blue);

            int x = Mathf.FloorToInt(red);
            int y = Mathf.FloorToInt(green);
            int z0 = Mathf.FloorToInt(blue);
            int z1 = z0 < 31 ? z0 + 1 : z0;

            Color A = LUTSample(LUT, x, y, z0, u, v);
            Color B = LUTSample(LUT, x, y, z0, u, v);

            return Color.Lerp(A, B, w);
        }

#endif
        #endregion

        #region RANDOM
        /// <summary>
        ///   Generates normally distributed numbers. Each operation makes two Gaussians for the price of one, and apparently they can be cached or something for better performance, but who cares.
        /// </summary>
        /// <param name="r"></param>
        /// <param name = "mu">Mean of the distribution</param>
        /// <param name = "sigma">Standard deviation</param>
        /// <returns></returns>
        public static float RandomGaussian(float mu = 0, float sigma = 1)
        {
            var u1 = RandomFloat(0, 1);
            var u2 = RandomFloat(0, 1);

            var rand_std_normal = Sqrt(-2.0f * Log(u1)) * Sin(2.0f * PI * u2);

            var rand_normal = mu + sigma * rand_std_normal;

            return rand_normal;
        }

        public static float RandomAngle(float minDegrees, float maxDegrees, float step = 1.0f)
        {
            return Deg2Rad * RandomFloat(minDegrees, maxDegrees);
        }

#if !UNITY
        private static Random _random = new Random();
#endif

        public static float RandomFloat(float min, float max)
        {
#if UNITY
            return UnityEngine.Random.Range(min, max);
#else
            return min + (float)(_random.NextDouble() * (max - min));
#endif
        }

        /// <summary>
        /// Returns a value between min and max - 1 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandomInt(int min, int max)
        {
#if UNITY
            return UnityEngine.Random.Range(min, max);
#else
            return min + _random.Next(max - min);
#endif
        }

        public static T RandomEnum<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(RandomInt(0, v.Length));
        }

        private static float Hash(float n) { return Frac(Sin(n) * 1e4f); }

        public static float Noise(float x)
        {
            float i = Floor(x);
            float f = Frac(x);
            float u = f * f * (3.0f - 2.0f * f);
            return Lerp(Hash(i), Hash(i + 1.0f), u);
        }


        #endregion

        #region PERCENT
        public static int Percent(float current, float max)
        {
            float p = current / max;
            if (p > 1)
            {
                p = 1;
            }

            return (int)(p * 100);
        }

        public static int Percent(float current, float min, float max)
        {
            float p = (current - min) / (max - min);
            if (p > 1)
            {
                p = 1;
            }

            return (int)(p * 100);
        }
        #endregion


        /// <summary>
        /// Maps a int value to a enum of type T.  The value can be bigger than the max enum value, it will be % into the correct range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T MapToEnum<T>(int value)
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(value % v.Length);
        }
    }
}
