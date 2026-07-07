#pragma warning disable CS8604, CS8601, CS8603
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaikoQuant.Core;
using TaikoNauts.Core.Taiko.Charts;

namespace TaikoQuant.Core.Scenes
{
    public class GamePlayScene : IScene, IDisposable
    {
        public class ActiveNote
        {
            public Chip Chip { get; set; } = null!;
            public bool Judged { get; set; } = false;
        }

        private readonly SongEntry? _song;
        private readonly int _diffId;
        private Song? _songData;
        private SongCourse? _course;

        private IFont? _fontScore;
        private IFont? _fontUI;
        private IFont? _fontTitle;

        private ITexture? _backgroundImage;
        private ITexture? _background1PImage;
        private ITexture? _baseImage;
        private ITexture? _donImage;
        private ITexture? _kaImage;
        private ITexture? _donBigImage;
        private ITexture? _kaBigImage;
        private ITexture? _subBackgroundImage;
        private ITexture? _frameImage;
        private ITexture? _courseSymbolImage;
        private ITexture? _miniTaikoImage;
        private ITexture? _backgroundRightImage;
        private ITexture? _stageImage;

        private ISound? _sndDong;
        private ISound? _sndKa;
        private IMusic? _music;
        private bool _audioStarted = false;
        private int _activeNoteIndex = 0;

        private SceneType? _nextScene;

        private GameSettings _settings = SettingsHelper.Load();

        private List<ActiveNote> _notes = new List<ActiveNote>();
        // Simple gauge (1 player) – position approximates OpenTaiko's default gauge location
        private Gauge _gauge = new Gauge(Layout.GaugeX, Layout.GaugeY, Layout.GaugeWidth, Layout.GaugeHeight);

        private long _playStartMs = 0;
        private float _songEndMs = 0f;

        private int _score = 0;
        private int _combo = 0;
        private int _maxCombo = 0;
        private int _ryoCount = 0;
        private int _kaCount = 0;
        private int _fukaCount = 0;

        // ゲーム解像度: 1280x720
        // AviUtl(1920x1080)座標 → 1280x720変換係数: *2/3
        // JUDGE_X, LANE_CY 等はすべて1280x720座標

        private static readonly int JUDGE_X = Layout.JudgeX;
        private const float SCROLL_SPEED = 1.0f;
        private const float BASE_SCROLL_PX_PER_MS = 0.435f;
        private const float JUDGE_RYO_MS = 35.0f;
        private const float JUDGE_KA_MS = 80.0f;
        private static readonly float PREROLL_MS = (1280f - JUDGE_X) / (BASE_SCROLL_PX_PER_MS * SCROLL_SPEED);

        // Notes.png スプライト定数 (GamePlay.cpp 準拠)
        private static readonly int[] NS_SRC_X = { 11, 159, 289, 401, 531, 679, 780, 1051, 1170, 1459 };
        private static readonly int[] NS_SRC_W = { 106, 74, 74, 110, 110, 70, 165, 106, 185, 179 };
        private const int NS_SRC_H = 130;
        // NS_DST_H: ノーツ描画高さ (1280x720基準)
        private const int NS_DST_H = 128;
        private static readonly int LANE_CY = Layout.LaneCY;   // .aup2計算値
        private static readonly int LANE_TOP = LANE_CY - 64;   // LANE_CY - 64
        private static readonly int LANE_BOTTOM = LANE_CY + 64; // LANE_CY + 64

        public GamePlayScene(params object[] args)
        {
            if (args.Length > 0 && args[0] is SongEntry song)
                _song = song;
            if (args.Length > 1 && args[1] is int diffId)
                _diffId = diffId;

            if (_song != null && !string.IsNullOrEmpty(_song.TjaPath))
            {
                try
                {
                    var tjaParser = new TJAParser(_song.TjaPath);
                    var result = tjaParser.LoadSongData(_song.TjaPath, _diffId);
                    _songData = result.song;
                    _course = result.course;

                    // Use the pre-filtered hittable notes
                    foreach (var chip in result.hittableNotes)
                    {
                        _notes.Add(new ActiveNote { Chip = chip, Judged = false });
                    }
                    _notes = _notes.OrderBy(n => n.Chip._time).ToList();

                    if (_notes.Count > 0)
                        _songEndMs = (float)_notes.Last().Chip._time + 8000f; // Wait longer after last note so music tail doesn't cut off
                    else
                        _songEndMs = 5000f;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[GamePlayScene] TJA parse error: {ex}");
                }
            }

        }

        public void Init(IRenderer renderer, IAudioService audio)
        {
            try
            {
                _fontUI = renderer.LoadFont("Theme/default/Fonts/FOT-OedoKtr.otf", 24);
                _fontScore = renderer.LoadFont("Theme/default/Fonts/FOT-OedoKtr.otf", 36);
                _fontTitle = renderer.LoadFont("Theme/default/Fonts/FOT-OedoKtr.otf", 36);

                _backgroundImage = renderer.LoadTexture("Theme/default/img/05_Game/1P_Background.png");
                _stageImage = renderer.LoadTexture("Theme/default/img/05_Game/Stage.png");
                _baseImage = renderer.LoadTexture("Theme/default/img/05_Game/Base.png");
                _donImage = renderer.LoadTexture("Theme/default/img/05_Game/Notes/don.png");
                _kaImage = renderer.LoadTexture("Theme/default/img/05_Game/Notes/ka.png");
                _donBigImage = renderer.LoadTexture("Theme/default/img/05_Game/Notes/big_don.png");
                _kaBigImage = renderer.LoadTexture("Theme/default/img/05_Game/Notes/big_ka.png");
            }
            catch { }

            try
            {
                _sndDong = audio.LoadSound("Theme/default/sounds/dong.wav");
                _sndKa = audio.LoadSound("Theme/default/sounds/ka.wav");
                if (_songData != null && !string.IsNullOrEmpty(_songData._header._wave))
                {
                    string songDir = Path.GetDirectoryName(_song!.TjaPath)!;
                    string wavePath = Path.Combine(songDir, _songData._header._wave);
                    if (File.Exists(wavePath))
                        _music = audio.LoadMusic(wavePath);
                }
            }
            catch { }

            // チャートタイマーはリソースロード完了後にスタートさせる
            _playStartMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private float GetChartMs()
        {
            float elapsed = (float)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _playStartMs);
            float offset = (float)(_songData?._header?._offset ?? 0.0);
            return elapsed - PREROLL_MS + (offset * 1000f);
        }

        private float CalcScrollPxPerMs(Chip chip)
        {
            float bpmRatio = (chip._bpm > 0.0) ? (float)(chip._bpm / 120.0) : 1f;
            float scrollX = (float)chip._scroll.Real;
            return BASE_SCROLL_PX_PER_MS * bpmRatio * scrollX * SCROLL_SPEED;
        }

        private float ChipScreenX(Chip chip)
        {
            float chartMs = GetChartMs();
            return JUDGE_X + ((float)chip._time - chartMs) * CalcScrollPxPerMs(chip);
        }

        private void JudgeNote(ActiveNote active, bool isDon, bool isKa)
        {
            float chartMs = GetChartMs();
            float delta = Math.Abs((float)active.Chip._time - chartMs);

            if (delta > JUDGE_KA_MS) return;

            bool isNoteDon = active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.Don
                          || active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.DON;
            bool isNoteKa = active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.Ka
                         || active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.KA;

            if ((isDon && isNoteDon) || (isKa && isNoteKa))
            {
                active.Judged = true;
                bool isBig = active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.DON
                          || active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.KA;

                if (delta <= JUDGE_RYO_MS)
                {
                    _ryoCount++;
                    _combo++;
                    _maxCombo = Math.Max(_maxCombo, _combo);
                    _score += isBig ? 660 : 330;
                }
                else
                {
                    _kaCount++;
                    _combo = 0;
                    _score += isBig ? 320 : 160;
                }
            }
        }

        public bool Update(IInputService input, IAudioService audio)
        {
            if (input.IsKeyPressed(GameKey.Escape))
            {
                _nextScene = SceneType.SongSelect;
                return true;
            }

            if (input.IsKeyPressed(GameKey.F1))
            {
                _settings.AutoPlay = !_settings.AutoPlay;
                SettingsHelper.Save(_settings);
            }

            float chartMs = GetChartMs();

            if (!_audioStarted && chartMs >= 0 && _music != null)
            {
                try { _music.Play(false); } catch { }
                _audioStarted = true;
            }

            bool donPressed = input.IsKeyPressed(GameKey.DonLeft) || input.IsKeyPressed(GameKey.DonRight);
            bool kaPressed = input.IsKeyPressed(GameKey.KaLeft) || input.IsKeyPressed(GameKey.KaRight);

            if (_settings.AutoPlay)
            {
                // AutoPlay logic: Hit the note exactly when it crosses the judgment line (delta <= 0)
                for (int i = _activeNoteIndex; i < _notes.Count; i++)
                {
                    var active = _notes[i];
                    if (active.Judged) continue;
                    
                    float delta = (float)active.Chip._time - chartMs;
                    
                    if (delta <= 0)
                    {
                        bool isDonNote = active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.Don ||
                                         active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.DON;
                        bool isKaNote = active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.Ka ||
                                        active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.KA;

                        if (isDonNote) donPressed = true;
                        if (isKaNote) kaPressed = true;

                        JudgeNode(active, isDonNote, isKaNote);
                    }
                    else if (delta > 0)
                    {
                        // Notes are sorted by time, so future notes won't be triggered yet
                        break;
                    }
                }
            }
            else
            {
                if (donPressed || kaPressed)
                {
                    float minDelta = float.MaxValue;
                    ActiveNote? target = null;
                    for (int i = _activeNoteIndex; i < _notes.Count; i++)
                    {
                        var active = _notes[i];
                        if (active.Judged) continue;
                        float delta = Math.Abs((float)active.Chip._time - chartMs);
                        if (delta <= JUDGE_KA_MS && delta < minDelta)
                        {
                            minDelta = delta;
                            target = active;
                        }
                        if ((float)active.Chip._time - chartMs > JUDGE_KA_MS) break;
                    }
                    if (target != null)
                        JudgeNode(target, donPressed, kaPressed);
                }
            }

            while (_activeNoteIndex < _notes.Count)
            {
                var active = _notes[_activeNoteIndex];
                if (!active.Judged && chartMs - (float)active.Chip._time > JUDGE_KA_MS)
                {
                    active.Judged = true;
                    _fukaCount++;
                    _combo = 0;
                }
                
                if (active.Judged)
                {
                    _activeNoteIndex++;
                }
                else
                {
                    break;
                }
            }

            if (chartMs > _songEndMs)
            {
                // Wait for music to finish if still playing
                if (_music is IMusic music && music.IsPlaying())
                {
                    // Keep the scene until the music ends
                }
                else
                {
                    _nextScene = SceneType.SongSelect;
                    return true;
                }
            }

            return false;
        }

        private int GetNoteSpriteIndex(TaikoNauts.Core.Taiko.Charts.NoteType type)
        {
            switch (type)
            {
                case TaikoNauts.Core.Taiko.Charts.NoteType.Don: return 1;  // 小ドン
                case TaikoNauts.Core.Taiko.Charts.NoteType.Ka: return 2;  // 小カッ
                case TaikoNauts.Core.Taiko.Charts.NoteType.DON: return 3;  // 大ドン
                case TaikoNauts.Core.Taiko.Charts.NoteType.KA: return 4;  // 大カッ
                default: return -1;
            }
        }

        private void JudgeNode(ActiveNote note, bool donPressed, bool kaPressed)
        {
            // Calculate timing delta for this note
            float chartMs = GetChartMs();
            float delta = Math.Abs((float)note.Chip._time - chartMs);

            // Determine note type flags
            bool isDonNote = note.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.Don ||
                             note.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.DON;
            bool isKaNote = note.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.Ka ||
                             note.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.KA;
            // Hit condition
            if ((donPressed && isDonNote) || (kaPressed && isKaNote))
            {
                note.Judged = true;
                bool isBig = note.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.DON ||
                             note.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.KA;
                // Play sound effect corresponding to the note type
                try
                {
                    if (isDonNote && _sndDong != null) _sndDong.Play();
                    else if (isKaNote && _sndKa != null) _sndKa.Play();
                }
                catch { /* ignore sound errors */ }

                if (delta <= JUDGE_RYO_MS)
                {
                    _ryoCount++;
                    _combo++;
                    _maxCombo = Math.Max(_maxCombo, _combo);
                    _score += isBig ? 660 : 330;
                }
                else
                {
                    _kaCount++;
                    _combo = 0;
                    _score += isBig ? 320 : 160;
                }
            }
        }


        public void Draw(IRenderer renderer)
        {
            // -------------------------------------------------------
            // 描画順: AviUtlレイヤー順(下→上)に従う
            // 全座標は1280x720基準
            // -------------------------------------------------------

            // [layer=1] 背景色 #606060
            renderer.Clear(0x606060FF);
            // [layer=0] Stage background
            if (_stageImage != null)
                renderer.DrawTexture(_stageImage, 0, 0, 1280, 720);

            // [layer=2] 1P_Background (full background)
            if (_backgroundImage != null)
                renderer.DrawTexture(_backgroundImage, Layout.BackgroundX, Layout.BackgroundY);

            if (_baseImage != null)
                renderer.DrawTexture(_baseImage, Layout.BaseX, Layout.BaseY, 205, 228);

            // ノーツ描画 (後→前の順: 右から左)
            int lastVisibleIndex = _activeNoteIndex - 1;
            for (int i = _activeNoteIndex; i < _notes.Count; i++)
            {
                float x = ChipScreenX(_notes[i].Chip);
                if (x > renderer.Width + 200) break; // found the right edge
                lastVisibleIndex = i;
            }

            for (int i = lastVisibleIndex; i >= _activeNoteIndex; i--)
            {
                var active = _notes[i];
                if (active.Judged) continue;

                float x = ChipScreenX(active.Chip);
                if (x < -200) continue;

                ITexture tex = null;
                switch (active.Chip._noteType)
                {
                    case TaikoNauts.Core.Taiko.Charts.NoteType.Don: tex = _donImage; break;
                    case TaikoNauts.Core.Taiko.Charts.NoteType.Ka: tex = _kaImage; break;
                    case TaikoNauts.Core.Taiko.Charts.NoteType.DON: tex = _donBigImage; break;
                    case TaikoNauts.Core.Taiko.Charts.NoteType.KA: tex = _kaBigImage; break;
                }

                if (tex != null)
                {
                    // 1920x1080 to 1280x720 scale conversion (2/3)
                    float scale = 2f / 3f;
                    int dstW = (int)(tex.Width * scale);
                    int dstH = (int)(tex.Height * scale);

                    renderer.DrawTextureRec(tex,
                        x - dstW / 2f,
                        Layout.LaneCY - dstH / 2f,
                        dstW,
                        dstH,
                        0, 0, tex.Width, tex.Height,
                        0xFFFFFFFF);
                }
            }
            _gauge.Draw(renderer);

            // スコア / コンボ表示
            /*
            if (_fontScore != null)
            {
                renderer.DrawText(_fontScore, $"Score: {_score}", 1000, 30, 36, 0, 0xFFFFFFFF);
                renderer.DrawText(_fontScore, $"Combo: {_combo}", 1000, 70, 36, 0, 0xFFFFFFFF);
            }
            */

            // 曲名表示
            if (_fontTitle != null && _songData != null)
            {
                renderer.DrawText(_fontTitle, _songData._header._title, 900, 140, 32, 0, 0xFFFFFFFF);
            }

            // AUTO表示
            if (_settings.AutoPlay && _fontScore != null)
            {
                renderer.DrawText(_fontScore, "AUTO", 1100, 60, 36, 0, 0xFF0000FF);
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
            _fontUI?.Dispose();
            _fontScore?.Dispose();
            _fontTitle?.Dispose();
            _backgroundImage?.Dispose();
            _baseImage?.Dispose();
            _donImage?.Dispose();
            _kaImage?.Dispose();
            _donBigImage?.Dispose();
            _kaBigImage?.Dispose();
            _stageImage?.Dispose();
            _music?.Dispose();
        }
    }
}