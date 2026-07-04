using System;

namespace TaikoQuant.Core
{
    /// <summary>
    /// Base class for all note-like objects in the chart.
    /// </summary>
    public class Note
    {
        public Note()
        {
        }

        public Note(Note other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            hit_ms = other.hit_ms;
            type = other.type;
            display = other.display;
            scroll_y = other.scroll_y;
            bpm = other.bpm;
            scroll_x = other.scroll_x;
            sudden_appear_ms = other.sudden_appear_ms;
            sudden_moving_ms = other.sudden_moving_ms;
            index = other.index;
            unload_ms = other.unload_ms;
            moji = other.moji;
            is_branch_start = other.is_branch_start;
            branch_params = other.branch_params;
            is_section_bar = other.is_section_bar;
        }

        /// <summary>
        /// Time (in milliseconds from start of song) when the note should be judged.
        /// </summary>
        public float hit_ms { get; set; }

        /// <summary>
        /// Type of the note.
        /// </summary>
        public NoteType type { get; set; } = NoteType.NONE;

        /// <summary>
        /// Whether the note should be rendered.
        /// </summary>
        public bool display { get; set; } = true;

        /// <summary>
        /// Vertical scroll offset (used for sudden/slow effects).
        /// </summary>
        public float scroll_y { get; set; } = 0.0f;

        /// <summary>
        /// Beats per minute at the note's position.
        /// </summary>
        public float bpm { get; set; } = 120.0f;

        /// <summary>
        /// Horizontal scroll multiplier.
        /// </summary>
        public float scroll_x { get; set; } = 1.0f;

        /// <summary>
        /// Delay before the note suddenly appears (for SUDDEN effect).
        /// </summary>
        public float sudden_appear_ms { get; set; } = 0.0f;

        /// <summary>
        /// Duration over which the note moves (for SCROLL changes?).
        /// </summary>
        public float sudden_moving_ms { get; set; } = 0.0f;

        /// <summary>
        /// Index in the note order.
        /// </summary>
        public int index { get; set; } = 0;

        /// <summary>
        /// Time when the note should be unloaded/fade out.
        /// </summary>
        public float unload_ms { get; set; } = 0.0f;

        /// <summary>
        /// Moji (lyric or display text) index.
        /// </summary>
        public int moji { get; set; } = 0;

        /// <summary>
        /// True if this note starts a branch.
        /// </summary>
        public bool is_branch_start { get; set; } = false;

        /// <summary>
        /// Parameters for the branch (if any).
        /// </summary>
        public string branch_params { get; set; } = string.Empty;

        /// <summary>
        /// True if this note is a bar line.
        /// </summary>
        public bool is_section_bar { get; set; } = false;
    }

    /// <summary>
    /// Drum roll (continuous) note.
    /// </summary>
    public class Drumroll : Note
    {
        public Drumroll() : base() { }
        public Drumroll(Note baseNote) : base(baseNote) { }
        // Additional roll-specific fields can be added here.
        public int dummy { get; set; } = 0;
    }

    /// <summary>
    /// Balloon note (can be popped for extra points).
    /// </summary>
    public class Balloon : Note
    {
        public Balloon() : base() { }
        public Balloon(Note baseNote, bool explosive = false) : base(baseNote)
        {
            this.explosive = explosive;
        }

        public int count { get; set; } = 0;
        public bool explosive { get; set; } = false;
    }

    /// <summary>
    /// Helper to check if a pointer to a native note is valid (ported from C++ safety check).
    /// In C# we don't need this, but we keep the method for parity.
    /// </summary>
    public static class NoteUtil
    {
        // In C# we don't have raw pointers, but we can keep the method stub.
        public static bool IsValidNotePtr(Note p)
        {
            // In managed code, null check is sufficient.
            return p != null;
        }
    }
}