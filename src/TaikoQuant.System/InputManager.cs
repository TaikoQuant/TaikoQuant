using Raylib_cs;
using System.Numerics; // For Vector2

namespace TaikoQuant.System
{
    /// <summary>
    /// キーボード入力を提供する静的に提供するシンプルな入力マネージャ。
    /// Raylib-cs の keyboard state をラップします。
    /// </summary>
    public static class InputManager
    {
        /// <summary>
        /// 指定されたキーが現在押されているかを返します。
        /// </summary>
        public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown(key);

        /// <summary>
        /// 指定されたキーがこのフレームで押されたかを返します。
        /// </summary>
        public static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed(key);

        /// <summary>
        /// 指定されたキーがこのフレームで離されたかを返します。
        /// </summary>
        public static bool IsKeyReleased(KeyboardKey key) => Raylib.IsKeyReleased(key);

        /// <summary>
        /// マウスの現在位置を取得します。
        /// </summary>
        public static Vector2 GetMousePosition() => Raylib.GetMousePosition();

        /// <summary>
        /// マウス左ボタンが現在押されているかを返します。
        /// </summary>
        public static bool IsMouseButtonDown(MouseButton button) => Raylib.IsMouseButtonDown(button);

        /// <summary>
        /// マウス左ボタンがこのフレームで押されたかを返します.
        /// </summary>
        public static bool IsMouseButtonPressed(MouseButton button) => Raylib.IsMouseButtonPressed(button);

        /// <summary>
        /// マウス左ボタンがこのフレームで離されたかを返します。
        /// </summary>
        public static bool IsMouseButtonReleased(MouseButton button) => Raylib.IsMouseButtonReleased(button);
    }
}