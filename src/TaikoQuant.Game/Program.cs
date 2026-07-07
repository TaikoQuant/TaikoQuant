using System;
using System.Diagnostics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using TaikoQuant.Core;
using TaikoQuant.Audio.ManagedBass;
using TaikoQuant.Rendering.Raylib;
using TaikoQuant.Core.Managers;

namespace TaikoQuant.Game
{
    internal class Program
    {
        private static void Main()
        {
            // Load settings.
            var settings = SettingsHelper.Load();

            // Initialize Raylib window.
            const int width = 1280;
            const int height = 720;

            if (settings.VSync)
            {
                SetConfigFlags(ConfigFlags.VSyncHint);
            }

            InitWindow(width, height, "TaikoQuant");

            if (!settings.VSync)
            {
                SetTargetFPS(120); // High FPS when vsync is off.
            }
            else
            {
                int refreshRate = GetMonitorRefreshRate(GetCurrentMonitor());
                SetTargetFPS(refreshRate > 0 ? refreshRate : 60); // Safety cap to prevent 100% CPU lock if VSync fails
            }

            // Initialize audio service.
            var audioService = new ManagedBassAudioService();
            audioService.SetMasterVolume(settings.MasterVolume);

            // Create input and renderer services.
            var inputService = new RaylibInputService();
            var renderer = new RaylibRenderer();

            // Create the scene manager.
            var sceneManager = new SceneManager(inputService, audioService, renderer);
            // Timer to measure per‑frame duration for title display.
            var frameTimer = Stopwatch.StartNew();

            // Main game loop.
            while (!WindowShouldClose())
            {
                // Update input state.
                inputService.Update();

                // Update audio buffers every frame (important for BASS stability).
                audioService.Update();

                // Update the scene manager.
                sceneManager.Update();

                // Draw.
                sceneManager.Draw();

                // Update window title with actual FPS instead of misleading frame time as audio latency.
                SetWindowTitle($"TaikoQuant | FPS: {GetFPS()}");
            }

            // Clean up.
            // sceneManager.Dispose(); // No Dispose method; cleanup handled elsewhere
            audioService.Dispose();
            renderer.Dispose();
            CloseWindow();
        }
    }
}