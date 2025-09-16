using Silk.NET.OpenGL;
using StbImageSharp;
using System.Numerics;

namespace SilkRay
{
	/// <summary>
	/// Texture-related functions implementation using Silk.NET
	/// </summary>
	public static class Textures
	{
		// Texture functions
		public static Texture2D LoadTexture(string fileName)
		{
			try
			{
				if (RaylibInternal.GL == null)
					throw new InvalidOperationException("OpenGL context not initialized");

				// Load image data using StbImageSharp
				byte[] fileData = File.ReadAllBytes(fileName);
				ImageResult image = ImageResult.FromMemory(fileData, ColorComponents.RedGreenBlueAlpha);
				
				int width = image.Width;
				int height = image.Height;
				byte[] imageData = image.Data;

				// Generate OpenGL texture
				uint textureId = RaylibInternal.GL.GenTexture();
				RaylibInternal.GL.BindTexture(TextureTarget.Texture2D, textureId);

				// Upload texture data
				unsafe
				{
					fixed (byte* ptr = imageData)
					{
						RaylibInternal.GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
							(uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
					}
				}

				// Generate mipmaps for advanced filtering support
				RaylibInternal.GL.GenerateMipmap(TextureTarget.Texture2D);

				// Set default texture parameters
				RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
				RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
				RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
				RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

				RaylibInternal.GL.BindTexture(TextureTarget.Texture2D, 0);

				// Calculate mipmap levels
				int mipmapLevels = (int)Math.Floor(Math.Log2(Math.Max(width, height))) + 1;
				return new Texture2D(textureId, width, height, mipmapLevels, 1);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading texture '{fileName}': {ex.Message}");
				return new Texture2D(0, 0, 0, 0, 0);
			}
		}

		public static void UnloadTexture(Texture2D texture)
		{
			if (RaylibInternal.GL != null && texture.Id != 0)
			{
				RaylibInternal.GL.DeleteTexture(texture.Id);
			}
		}

		public static bool IsTextureValid(Texture2D texture)
		{
			if (RaylibInternal.GL == null || texture.Id == 0)
				return false;

			// Check if the texture ID is valid in OpenGL
			return RaylibInternal.GL.IsTexture(texture.Id);
		}

		public static void UpdateTexture(Texture2D texture, byte[] pixels)
		{
			if (RaylibInternal.GL == null || texture.Id == 0 || pixels == null)
				return;

			try
			{
				// Bind the texture
				RaylibInternal.GL.BindTexture(TextureTarget.Texture2D, texture.Id);

				// Update texture data
				unsafe
				{
					fixed (byte* ptr = pixels)
					{
						RaylibInternal.GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0,
							(uint)texture.Width, (uint)texture.Height, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
					}
				}

				// Unbind texture
				RaylibInternal.GL.BindTexture(TextureTarget.Texture2D, 0);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error updating texture: {ex.Message}");
			}
		}

		public static void DrawTexture(Texture2D texture, int posX, int posY, Color tint)
		{
			DrawTextureEx(texture, new Vector2(posX, posY), 0.0f, 1.0f, tint);
		}

		public static void DrawTextureV(Texture2D texture, Vector2 position, Color tint)
		{
			DrawTextureEx(texture, position, 0.0f, 1.0f, tint);
		}

		public static void DrawTextureEx(Texture2D texture, Vector2 position, float rotation, float scale, Color tint)
		{
			if (RaylibInternal.Renderer == null)
			{
				Console.WriteLine("Warning: Renderer not initialized for DrawTextureEx");
				return;
			}
			
			if (texture.Id == 0)
			{
				Console.WriteLine("Warning: Invalid texture ID (0) in DrawTextureEx");
				return;
			}
			
			if (!IsTextureValid(texture))
			{
				Console.WriteLine($"Warning: Texture ID {texture.Id} is not valid in OpenGL context");
				return;
			}

			try
			{
				// Calculate texture rectangle
				Rectangle sourceRec = new(0, 0, texture.Width, texture.Height);
				Rectangle destRec = new(position.X, position.Y, texture.Width * scale, texture.Height * scale);
				Vector2 origin = new(0, 0);

				DrawTexturePro(texture, sourceRec, destRec, origin, rotation, tint);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in DrawTextureEx: {ex.Message}");
			}
		}

		public static void DrawTextureRec(Texture2D texture, Rectangle source, Vector2 position, Color tint)
		{
			Rectangle dest = new(position.X, position.Y, source.Width, source.Height);
			DrawTexturePro(texture, source, dest, new Vector2(0, 0), 0.0f, tint);
		}

		public static void DrawTexturePro(Texture2D texture, Rectangle source, Rectangle dest, Vector2 origin, float rotation, Color tint)
		{
			if (RaylibInternal.Renderer == null)
			{
				Console.WriteLine("Warning: Renderer not initialized for DrawTexturePro");
				return;
			}
			
			if (texture.Id == 0)
			{
				Console.WriteLine("Warning: Invalid texture ID (0) in DrawTexturePro");
				return;
			}
			
			if (!IsTextureValid(texture))
			{
				Console.WriteLine($"Warning: Texture ID {texture.Id} is not valid in OpenGL context");
				return;
			}

			try
			{
				RaylibInternal.Renderer.DrawTexture(texture, source, dest, origin, rotation, tint);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in DrawTexturePro: {ex.Message}");
			}
		}

		public static void DrawTextureNPatch(Texture2D texture, NPatchInfo nPatchInfo, Rectangle dest, Vector2 origin, float rotation, Color tint)
		{
			if (RaylibInternal.Renderer == null)
			{
				Console.WriteLine("Warning: Renderer not initialized for DrawTextureNPatch");
				return;
			}
			
			if (texture.Id == 0)
			{
				Console.WriteLine("Warning: Invalid texture ID (0) in DrawTextureNPatch");
				return;
			}
			
			if (!IsTextureValid(texture))
			{
				Console.WriteLine($"Warning: Texture ID {texture.Id} is not valid in OpenGL context");
				return;
			}

			try
			{

			// Full 9-patch implementation
			Rectangle src = nPatchInfo.source;
			
			// Calculate source patch dimensions
			float srcWidth = src.Width;
			float srcHeight = src.Height;
			
			// Calculate destination dimensions
			float destWidth = dest.Width;
			float destHeight = dest.Height;
			
			// Define the 9 sections of the source texture
			Rectangle[] srcPatches = new Rectangle[9];
			Rectangle[] destPatches = new Rectangle[9];
			
			// Source patches (texture coordinates)
			// Top row
			srcPatches[0] = new(src.X, src.Y, nPatchInfo.left, nPatchInfo.top); // Top-left corner
			srcPatches[1] = new(src.X + nPatchInfo.left, src.Y, srcWidth - nPatchInfo.left - nPatchInfo.right, nPatchInfo.top); // Top edge
			srcPatches[2] = new(src.X + srcWidth - nPatchInfo.right, src.Y, nPatchInfo.right, nPatchInfo.top); // Top-right corner
			
			// Middle row
			srcPatches[3] = new(src.X, src.Y + nPatchInfo.top, nPatchInfo.left, srcHeight - nPatchInfo.top - nPatchInfo.bottom); // Left edge
			srcPatches[4] = new(src.X + nPatchInfo.left, src.Y + nPatchInfo.top, srcWidth - nPatchInfo.left - nPatchInfo.right, srcHeight - nPatchInfo.top - nPatchInfo.bottom); // Center
			srcPatches[5] = new(src.X + srcWidth - nPatchInfo.right, src.Y + nPatchInfo.top, nPatchInfo.right, srcHeight - nPatchInfo.top - nPatchInfo.bottom); // Right edge
			
			// Bottom row
			srcPatches[6] = new(src.X, src.Y + srcHeight - nPatchInfo.bottom, nPatchInfo.left, nPatchInfo.bottom); // Bottom-left corner
			srcPatches[7] = new(src.X + nPatchInfo.left, src.Y + srcHeight - nPatchInfo.bottom, srcWidth - nPatchInfo.left - nPatchInfo.right, nPatchInfo.bottom); // Bottom edge
			srcPatches[8] = new(src.X + srcWidth - nPatchInfo.right, src.Y + srcHeight - nPatchInfo.bottom, nPatchInfo.right, nPatchInfo.bottom); // Bottom-right corner
			
			// Destination patches (screen coordinates)
			// Top row
			destPatches[0] = new(dest.X, dest.Y, nPatchInfo.left, nPatchInfo.top); // Top-left corner
			destPatches[1] = new(dest.X + nPatchInfo.left, dest.Y, destWidth - nPatchInfo.left - nPatchInfo.right, nPatchInfo.top); // Top edge (stretched horizontally)
			destPatches[2] = new(dest.X + destWidth - nPatchInfo.right, dest.Y, nPatchInfo.right, nPatchInfo.top); // Top-right corner
			
			// Middle row
			destPatches[3] = new(dest.X, dest.Y + nPatchInfo.top, nPatchInfo.left, destHeight - nPatchInfo.top - nPatchInfo.bottom); // Left edge (stretched vertically)
			destPatches[4] = new(dest.X + nPatchInfo.left, dest.Y + nPatchInfo.top, destWidth - nPatchInfo.left - nPatchInfo.right, destHeight - nPatchInfo.top - nPatchInfo.bottom); // Center (stretched both ways)
			destPatches[5] = new(dest.X + destWidth - nPatchInfo.right, dest.Y + nPatchInfo.top, nPatchInfo.right, destHeight - nPatchInfo.top - nPatchInfo.bottom); // Right edge (stretched vertically)
			
			// Bottom row
			destPatches[6] = new(dest.X, dest.Y + destHeight - nPatchInfo.bottom, nPatchInfo.left, nPatchInfo.bottom); // Bottom-left corner
			destPatches[7] = new(dest.X + nPatchInfo.left, dest.Y + destHeight - nPatchInfo.bottom, destWidth - nPatchInfo.left - nPatchInfo.right, nPatchInfo.bottom); // Bottom edge (stretched horizontally)
			destPatches[8] = new(dest.X + destWidth - nPatchInfo.right, dest.Y + destHeight - nPatchInfo.bottom, nPatchInfo.right, nPatchInfo.bottom); // Bottom-right corner
			
			// Draw all 9 patches
			for (int i = 0; i < 9; i++)
			{
				// Skip patches with zero or negative dimensions
				if (srcPatches[i].Width <= 0 || srcPatches[i].Height <= 0 || 
					destPatches[i].Width <= 0 || destPatches[i].Height <= 0)
					continue;
					
				DrawTexturePro(texture, srcPatches[i], destPatches[i], Vector2.Zero, rotation, tint);
			}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in DrawTextureNPatch: {ex.Message}");
			}
		}

		// Helper function to check anisotropic filtering support
		private static bool IsAnisotropicFilteringSupported()
		{
			try
			{
				if (RaylibInternal.GL == null) return false;
				
				// Check if the EXT_texture_filter_anisotropic extension is supported
				unsafe
				{
					byte* extensionsPtr = RaylibInternal.GL.GetString(StringName.Extensions);
					if (extensionsPtr == null) return false;
					
					string? extensions = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)extensionsPtr);
					return extensions != null && extensions.Contains("GL_EXT_texture_filter_anisotropic");
				}
			}
			catch
			{
				return false;
			}
		}

		// Texture filtering and mipmap functions
		public static void SetTextureFilter(Texture2D texture, int filter)
		{
			if (RaylibInternal.GL == null || texture.Id == 0)
			{
				Console.WriteLine("Warning: Cannot set texture filter - invalid texture or GL context");
				return;
			}

			try
			{
				RaylibInternal.GL.BindTexture(TextureTarget.Texture2D, texture.Id);
				
				switch (filter)
				{
					case TEXTURE_FILTER_POINT:
						RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
						RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
						break;
					case TEXTURE_FILTER_BILINEAR:
						RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
						RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
						break;
					case TEXTURE_FILTER_TRILINEAR:
						RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
						RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
						break;
					case TEXTURE_FILTER_ANISOTROPIC_4X:
					case TEXTURE_FILTER_ANISOTROPIC_8X:
					case TEXTURE_FILTER_ANISOTROPIC_16X:
						// Check if anisotropic filtering is supported
						if (IsAnisotropicFilteringSupported())
						{
							RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
							RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
							
							float anisotropy = filter switch
							{
								TEXTURE_FILTER_ANISOTROPIC_4X => 4.0f,
								TEXTURE_FILTER_ANISOTROPIC_8X => 8.0f,
								TEXTURE_FILTER_ANISOTROPIC_16X => 16.0f,
								_ => 4.0f
							};
							
							RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxAnisotropy, anisotropy);
						}
						else
						{
							Console.WriteLine($"Warning: Anisotropic filtering not supported, falling back to trilinear");
							// Fallback to trilinear filtering
							RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
							RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
						}
						break;
				}
				
				RaylibInternal.GL.BindTexture(TextureTarget.Texture2D, 0);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error setting texture filter: {ex.Message}");
			}
		}

		public static void SetTextureWrap(Texture2D texture, int wrap)
		{
			if (RaylibInternal.GL == null || texture.Id == 0)
			{
				Console.WriteLine("Warning: Cannot set texture wrap - invalid texture or GL context");
				return;
			}

			try
			{
				RaylibInternal.GL.BindTexture(TextureTarget.Texture2D, texture.Id);
				
				int wrapMode = wrap switch
				{
					TEXTURE_WRAP_REPEAT => (int)GLEnum.Repeat,
					TEXTURE_WRAP_CLAMP => (int)GLEnum.ClampToEdge,
					TEXTURE_WRAP_MIRROR_REPEAT => (int)GLEnum.MirroredRepeat,
					TEXTURE_WRAP_MIRROR_CLAMP => (int)GLEnum.MirrorClampToEdge,
					_ => (int)GLEnum.Repeat
				};
				
				RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wrapMode);
				RaylibInternal.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wrapMode);
				
				RaylibInternal.GL.BindTexture(TextureTarget.Texture2D, 0);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error setting texture wrap: {ex.Message}");
			}
		}

		public static void GenTextureMipmaps(Texture2D texture)
		{
			if (RaylibInternal.GL == null || texture.Id == 0)
			{
				Console.WriteLine("Warning: Cannot generate mipmaps - invalid texture or GL context");
				return;
			}

			try
			{
				RaylibInternal.GL.BindTexture(TextureTarget.Texture2D, texture.Id);
				RaylibInternal.GL.GenerateMipmap(TextureTarget.Texture2D);
				RaylibInternal.GL.BindTexture(TextureTarget.Texture2D, 0);
				
				Console.WriteLine($"Generated mipmaps for texture ID {texture.Id}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error generating mipmaps: {ex.Message}");
			}
		}
	}
}
