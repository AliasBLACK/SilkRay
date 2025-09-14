global using static SilkRay.RaylibAPI;

using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using TextCopy;

namespace SilkRay
{
    // Internal static fields for Raylib functionality
    internal static class RaylibInternal
    {
        public static IWindow? Window;
        public static GL? GL;
        public static Renderer? Renderer;
        public static IInputContext? Input;
        public static bool ShouldClose;
        public static ConfigFlags CurrentConfigFlags;
    }

    /// <summary>
    /// Core Raylib functions implementation using Silk.NET
    /// </summary>
    public static class RaylibAPI
    {
        // Window-related functions (rcore)
        public static void InitWindow(int width, int height, string title)
        {
            // Clean up any existing window first
            if (RaylibInternal.Window != null)
            {
                CloseWindow();
            }
            
            var options = WindowOptions.Default;
            
            // Handle special cases for fullscreen modes
            if (RaylibInternal.CurrentConfigFlags.HasFlag(ConfigFlags.BorderlessWindowedMode) ||
                RaylibInternal.CurrentConfigFlags.HasFlag(ConfigFlags.FullscreenMode))
            {
                // For fullscreen modes, use default size if width/height are 0
                if (width <= 0 || height <= 0)
                {
                    options.Size = new Silk.NET.Maths.Vector2D<int>(1920, 1080); // Default fullscreen size
                }
                else
                {
                    options.Size = new Silk.NET.Maths.Vector2D<int>(width, height);
                }
            }
            else
            {
                // For windowed modes, ensure minimum size
                options.Size = new Silk.NET.Maths.Vector2D<int>(Math.Max(width, 1), Math.Max(height, 1));
            }
            
            options.Title = title;
            options.VSync = true;
            options.UpdatesPerSecond = 60;
            options.FramesPerSecond = 60;
            
            // Ensure we have a proper OpenGL context
            options.API = new GraphicsAPI(
                ContextAPI.OpenGL,
                ContextProfile.Core,
                ContextFlags.Default,
                new APIVersion(3, 3)
            );

            // Apply config flags if they were set before InitWindow
            if (RaylibInternal.CurrentConfigFlags != 0)
            {
                options = ApplyConfigFlagsToWindowOptions(options, RaylibInternal.CurrentConfigFlags);
            }

            RaylibInternal.Window = Window.Create(options);
            
            RaylibInternal.Window.Load += OnLoad;
            RaylibInternal.Window.Update += OnUpdate;
            RaylibInternal.Window.Render += OnRender;
            RaylibInternal.Window.Resize += OnResize;
            RaylibInternal.Window.Closing += OnClosing;
            
            // Don't initialize immediately - let the window initialize properly through events
            RaylibInternal.Window.Initialize();
        }

        public static bool WindowShouldClose()
        {
            if (RaylibInternal.Window == null || !RaylibInternal.Window.IsInitialized) return true;
        
        // Process window events to keep window responsive
        try
        {
            RaylibInternal.Window.DoEvents();
            return RaylibInternal.ShouldClose || RaylibInternal.Window.IsClosing;
        }
        catch
        {
            return true;
        }
        }

        public static void CloseWindow()
        {
            RaylibInternal.ShouldClose = true;

            // Reset renderer state
            RaylibInternal.Renderer?.Dispose();
            RaylibInternal.Renderer = null;
            
            if (RaylibInternal.Window != null)
            {
                // Unsubscribe from events to prevent ObjectDisposedException
                RaylibInternal.Window.Load -= OnLoad;
                RaylibInternal.Window.Update -= OnUpdate;
                RaylibInternal.Window.Render -= OnRender;
                RaylibInternal.Window.Resize -= OnResize;
                RaylibInternal.Window.Closing -= OnClosing;
                
                RaylibInternal.Window.Close();
                RaylibInternal.Window.Dispose();
                RaylibInternal.Window = null;
            }
            
            // Reset flags for next window
            RaylibInternal.ShouldClose = false;
        }

        public static void SetTargetFPS(int fps)
        {
            // Silk.NET handles FPS through VSync and the render loop
            // This is a placeholder for API compatibility
        }

        public static int GetScreenWidth()
        {
            return RaylibInternal.Window?.Size.X ?? 0;
        }

        public static int GetScreenHeight()
        {
            return RaylibInternal.Window?.Size.Y ?? 0;
        }

        public static void SetWindowTitle(string title)
        {
            if (RaylibInternal.Window != null)
                RaylibInternal.Window.Title = title;
        }

        // Window state functions
        public static bool IsWindowReady()
        {
            return RaylibInternal.Window != null && RaylibInternal.Window.IsInitialized;
        }

        public static bool IsWindowFullscreen()
        {
            return RaylibInternal.Window?.WindowState == WindowState.Fullscreen;
        }

        public static bool IsWindowHidden()
        {
            return RaylibInternal.Window?.IsVisible == false;
        }

        public static bool IsWindowMinimized()
        {
            return RaylibInternal.Window?.WindowState == WindowState.Minimized;
        }

        public static bool IsWindowMaximized()
        {
            return RaylibInternal.Window?.WindowState == WindowState.Maximized;
        }

        public static bool IsWindowFocused()
        {
            // Silk.NET doesn't have a direct focus property, assume focused if not minimized
            return RaylibInternal.Window?.WindowState != WindowState.Minimized;
        }

        public static bool IsWindowResized()
        {
            // This would need state tracking - placeholder for now
            return false;
        }

        public static bool IsWindowState(uint flag)
        {
            // Window state flags - would need proper flag system
            return false;
        }

        public static void SetWindowState(uint flags)
        {
            // Set window state using flags - placeholder
        }

        public static void ClearWindowState(uint flags)
        {
            // Clear window state flags - placeholder
        }
        
        public static void ClearWindowState(WindowStateFlags flags)
        {
            // Clear specific window state flags
            if (RaylibInternal.Window == null) return;
            
            if (flags.HasFlag(WindowStateFlags.FullscreenMode))
            {
                if (RaylibInternal.Window.WindowState == WindowState.Fullscreen)
                    RaylibInternal.Window.WindowState = WindowState.Normal;
            }
            
            if (flags.HasFlag(WindowStateFlags.WindowMaximized))
            {
                if (RaylibInternal.Window.WindowState == WindowState.Maximized)
                    RaylibInternal.Window.WindowState = WindowState.Normal;
            }
            
            if (flags.HasFlag(WindowStateFlags.WindowMinimized))
            {
                if (RaylibInternal.Window.WindowState == WindowState.Minimized)
                    RaylibInternal.Window.WindowState = WindowState.Normal;
            }
        }

        public static void ToggleFullscreen()
        {
            if (RaylibInternal.Window != null)
            {
                RaylibInternal.Window.WindowState = RaylibInternal.Window.WindowState == WindowState.Fullscreen 
                    ? WindowState.Normal 
                    : WindowState.Fullscreen;
            }
        }

        public static void MaximizeWindow()
        {
            if (RaylibInternal.Window != null)
                RaylibInternal.Window.WindowState = WindowState.Maximized;
        }

        public static void MinimizeWindow()
        {
            if (RaylibInternal.Window != null)
                RaylibInternal.Window.WindowState = WindowState.Minimized;
        }

        public static void RestoreWindow()
        {
            if (RaylibInternal.Window != null)
                RaylibInternal.Window.WindowState = WindowState.Normal;
        }

        public static void SetWindowIcon(Image image)
        {
            // Icon setting would need image processing - placeholder
            // Would require converting Image to platform-specific icon format
        }

        // Window size and position functions
        public static void SetWindowSize(int width, int height)
        {
            if (RaylibInternal.Window != null)
            {
                RaylibInternal.Window.Size = new Silk.NET.Maths.Vector2D<int>(width, height);
            }
        }

        public static void SetWindowMinSize(int width, int height)
        {
            // Silk.NET doesn't have direct min size - placeholder
        }

        public static void SetWindowPosition(int x, int y)
        {
            if (RaylibInternal.Window != null)
            {
                RaylibInternal.Window.Position = new Silk.NET.Maths.Vector2D<int>(x, y);
            }
        }

        public static Vector2 GetWindowPosition()
        {
            if (RaylibInternal.Window != null)
            {
                var pos = RaylibInternal.Window.Position;
                return new Vector2(pos.X, pos.Y);
            }
            return Vector2.Zero;
        }

        public static Vector2 GetWindowScaleDPI()
        {
            // DPI scaling - would need platform-specific implementation
            return Vector2.One;
        }

        public static string GetMonitorName(int monitor)
        {
            // Monitor enumeration - placeholder
            return "Primary Monitor";
        }

        public static void SetClipboardText(string text)
        {
            try
            {
                ClipboardService.SetText(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting clipboard text: {ex.Message}");
            }
        }

        public static string GetClipboardText()
        {
            try
            {
                return ClipboardService.GetText() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting clipboard text: {ex.Message}");
                return string.Empty;
            }
        }

        // Window configuration functions
        public static void EnableEventWaiting()
        {
            // Event waiting configuration - placeholder
        }

        public static void DisableEventWaiting()
        {
            // Event waiting configuration - placeholder
        }

        // Monitor and display functions
        public static int GetMonitorCount()
        {
            // Monitor enumeration - would need platform-specific implementation
            return 1; // Assume single monitor for now
        }

        public static int GetCurrentMonitor()
        {
            // Current monitor detection - placeholder
            return 0;
        }

        public static Vector2 GetMonitorPosition(int monitor)
        {
            // Monitor position - placeholder
            return Vector2.Zero;
        }

        public static int GetMonitorWidth(int monitor)
        {
            // Monitor width - would need platform-specific implementation
            return 1920; // Default assumption
        }

        public static int GetMonitorHeight(int monitor)
        {
            // Monitor height - would need platform-specific implementation
            return 1080; // Default assumption
        }

        public static int GetMonitorPhysicalWidth(int monitor)
        {
            // Physical monitor width in millimeters - placeholder
            return 510; // ~24 inch monitor assumption
        }

        public static int GetMonitorPhysicalHeight(int monitor)
        {
            // Physical monitor height in millimeters - placeholder
            return 287; // ~24 inch monitor assumption
        }

        public static int GetMonitorRefreshRate(int monitor)
        {
            // Monitor refresh rate - placeholder
            return 60;
        }

        // Window flags and configuration
        public static void SetConfigFlags(ConfigFlags flags)
        {
            RaylibInternal.CurrentConfigFlags = flags;
            
            // If window is already created, apply flags that can be changed at runtime
            if (RaylibInternal.Window != null)
            {
                ApplyRuntimeConfigFlags(flags);
            }
        }
        
        public static void SetConfigFlags(uint flags)
        {
            SetConfigFlags((ConfigFlags)flags);
        }
        
        private static void ApplyRuntimeConfigFlags(ConfigFlags flags)
        {
            if (RaylibInternal.Window == null) return;
            
            // Apply window state flags that can be changed at runtime
            if (flags.HasFlag(ConfigFlags.WindowResizable))
            {
                // Silk.NET doesn't have direct resizable control after creation
                // This would need to be set during window creation
            }
            
            if (flags.HasFlag(ConfigFlags.WindowTopmost))
            {
                // Platform-specific implementation needed
                // Windows: SetWindowPos with HWND_TOPMOST
                // Linux: X11 _NET_WM_STATE_ABOVE
                // macOS: NSWindow level
            }
            
            if (flags.HasFlag(ConfigFlags.WindowMaximized))
            {
                RaylibInternal.Window.WindowState = WindowState.Maximized;
            }
            else if (flags.HasFlag(ConfigFlags.WindowMinimized))
            {
                RaylibInternal.Window.WindowState = WindowState.Minimized;
            }
            else if (flags.HasFlag(ConfigFlags.FullscreenMode))
            {
                RaylibInternal.Window.WindowState = WindowState.Fullscreen;
            }
            else
            {
                RaylibInternal.Window.WindowState = WindowState.Normal;
            }
            
            if (flags.HasFlag(ConfigFlags.WindowHidden))
            {
                RaylibInternal.Window.IsVisible = false;
            }
            else
            {
                RaylibInternal.Window.IsVisible = true;
            }
        }
        
        private static WindowOptions ApplyConfigFlagsToWindowOptions(WindowOptions options, ConfigFlags flags)
        {
            // Apply window creation flags
            if (flags.HasFlag(ConfigFlags.WindowResizable))
            {
                options.WindowBorder = WindowBorder.Resizable;
            }
            else
            {
                options.WindowBorder = WindowBorder.Fixed;
            }
            
            if (flags.HasFlag(ConfigFlags.WindowUndecorated))
            {
                options.WindowBorder = WindowBorder.Hidden;
            }
            
            if (flags.HasFlag(ConfigFlags.BorderlessWindowedMode))
            {
                // Borderless windowed mode: fullscreen window without decorations
                options.WindowState = WindowState.Fullscreen;
                options.WindowBorder = WindowBorder.Hidden;
            }
            else if (flags.HasFlag(ConfigFlags.FullscreenMode))
            {
                options.WindowState = WindowState.Fullscreen;
            }
            else if (flags.HasFlag(ConfigFlags.WindowMaximized))
            {
                options.WindowState = WindowState.Maximized;
            }
            else if (flags.HasFlag(ConfigFlags.WindowMinimized))
            {
                options.WindowState = WindowState.Minimized;
            }
            else
            {
                options.WindowState = WindowState.Normal;
            }
            
            if (flags.HasFlag(ConfigFlags.WindowHidden))
            {
                options.IsVisible = false;
            }
            
            if (flags.HasFlag(ConfigFlags.VsyncHint))
            {
                options.VSync = true;
            }
            
            if (flags.HasFlag(ConfigFlags.WindowTransparent))
            {
                options.TransparentFramebuffer = true;
            }
            
            // OpenGL context flags
            if (flags.HasFlag(ConfigFlags.OpenglCoreProfile))
            {
                options.API = new GraphicsAPI(
                    ContextAPI.OpenGL,
                    ContextProfile.Core,
                    ContextFlags.Default,
                    new APIVersion(3, 3)
                );
            }
            else if (flags.HasFlag(ConfigFlags.OpenglCompatProfile))
            {
                options.API = new GraphicsAPI(
                    ContextAPI.OpenGL,
                    ContextProfile.Compatability,
                    ContextFlags.Default,
                    new APIVersion(3, 3)
                );
            }
            
            if (flags.HasFlag(ConfigFlags.OpenglDebugContext))
            {
                options.API = new GraphicsAPI(
                    ContextAPI.OpenGL,
                    ContextProfile.Core,
                    ContextFlags.Debug,
                    new APIVersion(3, 3)
                );
            }
            
            if (flags.HasFlag(ConfigFlags.OpenglForwardCompat))
            {
                var currentFlags = ContextFlags.Default;
                if (flags.HasFlag(ConfigFlags.OpenglDebugContext))
                    currentFlags |= ContextFlags.Debug;
                
                options.API = new GraphicsAPI(
                    ContextAPI.OpenGL,
                    ContextProfile.Core,
                    currentFlags | ContextFlags.ForwardCompatible,
                    new APIVersion(3, 3)
                );
            }
            
            // MSAA
            if (flags.HasFlag(ConfigFlags.Msaa4xHint))
            {
                options.Samples = 4;
            }
            
            return options;
        }

        public static void SetWindowOpacity(float opacity)
        {
            // Window opacity - Silk.NET may not support this directly
            // Placeholder for API compatibility
        }

        public static void SetWindowFocused()
        {
            // Focus window - would need platform-specific implementation
        }
        
        // Additional window flag utility functions
        public static bool IsConfigFlagEnabled(ConfigFlags flag)
        {
            return RaylibInternal.CurrentConfigFlags.HasFlag(flag);
        }
        
        public static void EnableConfigFlag(ConfigFlags flag)
        {
            RaylibInternal.CurrentConfigFlags |= flag;
            if (RaylibInternal.Window != null)
            {
                ApplyRuntimeConfigFlags(RaylibInternal.CurrentConfigFlags);
            }
        }
        
        public static void DisableConfigFlag(ConfigFlags flag)
        {
            RaylibInternal.CurrentConfigFlags &= ~flag;
            if (RaylibInternal.Window != null)
            {
                ApplyRuntimeConfigFlags(RaylibInternal.CurrentConfigFlags);
            }
        }
        
        public static ConfigFlags GetConfigFlags()
        {
            return RaylibInternal.CurrentConfigFlags;
        }

        // Cursor functions
        public static void ShowCursor()
        {
            if (RaylibInternal.Window != null)
            {
                // Silk.NET cursor visibility - may need input context
            }
        }

        public static void HideCursor()
        {
            if (RaylibInternal.Window != null)
            {
                // Silk.NET cursor visibility - may need input context
            }
        }

        public static bool IsCursorHidden()
        {
            // Cursor visibility state - placeholder
            return false;
        }

        public static void EnableCursor()
        {
            ShowCursor();
        }

        public static void DisableCursor()
        {
            HideCursor();
        }

        public static bool IsCursorOnScreen()
        {
            // Cursor position detection - placeholder
            return true;
        }

        // Drawing-related functions (rcore)
        public static void BeginDrawing()
        {
            RaylibInternal.Renderer?.BeginDrawing();
        }

        public static void EndDrawing()
        {
            RaylibInternal.Renderer?.EndDrawing();
            
            // Present the frame by swapping buffers
            RaylibInternal.Window?.SwapBuffers();
        }

        public static void ClearBackground(Color color)
        {
            RaylibInternal.Renderer?.ClearBackground(color);
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
            RaylibInternal.Renderer?.DrawLine(startPosX, startPosY, endPosX, endPosY, color);
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
            RaylibInternal.Renderer?.DrawThickLine(startPos.X, startPos.Y, endPos.X, endPos.Y, thick, color);
        }

        public static void DrawCircle(int centerX, int centerY, float radius, Color color)
        {
            RaylibInternal.Renderer?.DrawCircle(centerX, centerY, radius, color);
        }

        public static void DrawCircleV(Vector2 center, float radius, Color color)
        {
            DrawCircle((int)center.X, (int)center.Y, radius, color);
        }

        public static void DrawCircleLines(int centerX, int centerY, float radius, Color color)
        {
            RaylibInternal.Renderer?.DrawCircleLines(centerX, centerY, radius, color, false);
        }

        public static void DrawCircleLinesV(Vector2 center, float radius, Color color)
        {
            DrawCircleLines((int)center.X, (int)center.Y, radius, color);
        }

        public static void DrawRectangle(int posX, int posY, int width, int height, Color color)
        {
            RaylibInternal.Renderer?.DrawRectangle(posX, posY, width, height, color);
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
            if (RaylibInternal.Window == null) return;

            RaylibInternal.GL = RaylibInternal.Window.CreateOpenGL();
            RaylibInternal.Renderer = new Renderer(RaylibInternal.GL, RaylibInternal.Window.Size.X, RaylibInternal.Window.Size.Y);
            RaylibInternal.Input = RaylibInternal.Window.CreateInput();
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
            RaylibInternal.Renderer?.Resize(size.X, size.Y);
        }

        private static void OnClosing()
        {
            RaylibInternal.ShouldClose = true;
            RaylibInternal.Renderer?.Dispose();
            RaylibInternal.GL?.Dispose();
        }

        // Input functions (basic)
        public static bool IsKeyPressed(KeyboardKey key)
        {
            if (RaylibInternal.Input == null) return false;
            
            var keyboards = RaylibInternal.Input.Keyboards;
            if (keyboards.Count == 0) return false;
            
            var keyboard = keyboards[0];
            
            return key switch
            {
                KeyboardKey.Escape => keyboard.IsKeyPressed(Silk.NET.Input.Key.Escape),
                KeyboardKey.Space => keyboard.IsKeyPressed(Silk.NET.Input.Key.Space),
                KeyboardKey.Enter => keyboard.IsKeyPressed(Silk.NET.Input.Key.Enter),
                _ => false
            };
        }

        public static bool IsKeyDown(KeyboardKey key)
        {
            if (RaylibInternal.Input == null) return false;
            
            var keyboards = RaylibInternal.Input.Keyboards;
            if (keyboards.Count == 0) return false;
            
            var keyboard = keyboards[0];
            
            return key switch
            {
                KeyboardKey.Escape => keyboard.IsKeyPressed(Silk.NET.Input.Key.Escape),
                KeyboardKey.Space => keyboard.IsKeyPressed(Silk.NET.Input.Key.Space),
                KeyboardKey.Enter => keyboard.IsKeyPressed(Silk.NET.Input.Key.Enter),
                _ => false
            };
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

    public struct Image
    {
        public byte[] Data;
        public int Width;
        public int Height;
        public int Mipmaps;
        public int Format;

        public Image(byte[] data, int width, int height, int mipmaps = 1, int format = 1)
        {
            Data = data;
            Width = width;
            Height = height;
            Mipmaps = mipmaps;
            Format = format;
        }
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

    // Window configuration flags - Complete Raylib ConfigFlags implementation
    [Flags]
    public enum ConfigFlags : uint
    {
        // Window-related flags
        VsyncHint = 0x00000040,           // Set to try enabling V-Sync on GPU
        FullscreenMode = 0x00000002,      // Set to run program in fullscreen
        WindowResizable = 0x00000004,     // Set to allow resizable window
        WindowUndecorated = 0x00000008,   // Set to disable window decoration (frame and buttons)
        WindowHidden = 0x00000080,        // Set to hide window
        WindowMinimized = 0x00000200,     // Set to minimize window (iconify)
        WindowMaximized = 0x00000400,     // Set to maximize window (expanded to monitor)
        WindowUnfocused = 0x00000800,     // Set to window non focused
        WindowTopmost = 0x00001000,       // Set to window always on top
        WindowAlwaysRun = 0x00000100,     // Set to allow windows running while minimized
        WindowTransparent = 0x00000010,   // Set to allow transparent framebuffer
        WindowHighdpi = 0x00002000,       // Set to support HighDPI
        WindowMousePassthrough = 0x00004000, // Set to support mouse passthrough, only supported when FLAG_WINDOW_UNDECORATED
        BorderlessWindowedMode = 0x00008000, // Set to run program in borderless windowed mode
        
        // Graphics-related flags
        Msaa4xHint = 0x00000020,          // Set to try enabling MSAA 4X
        InterlacedHint = 0x00010000,      // Set to try enabling interlaced video format (for V3D)
        
        // Additional Raylib flags
        WindowScaledMode = 0x00020000,    // Set to try enabling window scaling mode
        WindowCentered = 0x00040000,      // Set to center window on screen
        WindowAlwaysOnTop = WindowTopmost, // Alias for WindowTopmost
        
        // OpenGL context flags
        OpenglDebugContext = 0x00080000,  // Set to enable OpenGL debug context
        OpenglForwardCompat = 0x00100000, // Set to enable OpenGL forward compatibility
        OpenglCoreProfile = 0x00200000,   // Set to use OpenGL core profile
        OpenglCompatProfile = 0x00400000, // Set to use OpenGL compatibility profile
        
        // Audio flags
        AudioDeviceShared = 0x00800000,   // Set to use shared audio device
        
        // Display flags
        DisplayAutoRotation = 0x01000000, // Set to enable display auto-rotation (mobile platforms)
        
        // Input flags
        InputMouseCaptured = 0x02000000,  // Set to capture mouse input
        InputKeyboardCaptured = 0x04000000, // Set to capture keyboard input
        
        // Platform-specific flags
        PlatformDesktop = 0x08000000,     // Set platform to desktop
        PlatformAndroid = 0x10000000,     // Set platform to Android
        PlatformWeb = 0x20000000,         // Set platform to Web
        PlatformDrm = 0x40000000          // Set platform to DRM
    }

    // Window state flags
    [Flags]
    public enum WindowStateFlags : uint
    {
        FullscreenMode = 0x00000001,
        WindowResizable = 0x00000002,
        WindowUndecorated = 0x00000004,
        WindowHidden = 0x00000008,
        WindowMinimized = 0x00000010,
        WindowMaximized = 0x00000020,
        WindowUnfocused = 0x00000040,
        WindowTopmost = 0x00000080,
        WindowAlwaysRun = 0x00000100,
        WindowTransparent = 0x00000200,
        WindowHighdpi = 0x00000400,
        WindowMousePassthrough = 0x00000800,
        BorderlessWindowedMode = 0x00001000
    }
}
