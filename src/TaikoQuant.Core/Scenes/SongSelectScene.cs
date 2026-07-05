#pragma warning disable CS8604, CS8601, CS8603
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaikoQuant.Core;

namespace TaikoQuant.Core.Scenes
{
    /// <summary>
    /// Song selection screen: browse folders and TJA files to choose a song.
    /// </summary>
    public class SongSelectScene : IScene, IDisposable
    {
        private const int ITEM_HEIGHT = 84;
        private const int PAD_X = 180;
        private const int BOX_WIDTH = 1280 - PAD_X * 2;
        private const int CORNER_RADIUS = 12;

        private readonly string _songsRoot;
        private string _currentDir;

        private readonly List<SelectItem> _items = new List<SelectItem>();
        // Cached measured text widths for each item (updated after fonts are loaded).
        private readonly List<float> _itemWidths = new List<float>();
        private int _selectedIndex = 0;
        private float _scrollY = 0f;
        private float _targetScrollY = 0f;

        private SongEntry? _selectedSong;
        public SongEntry? SelectedSong => _selectedSong;

        private IFont? _fontUi;
        private IFont? _fontNormal;
        private IFont? _fontLarge;
        private ISound? _sndDong;
        private ISound? _sndKa;
        private bool _resourcesLoaded = false;
        private const string FontPath = "Theme/default/Fonts/NotoSansCJKjp-Regular.otf";

        // Generate a codepoint array covering basic ASCII and common Japanese ranges.
        private static int[] GenerateJapaneseCodepoints()
        {
            var list = new List<int>();
            // ASCII printable characters
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

        private static readonly Dictionary<string, string> _titleCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private SceneType? _nextScene;

        public SongSelectScene()
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            _songsRoot = Path.Combine(exeDir, "songs");
            if (!Directory.Exists(_songsRoot))
                _songsRoot = Path.Combine(exeDir, "..", "songs");
            
            if (!Directory.Exists(_songsRoot))
                Directory.CreateDirectory(_songsRoot);

            _currentDir = _songsRoot;
            RefreshItemList();
        }

        private void RefreshItemList()
        {
            _items.Clear();
            _itemWidths.Clear();
            
            if (!string.Equals(Path.GetFullPath(_currentDir), Path.GetFullPath(_songsRoot), StringComparison.OrdinalIgnoreCase))
            {
                _items.Add(new SelectItem { Type = SelectItemType.Folder, Title = "<- 戻る (Back)", Path = Directory.GetParent(_currentDir)?.FullName ?? _songsRoot });
            }

            if (Directory.Exists(_currentDir))
            {
                var dirs = Directory.GetDirectories(_currentDir);
                foreach (var d in dirs)
                {
                    _items.Add(new SelectItem { Type = SelectItemType.Folder, Title = "[Folder] " + Path.GetFileName(d), Path = d });
                }

                var files = Directory.GetFiles(_currentDir, "*.tja");
                foreach (var f in files)
                {
                    _items.Add(new SelectItem { Type = SelectItemType.Song, Title = GetTjaTitle(f), Path = f });
                }
            }

            if (_items.Count == 0)
            {
                _items.Add(new SelectItem { Type = SelectItemType.Folder, Title = "No songs found.", Path = _currentDir });
            }

            _selectedIndex = 0;
            _targetScrollY = 0;
            _scrollY = 0;
        }

        private string GetTjaTitle(string filePath)
        {
            if (_titleCache.TryGetValue(filePath, out string? title))
                return title!;
            string titleValue;
            try
            {
                var parser = new TJAParser(filePath);
                SongInfo info = parser.Parse(0);
                titleValue = info.metadata.title.FirstOrDefault().Value ?? Path.GetFileNameWithoutExtension(filePath);
            }
            catch (Exception)
            {
                titleValue = Path.GetFileNameWithoutExtension(filePath);
            }
            _titleCache[filePath] = titleValue;
            return titleValue;
        }

        public bool Update(IInputService input, IAudioService audio)
        {
            if (_items.Count > 0)
            {
                // Navigation: D/K (DonLeft / DonRight) map to Up / Down
                if (input.IsKeyPressed(GameKey.DonLeft))
                {
                    _selectedIndex--;
                    if (_selectedIndex < 0) _selectedIndex = _items.Count - 1;
                    _targetScrollY = _selectedIndex * ITEM_HEIGHT;
                    if (_sndDong != null) _sndDong.Play();
                }
                else if (input.IsKeyPressed(GameKey.DonRight))
                {
                    _selectedIndex++;
                    if (_selectedIndex >= _items.Count) _selectedIndex = 0;
                    _targetScrollY = _selectedIndex * ITEM_HEIGHT;
                    if (_sndDong != null) _sndDong.Play();
                }

                // Selection: F/J (KaRight / KaLeft) or Enter
                if (input.IsKeyPressed(GameKey.KaLeft) || input.IsKeyPressed(GameKey.KaRight) || input.IsKeyPressed(GameKey.Enter))
                {
                    if (_sndKa != null) _sndKa.Play();

                    var selected = _items[_selectedIndex];
                    if (selected.Type == SelectItemType.Folder)
                    {
                        if (selected.Title != "No songs found.")
                        {
                            _currentDir = selected.Path;
                            RefreshItemList();
                        }
                    }
                    else if (selected.Type == SelectItemType.Song)
                    {
                        _selectedSong = new SongEntry { Title = selected.Title, TjaPath = selected.Path };
                        _nextScene = SceneType.DiffSelect;
                        return true;
                    }
                }
            }

            if (input.IsKeyPressed(GameKey.Escape))
            {
                if (!string.Equals(Path.GetFullPath(_currentDir), Path.GetFullPath(_songsRoot), StringComparison.OrdinalIgnoreCase))
                {
                    _currentDir = Directory.GetParent(_currentDir)?.FullName ?? _songsRoot;
                    RefreshItemList();
                }
                else
                {
                    _nextScene = SceneType.Title;
                    return true;
                }
            }

            _scrollY += (_targetScrollY - _scrollY) * 0.2f;

            return false;
        }

        public void Draw(IRenderer renderer)
        {
            if (!_resourcesLoaded)
            {
                try {
                    int[] jpCodepoints = GenerateJapaneseCodepoints();
                    _fontUi = renderer.LoadFont(FontPath, 20, jpCodepoints);
                    _fontNormal = renderer.LoadFont(FontPath, 24, jpCodepoints);
                    _fontLarge = renderer.LoadFont(FontPath, 32, jpCodepoints);
                } catch { }
                _resourcesLoaded = true;
            }

            renderer.Clear(0x000000FF);

            float screenCenterY = renderer.Height / 2f;
            float startY = screenCenterY - _scrollY;

            for (int i = 0; i < _items.Count; i++)
            {
                float y = startY + (i * ITEM_HEIGHT) - (ITEM_HEIGHT / 2f);
                if (y < -ITEM_HEIGHT * 2 || y > renderer.Height + ITEM_HEIGHT * 2)
                    continue;

                bool isSelected = (i == _selectedIndex);
                uint textColor = isSelected ? 0xFFFFFFFF : 0xAAAAAAFF;
                IFont? fontToUse = isSelected ? _fontLarge : _fontNormal;
                float fontSize = isSelected ? 32f : 24f;

                if (fontToUse != null)
                {
                    string text = _items[i].Title;
                    // Ensure cache list has entry for this index.
                    if (_itemWidths.Count <= i)
                        _itemWidths.Add(0f);
                    // Compute once per font load; 0 means not measured yet.
                    if (_itemWidths[i] == 0f)
                        _itemWidths[i] = renderer.MeasureText(text, fontToUse, fontSize, 0);
                    float textWidth = _itemWidths[i];
                    float x = (renderer.Width - textWidth) / 2f;
                    renderer.DrawText(fontToUse, text, x, y + (ITEM_HEIGHT - fontSize) / 2f, fontSize, 0, textColor);
                }
            }

            if (_fontUi != null)
            {
                renderer.DrawText(_fontUi, "D/K: Select (Up/Down)   F/J: Confirm   ESC: Back", 20, 20, 20, 0, 0xCCCCCCFF);
                renderer.DrawText(_fontUi, $"Current Dir: {_currentDir}", 20, renderer.Height - 40, 20, 0, 0x888888FF);
            }
        }

        public SceneType? GetNextScene()
        {
            var next = _nextScene;
            _nextScene = null;
            return next;
        }

        public void Dispose()
        {
            _fontUi?.Dispose();
            _fontNormal?.Dispose();
            _fontLarge?.Dispose();
        }
    }

    public enum SelectItemType
    {
        Folder,
        Song
    }

    public class SelectItem
    {
        public SelectItemType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public class SongEntry
    {
        public string Title { get; set; } = string.Empty;
        public string TjaPath { get; set; } = string.Empty;
    }
}
