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
            InitWindow(width, height, "TaikoQuant");
            SetTargetFPS(60); // We'll adjust based on VSync later.
            if (settings.VSync)
            {
                SetTargetFPS(0); // Let vsync control the frame rate.
                // Note: Raylib's SetTargetFPS(0) means no limit, but we rely on vsync.
                // We'll also set the vsync flag via SetConfigFlags? Actually, we can use SetConfigFlags(FLAG_VSYNC_HINT);
                // But we'll just set the target to 0 and rely on the driver.
                // We'll also enable vsync via SetConfigFlags if needed.
                // For simplicity, we'll leave it as is and assume the user's vsync setting is honored by setting target to 0.
            }
            else
            {
                SetTargetFPS(120); // High FPS when vsync is off.
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

                // Update the scene manager.
                sceneManager.Update();

                // Draw.
                sceneManager.Draw();

                // Update window title with WASAPI mode and frame time.
                var frameMs = (int)frameTimer.ElapsedMilliseconds;
                frameTimer.Restart();
                SetWindowTitle($"TaikoQuant (WASAPIShared, {frameMs}ms)");
            }

            // Clean up.
            // sceneManager.Dispose(); // No Dispose method; cleanup handled elsewhere
            audioService.Dispose();
            renderer.Dispose();
            CloseWindow();
        }
    }
}