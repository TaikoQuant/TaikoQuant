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
        private ITexture? _baseImage;
        private ITexture? _notesImage;
        private ITexture? _subBackgroundImage;
        private ITexture? _frameImage;
        private ITexture? _courseSymbolImage;
        private ITexture? _miniTaikoImage;
        private ITexture? _backgroundRightImage;

        private ISound? _sndDong;
        private ISound? _sndKa;
        private IMusic? _music;

        private bool _resourcesLoaded = false;
        private bool _audioLoaded = false;
        private bool _audioStarted = false;

        private SceneType? _nextScene;

        private List<ActiveNote> _notes = new List<ActiveNote>();

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

        private const int JUDGE_X = 411;
        private const float SCROLL_SPEED = 1.0f;
        private const float BASE_SCROLL_PX_PER_MS = 0.435f;
        private const float JUDGE_RYO_MS = 35.0f;
        private const float JUDGE_KA_MS = 80.0f;
        private const float PREROLL_MS = (1280f - JUDGE_X) / (BASE_SCROLL_PX_PER_MS * SCROLL_SPEED);

        // Notes.png スプライト定数 (GamePlay.cpp 準拠)
        private static readonly int[] NS_SRC_X = { 11, 159, 289, 401, 531, 679, 780, 1051, 1170, 1459 };
        private static readonly int[] NS_SRC_W = { 106, 74, 74, 110, 110, 70, 165, 106, 185, 179 };
        private const int NS_SRC_H = 130;
        // NS_DST_H: ノーツ描画高さ (1280x720基準)
        private const int NS_DST_H = 128;
        private const int LANE_CY = 258;   // .aup2計算値
        private const int LANE_TOP = 194;   // 258 - 64
        private const int LANE_BOTTOM = 322; // 258 + 64

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
                    var tjaParser = new TJAParser();
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
						_songEndMs = (float)_notes.Last().Chip._time + 3000f;
					else
						_songEndMs = 5000f;
                    /*

                    if (_songData != null && _diffId >= 0 && _diffId < 5)
                        _course = _songData._songCourses[_diffId];

                    if (_course == null && _songData != null)
                        _course = _songData._songCourses.FirstOrDefault(c => c != null);

                    if (_course != null)
                    {
                        foreach (var chip in _course._chips)
                        {
                            if (IsHittable(chip))
                                _notes.Add(new ActiveNote { Chip = chip, Judged = false });
                        }
                        _notes = _notes.OrderBy(n => n.Chip._time).ToList();

                        if (_notes.Count > 0)
                            _songEndMs = (float)_notes.Last().Chip._time + 3000f;
                        else
                            _songEndMs = 5000f;
                    }
*/
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[GamePlayScene] TJA parse error: {ex}");
                }
            }

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
            float diff = Math.Abs((float)active.Chip._time - chartMs);

            if (diff > JUDGE_KA_MS) return;

            bool isNoteDon = active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.Don
                          || active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.DON;
            bool isNoteKa = active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.Ka
                         || active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.KA;

            if ((isDon && isNoteDon) || (isKa && isNoteKa))
            {
                active.Judged = true;
                bool isBig = active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.DON
                          || active.Chip._noteType == TaikoNauts.Core.Taiko.Charts.NoteType.KA;

                if (diff <= JUDGE_RYO_MS)
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

            if (!_resourcesLoaded) return false;

            if (!_audioLoaded)
            {
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
                _audioLoaded = true;
            }

            float chartMs = GetChartMs();

            if (!_audioStarted && chartMs >= 0 && _music != null)
            {
                try { _music.Play(false); } catch { }
                _audioStarted = true;
            }

            bool donPressed = input.IsKeyPressed(GameKey.DonLeft) || input.IsKeyPressed(GameKey.DonRight);
            bool kaPressed = input.IsKeyPressed(GameKey.KaLeft) || input.IsKeyPressed(GameKey.KaRight);

            if (donPressed && _sndDong != null) try { _sndDong.Play(); } catch { }
            if (kaPressed && _sndKa != null) try { _sndKa.Play(); } catch { }

            if (donPressed || kaPressed)
            {
                float minDelta = float.MaxValue;
                ActiveNote? target = null;
                foreach (var active in _notes)
                {
                    if (active.Judged) continue;
                    float delta = Math.Abs((float)active.Chip._time - chartMs);
                    if (delta <= JUDGE_KA_MS && delta < minDelta)
                    {
                        minDelta = delta;
                        target = active;
                    }
                }
                if (target != null)
                    JudgeNote(target, donPressed, kaPressed);
            }

            foreach (var active in _notes)
            {
                if (!active.Judged && chartMs - (float)active.Chip._time > JUDGE_KA_MS)
                {
                    active.Judged = true;
                    _fukaCount++;
                    _combo = 0;
                }
            }

            if (chartMs > _songEndMs)
            {
                _nextScene = SceneType.SongSelect;
                return true;
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

        public void Draw(IRenderer renderer)
        {
            if (!_resourcesLoaded)
            {
                try
                {
                    _fontUI = renderer.LoadFont("Theme/default/Fonts/FOT-OedoKtr.otf", 24);
                    _fontScore = renderer.LoadFont("Theme/default/Fonts/FOT-OedoKtr.otf", 36);
                    _fontTitle = renderer.LoadFont("Theme/default/Fonts/FOT-OedoKtr.otf", 36);

                    _backgroundImage = renderer.LoadTexture("Theme/default/img/05_Game/1P_Background.png");
                    _baseImage = renderer.LoadTexture("Theme/default/img/05_Game/Base.png");
                    _notesImage = renderer.LoadTexture("Theme/default/img/05_Game/Notes.png");
                    _subBackgroundImage = renderer.LoadTexture("Theme/default/img/05_Game/Background_Sub.png");
                    _frameImage = renderer.LoadTexture("Theme/default/img/05_Game/1P_Frame.png");
                    _courseSymbolImage = renderer.LoadTexture("Theme/default/img/05_Game/coursesymbol_oni.png");
                    _miniTaikoImage = renderer.LoadTexture("Theme/default/img/05_Game/MiniTaiko.png");
                    _backgroundRightImage = renderer.LoadTexture("Theme/default/img/05_Game/Background_1P.png");
                }
                catch { }
                _resourcesLoaded = true;
            }

            // -------------------------------------------------------
            // 描画順: AviUtlレイヤー順(下→上)に従う
            // 全座標は1280x720基準
            // -------------------------------------------------------

            // [layer=1] 背景色 #606060
            renderer.Clear(0x606060FF);

            // [layer=10] Background_1P: 右側背景
            if (_backgroundRightImage != null)
                renderer.DrawTexture(_backgroundRightImage, 803, 270, 333, 176);

            // [layer=13] 1P_Background: 左側背景
            if (_backgroundImage != null)
                renderer.DrawTexture(_backgroundImage, 167, 270, 333, 176);

            // [layer=14] MiniTaiko: ミニ太鼓
            if (_miniTaikoImage != null)
                renderer.DrawTexture(_miniTaikoImage, 267, 538, 200, 200); // サイズは暫定

            // [layer=15] 1P_Base: 太鼓本体
            if (_baseImage != null)
                renderer.DrawTexture(_baseImage, 842, 178, 205, 228);

            // [layer=16] 1P_Frame: レーン枠
            if (_frameImage != null)
                renderer.DrawTexture(_frameImage, 803, 246, 951, 224);

            // [layer=17] JudgementFrame (Notes.png col=0)
            if (_notesImage != null)
            {
                int judgeW = (int)(NS_SRC_W[0] * (NS_DST_H / (float)NS_SRC_H));
                renderer.DrawTextureRec(_notesImage,
                    JUDGE_X - judgeW / 2f, LANE_CY - NS_DST_H / 2f,
                    judgeW, NS_DST_H,
                    NS_SRC_X[0], 0, NS_SRC_W[0], NS_SRC_H, 0xFFFFFFFF);
            }

            // ノーツ描画 (後→前の順: 右から左)
            for (int i = _notes.Count - 1; i >= 0; i--)
            {
                var active = _notes[i];
                if (active.Judged) continue;

                float x = ChipScreenX(active.Chip);
                if (x < -200 || x > renderer.Width + 200) continue;

                int spriteIdx = GetNoteSpriteIndex(active.Chip._noteType);
                if (spriteIdx != -1 && _notesImage != null)
                {
                    int srcX = NS_SRC_X[spriteIdx];
                    int srcW = NS_SRC_W[spriteIdx];
                    int dstW = (int)(srcW * (NS_DST_H / (float)NS_SRC_H));

                    renderer.DrawTextureRec(_notesImage,
                        x - dstW / 2f, LANE_CY - NS_DST_H / 2f,
                        dstW, NS_DST_H,
                        srcX, 0, srcW, NS_SRC_H, 0xFFFFFFFF);
                }
            }

            // スコア / コンボ表示
            if (_fontScore != null)
            {
                renderer.DrawText(_fontScore, $"Score: {_score}", 1000, 30, 36, 0, 0xFFFFFFFF);
                renderer.DrawText(_fontScore, $"Combo: {_combo}", 1000, 70, 36, 0, 0xFFFFFFFF);
            }

            // 曲名表示
            if (_fontTitle != null && _songData != null)
            {
                renderer.DrawText(_fontTitle, _songData._header._title, 900, 140, 32, 0, 0xFFFFFFFF);
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
            _notesImage?.Dispose();
            _subBackgroundImage?.Dispose();
            _frameImage?.Dispose();
            _courseSymbolImage?.Dispose();
            _miniTaikoImage?.Dispose();
            _backgroundRightImage?.Dispose();
            _music?.Dispose();
        }
    }
}