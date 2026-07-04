using System;
using System.Collections.Generic;

namespace TaikoQuant.Core
{
    /// <summary>
    /// Metadata parsed from a TJA file.
    /// Mirrors struct TJAMetadata from SongInfo.h.
    /// </summary>
    public class TJAMetadata
    {
        /// <summary>
        /// Title strings, possibly keyed by language.
        /// </summary>
        public Dictionary<string, string> title { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Subtitle strings.
        /// </summary>
        public Dictionary<string, string> subtitle { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Genre of the song.
        /// </summary>
        public string genre { get; set; } = string.Empty;

        /// <summary>
        /// Beats per minute (float).
        /// </summary>
        public float bpm { get; set; } = 0.0f;

        /// <summary>
        /// Path to the wave/audio file (relative to song directory).
        /// </summary>
        public string wave { get; set; } = string.Empty;

        /// <summary>
        /// Offset (ms) applied to the entire chart.
        /// </summary>
        public float offset { get; set; } = 0.0f;

        /// <summary>
        /// Demo start position (ms).
        /// </summary>
        public float demostart { get; set; } = 0.0f;

        /// <summary>
        /// Path to background movie (if any).
        /// </summary>
        public string bgmovie { get; set; } = string.Empty;

        /// <summary>
        /// Offset for the movie.
        /// </summary>
        public float movieoffset { get; set; } = 0.0f;

        /// <summary>
        /// Preset for scene/visual effects.
        /// </summary>
        public string scene_preset { get; set; } = string.Empty;

        /// <summary>
        /// Course data indexed by difficulty (0=Easy,1=Normal,2=Hard,3=Oni,4=Edit).
        /// </summary>
        public Dictionary<int, CourseData> course_data { get; set; } = new Dictionary<int, CourseData>();
    }
}