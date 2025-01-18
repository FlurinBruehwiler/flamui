using System.Numerics;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Windowing;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Flamui;

public class Input
{
    public readonly MouseButtonState[] MouseButtonStates = new MouseButtonState[(int)MouseButton.Button12 + 1];

    public Vector2 MousePosition { get; private set; }
    public Vector2 LastMousePosition { get; private set; }
    public float ScrollDeltaX { get; private set; }
    public float ScrollDeltaY { get; private set; }
    public string TextInput { get; private set; } = string.Empty;

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

    public Input(IWindow window)
    {
        var input = window.CreateInput();

        for (int i = 0; i <= (int)MouseButton.Button12; i++)
        {
            MouseButtonStates[i] = new MouseButtonState();
        }

        foreach (var keyboard in input.Keyboards)
        {
            keyboard.KeyDown += (_, key, _) =>
            {
                KeyDown.Add(key);
                KeyPressed.Add(key);
            };

            keyboard.KeyUp += (_, key, _) =>
            {
                KeyUp.Add(key);
                KeyDown.Remove(key);
                KeyReleased.Add(key);
            };

            keyboard.KeyChar += OnTextInput;
        }

        foreach (var mouse in input.Mice)
        {
            _mouse = mouse;

            mouse.MouseDown += (_, button) =>
            {
                var mouseButton = GetMouseButton(button);
                mouseButton.IsMouseButtonDown = true;
                mouseButton.IsMouseButtonPressed = true;
            };
            mouse.MouseUp += (_, button) =>
            {
                var mouseButton = GetMouseButton(button);
                mouseButton.IsMouseButtonUp = true;
                mouseButton.IsMouseButtonDown = false;
                mouseButton.IsMouseButtonReleased = true;
            };
            mouse.Scroll += (_, wheel) =>
            {
                ScrollDeltaX = wheel.X;
                ScrollDeltaY = wheel.Y;
            };
            // mouse.MouseMove += (_, mouseDelta) =>
            // {
            //     MousePosition += mouseDelta;
            // };
        }
    }

    private void OnTextInput(IKeyboard keyboard, char charInput)
    {
        TextInput += charInput;
    }

    private IMouse _mouse;

    public void OnAfterFrame()
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
        MousePosition = _mouse.Position;

        ScrollDeltaX = 0;
    }

    private MouseButtonState GetMouseButton(MouseButton button)
    {
        return MouseButtonStates[(int)button];
    }
}

public class MouseButtonState
{
    /// <summary>
    /// Check if a mouse button has been pressed once
    /// </summary>
    public bool IsMouseButtonPressed { get; set; }

    /// <summary>
    /// Check if a mouse button is being pressed
    /// </summary>
    public bool IsMouseButtonDown { get; set; }

    /// <summary>
    /// Check if a mouse button has been released once
    /// </summary>
    public bool IsMouseButtonReleased { get; set; }

    /// <summary>
    /// Check if a mouse button is NOT being pressed
    /// </summary>
    public bool IsMouseButtonUp { get; set; }
}