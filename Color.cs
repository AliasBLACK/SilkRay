using System;
using System.Numerics;

namespace SilkRay
{
    /// <summary>
    /// Color structure representing RGBA values
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

        public Color(int r, int g, int b, int a = 255)
        {
            R = (byte)Math.Clamp(r, 0, 255);
            G = (byte)Math.Clamp(g, 0, 255);
            B = (byte)Math.Clamp(b, 0, 255);
            A = (byte)Math.Clamp(a, 0, 255);
        }

        /// <summary>
        /// Convert color to Vector4 (normalized 0.0-1.0)
        /// </summary>
        public Vector4 ToVector4()
        {
            return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
        }

        /// <summary>
        /// Create color from Vector4 (normalized 0.0-1.0)
        /// </summary>
        public static Color FromVector4(Vector4 vector)
        {
            return new Color(
                (byte)(vector.X * 255),
                (byte)(vector.Y * 255),
                (byte)(vector.Z * 255),
                (byte)(vector.W * 255)
            );
        }

        // Predefined colors matching Raylib
        public static readonly Color LightGray = new(200, 200, 200);
        public static readonly Color Gray = new(130, 130, 130);
        public static readonly Color DarkGray = new(80, 80, 80);
        public static readonly Color Yellow = new(253, 249, 0);
        public static readonly Color Gold = new(255, 203, 0);
        public static readonly Color Orange = new(255, 161, 0);
        public static readonly Color Pink = new(255, 109, 194);
        public static readonly Color Red = new(230, 41, 55);
        public static readonly Color Maroon = new(190, 33, 55);
        public static readonly Color Green = new(0, 228, 48);
        public static readonly Color Lime = new(0, 158, 47);
        public static readonly Color DarkGreen = new(0, 117, 44);
        public static readonly Color SkyBlue = new(102, 191, 255);
        public static readonly Color Blue = new(0, 121, 241);
        public static readonly Color DarkBlue = new(0, 82, 172);
        public static readonly Color Purple = new(200, 122, 255);
        public static readonly Color Violet = new(135, 60, 190);
        public static readonly Color DarkPurple = new(112, 31, 126);
        public static readonly Color Beige = new(211, 176, 131);
        public static readonly Color Brown = new(127, 106, 79);
        public static readonly Color DarkBrown = new(76, 63, 47);
        public static readonly Color White = new(255, 255, 255);
        public static readonly Color Black = new(0, 0, 0);
        public static readonly Color Blank = new(0, 0, 0, 0);
        public static readonly Color Magenta = new(255, 0, 255);
        public static readonly Color RayWhite = new(245, 245, 245);

        public override string ToString()
        {
            return $"Color({R}, {G}, {B}, {A})";
        }
    }
}
