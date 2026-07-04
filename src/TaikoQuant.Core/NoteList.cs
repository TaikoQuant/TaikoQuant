using System;
using System.Collections.Generic;

namespace TaikoQuant.Core
{
    /// <summary>
    /// Container for notes, draw notes, bar lines, and timeline objects.
    /// Objects from C++ but ported for structural parity.
    /// </summary>
    public class NoteList
    {
        // Lists of notes used for gameplay and rendering.
        public List<Note> PlayNotes { get; } = new List<Note>();
        public List<Note> DrawNotes { get; } = new List<Note>();
        public List<Note> Bars { get; } = new List<Note>();
        public List<TimelineObject> Timeline { get; } = new List<TimelineObject>();

        public NoteList() { }

        /// <summary>
        /// Clears all lists. In C# this simply removes references; the GC will collect later.
        /// </summary>
        public void Clear()
        {
            PlayNotes.Clear();
            DrawNotes.Clear();
            Bars.Clear();
            Timeline.Clear();
        }
    }
}