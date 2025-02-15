using System.Numerics;
using Silk.NET.Input;

namespace Flamui;

public partial class UiWindow
{
    public Vector2 MouseScreenPosition => Input.MousePosition;
    public Vector2 MousePosition => ScreenToWorld(Input.MousePosition);
    public Vector2 MouseDelta => Input.MousePosition - Input.LastMousePosition;
    public float ScrollDeltaX => Input.ScrollDeltaX;
    public float ScrollDeltaY => Input.ScrollDeltaY;
    public string TextInput => Input.TextInput;

    /// <summary>
    /// Check if a key has been pressed once
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyPressed(Key scancode)
    {
        return Input.KeyPressed.Contains(scancode);
    }

    /// <summary>
    /// Check if a key is being pressed
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyDown(Key scancode)
    {
        return Input.KeyDown.Contains(scancode);
    }

    /// <summary>
    /// Check if a key has been released once
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyReleased(Key scancode)
    {
        return Input.KeyReleased.Contains(scancode);
    }

    /// <summary>
    /// Check if a key is NOT being pressed
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyUp(Key scancode)
    {
        return Input.KeyUp.Contains(scancode);
    }

    /// <summary>
    /// Check if a mouse button has been pressed once
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonPressed(MouseButton mouseButtonKind)
    {
        return Input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonPressed;
    }

    /// <summary>
    /// Check if a mouse button is being pressed
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonDown(MouseButton mouseButtonKind)
    {
        return Input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonDown;
    }

    /// <summary>
    /// Check if a mouse button has been released once
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonReleased(MouseButton mouseButtonKind)
    {
        return Input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonReleased;
    }

    /// <summary>
    /// Check if a mouse button is NOT being pressed
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonUp(MouseButton mouseButtonKind)
    {
        return Input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonUp;
    }
}
