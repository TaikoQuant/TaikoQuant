using System;
using System.Text.Json;
using System.IO;

namespace TaikoQuant.Core
{
    /// <summary>
    /// Simple settings that persist across runs.
    /// </summary>
    public class GameSettings
    {
        public bool AutoPlay { get; set; } = false;
        public bool VSync { get; set; } = true;
        public float MasterVolume { get; set; } = 1.0f;
        public float SfxVolume { get; set; } = 1.0f;
        public float MusicVolume { get; set; } = 1.0f;
    }

    /// <summary>
    /// Helper to load/save settings as JSON.
    /// </summary>
    public static class SettingsHelper
    {
        private static readonly string _configPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TaikoQuant",
                "config.json");

        public static GameSettings Load()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    return JsonSerializer.Deserialize<GameSettings>(json) ?? new GameSettings();
                }
            }
            catch (Exception)
            {
                // If any error, return defaults.
            }
            return new GameSettings();
        }

        public static void Save(GameSettings settings)
        {
            try
            {
                var dir = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch (Exception)
            {
                // Ignore save errors.
            }
        }

        // Helper class to avoid naming conflict.
        private class GameSource
        {
            public bool AutoPlay { get; set; }
            public bool VSync { get; set; }
            public float MasterVolume { get; set; }
            public float SfxVolume { get; set; }
            public float MusicVolume { get; set; }
        }
    }
}