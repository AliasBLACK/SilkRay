# SilkRay

A C# implementation of core Raylib functions using Silk.NET OpenGL bindings. SilkRay provides a Raylib-compatible 2D graphics API for cross-platform game development and graphics programming.

## Overview

SilkRay provides a familiar Raylib-style API for 2D graphics programming in C# using modern Silk.NET libraries. It implements core 2D functionality including window management, shape drawing, input handling, camera system, and text rendering with full NativeAOT support.

## Features

### Graphics & Windowing
- Window management (`InitWindow`, `CloseWindow`, `WindowShouldClose`)
- Drawing lifecycle (`BeginDrawing`, `EndDrawing`, `ClearBackground`)
- Screen properties (`GetScreenWidth`, `GetScreenHeight`)
- Timing functions (`GetTime`, `GetFrameTime`, `GetFPS`)
- Monitor information (`GetMonitorWidth`, `GetMonitorHeight`, `GetMonitorName`)

### Input Handling
- **Keyboard**: `IsKeyDown`, `IsKeyPressed`, `IsKeyReleased`, `GetKeyPressed`, `GetCharPressed`
- **Mouse**: `IsMouseButtonDown`, `IsMouseButtonPressed`, `IsMouseButtonReleased`, `GetMousePosition`, `GetMouseWheelMove`
- **Gamepad**: `IsGamepadAvailable`, `IsGamepadButtonDown`, `GetGamepadAxisMovement`
- **Cursor**: `ShowCursor`, `HideCursor`, `SetMouseCursor`

### 2D Shape Drawing
- **Pixels**: `DrawPixel`, `DrawPixelV`
- **Lines**: `DrawLine`, `DrawLineV`, `DrawLineEx`
- **Rectangles**: `DrawRectangle`, `DrawRectangleV`, `DrawRectangleRec`, `DrawRectangleLines`, `DrawRectanglePro`
- **Circles**: `DrawCircle`, `DrawCircleV`, `DrawCircleLines`
- **Triangles**: `DrawTriangle`

### 2D Camera System
- **Camera2D**: Complete camera with target, offset, rotation, and zoom
- **Coordinate conversion**: `GetScreenToWorld2D`, `GetWorldToScreen2D`
- **Camera matrix**: `BeginMode2D`, `EndMode2D`

### Text Rendering
- **Text drawing**: `DrawText`, `DrawTextEx`
- **Font loading**: `LoadFont`, `UnloadFont`
- **Text measurement**: `MeasureText`, `MeasureTextEx`

### File Handling
- **File operations**: `FileExists`, `DirectoryExists`, `GetFileLength`, `GetFileModTime`
- **Path utilities**: `GetFileExtension`, `GetFileName`, `GetFileNameWithoutExt`, `GetDirectoryPath`
- **Extension checking**: `IsFileExtension`

### Data Structures
- `Color` - RGBA color with predefined constants (UPPERCASE naming)
- `Vector2` - 2D vector math operations
- `Rectangle` - Rectangle structure for bounds
- `Camera2D` - 2D camera with transformation matrix

## Quick Start

```csharp
using static SilkRay.Core;
using static SilkRay.Shapes;
using static SilkRay.Text;

// Initialize 2D window
InitWindow(800, 450, "My SilkRay App");
SetTargetFPS(60);

// Main loop
while (!WindowShouldClose())
{
    BeginDrawing();
    
    ClearBackground(RAYWHITE);
    
    // Draw 2D shapes
    DrawRectangle(100, 100, 200, 100, RED);
    DrawCircle(400, 200, 50, BLUE);
    DrawLine(0, 0, 800, 450, GREEN);
    DrawText("Hello SilkRay 2D!", 10, 10, 20, BLACK);
    
    EndDrawing();
}

CloseWindow();
```

## Installation

### NuGet Package
```bash
dotnet add package SilkRay
```

### From Source
```bash
git clone https://github.com/AliasBLACK/SilkRay.git
cd SilkRay
dotnet build
```

## Examples

Comprehensive example projects are available in a separate repository:

**Repository:** https://github.com/AliasBLACK/SilkRayExamples

1. **BasicShapes** - Core 2D shape drawing and animation
2. **MouseInput** - Complete mouse input handling with trails and cursors
3. **GamepadInput** - Gamepad support with analog sticks and buttons
4. **Camera2D** - 2D camera system with zoom, rotation, and following

All examples support NativeAOT compilation for optimal 2D performance.

## Building

### Regular Build
```bash
dotnet restore
dotnet build
```

### NativeAOT Build (Recommended for 2D games)
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## Dependencies

- Silk.NET.OpenGL (2.22.0)
- Silk.NET.Windowing (2.22.0)
- Silk.NET.Input (2.22.0)
- Silk.NET.Windowing.Glfw (2.22.0)
- Silk.NET.Input.Glfw (2.22.0)
- FontStashSharp (for text rendering)
- TextCopy (for clipboard operations)
- StbImageSharp (for image loading)
- .NET 8.0

## Performance

- **NativeAOT Compatible**: Zero-cost abstractions with ahead-of-time compilation
- **2D Optimized**: Efficient batch rendering for shapes and sprites
- **Memory Efficient**: Minimal allocations in 2D rendering hot paths
- **Cross-platform**: Uses OpenGL for consistent 2D performance across platforms
