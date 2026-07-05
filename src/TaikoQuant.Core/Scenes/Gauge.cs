using System;
namespace TaikoQuant.Core.Scenes
{
    /// <summary>
    /// Simple gauge implementation that mimics OpenTaiko's health gauge.
    /// It draws a filled rectangle representing the current gauge value (0‑100%).
    /// The visual style is deliberately minimal; it can be replaced later with
    /// texture‑based rendering if a more faithful recreation is needed.
    /// </summary>
    internal class Gauge
    {
        // Position and size – values taken from OpenTaiko's default skin (approx.)
        // These match the coordinates used in CActImplGauge for a 5‑player layout.
        private readonly int _x;
        private readonly int _y;
        private readonly int _width;
        private readonly int _height;

        // Current gauge percentage (0‑100)
        private float _percent = 100f;

        // Colors – OpenTaiko uses a green fill that turns red on danger.  We keep it simple.
        private const uint COLOR_NORMAL = 0xFF00FF00; // opaque green
        private const uint COLOR_DANGER = 0xFFFF0000; // opaque red
        private const uint COLOR_BORDER = 0xFFFFFFFF; // white border

        public Gauge(int x, int y, int width, int height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Adjust the gauge by the given delta (positive = gain, negative = loss).
        /// The value is clamped between 0 and 100.
        /// </summary>
        public void Add(float delta)
        {
            _percent = Math.Clamp(_percent + delta, 0f, 100f);
        }

        /// <summary>
        /// Set the gauge to an absolute percentage.
        /// </summary>
        public void Set(float percent)
        {
            _percent = Math.Clamp(percent, 0f, 100f);
        }

        /// <summary>
        /// Render the gauge using the provided renderer.
        /// </summary>
        public void Draw(IRenderer renderer)
        {
            // Background (border)
            renderer.DrawRectangleLines(_x, _y, _width, _height, COLOR_BORDER);

            // Fill width proportional to percent
            int fillW = (int)(_width * (_percent / 100f));
            uint fillColor = _percent > 20f ? COLOR_NORMAL : COLOR_DANGER; // danger when low
            if (fillW > 0)
            {
                renderer.DrawRectangle(_x, _y, fillW, _height, fillColor);
            }
        }
    }
}
