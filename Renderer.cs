using Veldrid;
using Veldrid.SPIRV;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace VeldridRaylib
{
    /// <summary>
    /// Vertex structure for rendering
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector2 Position;
        public Vector4 Color;

        public Vertex(Vector2 position, Vector4 color)
        {
            Position = position;
            Color = color;
        }
    }

    /// <summary>
    /// Projection matrix uniform buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ProjectionBuffer
    {
        public Matrix4x4 Projection;

        public ProjectionBuffer(Matrix4x4 projection)
        {
            Projection = projection;
        }
    }

    /// <summary>
    /// Core renderer for 2D graphics using Veldrid interfaces for maximum cross-compatibility
    /// </summary>
    public class Renderer : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly CommandList _commandList;
        private readonly DeviceBuffer _vertexBuffer;
        private readonly DeviceBuffer _indexBuffer;
        private readonly DeviceBuffer _projectionBuffer;
        private readonly ResourceSet _projectionResourceSet;
        private readonly Pipeline _pipeline;
        private readonly ResourceLayout _projectionLayout;

        private readonly List<Vertex> _vertices = new();
        private readonly List<uint> _indices = new();
        private uint _currentIndex = 0;
        private RgbaFloat _clearColor = RgbaFloat.Black;

        public Renderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            
            // Initialize command list
            _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
            
            // Create shaders using cross-platform approach
            var shaderSet = CreateShaders(_graphicsDevice);

            // Create vertex buffer
            _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(
                1024 * (uint)Marshal.SizeOf<Vertex>(), BufferUsage.VertexBuffer | BufferUsage.Dynamic));

            // Create index buffer
            _indexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(
                1024 * sizeof(uint), BufferUsage.IndexBuffer | BufferUsage.Dynamic));

            // Create projection buffer
            _projectionBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(
                (uint)Marshal.SizeOf<ProjectionBuffer>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            // Create resource layout - use consistent naming for all backends
            _projectionLayout = _graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            _projectionResourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                _projectionLayout, _projectionBuffer));

            // Create pipeline using cross-compatible vertex layout
            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

            var pipelineDescription = new GraphicsPipelineDescription()
            {
                BlendState = BlendStateDescription.SingleAlphaBlend,
                DepthStencilState = DepthStencilStateDescription.Disabled,
                RasterizerState = RasterizerStateDescription.Default,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new[] { _projectionLayout },
                ShaderSet = new Veldrid.ShaderSetDescription(
                    new[] { vertexLayout },
                    shaderSet),
                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipeline = _graphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);
        }

        private Shader[] CreateShaders(GraphicsDevice device)
        {
            // Use GLSL 450 shaders compiled to SPIR-V for maximum cross-platform compatibility
            string vertexShaderSource = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = Projection * vec4(Position, 0, 1);
    fsin_Color = Color;
}";

            string fragmentShaderSource = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

            
            // Compile GLSL to SPIR-V using Veldrid.SPIRV
            var vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(vertexShaderSource),
                "main");

            var fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(fragmentShaderSource),
                "main");

            // Use SPIR-V cross-compilation
            var shaders = device.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
            
            return shaders;
        }
        public void SetProjection(Matrix4x4 projection)
        {
            _graphicsDevice.UpdateBuffer(_projectionBuffer, 0, new ProjectionBuffer(projection));
        }

        public void SetClearColor(RgbaFloat clearColor)
        {
            _clearColor = clearColor;
        }

        public RgbaFloat GetClearColor()
        {
            return _clearColor;
        }

        public void BeginFrame()
        {
            _vertices.Clear();
            _indices.Clear();
            _currentIndex = 0;
        }

        public void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            var colorVec = color.ToVector4();
            
            _vertices.Add(new Vertex(p1, colorVec));
            _vertices.Add(new Vertex(p2, colorVec));
            _vertices.Add(new Vertex(p3, colorVec));

            _indices.Add(_currentIndex);
            _indices.Add(_currentIndex + 1);
            _indices.Add(_currentIndex + 2);

            _currentIndex += 3;
        }

        public void DrawQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Color color)
        {
            var colorVec = color.ToVector4();
            
            _vertices.Add(new Vertex(p1, colorVec));
            _vertices.Add(new Vertex(p2, colorVec));
            _vertices.Add(new Vertex(p3, colorVec));
            _vertices.Add(new Vertex(p4, colorVec));

            // First triangle
            _indices.Add(_currentIndex);
            _indices.Add(_currentIndex + 1);
            _indices.Add(_currentIndex + 2);

            // Second triangle
            _indices.Add(_currentIndex);
            _indices.Add(_currentIndex + 2);
            _indices.Add(_currentIndex + 3);

            _currentIndex += 4;
        }

        public void Flush(CommandList commandList)
        {
            if (_vertices.Count == 0) return;

            // Update buffers
            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, _vertices.ToArray());
            _graphicsDevice.UpdateBuffer(_indexBuffer, 0, _indices.ToArray());

            // Draw
            commandList.SetVertexBuffer(0, _vertexBuffer);
            commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
            commandList.SetPipeline(_pipeline);
            commandList.SetGraphicsResourceSet(0, _projectionResourceSet);
            commandList.DrawIndexed((uint)_indices.Count);
        }

        public CommandList GetCommandList() => _commandList;

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _projectionBuffer?.Dispose();
            _projectionResourceSet?.Dispose();
            _pipeline?.Dispose();
            _commandList?.Dispose();
        }
    }
}
