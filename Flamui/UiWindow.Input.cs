using System.Numerics;
using Silk.NET.GLFW;
using Silk.NET.Input;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Flamui;

//This is kind of the input abstraction on a per uiTree basis

public partial class UiTree
{
    private void CleanupInputAfterFrame()
    {
        foreach (var mouseButtonState in MouseButtonStates)
        {
            mouseButtonState.IsMouseButtonPressed = false;
            mouseButtonState.IsMouseButtonReleased = false;
        }

        KeyPressed.Clear();
        KeyReleased.Clear();

        TextInput = string.Empty;

        LastMousePosition = MousePosition;

        ScrollDelta = default;
    }

    //the UiTree should only know the mouse position relative to itself, and not care about or even know about the screen position,
    //we also want to only calculate it once per frame, and not constantly
    public Vector2 MousePosition; //this is the tricky part,
    public Vector2 LastMousePosition;
    public Vector2 MouseDelta => MousePosition - LastMousePosition;
    public Vector2 ScrollDelta;
    public string TextInput = string.Empty;

    public readonly MouseButtonState[] MouseButtonStates = Enumerable.Range(0, (int)MouseButton.Button12 + 1).Select(_ => new MouseButtonState()).ToArray();

    /// <summary>
    /// Keys that have been pressed once
    /// </summary>
    public HashSet<Key> KeyPressed { get; set; } = new();

    /// <summary>
    /// Keys that are being pressed
    /// </summary>
    public HashSet<Key> KeyDown { get; set; } = new();

    /// <summary>
    /// Keys that have been release once
    /// </summary>
    public HashSet<Key> KeyReleased { get; set; } = new();

    /// <summary>
    /// Keys that are not being pressed
    /// </summary>
    public HashSet<Key> KeyUp { get; set; } = new();

    /// <summary>
    /// Check if a key has been pressed once
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyPressed(Key scancode)
    {
        return KeyPressed.Contains(scancode);
    }

    /// <summary>
    /// Check if a key is being pressed
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyDown(Key scancode)
    {
        return KeyDown.Contains(scancode);
    }

    /// <summary>
    /// Check if a key has been released once
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyReleased(Key scancode)
    {
        return KeyReleased.Contains(scancode);
    }

    /// <summary>
    /// Check if a key is NOT being pressed
    /// </summary>
    /// <param name="scancode"></param>
    /// <returns></returns>
    public bool IsKeyUp(Key scancode)
    {
        return KeyUp.Contains(scancode);
    }

    /// <summary>
    /// Check if a mouse button has been pressed once
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonPressed(MouseButton mouseButtonKind)
    {
        return MouseButtonStates[(int)mouseButtonKind].IsMouseButtonPressed;
    }

    /// <summary>
    /// Check if a mouse button is being pressed
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonDown(MouseButton mouseButtonKind)
    {
        return MouseButtonStates[(int)mouseButtonKind].IsMouseButtonDown;
    }

    /// <summary>
    /// Check if a mouse button has been released once
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonReleased(MouseButton mouseButtonKind)
    {
        return MouseButtonStates[(int)mouseButtonKind].IsMouseButtonReleased;
    }

    /// <summary>
    /// Check if a mouse button is NOT being pressed
    /// </summary>
    /// <param name="mouseButtonKind"></param>
    /// <returns></returns>
    public bool IsMouseButtonUp(MouseButton mouseButtonKind)
    {
        return MouseButtonStates[(int)mouseButtonKind].IsMouseButtonUp;
    }

    public string GetClipboardText()
    {
        return UiTreeHost.GetClipboardText();
    }

    public void SetClipboardText(string text)
    {
        UiTreeHost.SetClipboardText(text);
    }
}
