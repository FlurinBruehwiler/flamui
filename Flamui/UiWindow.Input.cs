using System.Numerics;
using Silk.NET.Input;

namespace Flamui;

public partial class UiWindow
{
    public Vector2 MouseScreenPosition => _input.MousePosition;
    public Vector2 MousePosition => ScreenToWorld(_input.MousePosition);
    public Vector2 MouseDelta => _input.MousePosition - _input.LastMousePosition;
    public float ScrollDeltaX => _input.ScrollDeltaX;
    public float ScrollDeltaY => _input.ScrollDeltaY;
    public string TextInput => _input.TextInput;

    /// <summary>
    /// Check if a key has been pressed once
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyPressed(Key scancode)
    {
        return _input.KeyPressed.Contains(scancode);
    }

    /// <summary>
    /// Check if a key is being pressed
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyDown(Key scancode)
    {
        return _input.KeyDown.Contains(scancode);
    }

    /// <summary>
    /// Check if a key has been released once
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyReleased(Key scancode)
    {
        return _input.KeyReleased.Contains(scancode);
    }

    /// <summary>
    /// Check if a key is NOT being pressed
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyUp(Key scancode)
    {
        return _input.KeyUp.Contains(scancode);
    }

    /// <summary>
    /// Check if a mouse button has been pressed once
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonPressed(MouseButton mouseButtonKind)
    {
        return _input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonPressed;
    }

    /// <summary>
    /// Check if a mouse button is being pressed
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonDown(MouseButton mouseButtonKind)
    {
        return _input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonDown;
    }

    /// <summary>
    /// Check if a mouse button has been released once
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonReleased(MouseButton mouseButtonKind)
    {
        return _input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonReleased;
    }

    /// <summary>
    /// Check if a mouse button is NOT being pressed
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonUp(MouseButton mouseButtonKind)
    {
        return _input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonUp;
    }
}
