using Silk.NET.OpenGL;
using FontStashSharp;

namespace SilkRay
{
	/// <summary>
	/// OpenGL renderer for 2D graphics operations
	/// </summary>
	public unsafe class Renderer : IDisposable
	{
		private readonly GL _gl;
		private Shader? _shader;
		private Shader? _textureShader;
		private uint _vao;
		private uint _vbo;
		private uint _textureVao;
		private uint _textureVbo;
		private Vector2 _screenSize;
		private bool _disposed;
		private FontRenderer? _fontRenderer;

		public Renderer(GL gl, int width, int height)
		{
			_gl = gl ?? throw new ArgumentNullException(nameof(gl));
			_screenSize = new(width, height);
			Initialize();
		}

		private void Initialize()
		{
			try
			{
				// Create shader for 2D rendering
				const string vertexShader = @"#version 330 core
					layout (location = 0) in vec2 aPosition;
					uniform vec2 screenSize;
					void main()
					{
						// Convert from pixel coordinates to NDC (-1 to 1)
						vec2 pos = aPosition / screenSize;
						gl_Position = vec4(pos.x * 2.0 - 1.0, 1.0 - pos.y * 2.0, 0.0, 1.0);
					}";
					
				const string fragmentShader = @"#version 330 core
					out vec4 FragColor;
					uniform vec4 color;
					void main()
					{
						FragColor = color;
					}";

				_shader = new(_gl, vertexShader, fragmentShader);

				// Create texture shader
				const string textureVertexShader = @"#version 330 core
					layout (location = 0) in vec2 aPosition;
					layout (location = 1) in vec2 aTexCoord;
					uniform vec2 screenSize;
					out vec2 TexCoord;
					void main()
					{
						vec2 pos = aPosition / screenSize;
						gl_Position = vec4(pos.x * 2.0 - 1.0, 1.0 - pos.y * 2.0, 0.0, 1.0);
						TexCoord = aTexCoord;
					}";
					
				const string textureFragmentShader = @"#version 330 core
					out vec4 FragColor;
					in vec2 TexCoord;
					uniform sampler2D ourTexture;
					uniform vec4 tintColor;
					void main()
					{
						FragColor = texture(ourTexture, TexCoord) * tintColor;
					}";

				_textureShader = new(_gl, textureVertexShader, textureFragmentShader);

				// Set up vertex array and buffer objects for shapes
				_vao = _gl.GenVertexArray();
				_vbo = _gl.GenBuffer();
				
				_gl.BindVertexArray(_vao);
				_gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
				
				// Set up vertex attribute pointer
				_gl.EnableVertexAttribArray(0);
				_gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), (void*)0);
				
				// Set up texture vertex array and buffer objects
				_gl.GenVertexArrays(1, out _textureVao);
				_gl.GenBuffers(1, out _textureVbo);
				
				_gl.BindVertexArray(_textureVao);
				_gl.BindBuffer(BufferTargetARB.ArrayBuffer, _textureVbo);
				
				// Position (2 floats) + TexCoord (2 floats) = 4 floats per vertex
				_gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
				_gl.EnableVertexAttribArray(0);
				_gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
				_gl.EnableVertexAttribArray(1);
				
				_gl.BindVertexArray(0);

				// Initialize font renderer
				_fontRenderer = new(_gl, this);
				
				_gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
				
				// Enable blending for transparency
				_gl.Enable(EnableCap.Blend);
				_gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				_gl.Disable(EnableCap.DepthTest);
			}
			catch (Exception ex)
			{
				Dispose();
				throw new InvalidOperationException("Failed to initialize renderer", ex);
			}
		}

		public void BeginDrawing()
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
				
			// Don't clear here - let ClearBackground handle it
		}

		public void EndDrawing()
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
				
			_gl.Flush();
		}

		public void ClearBackground(Color color)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
				
			var c = color.ToVector4();
			_gl.ClearColor(c.X, c.Y, c.Z, c.W);
			_gl.Clear(ClearBufferMask.ColorBufferBit);
		}

		public void DrawRectangle(float x, float y, float width, float height, Color color)
		{
			if (_disposed || _shader == null)
				return;

			// Define rectangle vertices
			float[] vertices = {
				x, y + height,          // Bottom-left
				x, y,                   // Top-left
				x + width, y + height,  // Bottom-right
				x + width, y            // Top-right
			};

			// Bind shader and set uniforms
			_shader.Use();
			_shader.SetUniform("screenSize", _screenSize);
			_shader.SetUniform("color", color);

			// Upload vertex data and draw
			_gl.BindVertexArray(_vao);
			_gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
			
			fixed (float* v = &vertices[0])
			{
				_gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
			}

			_gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
			
			_gl.BindVertexArray(0);
		}

		public void DrawCircle(float centerX, float centerY, float radius, Color color)
		{
			DrawCircleLines(centerX, centerY, radius, color, true);
		}

		public void DrawCircleLines(float centerX, float centerY, float radius, Color color, bool filled = false)
		{
			if (_disposed || _shader == null)
				return;

			const int segments = 32;
			var vertices = new float[(segments + 1 + (filled ? 1 : 0)) * 2];
			
			int index = 0;
			if (filled)
			{
				// Center point for filled circle
				vertices[index++] = centerX;
				vertices[index++] = centerY;
			}

			// Generate circle vertices
			for (int i = 0; i <= segments; i++)
			{
				float angle = (float)(2.0 * Math.PI * i / segments);
				vertices[index++] = centerX + radius * (float)Math.Cos(angle);
				vertices[index++] = centerY + radius * (float)Math.Sin(angle);
			}

			// Bind shader and set uniforms
			_shader.Use();
			_shader.SetUniform("screenSize", _screenSize);
			_shader.SetUniform("color", color);

			// Upload vertex data and draw
			_gl.BindVertexArray(_vao);
			_gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
			
			fixed (float* v = &vertices[0])
			{
				_gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
			}

			if (filled)
				_gl.DrawArrays(PrimitiveType.TriangleFan, 0, segments + 1);
			else
				_gl.DrawArrays(PrimitiveType.LineLoop, 0, segments);
			
			_gl.BindVertexArray(0);
		}

		public void DrawLine(float startX, float startY, float endX, float endY, Color color)
		{
			if (_disposed || _shader == null)
				return;

			float[] vertices = {
				startX, startY,
				endX, endY
			};

			// Bind shader and set uniforms
			_shader.Use();
			_shader.SetUniform("screenSize", _screenSize);
			_shader.SetUniform("color", color);

			// Upload vertex data and draw
			_gl.BindVertexArray(_vao);
			_gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
			
			fixed (float* v = &vertices[0])
			{
				_gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
			}

			_gl.DrawArrays(PrimitiveType.Lines, 0, 2);
			
			_gl.BindVertexArray(0);
		}

		public void DrawThickLine(float startX, float startY, float endX, float endY, float thickness, Color color)
		{
			if (_disposed || _shader == null || thickness <= 0)
				return;

			// Calculate line direction and perpendicular vector
			float dx = endX - startX;
			float dy = endY - startY;
			float length = (float)Math.Sqrt(dx * dx + dy * dy);
			
			if (length == 0)
				return;

			// Normalize direction and calculate perpendicular
			dx /= length;
			dy /= length;
			
			float perpX = -dy * thickness * 0.5f;
			float perpY = dx * thickness * 0.5f;

			// Calculate rectangle vertices for thick line
			float[] vertices = {
				startX + perpX, startY + perpY,  // Top-left
				startX - perpX, startY - perpY,  // Bottom-left
				endX + perpX, endY + perpY,      // Top-right
				endX - perpX, endY - perpY       // Bottom-right
			};

			// Bind shader and set uniforms
			_shader.Use();
			_shader.SetUniform("screenSize", _screenSize);
			_shader.SetUniform("color", color);

			// Upload vertex data and draw as triangle strip
			_gl.BindVertexArray(_vao);
			_gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
			
			fixed (float* v = &vertices[0])
			{
				_gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
			}

			_gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
			
			_gl.BindVertexArray(0);
		}

		public void DrawTexture(Texture2D texture, Rectangle source, Rectangle dest, Vector2 origin, float rotation, Color tint)
		{
			if (_disposed || _textureShader == null || texture.Id == 0)
				return;

			// Calculate texture coordinates based on source rectangle
			float texLeft = source.X / texture.Width;
			float texTop = source.Y / texture.Height;
			float texRight = (source.X + source.Width) / texture.Width;
			float texBottom = (source.Y + source.Height) / texture.Height;

			// Convert rotation from degrees to radians
			float rotationRad = rotation * (float)(Math.PI / 180.0);
			float cos = (float)Math.Cos(rotationRad);
			float sin = (float)Math.Sin(rotationRad);

			// Define quad vertices relative to origin (before rotation)
			Vector2[] quadVertices = {
				new Vector2(-origin.X, dest.Height - origin.Y),                    // Bottom-left
				new Vector2(-origin.X, -origin.Y),                                 // Top-left
				new Vector2(dest.Width - origin.X, dest.Height - origin.Y),       // Bottom-right
				new Vector2(dest.Width - origin.X, -origin.Y)                     // Top-right
			};

			// Apply rotation and translation
			float[] vertices = new float[16]; // 4 vertices * (2 pos + 2 tex) = 16 floats
			float[] texCoords = { texLeft, texBottom, texLeft, texTop, texRight, texBottom, texRight, texTop };
			
			for (int i = 0; i < 4; i++)
			{
				// Rotate vertex around origin
				float rotatedX = quadVertices[i].X * cos - quadVertices[i].Y * sin;
				float rotatedY = quadVertices[i].X * sin + quadVertices[i].Y * cos;
				
				// Translate to final position
				vertices[i * 4] = dest.X + origin.X + rotatedX;
				vertices[i * 4 + 1] = dest.Y + origin.Y + rotatedY;
				vertices[i * 4 + 2] = texCoords[i * 2];
				vertices[i * 4 + 3] = texCoords[i * 2 + 1];
			}

			// Bind texture shader and set uniforms
			_textureShader.Use();
			_textureShader.SetUniform("screenSize", _screenSize);
			_textureShader.SetUniform("tintColor", tint);

			// Bind texture
			_gl.ActiveTexture(TextureUnit.Texture0);
			_gl.BindTexture(TextureTarget.Texture2D, texture.Id);
			_textureShader.SetUniform("ourTexture", 0);

			// Upload vertex data and draw
			_gl.BindVertexArray(_textureVao);
			_gl.BindBuffer(BufferTargetARB.ArrayBuffer, _textureVbo);
			
			fixed (float* v = &vertices[0])
			{
				_gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.DynamicDraw);
			}

			_gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
			
			_gl.BindVertexArray(0);
			_gl.BindTexture(TextureTarget.Texture2D, 0);
		}

		// Bitmap font text rendering using default Raylib font
		public void DrawBitmapText(string text, Vector2 position, Color color, float fontSize)
		{
			if (string.IsNullOrEmpty(text) || RaylibInternal.DefaultFontTexture == 0)
				return;

			var (_, charHeight) = DefaultFont.GetCharSize();
			float scale = fontSize / charHeight;
			
			float currentX = position.X;
			float currentY = position.Y;
			
			var fontTexture = new Texture2D(RaylibInternal.DefaultFontTexture, 128, 128);
			
			foreach (char c in text)
			{
				if (c == '\n')
				{
					currentX = position.X;
					currentY += charHeight * scale;
					continue;
				}
				
				int actualCharWidth = DefaultFont.GetCharWidth(c);
				var (u1, v1, u2, v2) = DefaultFont.GetCharTextureCoords(c);
				
				Rectangle sourceRect = new(u1 * 128, v1 * 128, (u2 - u1) * 128, (v2 - v1) * 128);
				Rectangle destRect = new(currentX, currentY, actualCharWidth * scale, charHeight * scale);
				
				DrawTexturePro(fontTexture, sourceRect, destRect, Vector2.Zero, 0.0f, color);
				
				currentX += actualCharWidth * scale + 1;
			}
		}

		// Text rendering with FontStashSharp
		public void DrawText(DynamicSpriteFont spriteFont, string text, Vector2 position, Color color, float fontSize, float spacing)
		{
			if (_disposed || spriteFont == null || string.IsNullOrEmpty(text) || _fontRenderer == null)
				return;

			try
			{
				_fontRenderer.DrawText(spriteFont, text, position, color);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in DrawText: {ex.Message}");
			}
		}

		public void Resize(int width, int height)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
				
			_screenSize = new(width, height);
			_gl.Viewport(0, 0, (uint)width, (uint)height);
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_gl.DeleteVertexArray(_vao);
				_gl.DeleteBuffer(_vbo);
				_gl.DeleteVertexArray(_textureVao);
				_gl.DeleteBuffer(_textureVbo);
				_shader?.Dispose();
				_textureShader?.Dispose();
				_fontRenderer?.Dispose();
				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}
	}
}
