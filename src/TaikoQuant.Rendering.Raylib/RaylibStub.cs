using System;

namespace TaikoQuant.Helpers
{
    // Minimal stub of the Raylib static class to satisfy the Game entry point.
    public static class RaylibStub
    {
        public static void InitWindow(int width, int height, string title) { }
        public static void SetTargetFPS(int fps) { }
        public static bool WindowShouldClose() => false;
        public static void BeginDrawing() { }
        public static void EndDrawing() { }
        public static void CloseWindow() { }
    }
}
