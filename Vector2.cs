using System.Numerics;

namespace VeldridRaylib
{
    /// <summary>
    /// 2D Vector structure
    /// </summary>
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new(0, 0);
        public static Vector2 One => new(1, 1);

        public float Length() => (float)Math.Sqrt(X * X + Y * Y);
        public float LengthSquared() => X * X + Y * Y;

        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float scalar) => new(a.X * scalar, a.Y * scalar);
        public static Vector2 operator /(Vector2 a, float scalar) => new(a.X / scalar, a.Y / scalar);

        public static implicit operator System.Numerics.Vector2(Vector2 v) => new(v.X, v.Y);
        public static implicit operator Vector2(System.Numerics.Vector2 v) => new(v.X, v.Y);
    }

    /// <summary>
    /// Rectangle structure
    /// </summary>
    public struct Rectangle
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float Left => X;
        public float Right => X + Width;
        public float Top => Y;
        public float Bottom => Y + Height;
    }
}
