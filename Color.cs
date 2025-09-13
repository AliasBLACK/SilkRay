using System.Numerics;

namespace VeldridRaylib
{
    /// <summary>
    /// Represents a color with RGBA components
    /// </summary>
    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Vector4 ToVector4()
        {
            return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
        }

        public static implicit operator Vector4(Color color)
        {
            return color.ToVector4();
        }

        // Common colors
        public static readonly Color White = new(255, 255, 255);
        public static readonly Color Black = new(0, 0, 0);
        public static readonly Color Red = new(255, 0, 0);
        public static readonly Color Green = new(0, 255, 0);
        public static readonly Color Blue = new(0, 0, 255);
        public static readonly Color Yellow = new(255, 255, 0);
        public static readonly Color Magenta = new(255, 0, 255);
        public static readonly Color Cyan = new(0, 255, 255);
        public static readonly Color Gray = new(128, 128, 128);
        public static readonly Color DarkGray = new(64, 64, 64);
        public static readonly Color LightGray = new(192, 192, 192);
    }
}
