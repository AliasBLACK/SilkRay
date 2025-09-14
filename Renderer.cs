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
        private uint _vao;
        private uint _vbo;
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

                // Set up vertex array and buffer objects
                _vao = _gl.GenVertexArray();
                _vbo = _gl.GenBuffer();
                
                _gl.BindVertexArray(_vao);
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
                
                // Set up vertex attribute pointer
                _gl.EnableVertexAttribArray(0);
                _gl.VertexAttribPointer(0, 2, GLEnum.Float, false, 2 * sizeof(float), (void*)0);
                
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
                _shader?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
