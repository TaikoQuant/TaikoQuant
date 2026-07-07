using System;

namespace TaikoQuant.Core
{
    /// <summary>
    /// Layout constants that correspond to OpenTaiko's skin configuration.
    /// Adjust the values to match the active skin if you change the skin.
    /// </summary>
    public static class Layout
    {
        // Background positions (left & right background share Y)
        public static readonly int BackgroundX = 0;   // left background X
        public static readonly int BackgroundY = 182;   // Y for both backgrounds
        public static readonly int BackgroundRightOffsetX = 947; // right background = left + offset

        // Base (drum body) position
        public static readonly int BaseX = 205;
        public static readonly int BaseY = 206;

        // Frame (lane frame) position
        public static readonly int FrameX = 329;
        public static readonly int FrameY = 136;

        // Gauge position and size (matches OpenTaiko's default gauge)
        public static readonly int GaugeX = 492;
        public static readonly int GaugeY = 144;
        public static readonly int GaugeWidth = 700; // corresponds to width used in constructor
        public static readonly int GaugeHeight = 44; // corresponds to height used in constructor

        // Judgment line (where notes hit)
        public const int JudgeX = 411; // AviUtl X -342.75 -> (960 - 342.75) * 2/3 = 411.5 -> 411
        public const int LaneCY = 258; // Y coordinate of the lane centre (AviUtl 540 - 152.27 = 387.73 -> * 2/3 = 258)
    }
}