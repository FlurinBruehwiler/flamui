using System.Numerics;

namespace ImSharpUISample;

public partial class UiWindow
{
    public Vector2 MousePosition => _input.MousePosition;
    public Vector2 MouseDelta => _input.MousePosition - _input.LastMousePosition;
    public int ScrollDelta => _input.ScrollDelta;
    public string TextInput => _input.TextInput;

    /// <summary>
    /// Check if a key has been pressed once
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyPressed(SDL_Scancode scancode)
    {
        return _input.KeyPressed.Contains(scancode);
    }

    /// <summary>
    /// Check if a key is being pressed
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyDown(SDL_Scancode scancode)
    {
        return _input.KeyDown.Contains(scancode);
    }

    /// <summary>
    /// Check if a key has been released once
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyReleased(SDL_Scancode scancode)
    {
        return _input.KeyReleased.Contains(scancode);
    }

    /// <summary>
    /// Check if a key is NOT being pressed
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyUp(SDL_Scancode scancode)
    {
        return _input.KeyUp.Contains(scancode);
    }

    /// <summary>
    /// Check if a mouse button has been pressed once
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonPressed(MouseButtonKind mouseButtonKind)
    {
        return _input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonPressed;
    }

    /// <summary>
    /// Check if a mouse button is being pressed
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonDown(MouseButtonKind mouseButtonKind)
    {
        return _input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonDown;
    }

    /// <summary>
    /// Check if a mouse button has been released once
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonReleased(MouseButtonKind mouseButtonKind)
    {
        return _input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonReleased;
    }

    /// <summary>
    /// Check if a mouse button is NOT being pressed
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonUp(MouseButtonKind mouseButtonKind)
    {
        return _input.MouseButtonStates[(int)mouseButtonKind].IsMouseButtonUp;
    }
}
