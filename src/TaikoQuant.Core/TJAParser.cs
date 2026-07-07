using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaikoNauts.Core.Taiko.Charts;

namespace TaikoQuant.Core
{
    /// <summary>
    /// TJAChartCore の TjaChartReader をラップし、TaikoQuant で使いやすい形に整形するクラス。
    /// 譜面の解析はすべて TjaChartReader に委譲します。
    /// </summary>
    public class TJAParser
    {
        private readonly string _tjaPath;

        public TJAParser(string tjaPath)
        {
            _tjaPath = tjaPath;
        }

        /// <summary>
        /// TJAファイルを解析し、指定難易度の Song・SongCourse・打撃対象 Chip リストを返す。
        /// </summary>
        /// <param name="tjaPath">TJAファイルパス</param>
        /// <param name="diffId">難易度 (0=Easy, 1=Normal, 2=Hard, 3=Oni, 4=Edit)</param>
        public (Song? song, SongCourse? course, List<Chip> hittableNotes) LoadSongData(string tjaPath, int diffId)
        {
            try
            {
                var reader = new TjaChartReader();
                var song = reader.GetSongDataFromTja(tjaPath, TjaChartReader.LoadType.Normal);
                if (song == null)
                    return (null, null, new List<Chip>());

                // 指定難易度のコースを取得。なければ最初に見つかったコースを使用
                SongCourse? course = null;
                if (diffId >= 0 && diffId < song._songCourses.Length)
                    course = song._songCourses[diffId];
                if (course == null)
                    course = song._songCourses.FirstOrDefault(c => c != null);

                var hittableNotes = new List<Chip>();
                if (course != null)
                {
                    // 分岐がある場合はノーマル分岐のチップを使用、なければ通常のチップリストを使用
                    var chips = course._hasBranch ? course._chipsNormal : course._chips;
                    foreach (var chip in chips)
                    {
                        if (IsHittable(chip))
                            hittableNotes.Add(chip);
                    }
                    hittableNotes = hittableNotes.OrderBy(c => c._time).ToList();
                }

                return (song, course, hittableNotes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TJAParser] Parse error: {ex}");
                return (null, null, new List<Chip>());
            }
        }

        /// <summary>
        /// 打撃判定の対象となるノーツかどうかを判定する。
        /// ロールエンド・小節線・None は除外する。
        /// </summary>
        public static bool IsHittable(Chip chip)
        {
            return chip._noteType switch
            {
                TaikoNauts.Core.Taiko.Charts.NoteType.Don => true,
                TaikoNauts.Core.Taiko.Charts.NoteType.Ka => true,
                TaikoNauts.Core.Taiko.Charts.NoteType.DON => true,
                TaikoNauts.Core.Taiko.Charts.NoteType.KA => true,
                TaikoNauts.Core.Taiko.Charts.NoteType.RollStart => true,
                TaikoNauts.Core.Taiko.Charts.NoteType.RollBigStart => true,
                TaikoNauts.Core.Taiko.Charts.NoteType.BalloonStart => true,
                TaikoNauts.Core.Taiko.Charts.NoteType.Kusudama => true,
                _ => false
            };
        }
    }
}
