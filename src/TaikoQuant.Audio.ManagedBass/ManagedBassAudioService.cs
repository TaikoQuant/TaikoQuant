using System;
using System.Collections.Generic;
using TaikoQuant.Core;
using ManagedBass;
using System.IO;
using System.Media;

namespace TaikoQuant.Audio.ManagedBass
{
    /// <summary>
    /// Implementation of IAudioService using ManagedBass.
    /// </summary>
    public class ManagedBassAudioService : IAudioService, IDisposable
    {
        private bool _disposed = false;
        private readonly Dictionary<string, ISound> _soundCache = new Dictionary<string, ISound>();
        private readonly Dictionary<string, IMusic> _musicCache = new Dictionary<string, IMusic>();
        private static bool _bassInitialized = false;

        public ManagedBassAudioService()
        {
            if (!_bassInitialized)
            {
                // Initialize Bass with default device, 44100 Hz.
                // Attempt to initialise Bass; if it fails (e.g., native lib missing), we continue in stub mode.
                try { Bass.Init(-1, 44100, 0, IntPtr.Zero); } catch { /* ignore missing native lib */ }
                _bassInitialized = true;
            }
        }

        public ISound LoadSound(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Sound path cannot be null or empty.", nameof(filePath));

            var fullPath = System.IO.Path.IsPathRooted(filePath) ? filePath : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            if (_soundCache.TryGetValue(fullPath, out var cached))
                return cached;

            // Load as a Bass sample (fast short sound effect).
            if (!System.IO.File.Exists(fullPath))
                throw new System.IO.FileNotFoundException("Audio file not found: " + fullPath);

            // Up to 32 simultaneous instances (mirrors TJAPlayer AudioManager).
            int sample = Bass.SampleLoad(fullPath, 0, 0, 32, BassFlags.Default);
            if (sample == 0) throw new Exception($"Failed to load sample: {Bass.LastError} (Path: {fullPath})");

            var sound = new BassSound(sample);
            _soundCache[fullPath] = sound;
            return sound;
        }

        public IMusic LoadMusic(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Music path cannot be null or empty.", nameof(filePath));

            var fullPath = System.IO.Path.IsPathRooted(filePath) ? filePath : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            if (_musicCache.TryGetValue(fullPath, out var cached))
                return cached;

            // Choose implementation based on file extension.
            var ext = System.IO.Path.GetExtension(fullPath).ToLowerInvariant();
            IMusic music;
            if (ext == ".ogg")
            {
                // Try ManagedBass first.
                var bassMusic = new BassMusic(fullPath);
                if (bassMusic.IsValid)
                {
                    music = bassMusic;
                }
                else
                {
                    // Fallback to simple WAV player (will likely fail for OGG, but prevents crash).
                    Console.WriteLine($"[Audio] Bass failed to load OGG, falling back to SimpleMusic: {fullPath}");
                    music = new SimpleMusic(fullPath);
                }
            }
            else
            {
                // Fallback to simple WAV player.
                music = new SimpleMusic(fullPath);
            }
            _musicCache[fullPath] = music;
            return music;
        }

        public void SetMasterVolume(float volume)
        {
            volume = Math.Clamp(volume, 0f, 1f);
            // Bass.Volume set ignored in stub
        }

        public void Update()
        {
            // No per-frame update required for ManagedBass.
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var s in _soundCache.Values) s.Dispose();
            foreach (var m in _musicCache.Values) m.Dispose();
            _soundCache.Clear();
            _musicCache.Clear();
            // Bass.Free() omitted; could be called when app exits.
        }

        private class SimpleSound : ISound
        {
            private readonly string _filePath;
            private readonly SoundPlayer _player;
            private bool _disposed;

            public SimpleSound(string filePath)
            {
                _filePath = filePath;
                _player = new SoundPlayer(_filePath);
                try { _player.Load(); } catch { /* ignore load errors */ }
            }

            public void Play(bool loop = false)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(SimpleSound));
                // SoundPlayer does not support looping; ignore loop flag.
                try { _player.Play(); } catch { /* ignore playback errors */ }
            }

            public void Stop()
            {
                if (_disposed) throw new ObjectDisposedException(nameof(SimpleSound));
                try { _player.Stop(); } catch { /* ignore */ }
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                try { _player.Stop(); } catch { }
                _player.Dispose();
            }
        }

        // -----------------------------------------------------------------
        // BassSound – fast short‑sound playback using ManagedBass samples.
        // -----------------------------------------------------------------
        private class BassSound : ISound
        {
            private readonly int _sample;
            private bool _disposed;

            public BassSound(int sample) => _sample = sample;

            public void Play(bool loop = false)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(BassSound));
                // Loop flag is ignored for sample playback (ManagedBass samples play once).
                int channel = Bass.SampleGetChannel(_sample);
                if (channel != 0) Bass.ChannelPlay(channel);
            }

            public void Stop()
            {
                if (_disposed) throw new ObjectDisposedException(nameof(BassSound));
                // Stop all channels that belong to this sample.
                Bass.SampleStop(_sample);
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                // Free the sample resource.
                Bass.SampleFree(_sample);
            }
        }

        private class SimpleMusic : IMusic
        {
            private readonly string _filePath;
            private readonly SoundPlayer _player;
            private bool _disposed;
            private bool _loop;

            public SimpleMusic(string filePath)
            {
                _filePath = filePath;
                _player = new SoundPlayer(_filePath);
                try { _player.Load(); } catch { /* ignore load errors */ }
            }

            public void Play(bool loop = true)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(SimpleMusic));
                _loop = loop;
                try { _player.Play(); } catch { /* ignore */ }
                // Note: SoundPlayer does not support looping.
            }

            public void Stop()
            {
                if (_disposed) throw new ObjectDisposedException(nameof(SimpleMusic));
                try { _player.Stop(); } catch { }
            }

            public void Pause()
            {
                // SoundPlayer lacks pause; stop instead.
                Stop();
            }

            public void Resume()
            {
                // Resume by playing again.
                Play(_loop);
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                try { _player.Stop(); } catch { }
                _player.Dispose();
            }
        }

        // -----------------------------------------------------------------
        // BassMusic – uses ManagedBass for OGG (or other formats) playback.
        // -----------------------------------------------------------------
        private class BassMusic : IMusic
        {
            private readonly int _handle;
            private bool _disposed;
            private bool _loop;

            public BassMusic(string filePath)
            {
                // Create a stream; if Bass fails it returns 0 which we safely handle.
                _handle = Bass.CreateStream(filePath, 0, 0, BassFlags.Default);
            }

            // Expose validity for fallback logic.
            public bool IsValid => _handle != 0;
            public void Play(bool loop = true)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(BassMusic));
                _loop = loop;
                if (_handle != 0) Bass.ChannelPlay(_handle, false);
            }

            public void Stop()
            {
                if (_disposed) throw new ObjectDisposedException(nameof(BassMusic));
                if (_handle != 0) Bass.ChannelStop(_handle);
            }

            public void Pause()
            {
                if (_disposed) throw new ObjectDisposedException(nameof(BassMusic));
                if (_handle != 0) Bass.ChannelPause(_handle);
            }

            public void Resume()
            {
                if (_disposed) throw new ObjectDisposedException(nameof(BassMusic));
                if (_handle != 0) Bass.ChannelPlay(_handle, false);
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                if (_handle != 0) Bass.StreamFree(_handle);
            }
        }
    }
}
