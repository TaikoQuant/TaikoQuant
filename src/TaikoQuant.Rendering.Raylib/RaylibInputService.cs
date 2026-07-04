using System;
using System.Collections.Generic;
using TaikoQuant.Core;
using Raylib_cs;

namespace TaikoQuant.Rendering.Raylib
{
    /// <summary>
    /// Input service using Raylib-cs.
    /// </summary>
    public class RaylibInputService : IInputService
    {
        private readonly Dictionary<GameKey, bool> _current = new();
        private readonly Dictionary<GameKey, bool> _previous = new();

        // Map GameKey to Raylib KeyboardKey
        private readonly Dictionary<GameKey, KeyboardKey> _keyMapping = new()
        {
            { GameKey.Enter, KeyboardKey.Enter },
            { GameKey.F1, KeyboardKey.F1 },
            { GameKey.F2, KeyboardKey.F2 },
            { GameKey.Escape, KeyboardKey.Escape },
            { GameKey.MenuUp, KeyboardKey.Up },
            { GameKey.MenuDown, KeyboardKey.Down },
            { GameKey.MenuLeft, KeyboardKey.Left },
            { GameKey.MenuRight, KeyboardKey.Right },
            { GameKey.MenuAccept, KeyboardKey.Enter },
            { GameKey.MenuCancel, KeyboardKey.Escape },
            { GameKey.DonLeft, KeyboardKey.D },
            { GameKey.DonRight, KeyboardKey.K },
            { GameKey.KaLeft, KeyboardKey.J },
            { GameKey.KaRight, KeyboardKey.F }
        };

        public RaylibInputService()
        {
            foreach (GameKey key in Enum.GetValues(typeof(GameKey)))
            {
                _current[key] = false;
                _previous[key] = false;
            }
        }

        public void Update()
        {
            foreach (GameKey key in Enum.GetValues(typeof(GameKey)))
            {
                _previous[key] = _current[key];
                
                if (_keyMapping.TryGetValue(key, out KeyboardKey raylibKey))
                {
                    _current[key] = Raylib_cs.Raylib.IsKeyDown(raylibKey);
                }
                else
                {
                    _current[key] = false;
                }
            }
        }

        public bool IsKeyPressed(GameKey key) => _current[key] && !_previous[key];
        public bool IsKeyDown(GameKey key) => _current[key];
        public bool IsKeyReleased(GameKey key) => !_current[key] && _previous[key];
    }
}
