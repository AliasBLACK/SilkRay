using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using System.Numerics;

namespace VeldridRaylib
{
    /// <summary>
    /// Main raylib API implementation using Veldrid
    /// </summary>
    public static class Raylib
    {
        private static Sdl2Window? _window;
        private static GraphicsDevice? _graphicsDevice;
        private static Renderer? _renderer;
        private static bool _windowShouldClose = false;
        private static Dictionary<Veldrid.Key, bool> _keyStates = new();
        private static Dictionary<Veldrid.Key, bool> _previousKeyStates = new();
        private static Vector2 _mousePosition = Vector2.Zero;

        // Window management
        public static void InitWindow(int width, int height, string title)
        {
            try
            {
                Console.WriteLine($"Creating window: {width}x{height} - {title}");
                var windowCI = new WindowCreateInfo(50, 50, width, height, WindowState.Normal, title);
                _window = VeldridStartup.CreateWindow(ref windowCI);
                Console.WriteLine("Window created successfully");
                
                // Try different graphics backends in order of preference for compatibility
                GraphicsBackend[] backends = { GraphicsBackend.OpenGL, GraphicsBackend.Direct3D11, GraphicsBackend.Vulkan };
                
                foreach (var backend in backends)
                {
                    try
                    {
                        Console.WriteLine($"Trying graphics backend: {backend}");
                        var options = new GraphicsDeviceOptions(
                            debug: false,
                            swapchainDepthFormat: null,
                            syncToVerticalBlank: true,
                            resourceBindingModel: ResourceBindingModel.Default);
                        
                        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, options, backend);
                        Console.WriteLine($"Graphics device created successfully: {_graphicsDevice.BackendType}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create graphics device with {backend}: {ex.Message}");
                        continue;
                    }
                }

                if (_graphicsDevice == null)
                {
                    throw new Exception("Failed to create graphics device with any backend");
                }

                Console.WriteLine("Creating renderer...");
                _renderer = new Renderer(_graphicsDevice);
                Console.WriteLine("Renderer created successfully");

                // Set up orthographic projection
                var projection = Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
                _renderer.SetProjection(projection);

                // Set up input handling
                _window.KeyDown += OnKeyDown;
                _window.KeyUp += OnKeyUp;
                _window.MouseMove += OnMouseMove;
                _window.Closed += () => _windowShouldClose = true;
                
                Console.WriteLine("Window initialization completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize window: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public static bool WindowShouldClose()
        {
            return _windowShouldClose;
        }

        public static void CloseWindow()
        {
            _windowShouldClose = true;
            _renderer?.Dispose();
            _graphicsDevice?.Dispose();
            _window?.Close();
        }

        public static void SetTargetFPS(int fps)
        {
            // Note: This is a simplified implementation
            // In a real implementation, you'd want proper frame timing
        }

        // Drawing functions
        public static void BeginDrawing()
        {
            if (_window == null || _graphicsDevice == null || _renderer == null) return;

            // Update previous key states BEFORE processing new events
            _previousKeyStates.Clear();
            foreach (var kvp in _keyStates)
            {
                _previousKeyStates[kvp.Key] = kvp.Value;
            }

            var inputSnapshot = _window.PumpEvents();

            _renderer.BeginFrame();
        }

        public static void EndDrawing()
        {
            if (_graphicsDevice == null || _renderer == null) return;

            var commandList = _renderer.GetCommandList();
            commandList.Begin();
            commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            
            // Clear the screen first
            commandList.ClearColorTarget(0, _renderer.GetClearColor());
            
            // Render all accumulated vertices
            _renderer.Flush(commandList);
            
            commandList.End();
            _graphicsDevice.SubmitCommands(commandList);
            _graphicsDevice.SwapBuffers();
        }

        public static void ClearBackground(Color color)
        {
            if (_graphicsDevice == null || _renderer == null) return;

            var colorVec = color.ToVector4();
            _renderer.SetClearColor(new RgbaFloat(colorVec.X, colorVec.Y, colorVec.Z, colorVec.W));
        }

        // Shape drawing functions
        public static void DrawRectangle(int posX, int posY, int width, int height, Color color)
        {
            DrawRectangleRec(new Rectangle(posX, posY, width, height), color);
        }

        public static void DrawRectangleRec(Rectangle rec, Color color)
        {
            if (_renderer == null) return;

            var p1 = new Vector2(rec.X, rec.Y);
            var p2 = new Vector2(rec.X + rec.Width, rec.Y);
            var p3 = new Vector2(rec.X + rec.Width, rec.Y + rec.Height);
            var p4 = new Vector2(rec.X, rec.Y + rec.Height);

            _renderer.DrawQuad(p1, p2, p3, p4, color);
        }

        public static void DrawCircle(int centerX, int centerY, float radius, Color color)
        {
            DrawCircleV(new Vector2(centerX, centerY), radius, color);
        }

        public static void DrawCircleV(Vector2 center, float radius, Color color)
        {
            if (_renderer == null) return;

            // Draw circle as multiple triangles
            int segments = Math.Max(12, (int)(radius * 0.5f));
            float angleStep = (float)(2 * Math.PI / segments);

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;

                var p1 = center;
                var p2 = center + new Vector2((float)Math.Cos(angle1) * radius, (float)Math.Sin(angle1) * radius);
                var p3 = center + new Vector2((float)Math.Cos(angle2) * radius, (float)Math.Sin(angle2) * radius);

                _renderer.DrawTriangle(p1, p2, p3, color);
            }
        }

        public static void DrawLine(int startPosX, int startPosY, int endPosX, int endPosY, Color color)
        {
            DrawLineV(new Vector2(startPosX, startPosY), new Vector2(endPosX, endPosY), color);
        }

        public static void DrawLineV(Vector2 startPos, Vector2 endPos, Color color)
        {
            if (_renderer == null) return;

            // Draw line as a thin rectangle
            var direction = endPos - startPos;
            var length = direction.Length();
            if (length < 0.001f) return;

            var normalized = direction / length;
            var perpendicular = new Vector2(-normalized.Y, normalized.X);
            float thickness = 1.0f;

            var p1 = startPos + perpendicular * thickness * 0.5f;
            var p2 = startPos - perpendicular * thickness * 0.5f;
            var p3 = endPos - perpendicular * thickness * 0.5f;
            var p4 = endPos + perpendicular * thickness * 0.5f;

            _renderer.DrawQuad(p1, p2, p3, p4, color);
        }

        public static void DrawPixel(int posX, int posY, Color color)
        {
            DrawRectangle(posX, posY, 1, 1, color);
        }

        // Text functions (simplified - no actual font rendering)
        public static void DrawText(string text, int posX, int posY, int fontSize, Color color)
        {
            // This is a placeholder - real text rendering would require font loading and glyph rendering
            // For now, just draw a rectangle to indicate text position
            int textWidth = text.Length * fontSize / 2;
            DrawRectangle(posX, posY, textWidth, fontSize, color);
        }

        public static int MeasureText(string text, int fontSize)
        {
            // Simplified text measurement
            return text.Length * fontSize / 2;
        }

        // Input functions
        public static bool IsKeyPressed(KeyCode key)
        {
            var veldridKey = (Veldrid.Key)(int)key;
            bool currentState = _keyStates.GetValueOrDefault(veldridKey, false);
            bool previousState = _previousKeyStates.GetValueOrDefault(veldridKey, false);
            return currentState && !previousState;
        }

        public static bool IsKeyDown(KeyCode key)
        {
            var veldridKey = (Veldrid.Key)(int)key;
            return _keyStates.GetValueOrDefault(veldridKey, false);
        }

        public static bool IsKeyReleased(KeyCode key)
        {
            var veldridKey = (Veldrid.Key)(int)key;
            return !_keyStates.GetValueOrDefault(veldridKey, false) && 
                   _previousKeyStates.GetValueOrDefault(veldridKey, false);
        }

        public static bool IsKeyUp(KeyCode key)
        {
            var veldridKey = (Veldrid.Key)(int)key;
            return !_keyStates.GetValueOrDefault(veldridKey, false);
        }

        public static Vector2 GetMousePosition()
        {
            return _mousePosition;
        }

        public static bool IsMouseButtonPressed(MouseButton button)
        {
            // Simplified - would need proper mouse button tracking
            return false;
        }

        // Utility functions
        public static int GetScreenWidth()
        {
            return _window?.Width ?? 0;
        }

        public static int GetScreenHeight()
        {
            return _window?.Height ?? 0;
        }

        public static float GetFrameTime()
        {
            // Simplified - would need proper frame timing
            return 1.0f / 60.0f;
        }

        public static int GetFPS()
        {
            // Simplified
            return 60;
        }

        // Event handlers
        private static void OnKeyDown(KeyEvent keyEvent)
        {
            _keyStates[keyEvent.Key] = true;
        }

        private static void OnKeyUp(KeyEvent keyEvent)
        {
            _keyStates[keyEvent.Key] = false;
        }

        private static void OnMouseMove(MouseMoveEventArgs args)
        {
            _mousePosition = new Vector2(args.MousePosition.X, args.MousePosition.Y);
        }
    }

    // Key codes enum (simplified)
    public enum KeyCode
    {
        Space = (int)Veldrid.Key.Space,
        Enter = (int)Veldrid.Key.Enter,
        Escape = (int)Veldrid.Key.Escape,
        A = (int)Veldrid.Key.A,
        B = (int)Veldrid.Key.B,
        C = (int)Veldrid.Key.C,
        D = (int)Veldrid.Key.D,
        E = (int)Veldrid.Key.E,
        F = (int)Veldrid.Key.F,
        G = (int)Veldrid.Key.G,
        H = (int)Veldrid.Key.H,
        I = (int)Veldrid.Key.I,
        J = (int)Veldrid.Key.J,
        K = (int)Veldrid.Key.K,
        L = (int)Veldrid.Key.L,
        M = (int)Veldrid.Key.M,
        N = (int)Veldrid.Key.N,
        O = (int)Veldrid.Key.O,
        P = (int)Veldrid.Key.P,
        Q = (int)Veldrid.Key.Q,
        R = (int)Veldrid.Key.R,
        S = (int)Veldrid.Key.S,
        T = (int)Veldrid.Key.T,
        U = (int)Veldrid.Key.U,
        V = (int)Veldrid.Key.V,
        W = (int)Veldrid.Key.W,
        X = (int)Veldrid.Key.X,
        Y = (int)Veldrid.Key.Y,
        Z = (int)Veldrid.Key.Z,
        Up = (int)Veldrid.Key.Up,
        Down = (int)Veldrid.Key.Down,
        Left = (int)Veldrid.Key.Left,
        Right = (int)Veldrid.Key.Right
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }
}
