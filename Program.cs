using VeldridRaylib;

namespace VeldridRaylib
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize window
            const int screenWidth = 800;
            const int screenHeight = 450;
            
            Raylib.InitWindow(screenWidth, screenHeight, "VeldridRaylib - Core API Demo");
            Raylib.SetTargetFPS(60);

            // Main game loop
            while (!Raylib.WindowShouldClose())
            {
                // Update
                // Handle input and game logic here

                // Draw
                Raylib.BeginDrawing();
                
                Raylib.ClearBackground(Color.DarkGray);

                // Draw some shapes to demonstrate the API
                Raylib.DrawRectangle(100, 100, 200, 150, Color.Red);
                Raylib.DrawCircle(400, 200, 50, Color.Blue);
                Raylib.DrawLine(50, 50, 750, 400, Color.Green);
                
                // Draw some pixels
                for (int i = 0; i < 20; i++)
                {
                    Raylib.DrawPixel(300 + i * 2, 300, Color.Yellow);
                }

                // Draw text (placeholder)
                Raylib.DrawText("Hello VeldridRaylib!", 10, 10, 20, Color.White);

                // Interactive elements
                if (Raylib.IsKeyDown(KeyCode.Space))
                {
                    Raylib.DrawCircle(400, 200, 80, Color.Magenta);
                }

                if (Raylib.IsKeyPressed(KeyCode.Enter))
                {
                    Raylib.DrawRectangle(200, 200, 100, 100, Color.Cyan);
                }

                // Mouse interaction
                var mousePos = Raylib.GetMousePosition();
                Raylib.DrawCircle((int)mousePos.X, (int)mousePos.Y, 10, Color.White);

                Raylib.EndDrawing();
            }

            // Cleanup
            Raylib.CloseWindow();
        }
    }
}
