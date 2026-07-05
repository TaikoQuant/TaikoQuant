using System;
using System.Collections.Generic;
using TaikoQuant.Core;
using ManagedBass;
using ManagedBass.Wasapi;
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
        private static bool _bassAvailable = false;

        public ManagedBassAudioService()
        {
            if (!_bassInitialized)
            {
                // NOTE: Do not shrink Configuration.DeviceBufferLength / UpdatePeriod too
                // aggressively here. Very small buffers (previously 20ms/10ms) caused the
                // audio device to underrun on short one-shot hit sounds, which cut the tail
                // of the sample off before it finished playing. Keep BASS's own defaults for
                // buffer/update timing (they are already tuned for low-latency playback) and
                // only bump the playback buffer slightly to give the mixer more headroom.
                try
                {
                    Bass.Configure(Configuration.PlaybackBufferLength, 300); // ms, headroom against underruns without adding perceptible latency
                    Bass.Configure(Configuration.UpdateThreads, 2);          // let BASS use more than one update thread

                    // Initialise Bass with the default device, 44100 Hz.
                    _bassAvailable = BassWasapi.Init(-1, 44100, WasapiInitFlags.Shared, IntPtr.Zero);
                    if (!_bassAvailable)
                    {
                        Console.WriteLine($"[Audio] Bass.Init failed: {Bass.LastError}. Falling back to SoundPlayer (expect degraded audio).");
                    }
                }
                catch (Exception ex)
                {
                    // Native lib missing or otherwise unavailable — continue in stub mode.
                    _bassAvailable = false;
                    Console.WriteLine($"[Audio] Bass unavailable, falling back to SoundPlayer: {ex.Message}");
                }
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

            if (!System.IO.File.Exists(fullPath))
                throw new System.IO.FileNotFoundException("Audio file not found: " + fullPath);

            ISound sound;
            if (_bassAvailable)
            {
                // BASS handles wav/mp3/ogg samples natively and mixes them on one audio
                // thread, which is far cheaper than one SoundPlayer/winmm handle per hit.
                // Up to 32 simultaneous instances (mirrors TJAPlayer AudioManager).
                int sample = Bass.SampleLoad(fullPath, 0, 0, 32, BassFlags.Default);
                if (sample == 0)
                {
                    Console.WriteLine($"[Audio] Bass failed to load sample ({Bass.LastError}), falling back to SoundPlayer: {fullPath}");
                    sound = new SimpleSound(fullPath);
                }
                else
                {
                    sound = new BassSound(sample);
                }
            }
            else
            {
                sound = new SimpleSound(fullPath);
            }

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

            // Always prefer BASS (wav/mp3/ogg all supported) — SoundPlayer is a synchronous,
            // WAV-only WinMM wrapper that was previously used for every non-OGG file and is
            // the main source of the stuttering/heavy playback in-game.
            IMusic music;
            if (_bassAvailable)
            {
                var bassMusic = new BassMusic(fullPath);
                if (bassMusic.IsValid)
                {
                    music = bassMusic;
                }
                else
                {
                    Console.WriteLine($"[Audio] Bass failed to load music ({Bass.LastError}), falling back to SimpleMusic: {fullPath}");
                    music = new SimpleMusic(fullPath);
                }
            }
            else
            {
                music = new SimpleMusic(fullPath);
            }

            _musicCache[fullPath] = music;
            return music;
        }

        public void SetMasterVolume(float volume)
        {
            volume = Math.Clamp(volume, 0f, 1f);
            if (_bassAvailable)
            {
                Bass.GlobalStreamVolume = (int)(volume * 10000);
                Bass.GlobalSampleVolume = (int)(volume * 10000);
            }
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
            if (_bassAvailable)
            {
                Bass.Free();
                _bassInitialized = false;
                _bassAvailable = false;
            }
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
        // BassMusic – uses ManagedBass for OGG/MP3/WAV playback.
        // -----------------------------------------------------------------
        private class BassMusic : IMusic
        {
            private readonly int _handle;
            private bool _disposed;
            private bool _loop;

            public BassMusic(string filePath)
            {
                // BassFlags.AsyncFile lets BASS read the file on its own background thread
                // instead of blocking the caller/game thread on disk I/O — this was the main
                // cause of stutter when streaming music during gameplay.
                _handle = Bass.CreateStream(filePath, 0, 0, BassFlags.Default | BassFlags.AsyncFile);
            }

            // Expose validity for fallback logic.
            public bool IsValid => _handle != 0;

            public void Play(bool loop = true)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(BassMusic));
                _loop = loop;
                if (_handle != 0)
                {
                    Bass.ChannelFlags(_handle, loop ? BassFlags.Loop : BassFlags.Default, BassFlags.Loop);
                    Bass.ChannelPlay(_handle, false);
                }
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
