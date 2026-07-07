using System;

namespace TaikoQuant.System
{
    /// <summary>
    /// 文字列操作のためのユーティリティメソッドを提供します。
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// 指定された長さを超える場合、末尾を「...」に置き換えて切り詰めます。
        /// </summary>
        /// <param name="input">対象文字列</param>
        /// <param name="maxLength">最大長（3未満の場合は例外）</param>
        /// <returns>切り詰めた文字列。長さ以下ならそのまま</returns>
        public static string Truncate(string? input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            if (maxLength < 3) throw new ArgumentOutOfRangeException(nameof(maxLength), "maxLength must be at least 3");
            return input.Length <= maxLength ? input : input[..(maxLength - 3)] + "...";
        }

        /// <summary>
        /// null 参照でも安全に String.Format を行います。
        /// </summary>
        public static string Format(string? format, params object?[] args)
        {
            return string.Format(format ?? string.Empty, args);
        }

        /// <summary>
        /// 指定したプレフィックスで始まるかどうかを大文字小文字を区別せずに判定します。
        /// </summary>
        public static bool StartsWithIgnoreCase(string? str, string? prefix)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(prefix)) return false;
            return str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 指定したサフィックスで終わるかどうかを大文字小文字を区別せずに判定します。
        /// </summary>
        public static bool EndsWithIgnoreCase(string? str, string? suffix)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(suffix)) return false;
            return str.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }
    }
}