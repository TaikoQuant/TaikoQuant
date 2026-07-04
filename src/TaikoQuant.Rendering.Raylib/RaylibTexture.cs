using System;
using TaikoQuant.Core;
using Raylib_cs;

namespace TaikoQuant.Rendering.Raylib
{
    public class RaylibTexture : ITexture, IDisposable
    {
        public Raylib_cs.Texture2D Texture { get; }

        public RaylibTexture(Raylib_cs.Texture2D texture)
        {
            Texture = texture;
        }

        // Fallback constructor loads a 1×1 white texture.
        public RaylibTexture()
            : this(Raylib_cs.Raylib.LoadTextureFromImage(
                Raylib_cs.Raylib.GenImageColor(1, 1,
                    new Raylib_cs.Color(255, 255, 255, 255))))
        {
        }

        public int Width => Texture.Width;   // ← 正しいプロパティ名
        public int Height => Texture.Height;  // ← 正しいプロパティ名

        public void Dispose() => Raylib_cs.Raylib.UnloadTexture(Texture);
    }
}