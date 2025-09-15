using System.Numerics;
using FontStashSharp;

namespace SilkRay
{
	// ================================
	// CORE STRUCTURES
	// ================================

	/// <summary>
	/// Vector2 structure for 2D math operations
	/// </summary>
	public struct Vector2(float x, float y)
	{
		public float X = x;
		public float Y = y;

		public Vector2(float value) : this(value, value)
		{
		}

		// Vector operations
		public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
		public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
		public static Vector2 operator *(Vector2 a, float scalar) => new(a.X * scalar, a.Y * scalar);
		public static Vector2 operator *(float scalar, Vector2 a) => new(a.X * scalar, a.Y * scalar);
		public static Vector2 operator /(Vector2 a, float scalar) => new(a.X / scalar, a.Y / scalar);
		public static Vector2 operator -(Vector2 a) => new(-a.X, -a.Y);

		// Comparison operators
		public static bool operator ==(Vector2 a, Vector2 b) => Math.Abs(a.X - b.X) < float.Epsilon && Math.Abs(a.Y - b.Y) < float.Epsilon;
		public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);

		// Properties
		public float Length => (float)Math.Sqrt(X * X + Y * Y);
		public float LengthSquared => X * X + Y * Y;

		// Static properties
		public static Vector2 Zero => new(0, 0);
		public static Vector2 One => new(1, 1);
		public static Vector2 UnitX => new(1, 0);
		public static Vector2 UnitY => new(0, 1);

		// Methods
		public Vector2 Normalize()
		{
			float length = Length;
			return length > 0 ? this / length : Zero;
		}

		public static float Distance(Vector2 a, Vector2 b)
		{
			return (a - b).Length;
		}

		public static float DistanceSquared(Vector2 a, Vector2 b)
		{
			return (a - b).LengthSquared;
		}

		public static float Dot(Vector2 a, Vector2 b)
		{
			return a.X * b.X + a.Y * b.Y;
		}

		public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
		{
			return a + (b - a) * t;
		}

		public override bool Equals(object? obj)
		{
			return obj is Vector2 vector && this == vector;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y);
		}

		public override string ToString()
		{
			return $"Vector2({X}, {Y})";
		}
	}

	/// <summary>
	/// Color structure representing RGBA values
	/// </summary>
	public struct Color(byte r, byte g, byte b, byte a = 255)
	{
		public byte R = r;
		public byte G = g;
		public byte B = b;
		public byte A = a;

		public Color(int r, int g, int b, int a = 255) : this(
			(byte)Math.Clamp(r, 0, 255),
			(byte)Math.Clamp(g, 0, 255),
			(byte)Math.Clamp(b, 0, 255),
			(byte)Math.Clamp(a, 0, 255))
		{
		}

		/// <summary>
		/// Convert color to Vector4 (normalized 0.0-1.0)
		/// </summary>
		public Vector4 ToVector4()
		{
			return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
		}

		/// <summary>
		/// Create color from Vector4 (normalized 0.0-1.0)
		/// </summary>
		public static Color FromVector4(Vector4 vector)
		{
			return new Color(
				(byte)(vector.X * 255),
				(byte)(vector.Y * 255),
				(byte)(vector.Z * 255),
				(byte)(vector.W * 255)
			);
		}

		// Predefined colors matching Raylib
		public static readonly Color LightGray = new(200, 200, 200);
		public static readonly Color Gray = new(130, 130, 130);
		public static readonly Color DarkGray = new(80, 80, 80);
		public static readonly Color Yellow = new(253, 249, 0);
		public static readonly Color Gold = new(255, 203, 0);
		public static readonly Color Orange = new(255, 161, 0);
		public static readonly Color Pink = new(255, 109, 194);
		public static readonly Color Red = new(230, 41, 55);
		public static readonly Color Maroon = new(190, 33, 55);
		public static readonly Color Green = new(0, 228, 48);
		public static readonly Color Lime = new(0, 158, 47);
		public static readonly Color DarkGreen = new(0, 117, 44);
		public static readonly Color SkyBlue = new(102, 191, 255);
		public static readonly Color Blue = new(0, 121, 241);
		public static readonly Color DarkBlue = new(0, 82, 172);
		public static readonly Color Purple = new(200, 122, 255);
		public static readonly Color Violet = new(135, 60, 190);
		public static readonly Color DarkPurple = new(112, 31, 126);
		public static readonly Color Beige = new(211, 176, 131);
		public static readonly Color Brown = new(127, 106, 79);
		public static readonly Color DarkBrown = new(76, 63, 47);
		public static readonly Color White = new(255, 255, 255);
		public static readonly Color Black = new(0, 0, 0);
		public static readonly Color Blank = new(0, 0, 0, 0);
		public static readonly Color Magenta = new(255, 0, 255);
		public static readonly Color RayWhite = new(245, 245, 245);

		public override string ToString()
		{
			return $"Color({R}, {G}, {B}, {A})";
		}
	}

	public struct Rectangle(float x, float y, float width, float height)
	{
		public float X = x;
		public float Y = y;
		public float Width = width;
		public float Height = height;
	}

	public struct Image(byte[] data, int width, int height, int mipmaps = 1, int format = 1)
	{
		public byte[] Data = data;
		public int Width = width;
		public int Height = height;
		public int Mipmaps = mipmaps;
		public int Format = format;
	}

	public struct Texture2D(uint id, int width, int height, int mipmaps = 1, int format = 1)
	{
		public uint Id = id;				// OpenGL texture id
		public int Width = width;			// Texture base width
		public int Height = height;		// Texture base height
		public int Mipmaps = mipmaps;		// Mipmap levels, 1 by default
		public int Format = format;		// Data format (PixelFormat type)
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

	public struct Font(string fileName, DynamicSpriteFont spriteFont, int baseSize = 18)
	{
		public string FileName = fileName;
		public DynamicSpriteFont? SpriteFont = spriteFont;
		public int BaseSize = baseSize;
		public bool IsValid = spriteFont != null;
	}

	// ================================
	// CONSTANTS AND ENUMS
	// ================================

	// Texture filtering modes
	public static class TextureFilter
	{
		public const int TEXTURE_FILTER_POINT = 0;				// No filter; just pixel approximation
		public const int TEXTURE_FILTER_BILINEAR = 1;			// Linear filtering
		public const int TEXTURE_FILTER_TRILINEAR = 2;			// Trilinear filtering (linear with mipmaps)
		public const int TEXTURE_FILTER_ANISOTROPIC_4X = 3;		// Anisotropic filtering 4x
		public const int TEXTURE_FILTER_ANISOTROPIC_8X = 4;		// Anisotropic filtering 8x
		public const int TEXTURE_FILTER_ANISOTROPIC_16X = 5;	// Anisotropic filtering 16x
	}

	public static class TextureWrap
	{
		public const int TEXTURE_WRAP_REPEAT = 0;				// Repeats texture in tiled mode
		public const int TEXTURE_WRAP_CLAMP = 1;				// Clamps texture to edge pixel in tiled mode
		public const int TEXTURE_WRAP_MIRROR_REPEAT = 2;		// Repeats texture with mirror in tiled mode
		public const int TEXTURE_WRAP_MIRROR_CLAMP = 3;			// Clamps texture to edge pixel with mirror in tiled mode
	}

	public static class MouseButton
	{
		public const int MOUSE_BUTTON_LEFT = 0;			// Mouse button left
		public const int MOUSE_BUTTON_RIGHT = 1;		// Mouse button right
		public const int MOUSE_BUTTON_MIDDLE = 2;		// Mouse button middle (pressed wheel)
		public const int MOUSE_BUTTON_SIDE = 3;			// Mouse button side (advanced mouse device)
		public const int MOUSE_BUTTON_EXTRA = 4;		// Mouse button extra (advanced mouse device)
		public const int MOUSE_BUTTON_FORWARD = 5;		// Mouse button forward (advanced mouse device)
		public const int MOUSE_BUTTON_BACK = 6;			// Mouse button back (advanced mouse device)
	}

	public static class MouseCursor
	{
		public const int MOUSE_CURSOR_DEFAULT = 0;			// Default pointer shape
		public const int MOUSE_CURSOR_ARROW = 1;			// Arrow shape
		public const int MOUSE_CURSOR_IBEAM = 2;			// Text writing cursor shape
		public const int MOUSE_CURSOR_CROSSHAIR = 3;		// Cross shape
		public const int MOUSE_CURSOR_POINTING_HAND = 4;	// Pointing hand cursor
		public const int MOUSE_CURSOR_RESIZE_EW = 5;		// Horizontal resize/move arrow shape
		public const int MOUSE_CURSOR_RESIZE_NS = 6;		// Vertical resize/move arrow shape
		public const int MOUSE_CURSOR_RESIZE_NWSE = 7;		// Top-left to bottom-right diagonal resize/move arrow shape
		public const int MOUSE_CURSOR_RESIZE_NESW = 8;		// The top-right to bottom-left diagonal resize/move arrow shape
		public const int MOUSE_CURSOR_RESIZE_ALL = 9;		// The omnidirectional resize/move cursor shape
		public const int MOUSE_CURSOR_NOT_ALLOWED = 10;		// The operation-not-allowed shape
	}

	// Keyboard keys
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
