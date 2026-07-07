namespace TaikoQuant.System;

/// <summary>
/// 数学ユーティリティ関数のコレクション。
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// 値を指定範囲にクランプします。
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>
    /// 線形補間（Lerp）を行います。
    /// </summary>
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * Clamp(t, 0f, 1f);
    }

    /// <summary>
    /// 指定된 범위 내에서 값을 반복시킵니다 (PingPong).
    /// </summary>
    public static float PingPong(float t, float length)
    {
        t = Repeat(t, length * 2f);
        return length - MathF.Abs(t - length);
    }

    private static float Repeat(float t, float length)
    {
        return MathF.Abs(t - MathF.Floor(t / length) * length);
    }

    /// <summary>
    /// 2点間の距離（ユークリッド距離）を返します。
    /// </summary>
    public static float Distance(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// 角度をラジアンに変換します。
    /// </summary>
    public static float Deg2Rad => MathF.PI / 180f;

    /// <summary>
    /// ラジアンを角度に変換します。
    /// </summary>
    public static float Rad2Deg => 180f / MathF.PI;
}