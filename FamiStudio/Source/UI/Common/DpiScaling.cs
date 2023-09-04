﻿using System;
using System.Diagnostics;

namespace FamiStudio
{
    public static class DpiScaling
    {
        private static bool initialized; 

        private static float windowScaling = 1;
        private static float fontScaling   = 1;
        private static bool forceUnitScale = false;

        public static bool IsInitialized => initialized;

        public static float Window { get { Debug.Assert(initialized); return forceUnitScale ? 1.0f : windowScaling; } }
        public static float Font   { get { Debug.Assert(initialized); return forceUnitScale ? 1.0f : fontScaling; } }
        public static bool ForceUnitScaling { get => forceUnitScale; set => forceUnitScale = value; }

        public static int ScaleCustom(float val, float scale)
        {
            Debug.Assert(initialized);
            return (int)Math.Round(val * scale);
        }

        public static float ScaleCustomFloat(float val, float scale)
        {
            Debug.Assert(initialized);
            return val * scale;
        }

        public static int ScaleForWindow(float val)
        {
            Debug.Assert(initialized);
            return (int)Math.Round(val * Window);
        }

        public static float ScaleForWindowFloat(float val)
        {
            Debug.Assert(initialized);
            return val * Window;
        }

        public static int ScaleForFont(float val)
        {
            Debug.Assert(initialized);
            return (int)Math.Round(val * Font);
        }

        public static float ScaleForFontFloat(float val)
        {
            Debug.Assert(initialized);
            return val * Font;
        }

        public static int[] GetAvailableScalings()
        {
            if (Platform.IsWindows || Platform.IsLinux)
                return new[] { 100, 125, 150, 175, 200, 225, 250 };
            else if (Platform.IsAndroid)
                return new[] { 66, 100, 133 };
            else if (Platform.IsMacOS)
                return new int[0]; // Intentional, we dont allow to manually set the scaling on MacOS.

            Debug.Assert(false);
            return new int[] { };
        }

        private static float RoundScaling(float value)
        {
            if (Platform.IsMacOS)
            {
                return value;
            }
            else
            {
                var scalings = GetAvailableScalings();
                var minDiff  = 100.0f;
                var minIndex = -1;

                for (int i = 0; i < scalings.Length; i++)
                {
                    var diff = Math.Abs(scalings[i] / 100.0f - value);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        minIndex = i;
                    }
                }
                return scalings[minIndex] / 100.0f;
            }
        }

        public static void Initialize(float scaling = -1.0f)
        {
            if (Platform.IsMobile)
            {
                var density = Platform.GetPixelDensity();

                if (Settings.DpiScaling != 0)
                {
                    windowScaling = Settings.DpiScaling / 100.0f;
                }
                else
                {
                    if (density < 360)
                        windowScaling = 0.666f;
                    else if (density >= 480)
                        windowScaling = 1.333f;
                    else
                        windowScaling = 1.0f;
                }

                fontScaling    = (float)Math.Round(windowScaling * 3);
                windowScaling  = (float)Math.Round(windowScaling * 6);
                forceUnitScale = false;
            }
            else
            {
                if (Settings.DpiScaling != 0)
                    windowScaling = RoundScaling(Settings.DpiScaling / 100.0f);
                else
                    windowScaling = RoundScaling(scaling);

                fontScaling = windowScaling;
            }

            initialized = true;
        }
    }
}
