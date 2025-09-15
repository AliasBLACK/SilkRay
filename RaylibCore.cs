global using static SilkRay.RaylibCore;
global using static SilkRay.RaylibShapes;
global using static SilkRay.RaylibTextures;
global using static SilkRay.RaylibText;
global using static SilkRay.KeyboardKeys;
global using static SilkRay.WindowFlags;
global using static SilkRay.MouseButton;
global using static SilkRay.MouseCursor;
global using static SilkRay.TextureFilter;
global using static SilkRay.TextureWrap;

using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.GLFW;
using Silk.NET.Input;
using TextCopy;
using StbImageSharp;
using FontStashSharp;
using System.IO;

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
		
		// File drop handling
		public static List<string> DroppedFiles = new();
	}

	/// <summary>
	/// Core Raylib functions implementation using Silk.NET
	/// </summary>
	public static class RaylibCore
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
			
			// Update keyboard state for proper key press detection
			// This must happen AFTER input processing but BEFORE the next frame
			UpdateKeyboardState();
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
			RaylibText.InitializeFontSystem();
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

		// File system functions
		public static bool FileExists(string fileName)
		{
			return File.Exists(fileName);
		}

		public static bool DirectoryExists(string dirPath)
		{
			return Directory.Exists(dirPath);
		}

		public static bool IsFileExtension(string fileName, string ext)
		{
			if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(ext))
				return false;
			
			string fileExt = Path.GetExtension(fileName);
			return string.Equals(fileExt, ext, StringComparison.OrdinalIgnoreCase);
		}

		public static int GetFileLength(string fileName)
		{
			if (!File.Exists(fileName))
				return 0;
			
			try
			{
				var fileInfo = new FileInfo(fileName);
				return (int)fileInfo.Length;
			}
			catch
			{
				return 0;
			}
		}

		public static string GetFileExtension(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return string.Empty;
			
			return Path.GetExtension(fileName);
		}

		public static string GetFileName(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return string.Empty;
			
			return Path.GetFileName(filePath);
		}

		public static string GetFileNameWithoutExt(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return string.Empty;
			
			return Path.GetFileNameWithoutExtension(filePath);
		}

		public static string GetDirectoryPath(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return string.Empty;
			
			string? dir = Path.GetDirectoryName(filePath);
			return dir ?? string.Empty;
		}

		public static string GetPrevDirectoryPath(string dirPath)
		{
			if (string.IsNullOrEmpty(dirPath))
				return string.Empty;
			
			try
			{
				DirectoryInfo? parent = Directory.GetParent(dirPath);
				return parent?.FullName ?? string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}

		public static string GetWorkingDirectory()
		{
			return Directory.GetCurrentDirectory();
		}

		public static string GetApplicationDirectory()
		{
			return AppDomain.CurrentDomain.BaseDirectory;
		}

		public static int MakeDirectory(string dirPath)
		{
			if (string.IsNullOrEmpty(dirPath))
				return -1;
			
			try
			{
				Directory.CreateDirectory(dirPath);
				return 0;
			}
			catch
			{
				return -1;
			}
		}

		public static bool ChangeDirectory(string dir)
		{
			if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
				return false;
			
			try
			{
				Directory.SetCurrentDirectory(dir);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool IsPathFile(string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;
			
			return File.Exists(path);
		}

		public static bool IsFileNameValid(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return false;
			
			try
			{
				char[] invalidChars = Path.GetInvalidFileNameChars();
				return fileName.IndexOfAny(invalidChars) == -1;
			}
			catch
			{
				return false;
			}
		}

		public static FilePathList LoadDirectoryFiles(string dirPath)
		{
			return LoadDirectoryFilesEx(dirPath, "*", false);
		}

		public static FilePathList LoadDirectoryFilesEx(string basePath, string filter, bool scanSubdirs)
		{
			var fileList = new FilePathList();
			
			if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
				return fileList;
			
			try
			{
				SearchOption searchOption = scanSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
				
				// Handle special "DIR" filter to include directories
				bool includeDirs = filter.Contains("DIR");
				string actualFilter = filter.Replace("DIR", "").Trim();
				if (string.IsNullOrEmpty(actualFilter))
					actualFilter = "*";
				
				var files = new List<string>();
				
				// Add files
				files.AddRange(Directory.GetFiles(basePath, actualFilter, searchOption));
				
				// Add directories if requested
				if (includeDirs)
				{
					files.AddRange(Directory.GetDirectories(basePath, "*", searchOption));
				}
				
				fileList.capacity = files.Count;
				fileList.count = files.Count;
				fileList.paths = files.ToArray();
			}
			catch
			{
				// Return empty list on error
			}
			
			return fileList;
		}

		public static void UnloadDirectoryFiles(FilePathList files)
		{
			// In C#, garbage collection handles memory management
			// This function exists for API compatibility
		}

		public static bool IsFileDropped()
		{
			return RaylibInternal.DroppedFiles.Count > 0;
		}

		public static FilePathList LoadDroppedFiles()
		{
			var fileList = new FilePathList
			{
				capacity = RaylibInternal.DroppedFiles.Count,
				count = RaylibInternal.DroppedFiles.Count,
				paths = RaylibInternal.DroppedFiles.ToArray()
			};
			
			return fileList;
		}

		public static void UnloadDroppedFiles(FilePathList files)
		{
			RaylibInternal.DroppedFiles.Clear();
		}

		public static long GetFileModTime(string fileName)
		{
			if (!File.Exists(fileName))
				return 0;
			
			try
			{
				var fileInfo = new FileInfo(fileName);
				// Return Unix timestamp
				return ((DateTimeOffset)fileInfo.LastWriteTime).ToUnixTimeSeconds();
			}
			catch
			{
				return 0;
			}
		}
	}
}
