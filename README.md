# VeldridRaylib - Raylib API Implementation using Veldrid

A C# implementation of the core raylib API using Veldrid as the graphics backend. This project provides a familiar raylib-style interface for 2D graphics programming while leveraging Veldrid's modern, cross-platform graphics capabilities.

## Features

### Core Window Management
- `InitWindow(width, height, title)` - Initialize graphics window
- `WindowShouldClose()` - Check if window should close
- `CloseWindow()` - Close window and cleanup resources
- `SetTargetFPS(fps)` - Set target frames per second

### Drawing Functions
- `BeginDrawing()` / `EndDrawing()` - Begin/end drawing frame
- `ClearBackground(color)` - Clear screen with specified color

### Shape Drawing
- `DrawRectangle(x, y, width, height, color)` - Draw rectangle
- `DrawRectangleRec(rectangle, color)` - Draw rectangle from Rectangle struct
- `DrawCircle(centerX, centerY, radius, color)` - Draw circle
- `DrawCircleV(center, radius, color)` - Draw circle from Vector2
- `DrawLine(startX, startY, endX, endY, color)` - Draw line
- `DrawLineV(startPos, endPos, color)` - Draw line from Vector2s
- `DrawPixel(x, y, color)` - Draw single pixel

### Text Rendering (Basic)
- `DrawText(text, x, y, fontSize, color)` - Draw text (placeholder implementation)
- `MeasureText(text, fontSize)` - Measure text width

### Input Handling
- `IsKeyPressed(key)` - Check if key was just pressed
- `IsKeyDown(key)` - Check if key is currently held down
- `IsKeyReleased(key)` - Check if key was just released
- `IsKeyUp(key)` - Check if key is not pressed
- `GetMousePosition()` - Get current mouse position

### Utility Functions
- `GetScreenWidth()` / `GetScreenHeight()` - Get screen dimensions
- `GetFrameTime()` - Get time for last frame
- `GetFPS()` - Get current FPS

## Dependencies

- **Veldrid** (4.9.0) - Modern graphics API abstraction
- **Veldrid.StartupUtilities** (4.9.0) - Window and graphics device creation helpers
- **Veldrid.SDL2** (4.9.0) - SDL2 backend for windowing
- **System.Numerics.Vectors** (4.5.0) - Vector math support

## Building and Running

```bash
dotnet restore
dotnet build
dotnet run
```

## Example Usage

```csharp
using VeldridRaylib;

// Initialize window
Raylib.InitWindow(800, 450, "My Game");
Raylib.SetTargetFPS(60);

// Main game loop
while (!Raylib.WindowShouldClose())
{
    // Update game logic here
    
    Raylib.BeginDrawing();
    
    Raylib.ClearBackground(Color.DarkGray);
    Raylib.DrawRectangle(100, 100, 200, 150, Color.Red);
    Raylib.DrawCircle(400, 200, 50, Color.Blue);
    Raylib.DrawText("Hello World!", 10, 10, 20, Color.White);
    
    Raylib.EndDrawing();
}

Raylib.CloseWindow();
```

## Architecture

The implementation consists of several key components:

- **Raylib.cs** - Main API interface matching raylib's function signatures
- **Renderer.cs** - Core 2D renderer using Veldrid's graphics pipeline
- **Shader.cs** - Basic vertex/fragment shaders for 2D rendering
- **Color.cs** - Color structure with predefined colors
- **Vector2.cs** - 2D vector and rectangle structures

## Current Limitations

- Text rendering is placeholder (no actual font rendering)
- Mouse button input is not fully implemented
- No texture/image loading support
- No audio support
- Limited to basic 2D shapes and primitives

## Future Enhancements

- Full text rendering with font support
- Texture and sprite rendering
- Audio system integration
- More advanced shape drawing (polygons, bezier curves)
- Performance optimizations
- Additional input handling features
