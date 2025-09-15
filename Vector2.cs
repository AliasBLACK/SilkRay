using System;

namespace SilkRay
{
	/// <summary>
	/// Vector2 structure for 2D math operations
	/// </summary>
	public struct Vector2(float x, float y)
	{
		public float X = x;
		public float Y = y;

		public Vector2(float value) : this(value, value)
		{
		}

		// Vector operations
		public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
		public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
		public static Vector2 operator *(Vector2 a, float scalar) => new(a.X * scalar, a.Y * scalar);
		public static Vector2 operator *(float scalar, Vector2 a) => new(a.X * scalar, a.Y * scalar);
		public static Vector2 operator /(Vector2 a, float scalar) => new(a.X / scalar, a.Y / scalar);
		public static Vector2 operator -(Vector2 a) => new(-a.X, -a.Y);

		// Comparison operators
		public static bool operator ==(Vector2 a, Vector2 b) => Math.Abs(a.X - b.X) < float.Epsilon && Math.Abs(a.Y - b.Y) < float.Epsilon;
		public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);

		// Properties
		public float Length => (float)Math.Sqrt(X * X + Y * Y);
		public float LengthSquared => X * X + Y * Y;

		// Static properties
		public static Vector2 Zero => new(0, 0);
		public static Vector2 One => new(1, 1);
		public static Vector2 UnitX => new(1, 0);
		public static Vector2 UnitY => new(0, 1);

		// Methods
		public Vector2 Normalize()
		{
			float length = Length;
			return length > 0 ? this / length : Zero;
		}

		public static float Distance(Vector2 a, Vector2 b)
		{
			return (a - b).Length;
		}

		public static float DistanceSquared(Vector2 a, Vector2 b)
		{
			return (a - b).LengthSquared;
		}

		public static float Dot(Vector2 a, Vector2 b)
		{
			return a.X * b.X + a.Y * b.Y;
		}

		public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
		{
			return a + (b - a) * t;
		}

		public override bool Equals(object? obj)
		{
			return obj is Vector2 vector && this == vector;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y);
		}

		public override string ToString()
		{
			return $"Vector2({X}, {Y})";
		}
	}
}
