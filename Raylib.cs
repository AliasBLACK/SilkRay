using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using System;

namespace SilkRay
{
    /// <summary>
    /// Core Raylib functions implementation using Silk.NET
    /// </summary>
    public static class Raylib
    {
        private static IWindow? _window;
        private static GL? _gl;
        private static Renderer? _renderer;
        private static IInputContext? _input;
        private static bool _shouldClose;

        // Window-related functions (rcore)
        public static void InitWindow(int width, int height, string title)
        {
            var options = WindowOptions.Default;
            options.Size = new Silk.NET.Maths.Vector2D<int>(width, height);
            options.Title = title;
            options.VSync = true;
            options.UpdatesPerSecond = 60;
            options.FramesPerSecond = 60;

            _window = Window.Create(options);
            
            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Resize += OnResize;
            _window.Closing += OnClosing;
            
            // Initialize the window immediately for traditional loop usage
            _window.Initialize();
            
            if (_window.IsInitialized)
            {
                OnLoad();
            }
        }

        public static bool WindowShouldClose()
        {
            if (_window == null || !_window.IsInitialized) return true;
            
            // Process window events to keep window responsive
            try
            {
                _window.DoEvents();
                return _shouldClose || _window.IsClosing;
            }
            catch
            {
                return true;
            }
        }

        public static void CloseWindow()
        {
            _shouldClose = true;
            _window?.Close();
        }

        public static void SetTargetFPS(int fps)
        {
            // Silk.NET handles FPS through VSync and the render loop
            // This is a placeholder for API compatibility
        }

        public static int GetScreenWidth()
        {
            return _window?.Size.X ?? 0;
        }

        public static int GetScreenHeight()
        {
            return _window?.Size.Y ?? 0;
        }

        public static void SetWindowTitle(string title)
        {
            if (_window != null)
                _window.Title = title;
        }

        // Drawing-related functions (rcore)
        public static void BeginDrawing()
        {
            _renderer?.BeginDrawing();
        }

        public static void EndDrawing()
        {
            _renderer?.EndDrawing();
            
            // Present the frame by swapping buffers
            _window?.SwapBuffers();
        }

        public static void ClearBackground(Color color)
        {
            _renderer?.ClearBackground(color);
        }

        // Shape drawing functions (rshapes)
        public static void DrawPixel(int posX, int posY, Color color)
        {
            DrawRectangle(posX, posY, 1, 1, color);
        }

        public static void DrawPixelV(Vector2 position, Color color)
        {
            DrawPixel((int)position.X, (int)position.Y, color);
        }

        public static void DrawLine(int startPosX, int startPosY, int endPosX, int endPosY, Color color)
        {
            _renderer?.DrawLine(startPosX, startPosY, endPosX, endPosY, color);
        }

        public static void DrawLineV(Vector2 startPos, Vector2 endPos, Color color)
        {
            DrawLine((int)startPos.X, (int)startPos.Y, (int)endPos.X, (int)endPos.Y, color);
        }

        public static void DrawLineEx(Vector2 startPos, Vector2 endPos, float thick, Color color)
        {
            if (thick <= 1.0f)
            {
                // For thin lines, use regular line drawing
                DrawLine((int)startPos.X, (int)startPos.Y, (int)endPos.X, (int)endPos.Y, color);
                return;
            }

            // Use the renderer's thick line drawing capability
            _renderer?.DrawThickLine(startPos.X, startPos.Y, endPos.X, endPos.Y, thick, color);
        }

        public static void DrawCircle(int centerX, int centerY, float radius, Color color)
        {
            _renderer?.DrawCircle(centerX, centerY, radius, color);
        }

        public static void DrawCircleV(Vector2 center, float radius, Color color)
        {
            DrawCircle((int)center.X, (int)center.Y, radius, color);
        }

        public static void DrawCircleLines(int centerX, int centerY, float radius, Color color)
        {
            _renderer?.DrawCircleLines(centerX, centerY, radius, color, false);
        }

        public static void DrawCircleLinesV(Vector2 center, float radius, Color color)
        {
            DrawCircleLines((int)center.X, (int)center.Y, radius, color);
        }

        public static void DrawRectangle(int posX, int posY, int width, int height, Color color)
        {
            _renderer?.DrawRectangle(posX, posY, width, height, color);
        }

        public static void DrawRectangleV(Vector2 position, Vector2 size, Color color)
        {
            DrawRectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y, color);
        }

        public static void DrawRectangleRec(Rectangle rec, Color color)
        {
            DrawRectangle((int)rec.X, (int)rec.Y, (int)rec.Width, (int)rec.Height, color);
        }

        public static void DrawRectangleLines(int posX, int posY, int width, int height, Color color)
        {
            // Draw rectangle outline using lines
            DrawLine(posX, posY, posX + width, posY, color);                    // Top
            DrawLine(posX + width, posY, posX + width, posY + height, color);   // Right
            DrawLine(posX + width, posY + height, posX, posY + height, color);  // Bottom
            DrawLine(posX, posY + height, posX, posY, color);                   // Left
        }

        public static void DrawRectangleLinesEx(Rectangle rec, float lineThick, Color color)
        {
            if (lineThick <= 1.0f)
            {
                // Use regular lines for thin thickness
                DrawRectangleLines((int)rec.X, (int)rec.Y, (int)rec.Width, (int)rec.Height, color);
            }
            else
            {
                // Draw thick lines by drawing filled rectangles for each side
                int thickness = (int)Math.Ceiling(lineThick);
                
                // Top line
                DrawRectangle((int)rec.X, (int)rec.Y, (int)rec.Width, thickness, color);
                // Bottom line
                DrawRectangle((int)rec.X, (int)(rec.Y + rec.Height - thickness), (int)rec.Width, thickness, color);
                // Left line
                DrawRectangle((int)rec.X, (int)rec.Y, thickness, (int)rec.Height, color);
                // Right line
                DrawRectangle((int)(rec.X + rec.Width - thickness), (int)rec.Y, thickness, (int)rec.Height, color);
            }
        }

        // Timing functions (rcore)
        public static double GetTime()
        {
            return DateTime.Now.TimeOfDay.TotalSeconds;
        }

        public static float GetFrameTime()
        {
            // This would need to be calculated based on actual frame timing
            return 1.0f / 60.0f; // Assume 60 FPS for now
        }

        public static int GetFPS()
        {
            return 60; // Placeholder
        }


        // Event handlers
        private static void OnLoad()
        {
            if (_window == null) return;

            _gl = _window.CreateOpenGL();
            _renderer = new Renderer(_gl, _window.Size.X, _window.Size.Y);
            _input = _window.CreateInput();
        }

        private static void OnUpdate(double deltaTime)
        {
            // Update logic handled by user code
        }

        private static void OnRender(double deltaTime)
        {
            // Rendering is handled by user code in the main loop
        }

        private static void OnResize(Silk.NET.Maths.Vector2D<int> size)
        {
            _renderer?.Resize(size.X, size.Y);
        }

        private static void OnClosing()
        {
            _renderer?.Dispose();
            _gl?.Dispose();
        }

        // Input functions (basic)
        public static bool IsKeyPressed(KeyboardKey key)
        {
            // This would need proper key mapping and state tracking
            return false; // Placeholder
        }

        public static bool IsKeyDown(KeyboardKey key)
        {
            // This would need proper key mapping and state tracking
            return false; // Placeholder
        }

        public static Vector2 GetMousePosition()
        {
            // This would need mouse input handling
            return Vector2.Zero; // Placeholder
        }

        public static bool IsMouseButtonPressed(MouseButton button)
        {
            // This would need mouse input handling
            return false; // Placeholder
        }

        // Text rendering functions (placeholder)
        public static void DrawText(string text, int posX, int posY, int fontSize, Color color)
        {
            // Text rendering placeholder - would need font loading and text mesh generation
            // This maintains API compatibility with Raylib
        }
    }

    // Additional structures for API compatibility
    public struct Rectangle(float x, float y, float width, float height)
    {
        public float X = x;
        public float Y = y;
        public float Width = width;
        public float Height = height;
    }

    // Enums for input (simplified)
    public enum KeyboardKey
    {
        Space = 32,
        Escape = 256,
        Enter = 257,
        // Add more keys as needed
    }

    public enum MouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2
    }
}
