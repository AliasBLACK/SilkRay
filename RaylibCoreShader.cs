using Silk.NET.OpenGL;
using System.Numerics;

namespace SilkRay
{
	/// <summary>
	/// Shader program wrapper for OpenGL shaders
	/// </summary>
	public class Shader : IDisposable
	{
		private readonly GL _gl;
		private readonly uint _program;
		private bool _disposed;

		public uint Program => _program;

		public Shader(GL gl, string vertexSource, string fragmentSource)
		{
			_gl = gl ?? throw new ArgumentNullException(nameof(gl));

			// Compile vertex shader
			uint vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
			
			// Compile fragment shader
			uint fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

			// Create and link program
			_program = _gl.CreateProgram();
			_gl.AttachShader(_program, vertexShader);
			_gl.AttachShader(_program, fragmentShader);
			_gl.LinkProgram(_program);

			// Check for linking errors
			_gl.GetProgram(_program, GLEnum.LinkStatus, out int linkStatus);
			if (linkStatus == 0)
			{
				string infoLog = _gl.GetProgramInfoLog(_program);
				throw new InvalidOperationException($"Shader program linking failed: {infoLog}");
			}

			// Clean up individual shaders
			_gl.DeleteShader(vertexShader);
			_gl.DeleteShader(fragmentShader);
		}

		private uint CompileShader(ShaderType type, string source)
		{
			uint shader = _gl.CreateShader(type);
			_gl.ShaderSource(shader, source);
			_gl.CompileShader(shader);

			// Check for compilation errors
			_gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compileStatus);
			if (compileStatus == 0)
			{
				string infoLog = _gl.GetShaderInfoLog(shader);
				_gl.DeleteShader(shader);
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
