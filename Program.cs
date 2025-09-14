using SilkRay;

namespace SilkRay
{
    class Program
    {
        static void Main()
        {
            // Initialize window
            const int screenWidth = 800;
            const int screenHeight = 450;
            
            Raylib.InitWindow(screenWidth, screenHeight, "SilkRay - Raylib Implementation with Silk.NET");
            Raylib.SetTargetFPS(60);

            // Main game loop - traditional Raylib style
            while (!Raylib.WindowShouldClose())
            {
                // Draw
                Raylib.BeginDrawing();
                
                Raylib.ClearBackground(Color.Gray);

                // Draw some shapes to demonstrate functionality
                Raylib.DrawText("SilkRay - Raylib with Silk.NET!", 190, 200, 20, Color.LightGray);
                
                // Draw rectangles
                Raylib.DrawRectangle(100, 100, 200, 100, Color.Red);
                Raylib.DrawRectangleLines(120, 120, 160, 60, Color.Blue);
                
                // Draw circles
                Raylib.DrawCircle(400, 200, 50, Color.Green);
                Raylib.DrawCircleLines(500, 200, 40, Color.Purple);
                
                // Draw lines
                Raylib.DrawLine(50, 350, 750, 350, Color.Black);
                Raylib.DrawLineV(new Vector2(50, 370), new Vector2(750, 370), Color.DarkGray);
                
                // Draw pixels
                for (int i = 0; i < 50; i++)
                {
                    Raylib.DrawPixel(100 + i * 2, 50, Color.Yellow);
                }

                Raylib.EndDrawing();
            }

            // Cleanup
            Raylib.CloseWindow();
        }
    }
}
