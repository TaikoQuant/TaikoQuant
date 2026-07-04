using System;

namespace TaikoQuant.Core
{
    /// <summary>
    /// Abstracts input handling from the underlying system-specific input APIs.
    /// </summary>
    public interface IInputService
    {
        /// <summary>
        /// Call once per frame to update internal state.
        /// </summary>
        void Update();

        /// <summary>
        /// Returns true if the key was pressed this frame (transition from up to down).
        /// </summary>
        bool IsKeyPressed(GameKey key);

        /// <summary>
        /// Returns true if the key is currently held down.
        /// </summary>
        bool IsKeyDown(GameKey key);

        /// <summary>
        /// Returns true if the key was released this frame (transition from down to up).
        /// </summary>
        bool IsKeyReleased(GameKey key);
    }

    /// <summary>
    /// Logical game keys mapped to physical keys.
    /// </summary>
    public enum GameKey
    {
        // Menu / UI
        Enter,
        F1,
        F2,
        Escape,
        MenuUp,
        MenuDown,
        MenuLeft,
        MenuRight,
        MenuAccept,
        MenuCancel,

        // Gameplay - Don (center drum)
        DonLeft,   // D key
        DonRight,  // K key

        // Gameplay - Ka (rim)
        KaLeft,    // J key
        KaRight,   // F key
    }
}