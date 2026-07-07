using System;

namespace TaikoQuant.Core
{
    /// <summary>
    /// Interface for a game scene (title, menu, gameplay, etc.).
    /// Update returns true if the scene requests a change to another scene.
    /// </summary>
    public interface IScene : IDisposable
    {
        /// <summary>
        /// Initialize the scene and load resources.
        /// </summary>
        /// <param name="renderer">Renderer to load textures/fonts with.</param>
        /// <param name="audio">Audio service to load sounds with.</param>
        void Init(IRenderer renderer, IAudioService audio);

        /// <summary>
        /// Update the scene logic.
        /// </summary>
        /// <param name="input">Input service for reading player input.</param>
        /// <param name="audio">Audio service for playing sounds.</param>
        /// <returns>
        /// True if the scene requests a change to another scene.
        /// </returns>
        bool Update(IInputService input, IAudioService audio);

        /// <summary>
        /// Draw the scene using the provided renderer.
        /// </summary>
        /// <param name="renderer">Renderer to draw with.</param>
        void Draw(IRenderer renderer);

        /// <summary>
        /// Gets the type of the next scene to transition to, if any.
        /// After calling this method, the scene should reset its next state.
        /// Returns null if no scene change is requested.
        /// </summary>
        SceneType? GetNextScene();
    }
}