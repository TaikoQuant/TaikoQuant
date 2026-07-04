namespace TaikoQuant.Core
{
    /// <summary>
    /// Data for a course (difficulty) inside a TJA file.
    /// Mirrors struct CourseData from SongInfo.h.
    /// </summary>
    public class CourseData
    {
        /// <summary>
        /// Level of the course.
        /// </summary>
        public int level { get; set; } = 0;

        /// <summary>
        /// Balloon thresholds? (array of ints).
        /// </summary>
        public System.Collections.Generic.List<int> balloon { get; set; } = new System.Collections.Generic.List<int>();

        /// <summary>
        /// Score initial values? (array of ints).
        /// </summary>
        public System.Collections.Generic.List<int> scoreinit { get; set; } = new System.Collections.Generic.List<int>();

        /// <summary>
        /// Score difference per node?.
        /// </summary>
        public int scorediff { get; set; } = 0;

        /// <summary>
        /// Whether the course has branching.
        /// </summary>
        public bool is_branching { get; set; } = false;
    }
}