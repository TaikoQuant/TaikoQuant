using System;
using System.IO;

namespace TaikoQuant.System
{
    /// <summary>
    /// ファイルシステムウォッチャーをラップし、設定ファイルの変更を監視して自動リロードを行います。
    /// </summary>
    /// <typeparam name="T">設定クラスの型（参照型、parameterless コンストラクタ必須）</typeparam>
    public class ConfigWatcher<T> : IDisposable where T : class, new()
    {
        private readonly ConfigManager<T> _config;
        private readonly FileSystemWatcher _watcher;

        /// <summary>
        /// 指定された設定ファイルの変更を監視します。
        /// </summary>
        /// <param name="filePath">監視対象の設定ファイルフルパス</param>
        /// <param name="config">監視対象の ConfigManager インスタンス</param>
        public ConfigWatcher(string filePath, ConfigManager<T> config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            var directory = Path.GetDirectoryName(filePath)!;
            var fileName = Path.GetFileName(filePath);

            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _watcher.Changed   += OnChanged;
            _watcher.Created   += OnChanged;
            _watcher.Deleted   += OnDeleted;
            _watcher.Renamed   += OnRenamed;
        }

        private void OnChanged(object sender, FileSystemEventArgs e) => _config.Reload();
        private void OnDeleted(object sender, FileSystemEventArgs e) => _config.Load(); // デフォルト作成または空に戻す
        private void OnRenamed(object sender, RenamedEventArgs e)   => _config.Reload();

        /// <summary>
        /// ファイルシステムウォッチャーのリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
    }
}