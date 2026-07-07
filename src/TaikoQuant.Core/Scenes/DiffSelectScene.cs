using System;
using System.Collections.Generic;
using TaikoQuant.Core;

namespace TaikoQuant.Core.Scenes
{
    /// <summary>
    /// Difficulty selection screen: choose a difficulty for the selected song.
    /// </summary>
    public class DiffSelectScene : IScene
    {
        private readonly SongEntry? _song;

        // Font for rendering text.
        private IFont? _font;
        private const string FontPath = "Theme/default/Fonts/NotoSansCJKjp-Regular.otf";

        // Generate a codepoint array covering ASCII and common Japanese ranges.
        private static int[] GenerateJapaneseCodepoints()
        {
            var list = new List<int>();
            // ASCII printable
            for (int cp = 32; cp <= 126; cp++) list.Add(cp);
            // Hiragana
            for (int cp = 0x3040; cp <= 0x309F; cp++) list.Add(cp);
            // Katakana
            for (int cp = 0x30A0; cp <= 0x30FF; cp++) list.Add(cp);
            // Common CJK Unified Ideographs (basic range)
            for (int cp = 0x4E00; cp <= 0x9FAF; cp++) list.Add(cp);
            // Punctuation and symbols
            for (int cp = 0x3000; cp <= 0x303F; cp++) list.Add(cp);
            return list.ToArray();
        }

        // The next scene to transition to, if any.
        private SceneType? _nextScene;

        public DiffSelectScene(params object[] args)
        {
            if (args.Length > 0 && args[0] is SongEntry song)
            {
                _song = song;
            }
        }

        public void Init(IRenderer renderer, IAudioService audio)
        {
            int[] jpCodepoints = GenerateJapaneseCodepoints();
            _font = renderer.LoadFont(FontPath, 24, jpCodepoints);
        }

        public void Dispose()
        {
            _font?.Dispose();
        }

        public bool Update(IInputService input, IAudioService audio)
        {
            // Load font if we have the audio service? Actually, we don't need audio for this scene.
            // We'll load the font in Draw when we have the renderer.

            // For now, just press Enter to go to GamePlay with a dummy difficulty.
            if (input.IsKeyPressed(GameKey.Enter))
            {
                // We'll hardcode difficulty 0 (Easy) for now.
                SelectedDifficulty = 0;
                _nextScene = SceneType.GamePlay;
                return true;
            }

            if (input.IsKeyPressed(GameKey.Escape))
            {
                // Go back to song select.
                _nextScene = SceneType.SongSelect;
                return true;
            }

            return false;
        }

        public void Draw(IRenderer renderer)
        {
            renderer.Clear(0x000000FF);
            string title = _song != null ? $"Select Difficulty: {_song.Title}" : "Select Difficulty";
            float titleWidth = renderer.MeasureText(title, _font, 24, 0);
            float titleX = (renderer.Width - titleWidth) / 2f;
            float titleY = 100f;
            renderer.DrawText(_font, title, titleX, titleY, 24, 0, 0xFFFFFFFF);

            string hint = "Enter: Confirm  ESC: Back";
            float hintWidth = renderer.MeasureText(hint, _font, 20, 0);
            float hintX = (renderer.Width - hintWidth) / 2f;
            float hintY = renderer.Height - 40f;
            renderer.DrawText(_font, hint, hintX, hintY, 20, 0, 0xFF888888);
        }

        public SceneType? GetNextScene()
        {
            var next = _nextScene;
            _nextScene = null;
            return next;
        }

        public int? SelectedDifficulty { get; private set; }
        public SongEntry? SelectedSong => _song;
    }
}