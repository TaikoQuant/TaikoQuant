using System;

namespace TaikoQuant.Core
{
    /// <summary>
    /// Represents a texture that can be rendered.
    /// </summary>
    public interface ITexture : IDisposable
    {
        int Width { get; }
        int Height { get; }
    }

    /// <summary>
    /// Represents a font that can be used to render text.
    /// </summary>
    public interface IFont : IDisposable
    {
        // Font properties and methods would go here
        // For now, we'll keep it simple since the main usage is in RaylibFont
    }

    /// <summary>
    /// Abstracts 2D rendering operations.
    /// </summary>
    public interface IRenderer : IDisposable
    {
        /// <summary>
        /// Width of the backbuffer in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Height of the backbuffer in pixels.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Clears the screen with the specified color.
        /// </summary>
        /// <param name="color">Color as 0xAARRGGBB.</param>
        void Clear(uint color);

        /// <summary>
        /// Begins a frame (call before drawing).
        /// </summary>
        void BeginScene();

        /// <summary>
        /// Ends a frame and presents the back buffer (call after drawing).
        /// </summary>
        void EndScene();

        /// <summary>
        /// Loads a texture from an image file.
        /// </summary>
        /// <param name="filePath">Path to image file (PNG, JPG, etc.).</param>
        /// <returns>A texture handle.</returns>
        ITexture LoadTexture(string filePath);

        /// <summary>
        /// Loads a font from a TTF file.
        /// </summary>
        /// <param name="filePath">Path to .ttf font file.</param>
        /// <param name="size">Font size in pixels.</param>
        /// <returns>A font handle.</returns>
        IFont LoadFont(string filePath, int size);

        /// <summary>
        /// Loads a font from a TTF file with an explicit codepoint list.
        /// </summary>
        /// <param name="filePath">Path to .ttf font file.</param>
        /// <param name="size">Font size in pixels.</param>
        /// <param name="codepoints">Array of Unicode codepoints to load.</param>
        /// <returns>A font handle.</returns>
        IFont LoadFont(string filePath, int size, int[] codepoints);

        /// <summary>
        /// Draws a texture.
        /// </summary>
        /// <param name="texture">Texture to draw.</param>
        /// <param name="x">X position (top-left).</param>
        /// <param name="y">Y position (top-left).</param>
        /// <param name="width">Width to stretch to; if 0 uses texture width.</param>
        /// <param name="height">Height to stretch to; if 0 uses texture height.</param>
        /// <param name="tint">Tint color (0xAARGBB). Use 0xFFFFFFFF for no tint.</param>
        void DrawTexture(ITexture texture, float x, float y, int width = 0, int height = 0, uint tint = 0xFFFFFFFF);

        /// <summary>
        /// Draws a portion of a texture (source rectangle).
        /// </summary>
        /// <param name="texture">Texture to draw.</param>
        /// <param name="x">Destination X.</param>
        /// <param name="y">Destination Y.</param>
        /// <param name="width">Destination width.</param>
        /// <param name="height">Destination height.</param>
        /// <param name="srcX">Source X in texture.</param>
        /// <param name="srcY">Source Y in texture.</param>
        /// <param name="srcWidth">Source width.</param>
        /// <param name="srcHeight">Source height.</p>
        /// <param name="tint">Tint color.</param>
        void DrawTextureRec(ITexture texture, float x, float y, int width, int height,
                            int srcX, int srcY, int srcWidth, int srcHeight, uint tint = 0xFFFFFFFF);

        /// <summary>
        /// Draws text using a font.
        /// </summary>
        /// <param name="font">Font to use.</param>
        /// <param name="text">Text to draw.</param>
        /// <param name="x">X position (left).</param>
        /// <param name="y">Y position (baseline?).</param>
        /// <param name="fontSize">Font size (overrides font's size if >0).</param>
        /// <param name="spacing">Character spacing.</param>
        /// <param name="color">Text color.</p>
        void DrawText(IFont font, string text, float x, float y, float fontSize = 0, float spacing = 0, uint color = 0xFFFFFFFF);

        /// <summary>
        /// Measures the size of a string when drawn with the given font.
        /// </summary>
        /// <param name="text">Text to measure.</param>
        /// <param name="font">Font to use.</param>
        /// <param name="fontSize">Font size (0 uses font's default size).</param>
        /// <param name="spacing">Character spacing.</p>
        /// <returns>Width of the text in pixels.</p>
        float MeasureText(string text, IFont font, float fontSize = 0, float spacing = 0);

        /// <summary>
        /// Draws a rectangle outline.
        /// </summary>
        /// <param name="x">X position (top-left).</p>
        /// <param name="y">Y position (top-left).</p>
        /// <param name="width">Width.</p>
        /// <param name="height">Height.</p>
        /// <param name="color">Line color.</p>
        void DrawRectangleLines(float x, float y, float width, float height, uint color);

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="x">X position (top-left).</p>
        /// <param name="y">Y position (top-left).</p>
        /// <param name="width">Width.</p>
        /// <param name="height">Height.</p>
        /// <param name="color">Fill color.</p>
        void DrawRectangle(float x, float y, float width, float height, uint color);
    void DrawLine(float x1, float y1, float x2, float y2, uint color);
    }
}