using System;

namespace TaikoQuant.System
{
    /// <summary>
    /// ゲーム内時間管理（デルタタイム、タイムスケール、固定タイムステップ）。
    /// ゲームループの各フレームで Time.Update(deltaTime) を呼び出すことを前提とします。
    /// </summary>
    public static class Time
    {
        private static double _time;             // スケール適用済み総経過時間（秒）
        private static double _unscaledTime;     // スケール未適用総経過時間（秒）

        private static double _deltaTime;        // 現在フレームのデルタタイム（スケール適用済み）
        private static double _unscaledDeltaTime;// 現在フレームのデルタタイム（スケール未適用）

        private static double _timeScale = 1.0;  // 時間の速度倍率（1.0 が通常速度）

        private static double _fixedDeltaTime = 0.02; // 固定タイムステップ（秒） デフォルト 50Hz
        private static double _accumulator;      // 固定アップデート用蓄積時間

        /// <summary>
        /// スケール適用済みデルタタイム（このフレームの経過時間 * timeScale）。
        /// ゲームロジックで使用すべきデルタタイム。
        /// </summary>
        public static float DeltaTime => (float)_deltaTime;

        /// <summary>
        /// スケール未適用デルタタイム（実際の経過時間）。物理などスケールに依存しない更新に使用。
        /// </summary>
        public static float UnscaledDeltaTime => (float)_unscaledDeltaTime;

        /// <summary>
        /// スケール適用済み総経過時間（ゲーム時間）。
        /// </summary>
        public static float ElapsedTime => (float)_time;

        /// <summary>
        /// スケール未適用総経過時間（実際の経過時間）。
        /// </summary>
        public static float UnscaledTime => (float)_unscaledTime;

        /// <summary>
        /// 時間の速度倍率。1.0 が通常速度。0 で一時停止、>1 で早送り。
        /// </summary>
        public static float TimeScale
        {
            get => (float)_timeScale;
            set => _timeScale = Math.Max(0, value);
        }

        /// <summary>
        /// 固定タイムステップ（秒）。FixedUpdate で使用する間隔。
        /// デフォルトは 0.02 秒（50 Hz）。
        /// </summary>
        public static float FixedDeltaTime
        {
            get => (float)_fixedDeltaTime;
            set => _fixedDeltaTime = Math.Max(0.0001, value);
        }

        /// <summary>
        /// 現在の固定タイムステップにおける経過時間（0～FixedDeltaTime）。
        /// 補間などに利用可能。
        /// </summary>
        public static float FixedTime => (float)(_accumulator % _fixedDeltaTime);

        /// <summary>
        /// ゲームループの各フレームで呼び出す。引数は実際の経過時間（秒）。
        /// </summary>
        /// <param name="unscaledDeltaTime">前フレームからの実際の経過時間（秒）</param>
        public static void Update(float unscaledDeltaTime)
        {
            if (unscaledDeltaTime < 0) unscaledDeltaTime = 0;

            _unscaledDeltaTime = unscaledDeltaTime;
            _deltaTime = unscaledDeltaTime * _timeScale;

            _unscaledTime += unscaledDeltaTime;
            _time += _deltaTime;

            // 固定タイムステップ用蓄積
            _accumulator += unscaledDeltaTime;
        }

        /// <summary>
        /// 固定アップデートが実行されるべきかを返し、内部のアキュムレータから固定ステップ分を消費します。
        /// 一般的には FixedUpdate ラップで使用:
        /// while (Time.FixedUpdateAllowed) { FixedUpdate(); }
        /// </summary>
        /// <returns>真なら固定アップデートを実行すべき</returns>
        public static bool FixedUpdateAllowed
        {
            get
            {
                if (_accumulator >= _fixedDeltaTime)
                {
                    _accumulator -= _fixedDeltaTime;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 時間をリセット（主にデバッグやリロード用）。
        /// </summary>
        public static void Reset()
        {
            _time = 0;
            _unscaledTime = 0;
            _deltaTime = 0;
            _unscaledDeltaTime = 0;
            _accumulator = 0;
        }
    }
}