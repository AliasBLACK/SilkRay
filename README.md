# SilkRay

A C# implementation of core Raylib functions using Silk.NET OpenGL bindings.

## Overview

SilkRay provides a familiar Raylib-style API for 2D graphics programming in C# using modern Silk.NET libraries. It implements the core functionality from Raylib's `rcore` and `rshapes` modules.

## Features

### Core Functions (rcore)
- Window management (`InitWindow`, `CloseWindow`, `WindowShouldClose`)
- Drawing lifecycle (`BeginDrawing`, `EndDrawing`, `ClearBackground`)
- Screen properties (`GetScreenWidth`, `GetScreenHeight`)
- Timing functions (`GetTime`, `GetFrameTime`, `GetFPS`)

### Shape Drawing (rshapes)
- **Pixels**: `DrawPixel`, `DrawPixelV`
- **Lines**: `DrawLine`, `DrawLineV`, `DrawLineEx`
- **Rectangles**: `DrawRectangle`, `DrawRectangleV`, `DrawRectangleRec`, `DrawRectangleLines`
- **Circles**: `DrawCircle`, `DrawCircleV`, `DrawCircleLines`

### Data Structures
- `Color` - RGBA color with predefined constants
- `Vector2` - 2D vector math operations
- `Rectangle` - Rectangle structure for bounds

## Quick Start

```csharp
using SilkRay;

// Initialize window
Raylib.InitWindow(800, 450, "My SilkRay App");
Raylib.SetTargetFPS(60);

// Main loop
while (!Raylib.WindowShouldClose())
{
    Raylib.BeginDrawing();
    
    Raylib.ClearBackground(Color.RayWhite);
    
    // Draw shapes
    Raylib.DrawRectangle(100, 100, 200, 100, Color.Red);
    Raylib.DrawCircle(400, 200, 50, Color.Blue);
    Raylib.DrawLine(0, 0, 800, 450, Color.Green);
    
    Raylib.EndDrawing();
}

Raylib.CloseWindow();
```

## Examples

The project includes several example programs:

1. **Basic Shapes** - Demonstrates all shape drawing functions
2. **Animation** - Shows a bouncing ball with trail effects
3. **Color Palette** - Displays all predefined colors

Run examples:
```csharp
Example.RunBasicShapesExample();
Example.RunAnimatedExample();
Example.RunColorPaletteExample();
```

## Building

```bash
dotnet restore
dotnet build
dotnet run
```

## Dependencies

- Silk.NET.OpenGL (2.20.0)
- Silk.NET.Windowing (2.20.0)
- Silk.NET.Input (2.20.0)
- .NET 8.0

## Architecture

- **Raylib.cs** - Main API that matches Raylib function signatures
- **Renderer.cs** - OpenGL rendering backend using Silk.NET
- **Shader.cs** - Shader program management
- **Color.cs** - Color utilities and predefined constants
- **Vector2.cs** - 2D math operations

## Limitations

- Text rendering not yet implemented
- Input handling is basic (placeholders for keyboard/mouse)
- Limited to 2D graphics
- No texture/image loading
- No audio support

## Future Enhancements

- Font loading and text rendering
- Complete input system implementation
- Texture and sprite support
- Additional shape primitives
- Performance optimizations

## License

This project is provided as-is for educational and development purposes.
