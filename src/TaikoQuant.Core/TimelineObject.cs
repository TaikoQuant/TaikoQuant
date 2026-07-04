using System;

namespace TaikoQuant.Core
{
    /// <summary>
    /// Represents a point in the song's timeline where BPM, scroll, or other parameters may change.
    /// Mirrors the struct TimelineObject from SongInfo.h.
    /// </summary>
    public class TimelineObject
    {
        /// <summary>
        /// Time (ms) when this timeline object takes effect.
        /// </summary>
        public float hit_ms { get; set; } = 0.0f;

        /// <summary>
        /// Time (ms) when the object should be loaded/prepared.
        /// </summary>
        public float load_ms { get; set; } = 0.0f;

        /// <summary>
        /// Beats per minute at this point.
        /// </        public float bpm { get; set; } = 0.0f;

        /// <summary>
        /// Change in BPM (delta) applied at this point.
        /// </summary>
        public float bpmchange { get; set; } = 0.0f;

        /// <summary>
        /// Delay offset (used for delayed notes?).
        /// </summary>
        public float delay { get; set; } = 0.0f;

        /// <summary>
        /// X coordinate of the judgment point (usually constant).
        /// </summary>
        public float judge_pos_x { get; set; } = 0.0f;

        /// <summary>
        /// Y coordinate of the judgment point.
        /// </        public float judge_pos_y { get; set; } = 0.0f;

        /// <summary>
        /// Delta X offset for note movement.
        /// </        public float delta_x { get; set; } = 0.0f;

        /// <summary>
        /// Delta Y offset for note movement.
        /// </        public float delta_y { get; set; } = 0.0f;

        /// <summary>
        /// True if this time is within a "go-go" (bonus) period.
        /// </        public bool gogo_time { get; set; } = false;

        /// <summary>
        /// Lyric or text to display at this point.
        /// </        public string lyric { get; set; } = string.Empty;
    }
}