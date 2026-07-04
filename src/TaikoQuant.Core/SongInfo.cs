using System;
using System.Collections.Generic;

namespace TaikoQuant.Core
{
    /// <summary>
    /// Holds all data for a song chart (metadata, extended data, notes, branches).
    /// Mirrors struct SongInfo from SongInfo.h.
    /// </summary>
    public class SongInfo
    {
        /// <summary>
        /// Metadata from the TJA file.
        /// </summary>
        public TJAMetadata metadata { get; set; } = new TJAMetadata();

        /// <summary>
        /// Extended flags from the TJA file.
        /// </summary>
        public TJAEXData ex_data { get; set; } = new TJAEXData();

        /// <summary>
        /// Main note list (for the main branch).
        /// </summary>
        public NoteList master_notes { get; set; } = new NoteList();

        /// <summary>
        /// Branch M notes (likely for "Mars" or something).
        /// </summary>
        public List<NoteList> branch_m { get; set; } = new List<NoteList>();

        /// <summary>
        /// Branch E notes.
        /// </summary>
        public List<NoteList> branch_e { get; set; } = new List<NoteList>();

        /// <summary>
        /// Branch N notes.
        /// </summary>
        public List<NoteList> branch_n { get; set; } = new List<NoteList>();

        /// <summary>
        /// Scroll type for the chart.
        /// </summary>
        public ScrollType scroll_type { get; set; } = ScrollType.NMSCROLL;
    }
}