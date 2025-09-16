using System.Numerics;

namespace SilkRay
{
	public static class Shapes
	{
		// Shape drawing functions (rshapes)
		public static void DrawPixel(int posX, int posY, Color color)
		{
			DrawRectangle(posX, posY, 1, 1, color);
		}

		public static void DrawPixelV(Vector2 position, Color color)
		{
			DrawPixel((int)position.X, (int)position.Y, color);
		}

		public static void DrawLine(int startPosX, int startPosY, int endPosX, int endPosY, Color color)
		{
			RaylibInternal.Renderer?.DrawLine(startPosX, startPosY, endPosX, endPosY, color);
		}

		public static void DrawLineV(Vector2 startPos, Vector2 endPos, Color color)
		{
			DrawLine((int)startPos.X, (int)startPos.Y, (int)endPos.X, (int)endPos.Y, color);
		}

		public static void DrawLineEx(Vector2 startPos, Vector2 endPos, float thick, Color color)
		{
			if (thick <= 1.0f)
			{
				// For thin lines, use regular line drawing
				DrawLine((int)startPos.X, (int)startPos.Y, (int)endPos.X, (int)endPos.Y, color);
				return;
			}

			// Use the renderer's thick line drawing capability
			RaylibInternal.Renderer?.DrawThickLine(startPos.X, startPos.Y, endPos.X, endPos.Y, thick, color);
		}

		public static void DrawCircle(int centerX, int centerY, float radius, Color color)
		{
			RaylibInternal.Renderer?.DrawCircle(centerX, centerY, radius, color);
		}

		public static void DrawCircleV(Vector2 center, float radius, Color color)
		{
			DrawCircle((int)center.X, (int)center.Y, radius, color);
		}

		public static void DrawCircleLines(int centerX, int centerY, float radius, Color color)
		{
			RaylibInternal.Renderer?.DrawCircleLines(centerX, centerY, radius, color, false);
		}

		public static void DrawCircleLinesV(Vector2 center, float radius, Color color)
		{
			DrawCircleLines((int)center.X, (int)center.Y, radius, color);
		}

		public static void DrawRectangle(int posX, int posY, int width, int height, Color color)
		{
			RaylibInternal.Renderer?.DrawRectangle(posX, posY, width, height, color);
		}

		public static void DrawRectangleV(Vector2 position, Vector2 size, Color color)
		{
			DrawRectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y, color);
		}

		public static void DrawRectangleRec(Rectangle rec, Color color)
		{
			DrawRectangle((int)rec.X, (int)rec.Y, (int)rec.Width, (int)rec.Height, color);
		}

		public static void DrawRectangleLines(int posX, int posY, int width, int height, Color color)
		{
			// Draw rectangle outline using lines
			DrawLine(posX, posY, posX + width, posY, color);                    // Top
			DrawLine(posX + width, posY, posX + width, posY + height, color);   // Right
			DrawLine(posX + width, posY + height, posX, posY + height, color);  // Bottom
			DrawLine(posX, posY + height, posX, posY, color);                   // Left
		}

		public static void DrawRectangleLinesEx(Rectangle rec, float lineThick, Color color)
		{
			if (lineThick <= 1.0f)
			{
				// Use regular lines for thin thickness
				DrawRectangleLines((int)rec.X, (int)rec.Y, (int)rec.Width, (int)rec.Height, color);
			}
			else
			{
				// Draw thick lines by drawing filled rectangles for each side
				int thickness = (int)Math.Ceiling(lineThick);
				
				// Top line
				DrawRectangle((int)rec.X, (int)rec.Y, (int)rec.Width, thickness, color);
				// Bottom line
				DrawRectangle((int)rec.X, (int)(rec.Y + rec.Height - thickness), (int)rec.Width, thickness, color);
				// Left line
				DrawRectangle((int)rec.X, (int)rec.Y, thickness, (int)rec.Height, color);
				// Right line
				DrawRectangle((int)(rec.X + rec.Width - thickness), (int)rec.Y, thickness, (int)rec.Height, color);
			}
		}

		public static void DrawRectanglePro(Rectangle rec, Vector2 origin, float rotation, Color color)
		{
			// For now, implement as a simple rectangle (rotation would need matrix transforms)
			DrawRectangle((int)(rec.X - origin.X), (int)(rec.Y - origin.Y), (int)rec.Width, (int)rec.Height, color);
		}

		public static void DrawTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color color)
		{
			// Draw triangle using lines for now
			DrawLineV(v1, v2, color);
			DrawLineV(v2, v3, color);
			DrawLineV(v3, v1, color);
		}
	}
}
