using Silk.NET.OpenGL;
using FontStashSharp;
using FontStashSharp.Interfaces;
using System.Numerics;

namespace SilkRay
{
	/// <summary>
	/// Original Raylib default font implementation using bitmap data.
	/// This provides the exact same default font as the original Raylib library.
	/// </summary>
	public static class DefaultFont
	{
		// Original Raylib default font bitmap data (128 characters, 8x8 pixels each)
		private static readonly uint[] DefaultFontData = {
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00200020, 0x0001b000, 0x00000000, 0x00000000, 0x8ef92520, 0x00020a00, 0x7dbe8000, 0x1f7df45f,
			0x4a2bf2a0, 0x0852091e, 0x41224000, 0x10041450, 0x2e292020, 0x08220812, 0x41222000, 0x10041450, 0x10f92020, 0x3efa084c, 0x7d22103c, 0x107df7de,
			0xe8a12020, 0x08220832, 0x05220800, 0x10450410, 0xa4a3f000, 0x08520832, 0x05220400, 0x10450410, 0xe2f92020, 0x0002085e, 0x7d3e0281, 0x107df41f,
			0x00200000, 0x8001b000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0xc0000fbe, 0xfbf7e00f, 0x5fbf7e7d, 0x0050bee8, 0x440808a2, 0x0a142fe8, 0x50810285, 0x0050a048,
			0x49e428a2, 0x0a142828, 0x40810284, 0x0048a048, 0x10020fbe, 0x09f7ebaf, 0xd89f3e84, 0x0047a04f, 0x09e48822, 0x0a142aa1, 0x50810284, 0x0048a048,
			0x04082822, 0x0a142fa0, 0x50810285, 0x0050a248, 0x00008fbe, 0xfbf42021, 0x5f817e7d, 0x07d09ce8, 0x00008000, 0x00000fe0, 0x00000000, 0x00000000,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x000c0180,
			0xdfbf4282, 0x0bfbf7ef, 0x42850505, 0x004804bf, 0x50a142c6, 0x08401428, 0x42852505, 0x00a808a0, 0x50a146aa, 0x08401428, 0x42852505, 0x00081090,
			0x5fa14a92, 0x0843f7e8, 0x7e792505, 0x00082088, 0x40a15282, 0x08420128, 0x40852489, 0x00084084, 0x40a16282, 0x0842022a, 0x40852451, 0x00088082,
			0xc0bf4282, 0xf843f42f, 0x7e85fc21, 0x3e0900bf, 0x00000000, 0x00000004, 0x00000000, 0x000c0180, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x04000402, 0x41482000, 0x00000000, 0x00000800,
			0x04000404, 0x4100203c, 0x00000000, 0x00000800, 0xf7df7df0, 0x514bef85, 0xbefbefbe, 0x04513bef, 0x14414500, 0x494a2885, 0xa28a28aa, 0x04510820,
			0xf44145f0, 0x474a289d, 0xa28a28aa, 0x04510be0, 0x14414510, 0x494a2884, 0xa28a28aa, 0x02910a00, 0xf7df7df0, 0xd14a2f85, 0xbefbe8aa, 0x011f7be0,
			0x00000000, 0x00400804, 0x20080000, 0x00000000, 0x00000000, 0x00600f84, 0x20080000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
			0xac000000, 0x00000f01, 0x00000000, 0x00000000, 0x24000000, 0x00000f01, 0x00000000, 0x06000000, 0x24000000, 0x00000f01, 0x00000000, 0x09108000,
			0x24fa28a2, 0x00000f01, 0x00000000, 0x013e0000, 0x2242252a, 0x00000f52, 0x00000000, 0x038a8000, 0x2422222a, 0x00000f29, 0x00000000, 0x010a8000,
			0x2412252a, 0x00000f01, 0x00000000, 0x010a8000, 0x24fbe8be, 0x00000f01, 0x00000000, 0x0ebe8000, 0xac020000, 0x00000f01, 0x00000000, 0x00048000,
			0x0003e000, 0x00000f00, 0x00000000, 0x00008000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000038, 0x8443b80e, 0x00203a03,
			0x02bea080, 0xf0000020, 0xc452208a, 0x04202b02, 0xf8029122, 0x07f0003b, 0xe44b388e, 0x02203a02, 0x081e8a1c, 0x0411e92a, 0xf4420be0, 0x01248202,
			0xe8140414, 0x05d104ba, 0xe7c3b880, 0x00893a0a, 0x283c0e1c, 0x04500902, 0xc4400080, 0x00448002, 0xe8208422, 0x04500002, 0x80400000, 0x05200002,
			0x083e8e00, 0x04100002, 0x804003e0, 0x07000042, 0xf8008400, 0x07f00003, 0x80400000, 0x04000022, 0x00000000, 0x00000000, 0x80400000, 0x04000002,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00800702, 0x1848a0c2, 0x84010000, 0x02920921, 0x01042642, 0x00005121, 0x42023f7f, 0x00291002,
			0xefc01422, 0x7efdfbf7, 0xefdfa109, 0x03bbbbf7, 0x28440f12, 0x42850a14, 0x20408109, 0x01111010, 0x28440408, 0x42850a14, 0x2040817f, 0x01111010,
			0xefc78204, 0x7efdfbf7, 0xe7cf8109, 0x011111f3, 0x2850a932, 0x42850a14, 0x2040a109, 0x01111010, 0x2850b840, 0x42850a14, 0xefdfbf79, 0x03bbbbf7,
			0x001fa020, 0x00000000, 0x00001000, 0x00000000, 0x00002070, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
			0x08022800, 0x00012283, 0x02430802, 0x01010001, 0x8404147c, 0x20000144, 0x80048404, 0x00823f08, 0xdfbf4284, 0x7e03f7ef, 0x142850a1, 0x0000210a,
			0x50a14684, 0x528a1428, 0x142850a1, 0x03efa17a, 0x50a14a9e, 0x52521428, 0x142850a1, 0x02081f4a, 0x50a15284, 0x4a221428, 0xf42850a1, 0x03efa14b,
			0x50a16284, 0x4a521428, 0x042850a1, 0x0228a17a, 0xdfbf427c, 0x7e8bf7ef, 0xf7efdfbf, 0x03efbd0b, 0x00000000, 0x04000000, 0x00000000, 0x00000008,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00200508, 0x00840400, 0x11458122, 0x00014210,
			0x00514294, 0x51420800, 0x20a22a94, 0x0050a508, 0x00200000, 0x00000000, 0x00050000, 0x08000000, 0xfefbefbe, 0xfbefbefb, 0xfbeb9114, 0x00fbefbe,
			0x20820820, 0x8a28a20a, 0x8a289114, 0x3e8a28a2, 0xfefbefbe, 0xfbefbe0b, 0x8a289114, 0x008a28a2, 0x228a28a2, 0x08208208, 0x8a289114, 0x088a28a2,
			0xfefbefbe, 0xfbefbefb, 0xfa2f9114, 0x00fbefbe, 0x00000000, 0x00000040, 0x00000000, 0x00000000, 0x00000000, 0x00000020, 0x00000000, 0x00000000,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00210100, 0x00000004, 0x00000000, 0x00000000, 0x14508200, 0x00001402, 0x00000000, 0x00000000,
			0x00000010, 0x00000020, 0x00000000, 0x00000000, 0xa28a28be, 0x00002228, 0x00000000, 0x00000000, 0xa28a28aa, 0x000022e8, 0x00000000, 0x00000000,
			0xa28a28aa, 0x000022a8, 0x00000000, 0x00000000, 0xa28a28aa, 0x000022e8, 0x00000000, 0x00000000, 0xbefbefbe, 0x00003e2f, 0x00000000, 0x00000000,
			0x00000004, 0x00002028, 0x00000000, 0x00000000, 0x80000000, 0x00003e0f, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
			0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
			0x00000000, 0x00000000, 0x00000000, 0x00000000
		};

		private const int FONT_TEXTURE_SIZE = 128;
		private const int FONT_CHAR_HEIGHT = 10;
		private const int FONT_TOTAL_CHARS = 224;
		private const int CHARS_DIVISOR = 1; // Padding between characters
		
		// Character widths for each of the 224 characters
		private static readonly int[] CharWidths = { 
			3, 1, 4, 6, 5, 7, 6, 2, 3, 3, 5, 5, 2, 4, 1, 7, 5, 2, 5, 5, 5, 5, 5, 5, 5, 5, 1, 1, 3, 4, 3, 6,
			7, 6, 6, 6, 6, 6, 6, 6, 6, 3, 5, 6, 5, 7, 6, 6, 6, 6, 6, 6, 7, 6, 7, 7, 6, 6, 6, 2, 7, 2, 3, 5,
			2, 5, 5, 5, 5, 5, 4, 5, 5, 1, 2, 5, 2, 5, 5, 5, 5, 5, 5, 5, 4, 5, 5, 5, 5, 5, 5, 3, 1, 3, 4, 4,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 5, 5, 5, 7, 1, 5, 3, 7, 3, 5, 4, 1, 7, 4, 3, 5, 3, 3, 2, 5, 6, 1, 2, 2, 3, 5, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 7, 6, 6, 6, 6, 6, 3, 3, 3, 3, 7, 6, 6, 6, 6, 6, 6, 5, 6, 6, 6, 6, 6, 6, 4, 6,
			5, 5, 5, 5, 5, 5, 9, 5, 5, 5, 5, 5, 2, 2, 3, 3, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 3, 5 
		};

		/// <summary>
		/// Creates a texture from the default font bitmap data using Raylib's exact format.
		/// </summary>
		public static unsafe uint CreateDefaultFontTexture(GL gl)
		{
			// Create 128x128 texture as per original Raylib
			const int textureSize = FONT_TEXTURE_SIZE;
			
			// Create RGBA pixels array
			byte[] pixels = new byte[textureSize * textureSize * 4];
			
			// Initialize all pixels to white with transparent alpha
			for (int i = 0; i < pixels.Length; i += 4)
			{
				pixels[i] = 255;     // R
				pixels[i + 1] = 255; // G  
				pixels[i + 2] = 255; // B
				pixels[i + 3] = 0;   // A (transparent)
			}
			
			// Parse the actual Raylib bitmap font data
			// Each bit represents one pixel, processed linearly through the 128x128 texture
			for (int i = 0, counter = 0; i < textureSize * textureSize && counter < DefaultFontData.Length; i += 32, counter++)
			{
				uint data = DefaultFontData[counter];
				
				for (int j = 0; j < 32; j++)
				{
					int pixelIndex = i + j;
					if (pixelIndex >= textureSize * textureSize) break;
					
					// Check if bit is set (LSB first for correct bit order)
					bool bitSet = (data & (1u << j)) != 0;
					
					// Convert linear index to x,y coordinates
					int x = pixelIndex % textureSize;
					int y = pixelIndex / textureSize;
					
					int arrayIndex = (y * textureSize + x) * 4;
					if (arrayIndex + 3 < pixels.Length)
					{
						pixels[arrayIndex + 3] = bitSet ? (byte)255 : (byte)0; // A
					}
				}
			}

			// Create OpenGL texture
			uint textureId = gl.GenTexture();
			gl.BindTexture(TextureTarget.Texture2D, textureId);
			
			fixed (byte* pixelPtr = pixels)
			{
				gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
					(uint)textureSize, (uint)textureSize, 0, 
					PixelFormat.Rgba, PixelType.UnsignedByte, pixelPtr);
			}
			
			// Set texture parameters
			gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
			gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
			gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
			gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
			
			gl.BindTexture(TextureTarget.Texture2D, 0);
			
			return textureId;
		}

		/// <summary>
		/// Gets the texture coordinates for a specific character using Raylib's layout algorithm.
		/// </summary>
		public static (float u1, float v1, float u2, float v2) GetCharTextureCoords(char c)
		{
			int charIndex = (int)c - 32; // ASCII space is at index 0
			if (charIndex < 0 || charIndex >= FONT_TOTAL_CHARS) charIndex = 0;
			
			int currentLine = 0;
			int currentPosX = CHARS_DIVISOR;
			int testPosX = CHARS_DIVISOR;
			
			float charX = 0, charY = 0, charWidth = 0;
			
			for (int i = 0; i <= charIndex; i++)
			{
				int width = CharWidths[i] + CHARS_DIVISOR;
				
				charX = currentPosX;
				charY = CHARS_DIVISOR + currentLine * (FONT_CHAR_HEIGHT + CHARS_DIVISOR);
				charWidth = width;
				
				testPosX += width;
				
				if (testPosX >= FONT_TEXTURE_SIZE)
				{
					currentLine++;
					currentPosX = CHARS_DIVISOR + width;
					testPosX = currentPosX;
					
					charX = CHARS_DIVISOR;
					charY = CHARS_DIVISOR + currentLine * (FONT_CHAR_HEIGHT + CHARS_DIVISOR);
				}
				else
				{
					currentPosX = testPosX;
				}
			}
			
			return (charX / (float)FONT_TEXTURE_SIZE, 
					charY / (float)FONT_TEXTURE_SIZE,
					(charX + charWidth) / (float)FONT_TEXTURE_SIZE,
					(charY + FONT_CHAR_HEIGHT) / (float)FONT_TEXTURE_SIZE);
		}

		/// <summary>
		/// Gets the character dimensions for the default font.
		/// </summary>
		public static (int width, int height) GetCharSize()
		{
			return (8, FONT_CHAR_HEIGHT);
		}
		
		/// <summary>
		/// Gets the actual width of a specific character from the character widths array.
		/// </summary>
		public static int GetCharWidth(char c)
		{
			int charIndex = (int)c - 32; // ASCII space is at index 0
			if (charIndex < 0 || charIndex >= FONT_TOTAL_CHARS) return CharWidths[0] + 1; // Return space width for unknown chars
			return CharWidths[charIndex] + 1;
		}
	}

	/// <summary>
	/// OpenGL texture manager for FontStashSharp integration.
	/// Manages font texture atlases and their OpenGL resources.
	/// </summary>
	public unsafe class SilkRayTexture2DManager(GL gl) : ITexture2DManager
	{
		private readonly GL _gl = gl ?? throw new ArgumentNullException(nameof(gl));
		private readonly Dictionary<object, FontTexture> _textures = new();

		public object CreateTexture(int width, int height)
		{
			uint textureId = _gl.GenTexture();
			_gl.BindTexture(TextureTarget.Texture2D, textureId);

			// Create empty texture with RGBA format
			_gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, 
				(uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

			// Set texture parameters for font rendering
			_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
			_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
			_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
			_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

			_gl.BindTexture(TextureTarget.Texture2D, 0);

			FontTexture texture = new(textureId, width, height);
			_textures[texture] = texture;
			return texture;
		}

		public System.Drawing.Point GetTextureSize(object texture)
		{
			if (_textures.TryGetValue(texture, out FontTexture? fontTexture))
			{
				return new System.Drawing.Point(fontTexture.Width, fontTexture.Height);
			}
			return new System.Drawing.Point(0, 0);
		}

		public void SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
		{
			if (_textures.TryGetValue(texture, out FontTexture? fontTexture))
			{
				_gl.BindTexture(TextureTarget.Texture2D, fontTexture.Id);

				fixed (byte* ptr = data)
				{
					_gl.TexSubImage2D(TextureTarget.Texture2D, 0, 
						bounds.X, bounds.Y, (uint)bounds.Width, (uint)bounds.Height,
						PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
				}

				_gl.BindTexture(TextureTarget.Texture2D, 0);
			}
		}

		public void Dispose()
		{
			foreach (var texture in _textures.Values)
			{
				_gl.DeleteTexture(texture.Id);
			}
			_textures.Clear();
		}

		public FontTexture? GetFontTexture(object texture)
		{
			_textures.TryGetValue(texture, out FontTexture? fontTexture);
			return fontTexture;
		}
	}

	/// <summary>
	/// Represents a font texture with OpenGL texture ID and dimensions.
	/// </summary>
	public class FontTexture(uint id, int width, int height)
	{
		public uint Id { get; } = id;
		public int Width { get; } = width;
		public int Height { get; } = height;
	}

	/// <summary>
	/// Custom renderer for FontStashSharp that integrates with SilkRay's OpenGL rendering pipeline.
	/// Handles both individual glyph drawing and quad-based rendering.
	/// </summary>
	public class SilkRayFontStashRenderer(Renderer renderer, SilkRayTexture2DManager textureManager) : IFontStashRenderer
	{
		private readonly Renderer _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
		private readonly SilkRayTexture2DManager _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));

		public ITexture2DManager TextureManager => _textureManager;

		public void Draw(object texture, System.Numerics.Vector2 position, System.Drawing.Rectangle? sourceRectangle, 
						FontStashSharp.FSColor color, float rotation, System.Numerics.Vector2 origin, float scale)
		{
			var fontTexture = _textureManager.GetFontTexture(texture);
			if (fontTexture == null) return;

			Texture2D texture2D = new(fontTexture.Id, fontTexture.Width, fontTexture.Height);

			// Convert parameters to SilkRay types
			Vector2 silkRayPosition = new(position.X, position.Y);
			Vector2 silkRayOrigin = new(origin.X, origin.Y);
			Color silkRayColor = new(color.R, color.G, color.B, color.A);

			Rectangle sourceRect;
			if (sourceRectangle.HasValue)
			{
				var src = sourceRectangle.Value;
				sourceRect = new(src.X, src.Y, src.Width, src.Height);
			}
			else
			{
				sourceRect = new(0, 0, fontTexture.Width, fontTexture.Height);
			}

			// Calculate destination rectangle with scale (ensure minimum scale of 1.0)
			float actualScale = scale <= 0 ? 1.0f : scale;
			Rectangle destRect = new(
				(int)silkRayPosition.X, (int)silkRayPosition.Y,
				(int)(sourceRect.Width * actualScale), (int)(sourceRect.Height * actualScale)
			);

			// Draw using existing texture rendering system
			_renderer.DrawTexture(texture2D, sourceRect, destRect, silkRayOrigin, rotation, silkRayColor);
		}

		public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, 
						   ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
		{
			var fontTexture = _textureManager.GetFontTexture(texture);
			if (fontTexture == null) return;

			var texture2D = new Texture2D(fontTexture.Id, fontTexture.Width, fontTexture.Height);

			// Extract quad bounds
			var minX = Math.Min(Math.Min(topLeft.Position.X, topRight.Position.X), 
							   Math.Min(bottomLeft.Position.X, bottomRight.Position.X));
			var minY = Math.Min(Math.Min(topLeft.Position.Y, topRight.Position.Y), 
							   Math.Min(bottomLeft.Position.Y, bottomRight.Position.Y));
			var maxX = Math.Max(Math.Max(topLeft.Position.X, topRight.Position.X), 
							   Math.Max(bottomLeft.Position.X, bottomRight.Position.X));
			var maxY = Math.Max(Math.Max(topLeft.Position.Y, topRight.Position.Y), 
							   Math.Max(bottomLeft.Position.Y, bottomRight.Position.Y));

			// Create source rectangle from texture coordinates
			Rectangle sourceRect = new(
				(int)(topLeft.TextureCoordinate.X * fontTexture.Width),
				(int)(topLeft.TextureCoordinate.Y * fontTexture.Height),
				(int)((topRight.TextureCoordinate.X - topLeft.TextureCoordinate.X) * fontTexture.Width),
				(int)((bottomLeft.TextureCoordinate.Y - topLeft.TextureCoordinate.Y) * fontTexture.Height)
			);

			// Create destination rectangle
			Rectangle destRect = new((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));

			// Extract color from vertex
			Color color = new(topLeft.Color.R, topLeft.Color.G, topLeft.Color.B, topLeft.Color.A);

			// Draw using existing texture rendering system
			_renderer.DrawTexture(texture2D, sourceRect, destRect, new Vector2(0, 0), 0.0f, color);
		}
	}

	/// <summary>
	/// High-level font renderer that provides TTF font rendering capabilities using FontStashSharp.
	/// Integrates with SilkRay's rendering system to display actual font glyphs.
	/// </summary>
	public unsafe class FontRenderer(GL gl, Renderer renderer) : IDisposable
	{
		private readonly GL _gl = gl ?? throw new ArgumentNullException(nameof(gl));
		private readonly Renderer _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
		private readonly SilkRayTexture2DManager _textureManager = new(gl);
		private readonly SilkRayFontStashRenderer _fontRenderer = new(renderer, new SilkRayTexture2DManager(gl));
		private bool _disposed;

		public void DrawText(DynamicSpriteFont spriteFont, string text, Vector2 position, Color color)
		{
			if (_disposed || spriteFont == null || string.IsNullOrEmpty(text))
				return;

			try
			{
				// Convert SilkRay color to FontStashSharp color
				FontStashSharp.FSColor fontColor = new(color.R, color.G, color.B, color.A);
				
				// Convert SilkRay Vector2 to System.Numerics.Vector2
				System.Numerics.Vector2 sysPosition = new(position.X, position.Y);
				
				// Use FontStashSharp's DrawText method with our custom renderer
				spriteFont.DrawText(_fontRenderer, text, sysPosition, fontColor, 0f, 
					System.Numerics.Vector2.Zero, System.Numerics.Vector2.One, 0f);
			}
			catch (Exception)
			{
				// Fallback to placeholder rendering if FontStashSharp fails
				DrawPlaceholderText(text, position, color, (int)spriteFont.FontSize);
			}
		}

		private void DrawPlaceholderText(string text, Vector2 position, Color color, int fontSize)
		{
			float currentX = position.X;
			float currentY = position.Y;
			float charWidth = fontSize * 0.6f;
			float charHeight = fontSize;

			foreach (char c in text)
			{
				if (c == '\n')
				{
					currentX = position.X;
					currentY += charHeight;
					continue;
				}

				if (c == ' ')
				{
					currentX += charWidth * 0.5f;
					continue;
				}

				// Use different colors to distinguish characters
				Color charColor = new(
					(byte)Math.Min(255, color.R + (c % 30)),
					(byte)Math.Min(255, color.G + (c % 20)), 
					(byte)Math.Min(255, color.B + (c % 25)),
					color.A
				);

				_renderer.DrawRectangle((int)currentX, (int)currentY, 
									  (int)charWidth, (int)charHeight, charColor);
				currentX += charWidth;
			}
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_textureManager?.Dispose();
				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>
	/// Text and Font functions implementation using Silk.NET and FontStashSharp
	/// </summary>
	public static class Text
	{
		// Font system initialization
		internal static void InitializeFontSystem()
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
}
