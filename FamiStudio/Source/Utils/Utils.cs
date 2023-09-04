﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FamiStudio
{
    static class Utils
    {
        public static int Clamp(int val, int min, int max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        public static float Clamp(float val, float min, float max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        public static double Clamp(double val, double min, double max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        public static float Saturate(float val)
        {
            return Clamp(val, 0.0f, 1.0f);
        }

        public static float Lerp(float v0, float v1, float alpha)
        {
            return v0 * (1.0f - alpha) + v1 * alpha;
        }

        public static double Lerp(double v0, double v1, double alpha)
        {
            return v0 * (1.0 - alpha) + v1 * alpha;
        }

        public static float BiLerp(float v00, float v01, float v10, float v11, float alpha0, float alpha1)
        {
            var l0 = Lerp(v00, v01, alpha0);
            var l1 = Lerp(v10, v11, alpha0);
            return Lerp(l0, l1, alpha1);
        }

        public static bool IsNearlyEqual(float a, float b, float delta = 1e-5f)
        {
            return MathF.Abs(a - b) < delta;
        }

        public static bool IsNearlyEqual(int a, int b, int delta = 10)
        {
            return MathF.Abs(a - b) < delta;
        }

        public static int SignedCeil(float x)
        {
            return (x > 0) ? (int)MathF.Ceiling(x) : (int)MathF.Floor(x);
        }

        public static int SignedFloor(float x)
        {
            return (x < 0) ? (int)MathF.Ceiling(x) : (int)MathF.Floor(x);
        }

        public static float ToHalf(ushort x)    // Will be obsolete once we completely move to .NET 6.0+
        {
            bool sign = (x & 0x8000) != 0;
            int exponent = (x & 0x7C00) >> 10;
            int significand = x & 0x03FF;
            if (exponent == 0 && significand == 0){
                return sign ? -0 : 0;
            } else if (exponent == 0 && significand != 0){
                return (float)((sign ? -1 : 1) * Math.Pow(2, -14) * (significand / 1024.0));
            } else if (exponent == 31 && significand == 0){
                return sign ? float.NegativeInfinity : float.PositiveInfinity;
            } else if (exponent == 31 && significand != 0){
                return float.NaN;
            } else {
                return (float)((sign ? -1 : 1) * Math.Pow(2, exponent-15) * (1 + significand / 1024.0));
            }
            return 0;
        }

        public static float Frac(float x)
        {
            return x - (int)x;
        }

        public static double Frac(double x)
        {
            return x - (int)x;
        }

        public static int IntegerPow(int x, int y)
        {
            int result = 1;
            for (long i = 0; i < y; i++)
                result *= x;
            return result;
        }

        public static byte[] IntToBytes24Bit(int x)
        {
            return new byte[] { (byte)(x & 0xff), (byte)(x >> 8 & 0xff), (byte)(x >> 16 & 0xff) };
        }

        public static int Bytes24BitToInt(byte[] x)
        {
            return x[0] | (x[1] << 8) | (x[2] << 16);
        }

        public static int Log2Int(int x)
        {
            if (x == 0)
                return int.MinValue;

            int bits = 0;
            while (x > 0)
            {
                bits++;
                x >>= 1;
            }
            return bits - 1;
        }

        public static int ParseIntWithTrailingGarbage(string s)
        {
            int idx = 0;

            for (; idx < s.Length; idx++)
            {
                if (!char.IsDigit(s[idx]))
                    break;
            }

            return idx == 0 ? 0 : int.Parse(s.Substring(0, idx));
        }

        public static int RoundDownAndClamp(int x, int factor, int min)
        {
            return Math.Max(RoundDown(x, factor), min);
        }

        public static int RoundUpAndClamp(int x, int factor, int max)
        {
            return Math.Min(RoundUp(x, factor), max);
        }

        public static int RoundDown(int x, int factor)
        {
            return (x / factor) * factor;
        }

        public static int RoundUp(int x, int factor)
        {
            return ((x + factor - 1) / factor) * factor;
        }

        public static int DivideAndRoundUp(int x, int y)
        {
            return (x + y - 1) / y;
        }

        public static int DivideAndRoundDown(int x, int y)
        {
            return x / y;
        }

        public static int AlignSampleOffset(int s)
        {
            return (s + 63) & 0xffc0;
        }

        public static int NumDecimalDigits(int n)
        {
            int digits = 1;
            while (n >= 10)
            {
                n /= 10;
                digits++;
            }
            return digits;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }

        public static void Swap<T>(IList<T> list, int a, int b)
        {
            T t = list[a];
            list[a] = list[b];
            list[b] = t;
        }

        public static int NextPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;

            return v;
        }

        public static int PrevPowerOfTwo(int v)
        {
            return NextPowerOfTwo(v) / 2;
        }

        public static int NumberOfSetBits(int i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        public static int NumberOfSetBits(long i)
        {
            i = i - ((i >> 1) & 0x5555555555555555L);
            i = (i & 0x3333333333333333L) + ((i >> 2) & 0x3333333333333333L);
            return (int)(unchecked(((i + (i >> 4)) & 0xF0F0F0F0F0F0F0FL) * 0x101010101010101L) >> 56);
        }

        static readonly byte[] BitLookups = new byte[]
        {
            0x0, 0x8, 0x4, 0xc, 0x2, 0xa, 0x6, 0xe,
            0x1, 0x9, 0x5, 0xd, 0x3, 0xb, 0x7, 0xf
        };

        public static byte ReverseBits(byte b)
        {
            return (byte)((BitLookups[b & 0xf] << 4) | BitLookups[b >> 4]);
        }

        public static void ReverseBits(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = ReverseBits(bytes[i]);
        }

        public static string MakeNiceAsmName(string name)
        {
            string niceName = "";
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c))
                    niceName += char.ToLower(c);
                else if (char.IsWhiteSpace(c) && niceName.Last() != '_')
                    niceName += '_';
                else if (c == '_' || c == '-')
                    niceName += c;
            }
            return niceName;
        }

        public static int[] GetFactors(int n, int start)
        {
            var factors = new List<int>();

            for (int i = start; i >= 2; i--)
            {
                if (n % i == 0)
                    factors.Add(i);
            }

            return factors.ToArray();
        }

        public static void DisposeAndNullify<T>(ref T obj) where T : IDisposable
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = default(T);
            }
        }

        public static int[] GetFactors(int n)
        {
            return GetFactors(n, n);
        }

        public static string AddFileSuffix(string filename, string suffix)
        {
            var extension = Path.GetExtension(filename);
            var filenameNoExtension = filename.Substring(0, filename.Length - extension.Length);

            return filenameNoExtension + suffix + extension;
        }

        public static void PadToNextBank(List<byte> bytes, int bankSize)
        {
            var offsetInPage = (bytes.Count & (bankSize - 1));
            if (offsetInPage != 0)
                bytes.AddRange(new byte[bankSize - offsetInPage]);
        }

        public static void PadToNextBank(ref byte[] bytes, int bankSize)
        {
            var offsetInPage = (bytes.Length & (bankSize - 1));
            if (offsetInPage != 0)
                Array.Resize(ref bytes, bankSize);
        }

        public static bool ResourceExists(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceInfo(name) != null;
        }

        public static float SmoothStep(float x)
        {
            return x * x * (3 - 2 * x);
        }

        public static float SmootherStep(float x)
        {
            return x * x * x * (x * (x * 6.0f - 15.0f) + 10.0f);
        }

        public static string GetTemporaryDiretory()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), "FamiStudio");

            try
            {
                Directory.Delete(tempFolder, true);
            }
            catch { }

            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }

        public static float DbToAmplitude(float db)
        {
            return (float)MathF.Pow(10.0f, db / 20.0f);
        }

        public static int Min(int[] array)
        {
            var min = array[0];
            for (int i = 1; i < array.Length; i++)
                min = Math.Min(min, array[i]);
            return min;
        }

        public static int Max(int[] array)
        {
            var max = array[0];
            for (int i = 1; i < array.Length; i++)
                max = Math.Max(max, array[i]);
            return max;
        }

        public static int Sum(int[] array)
        {
            var sum = array[0];
            for (int i = 1; i < array.Length; i++)
                sum += array[i];
            return sum;
        }

        public static int HashCombine(int a, int b)
        {
            return a ^ (b + unchecked((int)0x9e3779b9) + (a << 6) + (a >> 2));
        }

        public static void Permutations(int[] array, List<int[]> permutations, int idx = 0)
        {
            if (idx == array.Length)
            {
                // Avoid duplicates.
                if (permutations.FindIndex(a => CompareArrays(a, array) == 0) < 0)
                    permutations.Add(array.Clone() as int[]);
            }

            for (int i = idx; i < array.Length; i++)
            {
                Swap(ref array[idx], ref array[i]);
                Permutations(array, permutations, idx + 1);
                Swap(ref array[idx], ref array[i]);
            }
        }

        public static bool CompareFloats(float f1, float f2, float tolerance = 0.001f)
        {
            return MathF.Abs(f1 - f2) < tolerance;
        }

        public static int CompareArrays(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return int.MaxValue;

            for (int i = 0; i < a1.Length; i++)
            {
                var comp = a1[i].CompareTo(a2[i]);
                if (comp != 0)
                    return comp;
            }

            return 0;
        }

        public static int CompareArrays(int[] a1, int[] a2)
        {
            if (a1.Length != a2.Length)
                return int.MaxValue;

            for (int i = 0; i < a1.Length; i++)
            {
                var comp = a1[i].CompareTo(a2[i]);
                if (comp != 0)
                    return comp;
            }

            return 0;
        }

        public static int ComputeScrollAmount(int pos, int maxPos, int marginSize, float factor, bool minSide)
        {
            var diff = minSide ? pos - maxPos : maxPos - pos;
            var scrollAmount = 1.0f - Utils.Clamp(diff / (float)marginSize, 0.0f, 1.0f);
            return (int)(factor * scrollAmount) * (minSide ? -1 : 1);
        }

        public static string SplitVersionNumber(string version, out int betaNumber)
        {
            var dotIdx = version.LastIndexOf('.');
            betaNumber = int.Parse(version.Substring(dotIdx + 1), CultureInfo.InvariantCulture);
            return version.Substring(0, dotIdx);
        }

        public static int InterlockedMax(ref int location, int value)
        {
            int initialValue, newValue;
            do
            {
                initialValue = location;
                newValue = Math.Max(initialValue, value);
            }
            while (Interlocked.CompareExchange(ref location, newValue, initialValue) != initialValue);
            return initialValue;
        }

        public static unsafe string PtrToStringAnsi(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return "";

            var p = (byte*)ptr.ToPointer();
            var n = 0;
            for (; p[n] != 0; n++) ;

            return System.Text.Encoding.ASCII.GetString(p, n);
        }

        public static unsafe string PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return "";

            // The string is UTF8.
            var p = (byte*)ptr.ToPointer();
            var n = 0;
            for (; p[n] != 0; n++) ;

            return System.Text.Encoding.UTF8.GetString(p, n);
        }

        public static void NonBlockingParallelFor(int numItems, int maxThreads, ThreadSafeCounter counter, Func<int, int, bool> action)
        {
            var queue = new ConcurrentQueue<int>();

            for (int i = 0; i < numItems; i++)
                queue.Enqueue(i);

            for (int i = 0; i < NesApu.NUM_WAV_EXPORT_APU; i++)
            {
                var threadIndex = i; // Important, need to copy for lambda below.
                new Thread(() =>
                {
                    while (true)
                    {
                        if (!queue.TryDequeue(out var itemIndex))
                            break;

                        var keepGoing = action(itemIndex, threadIndex);
                        
                        Thread.MemoryBarrier(); // Extra safety, the increment below should generate a barrier tho.
                        counter.Increment();
                        
                        if (!keepGoing)
                            break;
                    }
                    
                }).Start();
            }
        }

        public static float Dot(float x0, float y0, float x1, float y1)
        {
            return x0 * x1 + y0 * y1;
        }

        public static float Cross(float x0, float y0, float x1, float y1)
        {
            return x0 * y1 - y0 * x1;
        }

        public static void Normalize(ref float x, ref float y)
        {
            var invLen = 1.0f / (float)MathF.Sqrt(x * x + y * y);
            x *= invLen;
            y *= invLen;
        }
    }

    public class ThreadSafeCounter
    {
        private int value = 0;
        public int Value => value;
        public void Increment()
        {
            Interlocked.Increment(ref value);
        }
    }
}
