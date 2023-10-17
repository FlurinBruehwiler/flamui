using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using static SDL2.SDL;

namespace ImSharpUISample;

public class Input
{
    public MouseButton[] MouseButtonStates =
    {
        new(),
        new(),
        new(),
    };

    public Vector2 MousePosition { get; set; }
    public Vector2 LastMousePosition { get; set; }
    public int ScrollDelta { get; set; }
    public string TextInput { get; set; } = string.Empty;

    /// <summary>
    /// Keys that have been pressed once
    /// </summary>
    public HashSet<SDL_Scancode> KeyPressed { get; set; } = new();

    /// <summary>
    /// Keys that are being pressed
    /// </summary>
    public HashSet<SDL_Scancode> KeyDown { get; set; } = new();

    /// <summary>
    /// Keys that have been release once
    /// </summary>
    public HashSet<SDL_Scancode> KeyReleased { get; set; } = new();

    /// <summary>
    /// Keys that are not being pressed
    /// </summary>
    public HashSet<SDL_Scancode> KeyUp { get; set; } = new();


    public void HandleEvents(ConcurrentQueue<SDL_Event> events)
    {
        while (events.TryDequeue(out var sdlEvent))
        {
            HandleEvent(sdlEvent);
        }
    }

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

        ScrollDelta = 0;
    }

    private void HandleEvent(SDL_Event sdlEvent)
    {
        switch (sdlEvent.type)
        {
            case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                HandleMouseDown(sdlEvent.button);
                break;
            case SDL_EventType.SDL_MOUSEBUTTONUP:
                HandleMouseUp(sdlEvent.button);
                break;
            case SDL_EventType.SDL_MOUSEMOTION:
                HandleMouseMove(sdlEvent.motion);
                break;
            case SDL_EventType.SDL_MOUSEWHEEL:
                HandleMouseWheel(sdlEvent.wheel);
                break;
            case SDL_EventType.SDL_TEXTINPUT:
                HandleTextInput(sdlEvent.text);
                break;
            case SDL_EventType.SDL_KEYDOWN:
                HandleKeyDown(sdlEvent.key);
                break;
            case SDL_EventType.SDL_KEYUP:
                HandleKeyUp(sdlEvent.key);
                break;
        }
    }

    private void HandleTextInput(SDL_TextInputEvent sdlEventText)
    {
        unsafe
        {
            //ToDo https://wiki.libsdl.org/SDL2/Tutorials-TextInput
            TextInput += Marshal.PtrToStringUTF8((IntPtr)sdlEventText.text);
        }
    }

    private void HandleMouseMove(SDL_MouseMotionEvent sdlEventMotion)
    {
        MousePosition = new Vector2(sdlEventMotion.x, sdlEventMotion.y);
    }

    private void HandleMouseWheel(SDL_MouseWheelEvent sdlEventWheel)
    {
        ScrollDelta -= sdlEventWheel.y;
    }

    private void HandleKeyUp(SDL_KeyboardEvent sdlEventKey)
    {
        var keycode = sdlEventKey.keysym.scancode;
        KeyUp.Add(keycode);
        KeyDown.Remove(keycode);
        KeyReleased.Add(keycode);
    }

    private void HandleKeyDown(SDL_KeyboardEvent sdlEventKey)
    {
        var keycode = sdlEventKey.keysym.scancode;
        KeyDown.Add(keycode);
        KeyPressed.Add(keycode);
    }

    private void HandleMouseUp(SDL_MouseButtonEvent sdlEventButton)
    {
        var mouseButton = GetMouseButton(sdlEventButton.button);
        mouseButton.IsMouseButtonUp = true;
        mouseButton.IsMouseButtonDown = false;
        mouseButton.IsMouseButtonReleased = true;
    }

    private void HandleMouseDown(SDL_MouseButtonEvent sdlEventButton)
    {
        var mouseButton = GetMouseButton(sdlEventButton.button);
        mouseButton.IsMouseButtonDown = true;
        mouseButton.IsMouseButtonPressed = true;
    }

    private MouseButton GetMouseButton(byte button)
    {
        return (uint)button switch
        {
            SDL_BUTTON_LEFT => MouseButtonStates[(int)MouseButtonKind.Left],
            SDL_BUTTON_MIDDLE => MouseButtonStates[(int)MouseButtonKind.Middle],
            SDL_BUTTON_RIGHT => MouseButtonStates[(int)MouseButtonKind.Right],
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };
    }
}

public class MouseButton
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


public enum MouseButtonKind
{
    Left = 0,
    Middle = 1,
    Right = 2
}
