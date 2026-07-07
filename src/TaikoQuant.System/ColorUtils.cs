using System;
using System.Globalization;
using Raylib_cs;

namespace TaikoQuant.System
{
    /// <summary>
    /// 色（Color）に関するユーティリティメソッドを提供します。
    /// 注意: このクラスは独自の Color 構造体を使用します。
    /// Raylib_cs.Color との変換については、暗黙の変換演算子を提供します。
    /// </item>
    public struct Color
    {
        public byte R, G, B, A;

        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Raylib_cs.Color への暗黙の変換。
        /// Raylib_cs.Color のコンストラクタが (a, r, g, b) であることを前提とします。
        /// </summary>
        public static implicit operator Raylib_cs.Color(Color c) => new Raylib_cs.Color(c.A, c.R, c.G, c.B);
    }

    /// <summary>
    /// 色（Color）に関するユーティリティメソッドを提供します。
    /// </summary>
    public static class ColorUtils
    {
        /// <summary>
        /// RGBA 値を 0xAARRGGBB 形式の 32 ビット整数に変換します（Alpha が上位バイト）。
        /// </summary>
        public static uint ToAbgr(Color color) => ((uint)color.A << 24) | ((uint)color.R << 16) | ((uint)color.G << 8) | color.B;

        /// <summary>
        /// 0xAARRGGBB 形式の 32 ビット整数から Color を生成します。
        /// </summary>
        public static Color FromAbgr(uint abgr)
        {
            byte a = (byte)(abgr >> 24);
            byte r = (byte)(abgr >> 16);
            byte g = (byte)(abgr >> 8);
            byte b = (byte)(abgr);
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// RGBA 値をヘックス文字列形式 "#RRGGBB" または "#AARRGGBB" に変換します。
        /// </summary>
        /// <param name="includeAlpha">アルファチャンネルを含めるかどうか（既定: false）</param>
        public static string ToHex(Color color, bool includeAlpha = false)
        {
            if (includeAlpha)
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
            else
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// "#RRGGBB" または "#AARRGGBB" 形式の文字列から Color を生成します。
        /// </summary>
        /// <param name="hex">"#RRGGBB" または "#AARRGGBB"（大文字小文字不問）</param>
        /// <returns>変換された Color。変換失敗時は magenta (255,0,255)</returns>
        public static Color FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return new Color(255, 0, 255);
            string clean = hex.TrimStart('#');
            if (clean.Length == 6) // RRGGBB
            {
                if (byte.TryParse(clean.Substring(0, 2), NumberStyles.HexNumber, null, out byte r) &&
                    byte.TryParse(clean.Substring(2, 2), NumberStyles.HexNumber, null, out byte g) &&
                    byte.TryParse(clean.Substring(4, 2), NumberStyles.HexNumber, null, out byte b))
                {
                    return new Color(r, g, b, 255);
                }
            }
            else if (clean.Length == 8) // AARRGGBB
            {
                if (byte.TryParse(clean.Substring(0, 2), NumberStyles.HexNumber, null, out byte a) &&
                    byte.TryParse(clean.Substring(2, 2), NumberStyles.HexNumber, null, out byte r) &&
                    byte.TryParse(clean.Substring(4, 2), NumberStyles.HexNumber, null, out byte g) &&
                    byte.TryParse(clean.Substring(6, 2), NumberStyles.HexNumber, null, out byte b))
                {
                    return new Color(r, g, b, a);
                }
            }
            return new Color(255, 0, 255); // fallback to magenta
        }

        /// <summary>
        /// HSV (色相、彩度、値) から RGB に変換します。
        /// 入力: h∈[0,360), s∈[0,1], v∈[0,1]
        /// 出力: r,g,b∈[0,255]
        /// </summary>
        public static Color FromHsv(float h, float s, float v, byte a = 255)
        {
            // Normalize hue to [0, 360)
            h = h % 360f;
            if (h < 0) h += 360f;

            // Clamp saturation and value to [0, 1]
            s = s < 0f ? 0f : (s > 1f ? 1f : s);
            v = v < 0f ? 0f : (v > 1f ? 1f : v);

            float c = v * s;
            float x = c * (1f - Math.Abs((h / 60f) % 2f - 1f));
            float m = v - c;

            float r1, g1, b1;
            if (h < 60f)      { r1 = c; g1 = x; b1 = 0f; }
            else if (h < 120f){ r1 = x; g1 = c; b1 = 0f; }
            else if (h < 180f){ r1 = 0f; g1 = c; b1 = x; }
            else if (h < 240f){ r1 = 0f; g1 = x; b1 = c; }
            else if (h < 300f){ r1 = x; g1 = 0f; b1 = c; }
            else             { r1 = c; g1 = 0f; b1 = x; }

            byte r = (byte)((r1 + m) * 255f);
            byte g = (byte)((g1 + m) * 255f);
            byte b = (byte)((b1 + m) * 255f);
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// RGB から HSV に変換します（色相 0-360, 彩度・値 0-1）。
        /// </summary>
        public static (float h, float s, float v) ToHsv(Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float max = Math.Max(Math.Max(r, g), b);
            float min = Math.Min(Math.Min(r, g), b);
            float delta = max - min;

            float h = 0f;
            if (delta != 0f)
            {
                if (max == r) h = ((g - b) / delta) % 6f;
                else if (max == g) h = (b - r) / delta + 2f;
                else               h = (r - g) / delta + 4f;

                h *= 60f;
                if (h < 0) h += 360f;
            }

            float s = (max == 0f) ? 0f : delta / max;
            float v = max;

            return (h, s, v);
        }
    }
}