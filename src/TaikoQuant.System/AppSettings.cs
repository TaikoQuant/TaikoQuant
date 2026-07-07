namespace TaikoQuant.System
{
    /// <summary>
    /// アプリケーション設定のサンプルクラス。
    /// ConfigManager<AppSettings> で利用することを想定しています。
    /// </summary>
    public class AppSettings
    {
        /// <summary>マスターボリューム (0.0〜1.0)</summary>
        public float MasterVolume { get; set; } = 1.0f;

        /// <summary>BGM ボリューム (0.0〜1.0)</summary>
        public float MusicVolume { get; set; } = 0.8f;

        /// <summary>SE ボリューム (0.0〜1.0)</summary>
        public float SfxVolume { get; set; } = 0.9f;

        /// <summary>フルスクリーンモードか</summary>
        public bool FullScreen { get; set; } = false;

        /// <summary>ウィンドウ幅</summary>
        public int Width { get; set; } = 1280;

        /// <summary>ウィンドウ高さ</summary>
        public int Height { get; set; } = 720;

        /// <summary>言語コード (例: "ja-JP", "en-US")</summary>
        public string Language { get; set; } = "ja-JP";

        /// <summary>VSYNC を有効にするか</summary>
        public bool VSync { get; set; } = true;
    }
}