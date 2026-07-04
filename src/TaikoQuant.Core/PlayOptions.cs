namespace TaikoQuant.Core
{
    /// <summary>
    /// Options that affect gameplay (note mode, speed, hidden, etc.).
    /// Mirrors the struct PlayOptions from SongInfo.h.
    /// </summary>
    public class PlayOptions
    {
        /// <summary>
        /// Note mode (0 = normal, 1 = mirror?, etc.) – keep as int for compatibility.
        /// </        public int noteMode { get; set; } = 0;

        /// <summary>
        /// Index into the speed table (0.5x, 0.75x, 1.0x, ...).
        /// </        public int speedIndex { get; set; } = 2; // 1.0x default

        /// <summary>
        /// If true, notes are hidden until they reach the judgment line.
        /// </        public bool hidden { get; set; } = false;
    }
}