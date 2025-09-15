using Silk.NET.OpenGL;
using FontStashSharp;
using FontStashSharp.Interfaces;

namespace SilkRay
{
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
}
