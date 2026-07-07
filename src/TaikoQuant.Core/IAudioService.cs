using System;

namespace TaikoQuant.Core
{
    /// <summary>
    /// Abstracts audio playback and loading.
    /// </summary>
    public interface IAudioService : IDisposable
    {
        /// <summary>
        /// Loads a short sound effect (e.g., drum hits).
        /// </summary>
        /// <param name="filePath">Path to the audio file (WAV/OGG).</param>
        /// <returns>A handle to the sound effect.</returns>
        ISound LoadSound(string filePath);

        /// <summary>
        /// Loads a music stream (e.g., background song).
        /// </summary>
        /// <param name="filePath">Path to the music file (OGG/MP3/WAV).</param>
        /// <returns>A handle to the music stream.</returns>
        IMusic LoadMusic(string filePath);

        /// <summary>
        /// Sets the master volume (0.0 to 1.0).
        /// </summary>
        void SetMasterVolume(float volume);

        /// <summary>
        /// Updates any audio streaming (call once per frame).
        /// </summary>
        void Update();
    }

    /// <summary>
    /// Represents a sound effect that can be played.
    /// </summary>
    public interface ISound : IDisposable
    {
        /// <summary>
        /// Plays the sound effect.
        /// </summary>
        /// <param name="loop">If true, the sound will loop.</param>
        void Play(bool loop = false);

        /// <summary>
        /// Stops the sound if it is playing.
        /// </summary>
        void Stop();

        /// <summary>
        /// Sets the volume of this sound (0.0 to 1.0).
        /// </summary>
        /// <param name="volume">Volume level.</param>
        void SetVolume(float volume);
        bool IsPlaying();
    }

    /// <summary>
    /// Represents a music stream that can be played.
    /// </summary>
    public interface IMusic : IDisposable
    {
        /// <summary>
        /// Plays the music.
        /// </summary>
        /// <param name="loop">If true, the music will loop.</param>
        void Play(bool loop = true);

        /// <summary>
        /// Stops the music.
        /// </summary>
        void Stop();

        /// <summary>
        /// Pauses the music.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the music from pause.
        /// </summary>
        void Resume();

        /// <summary>
        /// Sets the volume of this music (0.0 to 1.0).
        /// </summary>
        /// <param name="volume">Volume level.</param>
        void SetVolume(float volume);
        bool IsPlaying();
    }
}