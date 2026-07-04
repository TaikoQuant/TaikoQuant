namespace TaikoQuant.Core
{
    /// <summary>
    /// Extended data flags from a TJA file.
    /// Mirrors struct TJAEXData from SongInfo.h.
    /// </summary>
    public class TJAEXData
    {
        /// <summary>
        /// Indicates the audio track is the "new" version.
        /// </summary>
        public bool new_audio { get; set; } = false;

        /// <summary>
        /// Indicates the audio track is the "old" version.
        /// </summary>
        public bool old_audio { get; set; } = false;

        /// <summary>
        /// True if the song has a time limit (e.g., mission mode).
        /// </summary>
        public bool limited_time { get; set; } = false;
    }
}