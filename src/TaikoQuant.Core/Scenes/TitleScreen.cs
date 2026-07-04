using System;
using TaikoQuant.Core;

namespace TaikoQuant.Core.Scenes
{
    /// <summary>
    /// Title screen scene: shows the game title and prompts to press Enter.
    /// </summary>
    public class TitleScreen : IScene
    {
        // We'll store the font so we don't have to look it up every frame.
        // However, the font is loaded via the renderer, which we don't have in the constructor.
        // We'll load it on first draw and cache it.
        private IFont _titleFont = null!;
        private IFont _subFont = null!;
        private bool _fontsLoaded = false;

        // We need to know the path to the font file. We'll make it configurable or relative.
        private const string FontPath = "Theme/default/Fonts/FOT-OedoKtr.otf";

        // The next scene to transition to, if any.
        private SceneType? _nextScene;

        public TitleScreen()
        {
            // Constructor: nothing to do now.
            _nextScene = null;
        }

        public void Dispose()
        {
            // We'll just set the references to null.
            _titleFont = null!;
            _subFont = null!;
        }

        public bool Update(IInputService input, IAudioService audio)
        {
            // Check for Enter key to start the game.
            if (input.IsKeyPressed(GameKey.Enter))
            {
                // Request a scene change to SongSelect.
                _nextScene = SceneType.SongSelect;
                return true;
            }

            return false;
        }

        public void Draw(IRenderer renderer)
        {
            // Load fonts if not already loaded.
            if (!_fontsLoaded)
            {
                _titleFont = renderer.LoadFont(FontPath, 48);
                _subFont = renderer.LoadFont(FontPath, 24);
                _fontsLoaded = true;
            }

            // Clear the screen first (background black).
            renderer.Clear(0x000000FF);

            int screenWidth = renderer.Width;
            int screenHeight = renderer.Height;

            // Draw title (centered).
            string title = "TaikoQuant";
            float titleWidth = renderer.MeasureText(title, _titleFont, 48, 0);
            float titleX = (screenWidth - titleWidth) / 2f;
            float titleY = (screenHeight / 2f) - 60; // a bit higher than vertical centre
            renderer.DrawText(_titleFont, title, titleX, titleY, 48, 0, 0xFFFFFFFF);

            // Draw instruction (centered below title).
            string instruction = "Press ENTER to start";
            float instrWidth = renderer.MeasureText(instruction, _subFont, 24, 0);
            float instrX = (screenWidth - instrWidth) / 2f;
            float instrY = (screenHeight / 2f) + 20; // a bit below centre
            renderer.DrawText(_subFont, instruction, instrX, instrY, 24, 0, 0xC8C8C8);
        }

        public SceneType? GetNextScene()
        {
            var next = _nextScene;
            _nextScene = null;
            return next;
        }
    }
}