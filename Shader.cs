using Veldrid;
using Veldrid.SPIRV;
using System.Text;

namespace VeldridRaylib
{
    /// <summary>
    /// Shader management for basic rendering
    /// </summary>
    public class ShaderSet : IDisposable
    {
        public Shader VertexShader { get; private set; } = null!;
        public Shader FragmentShader { get; private set; } = null!;

        public ShaderSet(GraphicsDevice device)
        {
            CreateShaders(device);
        }

        private void CreateShaders(GraphicsDevice device)
        {
            // GLSL 450 shaders that will be cross-compiled to SPIR-V
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

            // Cross-compile GLSL to SPIR-V for all backends
            var vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(vertexShaderSource),
                "main");

            var fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(fragmentShaderSource),
                "main");

            var shaders = device.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
            VertexShader = shaders[0];
            FragmentShader = shaders[1];
        }

        public void Dispose()
        {
            VertexShader?.Dispose();
            FragmentShader?.Dispose();
        }
    }
}
