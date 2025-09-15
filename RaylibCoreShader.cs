using Silk.NET.OpenGL;
using System.Numerics;

namespace SilkRay
{
	/// <summary>
	/// Shader program wrapper for OpenGL shaders
	/// </summary>
	public class Shader(GL gl, string vertexSource, string fragmentSource) : IDisposable
	{
		private readonly GL _gl = gl ?? throw new ArgumentNullException(nameof(gl));
		private readonly uint _program = CreateProgram(gl, vertexSource, fragmentSource);
		private bool _disposed;

		public uint Program => _program;

		private static uint CreateProgram(GL gl, string vertexSource, string fragmentSource)
		{
			// Compile vertex shader
			uint vertexShader = CompileShader(gl, ShaderType.VertexShader, vertexSource);
			
			// Compile fragment shader
			uint fragmentShader = CompileShader(gl, ShaderType.FragmentShader, fragmentSource);

			// Create and link program
			uint program = gl.CreateProgram();
			gl.AttachShader(program, vertexShader);
			gl.AttachShader(program, fragmentShader);
			gl.LinkProgram(program);

			// Check for linking errors
			gl.GetProgram(program, GLEnum.LinkStatus, out int linkStatus);
			if (linkStatus == 0)
			{
				string infoLog = gl.GetProgramInfoLog(program);
				throw new InvalidOperationException($"Shader program linking failed: {infoLog}");
			}

			// Clean up individual shaders
			gl.DeleteShader(vertexShader);
			gl.DeleteShader(fragmentShader);
			
			return program;
		}

		private static uint CompileShader(GL gl, ShaderType type, string source)
		{
			uint shader = gl.CreateShader(type);
			gl.ShaderSource(shader, source);
			gl.CompileShader(shader);

			// Check for compilation errors
			gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compileStatus);
			if (compileStatus == 0)
			{
				string infoLog = gl.GetShaderInfoLog(shader);
				gl.DeleteShader(shader);
				throw new InvalidOperationException($"Shader compilation failed ({type}): {infoLog}");
			}

			return shader;
		}


		public void Use()
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			
			_gl.UseProgram(_program);
		}

		public void SetUniform(string name, int value)
		{
			int location = _gl.GetUniformLocation(_program, name);
			if (location >= 0)
				_gl.Uniform1(location, value);
		}

		public void SetUniform(string name, float value)
		{
			int location = _gl.GetUniformLocation(_program, name);
			if (location >= 0)
				_gl.Uniform1(location, value);
		}

		public void SetUniform(string name, Vector2 value)
		{
			int location = _gl.GetUniformLocation(_program, name);
			if (location >= 0)
				_gl.Uniform2(location, value.X, value.Y);
		}

		public void SetUniform(string name, Vector4 value)
		{
			int location = _gl.GetUniformLocation(_program, name);
			if (location >= 0)
				_gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
		}

		public void SetUniform(string name, Color color)
		{
			SetUniform(name, color.ToVector4());
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_gl.DeleteProgram(_program);
				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}
	}
}
