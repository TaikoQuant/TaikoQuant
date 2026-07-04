namespace TaikoQuant.Core
{
    /// <summary>
    /// Types of notes that can appear in the chart.
    /// Matches the enum from SongInfo.h.
    /// </summary>
    public enum NoteType
    {
        NONE = -1,
        DON = 0,
        KAT = 1,
        DON_L = 2,
        KAT_L = 3,
        ROLL_HEAD = 4,
        ROLL_HEAD_L = 5,
        BALLOON_HEAD = 6,
        KUSUDAMA = 7,
        TAIL = 8
    }

    /// <summary>
    /// Scroll types affecting how notes move.
    /// </summary>
    public enum ScrollType
    {
        NMSCROLL,
        BMSCROLL,
        HBSCROLL
    }

    /// <summary>
    /// Judgment result for a note hit.
    /// </summary>
    public enum JudgeResult
    {
        None,
        Ryo,
        Ka,
        Fuka
    }
}