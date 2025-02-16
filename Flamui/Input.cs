using System.Collections;
using System.Numerics;
using System.Reflection;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Windowing;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Flamui;

public class Input
{
    public readonly MouseButtonState[] MouseButtonStates = new MouseButtonState[(int)MouseButton.Button12 + 1];

    public Vector2 MousePosition => _mouse.Position;
    public Vector2 LastMousePosition { get; private set; }
    public float ScrollDeltaX { get; private set; }
    public float ScrollDeltaY { get; private set; }
    public string TextInput { get; set; } = string.Empty;
    public string ClipboardText
    {
        get => _keyboard.ClipboardText;
        set => _keyboard.ClipboardText = value;
    }

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


    private IMouse _mouse;
    private IKeyboard _keyboard;

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

        ScrollDeltaX = 0;
        ScrollDeltaY = 0;
    }

    private MouseButtonState GetMouseButton(MouseButton button)
    {
        return MouseButtonStates[(int)button];
    }

    public static unsafe Input ConstructInputFromWindow(IWindow window)
    {
        var input = window.CreateInput();

        var inputObj = new Input();

        for (int i = 0; i <= (int)MouseButton.Button12; i++)
        {
            inputObj.MouseButtonStates[i] = new MouseButtonState();
        }

        foreach (var keyboard in input.Keyboards)
        {
            inputObj._keyboard = keyboard;

            keyboard.KeyDown += (_, key, _) =>
            {
                inputObj.KeyDown.Add(key);
                inputObj.KeyPressed.Add(key);
            };

            keyboard.KeyUp += (_, key, _) =>
            {
                inputObj.KeyUp.Add(key);
                inputObj.KeyDown.Remove(key);
                inputObj.KeyReleased.Add(key);
            };

            keyboard.KeyChar += (keyboard1, c) =>
            {
                inputObj.TextInput += c;
            };

            //fuck the fucking silk.net input abstraction!!!!!!!! should just use the raw glfw bindings
            var subs = (IDictionary)keyboard.GetType().Assembly.GetType("Silk.NET.Input.Glfw.GlfwInputPlatform")!.GetField("_subs", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
            var glfwEvents = subs[window.Handle]!;
            var field = glfwEvents.GetType().GetEvent("Key", BindingFlags.Instance | BindingFlags.Public)!;
            var convertKeysMethod = keyboard.GetType().GetMethod("ConvertKey", BindingFlags.Static | BindingFlags.NonPublic, [typeof(Keys)])!;
            field.AddEventHandler(glfwEvents, new GlfwCallbacks.KeyCallback( (_, key, _, action, _) =>
            {
                if (action == InputAction.Repeat)
                {
                    var k = (Key)convertKeysMethod.Invoke(null, [key])!;
                    inputObj.KeyPressed.Add(k);
                }
            }));
        }

        foreach (var mouse in input.Mice)
        {
            inputObj._mouse = mouse;

            mouse.MouseDown += (_, button) =>
            {
                var mouseButton = inputObj.GetMouseButton(button);
                mouseButton.IsMouseButtonDown = true;
                mouseButton.IsMouseButtonPressed = true;
            };
            mouse.MouseUp += (_, button) =>
            {
                var mouseButton = inputObj.GetMouseButton(button);
                mouseButton.IsMouseButtonUp = true;
                mouseButton.IsMouseButtonDown = false;
                mouseButton.IsMouseButtonReleased = true;
            };
            mouse.Scroll += (_, wheel) =>
            {
                inputObj.ScrollDeltaX = wheel.X;
                inputObj.ScrollDeltaY = wheel.Y;
            };
            // mouse.MouseMove += (_, mouseDelta) =>
            // {
            //     MousePosition += mouseDelta;
            // };
        }

        return inputObj;
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