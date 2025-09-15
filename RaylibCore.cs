global using static SilkRay.RaylibAPI;
global using static SilkRay.RaylibShapes;
global using static SilkRay.RaylibTextures;
global using static SilkRay.KeyboardKeys;
global using static SilkRay.WindowFlags;
global using static SilkRay.MouseButton;
global using static SilkRay.MouseCursor;

using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.GLFW;
using Silk.NET.Input;
using TextCopy;
using StbImageSharp;
using FontStashSharp;

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
		public static uint CurrentConfigFlags;
		public static bool GlfwInitialized = false;
		
		// Font system
		public static FontSystem? FontSystem = null;
		public static DynamicSpriteFont? DefaultFont = null;
		public static Dictionary<string, DynamicSpriteFont> LoadedFonts = new();
		public static Dictionary<string, FontSystem> LoadedFontSystems = new();
		
		// Default bitmap font
		public static uint DefaultFontTexture = 0;
		
		// Keyboard input state tracking
		public static Dictionary<int, bool> CurrentKeyState = new();
		public static Dictionary<int, bool> PreviousKeyState = new();
		public static Queue<int> KeyPressedQueue = new();
		public static Queue<int> CharPressedQueue = new();
		public static int ExitKey = KEY_ESCAPE;
	}

	/// <summary>
	/// Core Raylib functions implementation using Silk.NET
	/// </summary>
	public static class RaylibAPI
	{
		// Helper function to ensure GLFW is initialized
		private static unsafe void EnsureGlfwInitialized()
		{
			if (!RaylibInternal.GlfwInitialized)
			{
				var glfw = Glfw.GetApi();
				if (glfw.Init())
				{
					RaylibInternal.GlfwInitialized = true;
				}
				else
				{
					throw new Exception("Failed to initialize GLFW");
				}
			}
		}
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
			if ((RaylibInternal.CurrentConfigFlags & FLAG_BORDERLESS_WINDOWED_MODE) != 0 ||
				(RaylibInternal.CurrentConfigFlags & FLAG_FULLSCREEN_MODE) != 0)
			{
				// For fullscreen modes, use default size if width/height are 0
				if (width <= 0 || height <= 0)
				{
					options.Size = new(1920, 1080); // Default fullscreen size
				}
				else
				{
					options.Size = new(width, height);
				}
			}
			else
			{
				// For windowed modes, ensure minimum size
				options.Size = new(Math.Max(width, 1), Math.Max(height, 1));
			}
			
			options.Title = title;
			options.VSync = true;
			options.UpdatesPerSecond = 60;
			options.FramesPerSecond = 60;
			
			// Ensure we have a proper OpenGL context
			options.API = new(
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
			
			// Update keyboard state for proper key press detection
			UpdateKeyboardState();
			
			// Check for exit key press (Raylib behavior)
			bool exitKeyPressed = IsKeyDown(RaylibInternal.ExitKey);
			
			return RaylibInternal.ShouldClose || RaylibInternal.Window.IsClosing || exitKeyPressed;
		}
		catch
		{
			return true;
		}
		}

		private static void UpdateKeyboardState()
		{
			// Copy current state to previous state
			RaylibInternal.PreviousKeyState.Clear();
			foreach (var kvp in RaylibInternal.CurrentKeyState)
			{
				RaylibInternal.PreviousKeyState[kvp.Key] = kvp.Value;
			}
		}

		public static void CloseWindow()
		{
			RaylibInternal.ShouldClose = true;

			// Dispose resources in proper order: Renderer -> GL Context -> Input -> Window
			
			// 1. Dispose renderer first (uses GL context)
			RaylibInternal.Renderer?.Dispose();
			RaylibInternal.Renderer = null;
			
			// 2. Dispose GL context
			RaylibInternal.GL?.Dispose();
			RaylibInternal.GL = null;
			
			// 3. Dispose input context
			RaylibInternal.Input?.Dispose();
			RaylibInternal.Input = null;
			
			// 4. Finally dispose window
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
			
			// Reset keyboard state
			RaylibInternal.CurrentKeyState.Clear();
			RaylibInternal.PreviousKeyState.Clear();
			RaylibInternal.KeyPressedQueue.Clear();
			RaylibInternal.CharPressedQueue.Clear();
			
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
			if (RaylibInternal.Window == null) return false;
			
			if ((flag & FLAG_FULLSCREEN_MODE) != 0)
				return RaylibInternal.Window.WindowState == WindowState.Fullscreen;
			
			if ((flag & FLAG_WINDOW_MAXIMIZED) != 0)
				return RaylibInternal.Window.WindowState == WindowState.Maximized;
			
			if ((flag & FLAG_WINDOW_MINIMIZED) != 0)
				return RaylibInternal.Window.WindowState == WindowState.Minimized;
			
			if ((flag & FLAG_WINDOW_HIDDEN) != 0)
				return !RaylibInternal.Window.IsVisible;
			
			if ((flag & FLAG_WINDOW_RESIZABLE) != 0)
				return RaylibInternal.Window.WindowBorder == WindowBorder.Resizable;
			
			if ((flag & FLAG_WINDOW_UNDECORATED) != 0)
				return RaylibInternal.Window.WindowBorder == WindowBorder.Hidden;
			
			return false;
		}

		public static void SetWindowState(uint flags)
		{
			if (RaylibInternal.Window == null) return;
			
			if ((flags & FLAG_FULLSCREEN_MODE) != 0)
				RaylibInternal.Window.WindowState = WindowState.Fullscreen;
			else if ((flags & FLAG_WINDOW_MAXIMIZED) != 0)
				RaylibInternal.Window.WindowState = WindowState.Maximized;
			else if ((flags & FLAG_WINDOW_MINIMIZED) != 0)
				RaylibInternal.Window.WindowState = WindowState.Minimized;
			else
				RaylibInternal.Window.WindowState = WindowState.Normal;
			
			if ((flags & FLAG_WINDOW_HIDDEN) != 0)
				RaylibInternal.Window.IsVisible = false;
			else if ((flags & FLAG_WINDOW_RESIZABLE) != 0)
				RaylibInternal.Window.WindowBorder = WindowBorder.Resizable;
			else if ((flags & FLAG_WINDOW_UNDECORATED) != 0)
				RaylibInternal.Window.WindowBorder = WindowBorder.Hidden;
		}

		public static void ClearWindowState(uint flags)
		{
			if (RaylibInternal.Window == null) return;
			
			if ((flags & FLAG_FULLSCREEN_MODE) != 0)
			{
				if (RaylibInternal.Window.WindowState == WindowState.Fullscreen)
					RaylibInternal.Window.WindowState = WindowState.Normal;
			}
			
			if ((flags & FLAG_WINDOW_MAXIMIZED) != 0)
			{
				if (RaylibInternal.Window.WindowState == WindowState.Maximized)
					RaylibInternal.Window.WindowState = WindowState.Normal;
			}
			
			if ((flags & FLAG_WINDOW_MINIMIZED) != 0)
			{
				if (RaylibInternal.Window.WindowState == WindowState.Minimized)
					RaylibInternal.Window.WindowState = WindowState.Normal;
			}
			
			if ((flags & FLAG_WINDOW_HIDDEN) != 0)
				RaylibInternal.Window.IsVisible = true;
			
			if ((flags & FLAG_WINDOW_UNDECORATED) != 0)
				RaylibInternal.Window.WindowBorder = WindowBorder.Fixed;
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
				RaylibInternal.Window.Size = new(width, height);
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
				RaylibInternal.Window.Position = new(x, y);
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
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					var monitors = glfw.GetMonitors(out int count);
					
					if (monitor >= 0 && monitor < count)
					{
						var name = glfw.GetMonitorName(monitors[monitor]);
						return name ?? $"Monitor {monitor + 1}";
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting monitor name: {ex.Message}");
			}
			
			return monitor == 0 ? "Primary Monitor" : $"Monitor {monitor + 1}";
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
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					glfw.GetMonitors(out int count);
					return count;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting monitor count: {ex.Message}");
				return 1;
			}
		}

		public static int GetCurrentMonitor()
		{
			if (RaylibInternal.Window == null) return 0;
			
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					var monitors = glfw.GetMonitors(out int count);
					
					// Get window position and size
					var windowPos = RaylibInternal.Window.Position;
					var windowSize = RaylibInternal.Window.Size;
					var windowCenterX = windowPos.X + windowSize.X / 2;
					var windowCenterY = windowPos.Y + windowSize.Y / 2;
					
					for (int i = 0; i < count; i++)
					{
						glfw.GetMonitorPos(monitors[i], out int x, out int y);
						var videoMode = glfw.GetVideoMode(monitors[i]);
						
						if (videoMode != null)
						{
							int width = videoMode->Width;
							int height = videoMode->Height;
							
							if (windowCenterX >= x && windowCenterX < x + width &&
								windowCenterY >= y && windowCenterY < y + height)
							{
								return i;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting current monitor: {ex.Message}");
			}
			
			return 0; // Default to primary monitor
		}

		public static Vector2 GetMonitorPosition(int monitor)
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					var monitors = glfw.GetMonitors(out int count);
					
					if (monitor >= 0 && monitor < count)
					{
						glfw.GetMonitorPos(monitors[monitor], out int x, out int y);
						return new Vector2(x, y);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting monitor position: {ex.Message}");
			}
			
			return Vector2.Zero;
		}

		public static int GetMonitorWidth(int monitor)
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					var monitors = glfw.GetMonitors(out int count);
					
					if (monitor >= 0 && monitor < count)
					{
						var videoMode = glfw.GetVideoMode(monitors[monitor]);
						if (videoMode != null)
						{
							return videoMode->Width;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting monitor width: {ex.Message}");
			}
			
			return 1920; // Default fallback
		}

		public static int GetMonitorHeight(int monitor)
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					var monitors = glfw.GetMonitors(out int count);
					
					if (monitor >= 0 && monitor < count)
					{
						var videoMode = glfw.GetVideoMode(monitors[monitor]);
						if (videoMode != null)
						{
							return videoMode->Height;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting monitor height: {ex.Message}");
			}
			
			return 1080; // Default fallback
		}

		public static int GetMonitorPhysicalWidth(int monitor)
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					var monitors = glfw.GetMonitors(out int count);
					
					if (monitor >= 0 && monitor < count)
					{
						glfw.GetMonitorPhysicalSize(monitors[monitor], out int width, out int height);
						return width;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting monitor physical width: {ex.Message}");
			}
			
			return 510; // Default ~24 inch monitor assumption
		}

		public static int GetMonitorPhysicalHeight(int monitor)
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					var monitors = glfw.GetMonitors(out int count);
					
					if (monitor >= 0 && monitor < count)
					{
						glfw.GetMonitorPhysicalSize(monitors[monitor], out int width, out int height);
						return height;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting monitor physical height: {ex.Message}");
			}
			
			return 287; // Default ~24 inch monitor assumption
		}

		public static int GetMonitorRefreshRate(int monitor)
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					var monitors = glfw.GetMonitors(out int count);
					
					if (monitor >= 0 && monitor < count)
					{
						var videoMode = glfw.GetVideoMode(monitors[monitor]);
						if (videoMode != null)
						{
							return videoMode->RefreshRate;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting monitor refresh rate: {ex.Message}");
			}
			
			return 60; // Default fallback
		}

		// Window flags and configuration
		public static void SetConfigFlags(uint flags)
		{
			RaylibInternal.CurrentConfigFlags = flags;
			
			// If window is already created, apply flags that can be changed at runtime
			if (RaylibInternal.Window != null)
			{
				ApplyRuntimeConfigFlags(flags);
			}
		}
		
		private static void ApplyRuntimeConfigFlags(uint flags)
		{
			if (RaylibInternal.Window == null) return;
			
			// Apply window state flags that can be changed at runtime
			if ((flags & FLAG_WINDOW_RESIZABLE) != 0)
			{
				// Silk.NET doesn't have direct resizable control after creation
				// This would need to be set during window creation
			}
			
			if ((flags & FLAG_WINDOW_TOPMOST) != 0)
			{
				// Platform-specific implementation needed
				// Windows: SetWindowPos with HWND_TOPMOST
				// Linux: X11 _NET_WM_STATE_ABOVE
				// macOS: NSWindow level
			}
			
			if ((flags & FLAG_WINDOW_MAXIMIZED) != 0)
			{
				RaylibInternal.Window.WindowState = WindowState.Maximized;
			}
			else if ((flags & FLAG_WINDOW_MINIMIZED) != 0)
			{
				RaylibInternal.Window.WindowState = WindowState.Minimized;
			}
			else if ((flags & FLAG_FULLSCREEN_MODE) != 0)
			{
				RaylibInternal.Window.WindowState = WindowState.Fullscreen;
			}
			else
			{
				RaylibInternal.Window.WindowState = WindowState.Normal;
			}
			
			if ((flags & FLAG_WINDOW_HIDDEN) != 0)
			{
				RaylibInternal.Window.IsVisible = false;
			}
			else
			{
				RaylibInternal.Window.IsVisible = true;
			}
		}
		
		private static WindowOptions ApplyConfigFlagsToWindowOptions(WindowOptions options, uint flags)
		{
			// Apply window creation flags
			if ((flags & FLAG_WINDOW_RESIZABLE) != 0)
			{
				options.WindowBorder = WindowBorder.Resizable;
			}
			else
			{
				options.WindowBorder = WindowBorder.Fixed;
			}
			
			if ((flags & FLAG_WINDOW_UNDECORATED) != 0)
			{
				options.WindowBorder = WindowBorder.Hidden;
			}
			
			if ((flags & FLAG_BORDERLESS_WINDOWED_MODE) != 0)
			{
				// Borderless windowed mode: fullscreen window without decorations
				options.WindowState = WindowState.Fullscreen;
				options.WindowBorder = WindowBorder.Hidden;
			}
			else if ((flags & FLAG_FULLSCREEN_MODE) != 0)
			{
				options.WindowState = WindowState.Fullscreen;
			}
			else if ((flags & FLAG_WINDOW_MAXIMIZED) != 0)
			{
				options.WindowState = WindowState.Maximized;
			}
			else if ((flags & FLAG_WINDOW_MINIMIZED) != 0)
			{
				options.WindowState = WindowState.Minimized;
			}
			else
			{
				options.WindowState = WindowState.Normal;
			}
			
			if ((flags & FLAG_WINDOW_HIDDEN) != 0)
			{
				options.IsVisible = false;
			}
			
			if ((flags & FLAG_VSYNC_HINT) != 0)
			{
				options.VSync = true;
			}
			
			if ((flags & FLAG_WINDOW_TRANSPARENT) != 0)
			{
				options.TransparentFramebuffer = true;
			}
			
			// MSAA
			if ((flags & FLAG_MSAA_4X_HINT) != 0)
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
		public static bool IsConfigFlagEnabled(uint flag)
		{
			return (RaylibInternal.CurrentConfigFlags & flag) != 0;
		}
		
		public static void EnableConfigFlag(uint flag)
		{
			RaylibInternal.CurrentConfigFlags |= flag;
			if (RaylibInternal.Window != null)
			{
				ApplyRuntimeConfigFlags(RaylibInternal.CurrentConfigFlags);
			}
		}
		
		public static void DisableConfigFlag(uint flag)
		{
			RaylibInternal.CurrentConfigFlags &= ~flag;
			if (RaylibInternal.Window != null)
			{
				ApplyRuntimeConfigFlags(RaylibInternal.CurrentConfigFlags);
			}
		}
		
		public static uint GetConfigFlags()
		{
			return RaylibInternal.CurrentConfigFlags;
		}

		// Cursor functions
		public static void ShowCursor()
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					
					if (RaylibInternal.Window?.Native?.Glfw is { } glfwHandle)
					{
						var windowHandle = (WindowHandle*)glfwHandle!;
						glfw.SetInputMode(windowHandle, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error showing cursor: {ex.Message}");
			}
		}

		public static void HideCursor()
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					
					if (RaylibInternal.Window?.Native?.Glfw is { } glfwHandle)
					{
						var windowHandle = (WindowHandle*)glfwHandle!;
						glfw.SetInputMode(windowHandle, CursorStateAttribute.Cursor, CursorModeValue.CursorHidden);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error hiding cursor: {ex.Message}");
			}
		}

		public static bool IsCursorHidden()
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					
					if (RaylibInternal.Window?.Native?.Glfw is { } glfwHandle)
					{
						var windowHandle = (WindowHandle*)glfwHandle!;
						var cursorMode = glfw.GetInputMode(windowHandle, CursorStateAttribute.Cursor);
						return cursorMode == (int)CursorModeValue.CursorHidden || cursorMode == (int)CursorModeValue.CursorDisabled;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error checking cursor visibility: {ex.Message}");
			}
			
			return false;
		}

		public static void EnableCursor()
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					
					if (RaylibInternal.Window?.Native?.Glfw is { } glfwHandle)
					{
						var windowHandle = (WindowHandle*)glfwHandle!;
						glfw.SetInputMode(windowHandle, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error enabling cursor: {ex.Message}");
			}
		}

		public static void DisableCursor()
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					
					if (RaylibInternal.Window?.Native?.Glfw is { } glfwHandle)
					{
						var windowHandle = (WindowHandle*)glfwHandle!;
						glfw.SetInputMode(windowHandle, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error disabling cursor: {ex.Message}");
			}
		}

		public static bool IsCursorOnScreen()
		{
			try
			{
				unsafe
				{
					EnsureGlfwInitialized();
					var glfw = Glfw.GetApi();
					
					if (RaylibInternal.Window?.Native?.Glfw is { } glfwHandle)
					{
						var windowHandle = (WindowHandle*)glfwHandle!;
						
						// Get cursor position
						glfw.GetCursorPos(windowHandle, out double xpos, out double ypos);
						
						// Get window size
						glfw.GetWindowSize(windowHandle, out int width, out int height);
						
						// Check if cursor is within window bounds
						return xpos >= 0 && xpos < width && ypos >= 0 && ypos < height;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error checking cursor position: {ex.Message}");
			}
			
			return false;
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
			RaylibInternal.Renderer = new(RaylibInternal.GL, RaylibInternal.Window.Size.X, RaylibInternal.Window.Size.Y);
			RaylibInternal.Input = RaylibInternal.Window.CreateInput();
			
			// Set up keyboard event handlers
			if (RaylibInternal.Input != null && RaylibInternal.Input.Keyboards.Count > 0)
			{
				var keyboard = RaylibInternal.Input.Keyboards[0];
				keyboard.KeyDown += OnKeyDown;
				keyboard.KeyUp += OnKeyUp;
				keyboard.KeyChar += OnKeyChar;
			}
			
			// Initialize font system
			InitializeFontSystem();
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
		}

		private static void OnKeyDown(IKeyboard keyboard, Silk.NET.Input.Key key, int scanCode)
		{
			int keyCode = (int)key;
			
			// Update current key state
			RaylibInternal.CurrentKeyState[keyCode] = true;
			
			// Add to pressed queue if this is a new press (wasn't down in previous frame)
			if (!RaylibInternal.PreviousKeyState.GetValueOrDefault(keyCode, false))
			{
				RaylibInternal.KeyPressedQueue.Enqueue(keyCode);
			}
		}

		private static void OnKeyUp(IKeyboard keyboard, Silk.NET.Input.Key key, int scanCode)
		{
			int keyCode = (int)key;
			RaylibInternal.CurrentKeyState[keyCode] = false;
		}

		private static void OnKeyChar(IKeyboard keyboard, char character)
		{
			RaylibInternal.CharPressedQueue.Enqueue((int)character);
		}

		// Input functions (basic)
		public static bool IsKeyPressed(int key)
		{
			// Check if key was just pressed (down now, but not down in previous frame)
			bool currentState = RaylibInternal.CurrentKeyState.GetValueOrDefault(key, false);
			bool previousState = RaylibInternal.PreviousKeyState.GetValueOrDefault(key, false);
			return currentState && !previousState;
		}

		public static bool IsKeyPressedRepeat(int key)
		{
			if (RaylibInternal.Input == null) return false;
			
			var keyboards = RaylibInternal.Input.Keyboards;
			if (keyboards.Count == 0) return false;
			
			var keyboard = keyboards[0];
			
			// For repeat, we check if key is currently pressed (includes initial press and repeats)
			return keyboard.IsKeyPressed((Silk.NET.Input.Key)key);
		}

		public static bool IsKeyDown(int key)
		{
			// Check if key is currently being held down
			return RaylibInternal.CurrentKeyState.GetValueOrDefault(key, false);
		}

		public static bool IsKeyReleased(int key)
		{
			// Check if key was just released (not down now, but was down in previous frame)
			bool currentState = RaylibInternal.CurrentKeyState.GetValueOrDefault(key, false);
			bool previousState = RaylibInternal.PreviousKeyState.GetValueOrDefault(key, false);
			return !currentState && previousState;
		}

		public static bool IsKeyUp(int key)
		{
			// Check if key is NOT being pressed
			return !RaylibInternal.CurrentKeyState.GetValueOrDefault(key, false);
		}

		public static int GetKeyPressed()
		{
			// Get key pressed from queue, return 0 if queue is empty
			if (RaylibInternal.KeyPressedQueue.Count > 0)
			{
				return RaylibInternal.KeyPressedQueue.Dequeue();
			}
			return 0;
		}

		public static int GetCharPressed()
		{
			// Get character pressed from queue, return 0 if queue is empty
			if (RaylibInternal.CharPressedQueue.Count > 0)
			{
				return RaylibInternal.CharPressedQueue.Dequeue();
			}
			return 0;
		}

		public static void SetExitKey(int key)
		{
			RaylibInternal.ExitKey = key;
		}

		public static Vector2 GetMousePosition()
		{
			if (RaylibInternal.Input == null) return Vector2.Zero;
			
			var mice = RaylibInternal.Input.Mice;
			if (mice.Count == 0) return Vector2.Zero;
			
			var mouse = mice[0];
			return new Vector2(mouse.Position.X, mouse.Position.Y);
		}

		public static bool IsMouseButtonPressed(int button)
		{
			if (RaylibInternal.Input == null) return false;
			
			var mice = RaylibInternal.Input.Mice;
			if (mice.Count == 0) return false;
			
			var mouse = mice[0];
			
			return mouse.IsButtonPressed((Silk.NET.Input.MouseButton)button);
		}


		// Font system initialization
		private static void InitializeFontSystem()
		{
			try
			{
				if (RaylibInternal.GL == null)
				{
					Console.WriteLine("Warning: Cannot initialize font system - OpenGL context not available");
					return;
				}

				// Create FontSystem with OpenGL texture renderer
				FontSystemSettings fontSystemSettings = new()
				{
					TextureWidth = 1024,
					TextureHeight = 1024,
					KernelWidth = 2,
					KernelHeight = 2,
					FontResolutionFactor = 1
				};

				// Initialize FontStashSharp system
				RaylibInternal.FontSystem = new(fontSystemSettings);
				
				// Create default bitmap font texture
				RaylibInternal.DefaultFontTexture = DefaultFont.CreateDefaultFontTexture(RaylibInternal.GL);
				Console.WriteLine("Default font loaded successfully");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error initializing font system: {ex.Message}");
			}
		}


		// Font loading functions
		public static Font LoadFont(string fileName)
		{
			try
			{
				if (RaylibInternal.FontSystem == null)
				{
					Console.WriteLine("Warning: Font system not initialized");
					return new Font();
				}

				if (!File.Exists(fileName))
				{
					Console.WriteLine($"Warning: Font file not found: {fileName}");
					return new Font();
				}

				// Create a new FontSystem specifically for this font
				FontSystem fontSystem = new();
				byte[] fontData = File.ReadAllBytes(fileName);
				fontSystem.AddFont(fontData);
				
				// Get a default size font from this specific font system
				var spriteFont = fontSystem.GetFont(18);
				
				// Store both the font and its system for later use
				RaylibInternal.LoadedFonts[fileName] = spriteFont;
				RaylibInternal.LoadedFontSystems[fileName] = fontSystem;
				
				return new Font(fileName, spriteFont, 18);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading font '{fileName}': {ex.Message}");
				return new Font();
			}
		}

		public static void UnloadFont(Font font)
		{
			if (!string.IsNullOrEmpty(font.FileName))
			{
				RaylibInternal.LoadedFonts.Remove(font.FileName);
				RaylibInternal.LoadedFontSystems.Remove(font.FileName);
			}
		}

		// Text rendering functions using bitmap font as fallback
		public static void DrawText(string text, int posX, int posY, int fontSize, Color color)
		{
			if (RaylibInternal.Renderer == null) return;
			
			// Use bitmap font for default text rendering
			RaylibInternal.Renderer.DrawBitmapText(text, new Vector2(posX, posY), color, fontSize);
		}

		public static void DrawTextEx(Font font, string text, Vector2 position, float fontSize, float spacing, Color color)
		{
			if (string.IsNullOrEmpty(text) || RaylibInternal.Renderer == null)
				return;

			try
			{
				DynamicSpriteFont? spriteFont = null;
				
				// If a specific font is provided and it's valid, use the stored font
				if (font.IsValid && !string.IsNullOrEmpty(font.FileName))
				{
					// Try to get the loaded font system from our cache
					if (RaylibInternal.LoadedFontSystems.TryGetValue(font.FileName, out var fontSystem))
					{
						// Get the font at the requested size from the loaded font's system
						spriteFont = fontSystem.GetFont((int)fontSize);
					}
					else if (font.SpriteFont != null)
					{
						// Use the font's own SpriteFont if available
						spriteFont = font.SpriteFont;
					}
				}
				
				// Fallback to default font if no specific font or font loading failed
				if (spriteFont == null && RaylibInternal.FontSystem != null)
				{
					spriteFont = RaylibInternal.DefaultFont ?? RaylibInternal.FontSystem.GetFont((int)fontSize);
				}
				
				if (spriteFont == null)
				{
					Console.WriteLine("Warning: No font available for text rendering");
					return;
				}

				// Create a simple text renderer that works with our existing system
				RaylibInternal.Renderer.DrawText(spriteFont, text, position, color, fontSize, spacing);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error rendering text: {ex.Message}");
			}
		}

		// Text measurement functions
		public static int MeasureText(string text, int fontSize)
		{
			var font = RaylibInternal.DefaultFont != null 
				? new Font("", RaylibInternal.DefaultFont, fontSize) 
				: new Font();
			return (int)MeasureTextEx(font, text, fontSize, 1.0f).X;
		}

		public static Vector2 MeasureTextEx(Font font, string text, float fontSize, float spacing)
		{
			if (string.IsNullOrEmpty(text) || RaylibInternal.FontSystem == null)
				return Vector2.Zero;

			try
			{
				var spriteFont = font.SpriteFont ?? RaylibInternal.DefaultFont ?? RaylibInternal.FontSystem.GetFont((int)fontSize);
				if (spriteFont == null)
					return Vector2.Zero;

				// Use FontStashSharp's built-in measurement
				var size = spriteFont.MeasureString(text);
				return new Vector2(size.X, size.Y);
			}
			catch
			{
				return Vector2.Zero;
			}
		}
	}

	// Texture filtering modes
	public enum TextureFilter
	{
		Point = 0,              // No filter, just pixel approximation
		Bilinear,               // Linear filtering
		Trilinear,              // Trilinear filtering (linear with mipmaps)
		Anisotropic4x,          // Anisotropic filtering 4x
		Anisotropic8x,          // Anisotropic filtering 8x
		Anisotropic16x          // Anisotropic filtering 16x
	}

	public enum TextureWrap
	{
		Repeat = 0,             // Repeats texture in tiled mode
		Clamp,                  // Clamps texture to edge pixel in tiled mode
		MirrorRepeat,           // Repeats texture with mirror in tiled mode
		MirrorClamp             // Clamps texture to edge pixel with mirror in tiled mode
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

	public struct Texture2D
	{
		public uint Id;          // OpenGL texture id
		public int Width;        // Texture base width
		public int Height;       // Texture base height
		public int Mipmaps;      // Mipmap levels, 1 by default
		public int Format;       // Data format (PixelFormat type)

		public Texture2D(uint id, int width, int height, int mipmaps = 1, int format = 1)
		{
			Id = id;
			Width = width;
			Height = height;
			Mipmaps = mipmaps;
			Format = format;
		}
	}

	public struct NPatchInfo(Rectangle source, int left, int top, int right, int bottom, int layout)
	{
		public Rectangle source = source;   // Texture source rectangle
		public int left = left;             // Left border offset
		public int top = top;               // Top border offset
		public int right = right;           // Right border offset
		public int bottom = bottom;         // Bottom border offset
		public int layout = layout;         // Layout of the n-patch: 3x3, 1x3 or 3x1
	}

	public struct Font
	{
		public string FileName;             // Font file name
		public DynamicSpriteFont? SpriteFont; // FontStashSharp sprite font
		public int BaseSize;                // Base font size
		public bool IsValid;                // Whether the font is valid

		public Font()
		{
			FileName = string.Empty;
			SpriteFont = null;
			BaseSize = 18;
			IsValid = false;
		}

		public Font(string fileName, DynamicSpriteFont spriteFont, int baseSize = 18)
		{
			FileName = fileName;
			SpriteFont = spriteFont;
			BaseSize = baseSize;
			IsValid = spriteFont != null;
		}
	}

	public static class MouseButton
	{
		public const int MOUSE_BUTTON_LEFT = 0;       // public const int MOUSE button left
		public const int MOUSE_BUTTON_RIGHT = 1;       // public const int MOUSE button right
		public const int MOUSE_BUTTON_MIDDLE = 2;       // public const int MOUSE button public const int iddle (pressed wheel)
		public const int MOUSE_BUTTON_SIDE = 3;       // public const int MOUSE button side (advanced public const int MOUSE device)
		public const int MOUSE_BUTTON_EXTRA = 4;       // public const int MOUSE button extra (advanced public const int MOUSE device)
		public const int MOUSE_BUTTON_FORWARD = 5;       // public const int MOUSE button forward (advanced public const int MOUSE device)
		public const int MOUSE_BUTTON_BACK = 6;       // public const int MOUSE button back (advanced public const int MOUSE device)
	}

	public static class MouseCursor
	{
		public const int MOUSE_CURSOR_DEFAULT = 0;     // Default pointer shape
		public const int MOUSE_CURSOR_ARROW = 1;     // Arrow shape
		public const int MOUSE_CURSOR_IBEAM = 2;     // Text writing cursor shape
		public const int MOUSE_CURSOR_CROSSHAIR = 3;     // Cross shape
		public const int MOUSE_CURSOR_POINTING_HAND = 4;     // Pointing hand cursor
		public const int MOUSE_CURSOR_RESIZE_EW = 5;     // Horizontal resize/move arrow shape
		public const int MOUSE_CURSOR_RESIZE_NS = 6;     // Vertical resize/move arrow shape
		public const int MOUSE_CURSOR_RESIZE_NWSE = 7;     // Top-left to bottom-right diagonal resize/move arrow shape
		public const int MOUSE_CURSOR_RESIZE_NESW = 8;     // The top-right to bottom-left diagonal resize/move arrow shape
		public const int MOUSE_CURSOR_RESIZE_ALL = 9;     // The omnidirectional resize/move cursor shape
		public const int MOUSE_CURSOR_NOT_ALLOWED = 10;     // The operation-not-allowed shape
	}



	// Global keyboard key constants (Raylib style)
	public static class KeyboardKeys
	{
		// Alphanumeric keys
		public const int KEY_SPACE = 32;
		public const int KEY_APOSTROPHE = 39;    // '
		public const int KEY_COMMA = 44;         // ,
		public const int KEY_MINUS = 45;         // -
		public const int KEY_PERIOD = 46;        // .
		public const int KEY_SLASH = 47;         // /
		public const int KEY_ZERO = 48;
		public const int KEY_ONE = 49;
		public const int KEY_TWO = 50;
		public const int KEY_THREE = 51;
		public const int KEY_FOUR = 52;
		public const int KEY_FIVE = 53;
		public const int KEY_SIX = 54;
		public const int KEY_SEVEN = 55;
		public const int KEY_EIGHT = 56;
		public const int KEY_NINE = 57;
		public const int KEY_SEMICOLON = 59;     // ;
		public const int KEY_EQUAL = 61;         // =
		public const int KEY_A = 65;
		public const int KEY_B = 66;
		public const int KEY_C = 67;
		public const int KEY_D = 68;
		public const int KEY_E = 69;
		public const int KEY_F = 70;
		public const int KEY_G = 71;
		public const int KEY_H = 72;
		public const int KEY_I = 73;
		public const int KEY_J = 74;
		public const int KEY_K = 75;
		public const int KEY_L = 76;
		public const int KEY_M = 77;
		public const int KEY_N = 78;
		public const int KEY_O = 79;
		public const int KEY_P = 80;
		public const int KEY_Q = 81;
		public const int KEY_R = 82;
		public const int KEY_S = 83;
		public const int KEY_T = 84;
		public const int KEY_U = 85;
		public const int KEY_V = 86;
		public const int KEY_W = 87;
		public const int KEY_X = 88;
		public const int KEY_Y = 89;
		public const int KEY_Z = 90;
		public const int KEY_LEFT_BRACKET = 91;   // [
		public const int KEY_BACKSLASH = 92;      // \
		public const int KEY_RIGHT_BRACKET = 93;  // ]
		public const int KEY_GRAVE = 96;          // `

		// Function keys
		public const int KEY_ESCAPE = 256;
		public const int KEY_ENTER = 257;
		public const int KEY_TAB = 258;
		public const int KEY_BACKSPACE = 259;
		public const int KEY_INSERT = 260;
		public const int KEY_DELETE = 261;
		public const int KEY_RIGHT = 262;
		public const int KEY_LEFT = 263;
		public const int KEY_DOWN = 264;
		public const int KEY_UP = 265;
		public const int KEY_PAGE_UP = 266;
		public const int KEY_PAGE_DOWN = 267;
		public const int KEY_HOME = 268;
		public const int KEY_END = 269;
		public const int KEY_CAPS_LOCK = 280;
		public const int KEY_SCROLL_LOCK = 281;
		public const int KEY_NUM_LOCK = 282;
		public const int KEY_PRINT_SCREEN = 283;
		public const int KEY_PAUSE = 284;
		public const int KEY_F1 = 290;
		public const int KEY_F2 = 291;
		public const int KEY_F3 = 292;
		public const int KEY_F4 = 293;
		public const int KEY_F5 = 294;
		public const int KEY_F6 = 295;
		public const int KEY_F7 = 296;
		public const int KEY_F8 = 297;
		public const int KEY_F9 = 298;
		public const int KEY_F10 = 299;
		public const int KEY_F11 = 300;
		public const int KEY_F12 = 301;
		public const int KEY_LEFT_SHIFT = 340;
		public const int KEY_LEFT_CONTROL = 341;
		public const int KEY_LEFT_ALT = 342;
		public const int KEY_LEFT_SUPER = 343;
		public const int KEY_RIGHT_SHIFT = 344;
		public const int KEY_RIGHT_CONTROL = 345;
		public const int KEY_RIGHT_ALT = 346;
		public const int KEY_RIGHT_SUPER = 347;
		public const int KEY_KB_MENU = 348;

		// Keypad keys
		public const int KEY_KP_0 = 320;
		public const int KEY_KP_1 = 321;
		public const int KEY_KP_2 = 322;
		public const int KEY_KP_3 = 323;
		public const int KEY_KP_4 = 324;
		public const int KEY_KP_5 = 325;
		public const int KEY_KP_6 = 326;
		public const int KEY_KP_7 = 327;
		public const int KEY_KP_8 = 328;
		public const int KEY_KP_9 = 329;
		public const int KEY_KP_DECIMAL = 330;
		public const int KEY_KP_DIVIDE = 331;
		public const int KEY_KP_MULTIPLY = 332;
		public const int KEY_KP_SUBTRACT = 333;
		public const int KEY_KP_ADD = 334;
		public const int KEY_KP_ENTER = 335;
		public const int KEY_KP_EQUAL = 336;
	}

	// Global window flag constants (Raylib style)
	public static class WindowFlags
	{
		public const int FLAG_VSYNC_HINT = 0x00000040;           // Set to try enabling V-Sync on GPU
		public const int FLAG_FULLSCREEN_MODE = 0x00000002;      // Set to run program in fullscreen
		public const int FLAG_WINDOW_RESIZABLE = 0x00000004;     // Set to allow resizable window
		public const int FLAG_WINDOW_UNDECORATED = 0x00000008;   // Set to disable window decoration (frame and buttons)
		public const int FLAG_WINDOW_HIDDEN = 0x00000080;        // Set to hide window
		public const int FLAG_WINDOW_MINIMIZED = 0x00000200;     // Set to minimize window (iconify)
		public const int FLAG_WINDOW_MAXIMIZED = 0x00000400;     // Set to maximize window (expanded to monitor)
		public const int FLAG_WINDOW_UNFOCUSED = 0x00000800;     // Set to window non focused
		public const int FLAG_WINDOW_TOPMOST = 0x00001000;       // Set to window always on top
		public const int FLAG_WINDOW_ALWAYS_RUN = 0x00000100;    // Set to allow windows running while minimized
		public const int FLAG_WINDOW_TRANSPARENT = 0x00000010;   // Set to allow transparent framebuffer
		public const int FLAG_WINDOW_HIGHDPI = 0x00002000;       // Set to support HighDPI
		public const int FLAG_WINDOW_MOUSE_PASSTHROUGH = 0x00004000; // Set to support mouse passthrough, only supported when FLAG_WINDOW_UNDECORATED
		public const int FLAG_BORDERLESS_WINDOWED_MODE = 0x00008000; // Set to run program in borderless windowed mode
		public const int FLAG_MSAA_4X_HINT = 0x00000020;         // Set to try enabling MSAA 4X
		public const int FLAG_INTERLACED_HINT = 0x00010000;      // Set to try enabling interlaced video format (for V3D)
	}
}
