using System;
using System.Collections.Generic;
using TaikoQuant.Core;
using ManagedBass;
using ManagedBass.Wasapi;
using ManagedBass.Fx;
using ManagedBass.Mix;
using System.Media;

namespace TaikoQuant.Audio.ManagedBass
{
    /// <summary>
    /// 効果音のプチプチ音（ノイズ・クリッピング）を防ぐため、SEをSampleLoad方式に修正した実装。
    /// </summary>
    public class ManagedBassAudioService : IAudioService, IDisposable
    {
        private bool _disposed = false;

        private readonly Dictionary<string, ISound> _soundCache = new Dictionary<string, ISound>();
        private readonly Dictionary<string, int> _sampleCache = new Dictionary<string, int>();

        private static bool _bassInitialized = false;
        private static bool _bassAvailable = false;

        private readonly int _sfxMixer;
        private readonly int _sampleRate = 44100;

        public ManagedBassAudioService()
        {
            if (!_bassInitialized)
            {
                try
                {
                    Bass.Configure(Configuration.UpdatePeriod, 10);
                    Bass.Configure(Configuration.DeviceBufferLength, 50);

                    _bassAvailable = BassWasapi.Init(-1, -1, -1, (WasapiInitFlags)0, 0.0f, 0.0f, null, IntPtr.Zero);
                    if (!_bassAvailable)
                    {
                        Console.WriteLine($"[Audio] BassWasapi.Init failed: {Bass.LastError}. Falling back to SoundPlayer.");
                    }
                }
                catch (Exception ex)
                {
                    _bassAvailable = false;
                    Console.WriteLine($"[Audio] BassWasapi unavailable, falling back to SoundPlayer: {ex.Message}");
                }
                _bassInitialized = true;
            }

            if (_bassAvailable)
            {
                try
                {
                    if (!Bass.Init(-1, _sampleRate, DeviceInitFlags.Default, IntPtr.Zero))
                    {
                        Console.WriteLine($"[Audio] Bass.Init failed: {Bass.LastError}. Falling back to SoundPlayer.");
                        _bassAvailable = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Audio] Bass.Init exception, falling back to SoundPlayer: {ex.Message}");
                    _bassAvailable = false;
                }
            }

            if (_bassAvailable)
            {
                try
                {
                    _sfxMixer = BassMix.CreateMixerStream(_sampleRate, 2, BassFlags.Default);

                    if (_sfxMixer == 0)
                    {
                        Console.WriteLine($"[Audio] Failed to create BASS mixer (Code: {Bass.LastError}). Falling back to SoundPlayer.");
                        _bassAvailable = false;
                    }
                    else
                    {
                        Bass.ChannelPlay(_sfxMixer, false);
                    }
                }
                catch (DllNotFoundException ex)
                {
                    Console.WriteLine($"[Audio] bassmix.dll not found! Please ensure bassmix.dll is in the output directory: {ex.Message}");
                    _bassAvailable = false;
                    _sfxMixer = 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Audio] Unexpected error creating mixer: {ex.Message}");
                    _bassAvailable = false;
                    _sfxMixer = 0;
                }
            }
            else
            {
                _sfxMixer = 0;
            }
        }

        public ISound LoadSound(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath cannot be null or empty.", nameof(filePath));

            var fullPath = System.IO.Path.IsPathRooted(filePath)
                ? filePath
                : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);

            if (_soundCache.TryGetValue(fullPath, out var cached))
                return cached;

            if (!System.IO.File.Exists(fullPath))
                throw new System.IO.FileNotFoundException("Audio file not found: " + fullPath);

            ISound sound;
            if (_bassAvailable)
            {
                // プチノイズ防止・連打対応のため、SampleLoadでサンプルとしてメモリにキャッシュする
                if (!_sampleCache.TryGetValue(fullPath, out int sample))
                {
                    // 32同時発音に設定
                    sample = Bass.SampleLoad(fullPath, 0, 0, 32, BassFlags.Default);
                    if (sample != 0)
                    {
                        _sampleCache[fullPath] = sample;
                    }
                }

                if (sample == 0)
                {
                    Console.WriteLine($"[Audio] SampleLoad failed ({Bass.LastError}): {fullPath}");
                    sound = new SimpleSound(fullPath);
                }
                else
                {
                    sound = new BassSound(sample, _sfxMixer);
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
                throw new ArgumentException("filePath cannot be null or empty.", nameof(filePath));

            var fullPath = System.IO.Path.IsPathRooted(filePath)
                ? filePath
                : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);

            if (!System.IO.File.Exists(fullPath))
                throw new System.IO.FileNotFoundException("Music file not found: " + fullPath);

            if (_bassAvailable)
            {
                // 音楽は Float なしで通常作成するとクリッピングノイズが軽減される場合があります
                int handle = Bass.CreateStream(fullPath, 0, 0, BassFlags.Prescan);
                if (handle == 0)
                {
                    Console.WriteLine($"[Audio] CreateStream failed ({Bass.LastError}): {fullPath}");
                    return new SimpleMusic(fullPath);
                }

                if (_sfxMixer != 0)
                {
                    bool added = BassMix.MixerAddChannel(_sfxMixer, handle, BassFlags.AutoFree);
                    if (!added)
                    {
                        Console.WriteLine($"[Audio] MixerAddChannel for music failed ({Bass.LastError}): {fullPath}");
                    }
                }
                return new BassMusic(handle);
            }
            else
            {
                return new SimpleMusic(fullPath);
            }
        }

        public void SetMasterVolume(float volume)
        {
            volume = Math.Clamp(volume, 0f, 1f);
            if (_bassAvailable)
            {
                Bass.GlobalStreamVolume = (int)(volume * 10000);
                Bass.GlobalSampleVolume = (int)(volume * 10000);
                if (_sfxMixer != 0)
                {
                    Bass.ChannelSetAttribute(_sfxMixer, ChannelAttribute.Volume, volume);
                }
            }
        }

        public void Update()
        {
            if (_bassAvailable)
            {
                Bass.Update(0);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var s in _soundCache.Values) s.Dispose();
            _soundCache.Clear();

            foreach (var sample in _sampleCache.Values)
            {
                Bass.SampleFree(sample);
            }
            _sampleCache.Clear();

            if (_bassAvailable)
            {
                if (_sfxMixer != 0) Bass.ChannelStop(_sfxMixer);
                BassWasapi.Free();
                Bass.Free();
                _bassInitialized = false;
                _bassAvailable = false;
            }
        }

        private class SimpleSound : ISound
        {
#pragma warning disable CA1416
            private readonly SoundPlayer _player;
            private bool _disposed;
            private float _volume = 1.0f;
            private bool _playing = false;

            public SimpleSound(string filePath)
            {
                _player = new SoundPlayer(filePath);
                try { _player.Load(); } catch { }
            }

            public void Play(bool loop = false)
            {
                if (_disposed) return;
                try { _player.Play(); } catch { }
                _playing = true;
            }

            public void Stop()
            {
                if (_disposed) return;
                try { _player.Stop(); } catch { }
                _playing = false;
            }

            public void SetVolume(float volume)
            {
                if (_disposed) return;
                _volume = Math.Clamp(volume, 0f, 1f);
            }

            public bool IsPlaying() => _playing;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                try { _player.Stop(); } catch { }
                _playing = false;
                _player.Dispose();
            }
#pragma warning restore CA1416
        }

        private class BassSound : ISound
        {
            private readonly int _sample;
            private readonly int _mixer;
            private bool _disposed;
            private int _channel = 0;
            private float _volume = 1.0f;

            public BassSound(int sample, int mixer)
            {
                _sample = sample;
                _mixer = mixer;
            }

            public void Play(bool loop = false)
            {
                if (_disposed) return;
                int ch = Bass.SampleGetChannel(_sample);
                if (ch != 0)
                {
                    _channel = ch;
                    if (_mixer != 0)
                    {
                        // AutoFreeを使用せず、チャネルは手動で管理することで再生途中で切れることを防止
                        BassMix.MixerAddChannel(_mixer, ch, 0);
                    }
                    Bass.ChannelPlay(ch, true);
                    ApplyVolume();
                }
            }

            public void Stop()
            {
                if (_disposed) return;
                if (_channel != 0)
                {
                    Bass.ChannelStop(_channel);
                    _channel = 0;
                }
            }

            public void SetVolume(float volume)
            {
                if (_disposed) return;
                _volume = Math.Clamp(volume, 0f, 1f);
                ApplyVolume();
            }

            public bool IsPlaying()
            {
                if (_disposed || _channel == 0) return false;
                return Bass.ChannelIsActive(_channel) == PlaybackState.Playing;
            }

            private void ApplyVolume()
            {
                if (_channel != 0)
                {
                    // 音割れ(クリッピング)を防ぐためにボリュームをわずかに絞る、またはそのまま適用
                    Bass.ChannelSetAttribute(_channel, ChannelAttribute.Volume, _volume);
                }
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                if (_channel != 0)
                {
                    Bass.ChannelStop(_channel);
                }
            }
        }

        private class SimpleMusic : IMusic
        {
#pragma warning disable CA1416
            private readonly SoundPlayer _player;
            private bool _disposed;
            private bool _loop;
            private float _volume = 1.0f;
            private bool _playing = false;

            public SimpleMusic(string filePath)
            {
                _player = new SoundPlayer(filePath);
                try { _player.Load(); } catch { }
            }

            public void Play(bool loop = true)
            {
                if (_disposed) return;
                _loop = loop;
                try { _player.Play(); } catch { }
                _playing = true;
            }

            public void Stop()
            {
                if (_disposed) return;
                try { _player.Stop(); } catch { }
                _playing = false;
            }

            public void Pause() => Stop();
            public void Resume() => Play(_loop);

            public void SetVolume(float volume)
            {
                if (_disposed) return;
                _volume = Math.Clamp(volume, 0f, 1f);
            }

            public bool IsPlaying() => _playing;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                try { _player.Stop(); } catch { }
                _playing = false;
                _player.Dispose();
            }
#pragma warning restore CA1416
        }

        private class BassMusic : IMusic
        {
            private readonly int _handle;
            private bool _disposed;
            private bool _loop;
            private float _volume = 1.0f;

            public BassMusic(int handle) => _handle = handle;

            public void Play(bool loop = false)
            {
                if (_disposed || _handle == 0) return;
                _loop = loop;
                Bass.ChannelFlags(_handle, loop ? BassFlags.Loop : BassFlags.Default, BassFlags.Loop);
                Bass.ChannelPlay(_handle, true);
                ApplyVolume();
            }

            public void Stop()
            {
                if (_disposed || _handle == 0) return;
                Bass.ChannelStop(_handle);
            }

            public void Pause()
            {
                if (_disposed || _handle == 0) return;
                Bass.ChannelPause(_handle);
            }

            public void Resume()
            {
                if (_disposed || _handle == 0) return;
                Bass.ChannelPlay(_handle, false);
                ApplyVolume();
            }

            public void SetVolume(float volume)
            {
                if (_disposed) return;
                _volume = Math.Clamp(volume, 0f, 1f);
                ApplyVolume();
            }

            public bool IsPlaying()
            {
                return !_disposed && Bass.ChannelIsActive(_handle) == PlaybackState.Playing;
            }

            private void ApplyVolume()
            {
                if (_handle != 0)
                {
                    Bass.ChannelSetAttribute(_handle, ChannelAttribute.Volume, _volume);
                }
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