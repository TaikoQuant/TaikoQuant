using System;
using System.IO;
using System.Text.Json;

namespace TaikoQuant.System
{
    /// <summary>
    /// JSON ファイルベースの設定を管理するジェネリッククラス。
    /// </summary>
    /// <typeparam name="T">設定クラスの型（参照型、parameterless コンストラクタ必須）</typeparam>
    public class ConfigManager<T> where T : class, new()
    {
        private readonly string _filePath;
        private T? _data;
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
        /// <summary>
        /// 設定が再読み込みまたは保存されたときに発生します。
        /// </summary>
        public event Action<T>? OnChanged;

        /// <summary>
        /// 指定されたファイルパスの設定ファイルを管理します。
        /// </summary>
        /// <param name="filePath">設定ファイルのフルパスまたは相対パス</param>
        public ConfigManager(string filePath)
        {
            _filePath = Path.GetFullPath(filePath);
            LoadOrCreateDefault();
        }

        /// <summary>
        /// 現在の設定インスタンスを取得します。ロードされていない場合は新しいインスタンスを返します。
        /// </summary>
        public T Current => _data ?? new T();

        /// <summary>
        /// ファイルから設定を読み込みます。ファイルが存在しない場合はデフォルト値で新規作成し、保存します。
        /// </summary>
        public void Load()
        {
            if (!File.Exists(_filePath))
            {
                _data = new T();
                Save(); // デフォルト設定をファイルに書き込む
            }
            else
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    _data = JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
                }
                catch (Exception ex)
                {
                    // 読み込み失敗時はデフォルトで復旧し、警告を出す
                    Console.Error.WriteLine($"[ConfigManager] 設定ファイルの読み込みに失敗しました ({_filePath}): {ex.Message}");
                    _data = new T();
                }
            }

            OnChanged?.Invoke(_data);
        }

        /// <summary>
        /// 現在の設定をファイルに保存します。
        /// </summary>
        public void Save()
        {
            var data = _data ?? new T();
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            File.WriteAllText(_filePath, json);
            OnChanged?.Invoke(_data);
        }

        /// <summary>
        /// ファイルから再読み込みします（外部から変更された場合など）。
        /// </summary>
        public void Reload() => Load();

        /// <summary>
        /// デフォルト値で設定を初期化または再初期化します。
        /// </summary>
        public void ResetToDefault()
        {
            _data = new T();
            Save();
        }

        private void LoadOrCreateDefault() => Load();
    }
}