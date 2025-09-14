using Silk.NET.OpenGL;
using System;
using System.Numerics;

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

        public Renderer(GL gl, int width, int height)
        {
            _gl = gl ?? throw new ArgumentNullException(nameof(gl));
            _screenSize = new Vector2(width, height);
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

                _shader = new Shader(_gl, vertexShader, fragmentShader);

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

                _textureShader = new Shader(_gl, textureVertexShader, textureFragmentShader);

                // Set up vertex array and buffer objects for shapes
                _vao = _gl.GenVertexArray();
                _vbo = _gl.GenBuffer();
                
                _gl.BindVertexArray(_vao);
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
                
                // Set up vertex attribute pointer
                _gl.EnableVertexAttribArray(0);
                _gl.VertexAttribPointer(0, 2, GLEnum.Float, false, 2 * sizeof(float), (void*)0);
                
                // Set up texture vertex array and buffer objects
                _textureVao = _gl.GenVertexArray();
                _textureVbo = _gl.GenBuffer();
                
                _gl.BindVertexArray(_textureVao);
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _textureVbo);
                
                // Set up vertex attribute pointers for texture rendering (position + texcoord)
                _gl.EnableVertexAttribArray(0);
                _gl.VertexAttribPointer(0, 2, GLEnum.Float, false, 4 * sizeof(float), (void*)0);
                _gl.EnableVertexAttribArray(1);
                _gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
                
                // Unbind
                _gl.BindVertexArray(0);
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

            // Define quad vertices with position and texture coordinates
            float[] vertices = {
                // Position (x, y), TexCoord (u, v)
                dest.X, dest.Y + dest.Height, texLeft, texBottom,  // Bottom-left
                dest.X, dest.Y, texLeft, texTop,                   // Top-left
                dest.X + dest.Width, dest.Y + dest.Height, texRight, texBottom, // Bottom-right
                dest.X + dest.Width, dest.Y, texRight, texTop      // Top-right
            };

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

        public void Resize(int width, int height)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
                
            _screenSize = new Vector2(width, height);
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
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
