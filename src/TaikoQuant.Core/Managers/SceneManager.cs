using System;
using System.Collections.Generic;
using TaikoQuant.Core;

namespace TaikoQuant.Core.Managers
{
    /// <summary>
    /// Manages the current scene and handles transitions between scenes.
    /// </summary>
    public class SceneManager
    {
        private readonly Dictionary<SceneType, Func<object[], IScene>> _sceneFactories;
        private IScene _currentScene = null!;
        private readonly IInputService _input;
        private readonly IAudioService _audio;
        private readonly IRenderer _renderer;

        public SceneManager(IInputService input, IAudioService audio, IRenderer renderer)
        {
            _input = input;
            _audio = audio;
            _renderer = renderer;

            // Register scene factories.
            _sceneFactories = new Dictionary<SceneType, Func<object[], IScene>>
            {
                { SceneType.Title, _ => new Scenes.TitleScreen() },
                { SceneType.SongSelect, _ => new Scenes.SongSelectScene() },
                { SceneType.DiffSelect, args => new Scenes.DiffSelectScene(args) },
                { SceneType.GamePlay, args => new Scenes.GamePlayScene(args) }
            };

            // Start with the title scene.
            ChangeScene(SceneType.Title);
        }

        public void ChangeScene(SceneType sceneType, params object[] args)
        {
            // Dispose the current scene if it exists.
            if (_currentScene != null)
            {
                _currentScene.Dispose();
            }

            // Create the new scene.
            if (_sceneFactories.TryGetValue(sceneType, out var factory))
            {
                _currentScene = factory(args);
                _currentScene.Init(_renderer, _audio);
            }
            else
            {
                throw new ArgumentException($"Unknown scene type: {sceneType}");
            }
        }

        public void Update()
        {
            // Update the current scene.
            bool requestsChange = _currentScene.Update(_input, _audio);

            if (requestsChange)
            {
                // Get the requested next scene.
                SceneType? nextScene = _currentScene.GetNextScene();
                if (nextScene.HasValue)
                {
                    // If the next scene is DiffSelect or GamePlay, we may need to pass arguments.
                    object[] args = Array.Empty<object>();
                    if (nextScene == SceneType.DiffSelect)
                    {
                        // Pass the selected song from the SongSelectScene.
                        if (_currentScene is Scenes.SongSelectScene songSelect)
                        {
                            args = new object[] { songSelect.SelectedSong };
                        }
                    }
                    else if (nextScene == SceneType.GamePlay)
                    {
                        // For GamePlay, we need the song and difficulty from DiffSelect.
                        // We'll get them from the DiffSelectScene.
                        if (_currentScene is Scenes.DiffSelectScene diffSelect)
                        {
                            args = new object[] { diffSelect.SelectedSong, diffSelect.SelectedDifficulty };
                        }
                    }

                    ChangeScene(nextScene.Value, args);
                }
                // If no specific next scene is requested, we do not change the scene.
                // The current scene may have reset its request via GetNextScene().
            }
        }

        public void Draw()
        {
            _renderer.BeginScene();
            _currentScene.Draw(_renderer);
            _renderer.EndScene();
        }
    }
}