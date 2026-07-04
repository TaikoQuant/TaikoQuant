using System;
using TaikoQuant.Core;
using Raylib_cs;

namespace TaikoQuant.Rendering.Raylib
{
    public class RaylibFont : IFont, IDisposable
    {
        public Raylib_cs.Font Font { get; }

        public RaylibFont(Raylib_cs.Font font) => Font = font;

        // Convenience constructor loads a TTF file.
        public RaylibFont(string filePath, int size)
            : this(Raylib_cs.Raylib.LoadFontEx(filePath, size, null, 0))
        {
        }

        public void Dispose() => Raylib_cs.Raylib.UnloadFont(Font);
    }
}