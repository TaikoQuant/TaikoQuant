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
        private static readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");

        public static GameSettings Load()
        {
            var settings = new GameSettings();
            try
            {
                if (File.Exists(_configPath))
                {
                    var lines = File.ReadAllLines(_configPath);
                    foreach (var line in lines)
                    {
                        var span = line.Trim();
                        if (string.IsNullOrEmpty(span) || span.StartsWith(";") || span.StartsWith("#")) continue;

                        var split = span.Split('=', 2);
                        if (split.Length == 2)
                        {
                            var key = split[0].Trim();
                            var val = split[1].Trim();

                            if (key.Equals("AutoPlay", StringComparison.OrdinalIgnoreCase))
                                if (bool.TryParse(val, out bool b)) settings.AutoPlay = b;
                            if (key.Equals("VSync", StringComparison.OrdinalIgnoreCase))
                                if (bool.TryParse(val, out bool b)) settings.VSync = b;
                            if (key.Equals("MasterVolume", StringComparison.OrdinalIgnoreCase))
                                if (float.TryParse(val, out float f)) settings.MasterVolume = f;
                            if (key.Equals("SfxVolume", StringComparison.OrdinalIgnoreCase))
                                if (float.TryParse(val, out float f)) settings.SfxVolume = f;
                            if (key.Equals("MusicVolume", StringComparison.OrdinalIgnoreCase))
                                if (float.TryParse(val, out float f)) settings.MusicVolume = f;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If any error, return defaults.
            }
            return settings;
        }

        public static void Save(GameSettings settings)
        {
            try
            {
                var dir = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var lines = new[]
                {
                    "[Settings]",
                    $"AutoPlay={settings.AutoPlay}",
                    $"VSync={settings.VSync}",
                    $"MasterVolume={settings.MasterVolume}",
                    $"SfxVolume={settings.SfxVolume}",
                    $"MusicVolume={settings.MusicVolume}"
                };
                File.WriteAllLines(_configPath, lines);
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