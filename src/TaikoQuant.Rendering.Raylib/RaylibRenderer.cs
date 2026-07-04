using System;
using TaikoQuant.Core;
using Raylib_cs;
using static Raylib_cs.Raylib;   // Pull static Raylib‑cs functions into scope

namespace TaikoQuant.Rendering.Raylib
{
    public class RaylibRenderer : IRenderer, IDisposable
    {
        // Window dimensions – Raylib supplies these helpers.
        public int Width => GetScreenWidth();
        public int Height => GetScreenHeight();

        public void BeginScene() => BeginDrawing();
        public void EndScene() => EndDrawing();

        public void Clear(uint color) =>
            ClearBackground(new Raylib_cs.Color(
                (int)(color >> 24 & 0xFF),
                (int)(color >> 16 & 0xFF),
                (int)(color >> 8 & 0xFF),
                (int)(color & 0xFF)));

        // NOTE: use the *static* Raylib functions.
        public ITexture LoadTexture(string filePath) =>
            new RaylibTexture(Raylib_cs.Raylib.LoadTexture(filePath));

        public IFont LoadFont(string filePath, int size) =>
            new RaylibFont(Raylib_cs.Raylib.LoadFontEx(filePath, size, null, 0));

        // Overload that accepts explicit codepoint list for extended glyph support.
        public IFont LoadFont(string filePath, int size, int[] codepoints) =>
            new RaylibFont(Raylib_cs.Raylib.LoadFontEx(filePath, size, codepoints, codepoints.Length));

        // -------------------------------------------------
        // Simple texture draw – ignores width/height parameters
        // -------------------------------------------------
        public void DrawTexture(ITexture texture, float x, float y,
                                int width = 0, int height = 0,
                                uint tint = 0xFFFFFFFF)
        {
            var tex = (RaylibTexture)texture;
            // Fully‑qualified static call (avoids recursion with our own method name)
            Raylib_cs.Raylib.DrawTexture(tex.Texture, (int)x, (int)y,
                new Raylib_cs.Color(
                    (int)(tint >> 24 & 0xFF),
                    (int)(tint >> 16 & 0xFF),
                    (int)(tint >> 8 & 0xFF),
                    (int)(tint & 0xFF)));
        }

        // -------------------------------------------------
        // Draw a sub‑rectangle of a texture (currently unused)
        // -------------------------------------------------
        public void DrawTextureRec(ITexture texture, float x, float y,
                                   int width, int height,
                                   int srcX, int srcY,
                                   int srcWidth, int srcHeight,
                                   uint tint = 0xFFFFFFFF)
        {
            var tex = (RaylibTexture)texture;
            var srcRect = new Raylib_cs.Rectangle(srcX, srcY, srcWidth, srcHeight);
            var destRect = new Raylib_cs.Rectangle(x, y, width > 0 ? width : srcWidth, height > 0 ? height : srcHeight);
            var origin = new System.Numerics.Vector2(0, 0);
            
            Raylib_cs.Raylib.DrawTexturePro(
                tex.Texture, 
                srcRect, 
                destRect, 
                origin, 
                0f, 
                new Raylib_cs.Color(
                    (int)(tint >> 24 & 0xFF),
                    (int)(tint >> 16 & 0xFF),
                    (int)(tint >> 8 & 0xFF),
                    (int)(tint & 0xFF)));
        }

        // -------------------------------------------------
        // Text drawing – fall back to a constant size if none supplied
        // -------------------------------------------------
        public void DrawText(IFont font, string text,
                             float x, float y,
                             float fontSize = 0, float spacing = 0,
                             uint color = 0xFFFFFFFF)
        {
            var f = (RaylibFont)font;
            float size = fontSize > 0 ? fontSize : 20f;

            // Fully-qualified static call - Vector2 lives under System.Numerics
            Raylib_cs.Raylib.DrawTextEx(f.Font, text,
                new System.Numerics.Vector2(x, y), size, spacing,
                new Raylib_cs.Color(
                    (int)(color >> 24 & 0xFF),
                    (int)(color >> 16 & 0xFF),
                    (int)(color >> 8 & 0xFF),
                    (int)(color & 0xFF)));
        }

        // -------------------------------------------------
        // Measure text width – also uses a constant fallback size
        // -------------------------------------------------
        public float MeasureText(string text, IFont font,
                                 float fontSize = 0, float spacing = 0) =>
            // Fully‑qualified static call
            Raylib_cs.Raylib.MeasureTextEx(((RaylibFont)font).Font,
                                         text,
                                         fontSize > 0 ? fontSize : 20f,
                                         spacing).X;

        // -------------------------------------------------
        // Primitive drawing helpers
        // -------------------------------------------------
        public void DrawRectangleLines(float x, float y,
                                       float width, float height,
                                       uint color) =>
            Raylib_cs.Raylib.DrawRectangleLines((int)x, (int)y,
                (int)width, (int)height,
                new Raylib_cs.Color(
                    (int)(color >> 24 & 0xFF),
                    (int)(color >> 16 & 0xFF),
                    (int)(color >> 8 & 0xFF),
                    (int)(color & 0xFF)));

        public void DrawRectangle(float x, float y,
                                   float width, float height,
                                   uint color) =>
            Raylib_cs.Raylib.DrawRectangle((int)x, (int)y,
                (int)width, (int)height,
                new Raylib_cs.Color(
                    (int)(color >> 24 & 0xFF),
                    (int)(color >> 16 & 0xFF),
                    (int)(color >> 8 & 0xFF),
                    (int)(color & 0xFF)));

        public void DrawLine(float x1, float y1,
                             float x2, float y2,
                             uint color) =>
            Raylib_cs.Raylib.DrawLine((int)x1, (int)y1,
                (int)x2, (int)y2,
                new Raylib_cs.Color(
                    (int)(color >> 24 & 0xFF),
                    (int)(color >> 16 & 0xFF),
                    (int)(color >> 8 & 0xFF),
                    (int)(color & 0xFF)));

        public void Dispose()
        {
            // Unload textures / fonts here if you keep references.
        }
    }
}