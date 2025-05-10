using System.Collections;
using System.Numerics;
using System.Reflection;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Windowing;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Flamui;

/*

The question is, what is this class????

Should it be the global abstraction over input or the per UiTree abstraction, I'm inclined to say the latter.

From an architectural standpoint we get global input callbacks form GLFW,
we then need to decide to what UiTrees we want to send this input.

Ok, so lets say, that this class here just is here for distributing glfw events to UiTrees.
For testing, we have a testing driver, that sends events to a UiTree, i.e. this class will not be used for testing.

How do we decide where to send the keyboard input event? Is there some kind of focus system, like if one UiTree is focus,
the KeyBoard events only get send to that one (this is how html does it)

With mouse events, we send it to the UiTree the Mouse is currently hovering, and only the innermost. (again, this is how html does it)

Who keeps track which area on screen is occupied by which uiTree?

Either we have keep a list around of all UiTrees. I don't really like this, because then we need to keep it in sync.
We should rather at the start of the input processing phase, walk the root UiTree and search for nested UiTrees.
But this is way more expensive because we need to walk the entire tree. How ofter per frame do we walk the entire tree, we should start counting this!!!!



 */
public static class Input
{


    private static MouseButtonState GetMouseButton(MouseButton button, PhysicalWindow window)
    {
        var uiTree = GetHoveredUiTree(window);

        return uiTree.MouseButtonStates[(int)button];
    }

    //for keyboard input
    private static UiTree GetFocusedUiTree(PhysicalWindow window)
    {
        //todo, implement actual logic
        return window.UiTree;
    }

    //for mouse input
    private static UiTree GetHoveredUiTree(PhysicalWindow window)
    {
        //todo, implement actual logic
        return window.UiTree;
    }

    public static unsafe void SetupInputCallbacks(PhysicalWindow window)
    {
        var glfwWindow = window.GlfWindow;

        var input = glfwWindow.CreateInput();

        foreach (var keyboard in input.Keyboards)
        {
            keyboard.KeyDown += (_, key, _) =>
            {
                var uiTree = GetFocusedUiTree(window);

                uiTree.KeyDown.Add(key);
                uiTree.KeyPressed.Add(key);
            };

            keyboard.KeyUp += (_, key, _) =>
            {
                var uiTree = GetFocusedUiTree(window);

                uiTree.KeyUp.Add(key);
                uiTree.KeyDown.Remove(key);
                uiTree.KeyReleased.Add(key);
            };

            keyboard.KeyChar += (_, c) =>
            {
                var uiTree = GetFocusedUiTree(window);

                uiTree.TextInput += c;
            };

            //f*** the silk.net input abstraction!!!!!!!! should just use the raw glfw bindings
            var subs = (IDictionary)keyboard.GetType().Assembly.GetType("Silk.NET.Input.Glfw.GlfwInputPlatform")!.GetField("_subs", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
            var glfwEvents = subs[glfwWindow.Handle]!;
            var field = glfwEvents.GetType().GetEvent("Key", BindingFlags.Instance | BindingFlags.Public)!;
            var convertKeysMethod = keyboard.GetType().GetMethod("ConvertKey", BindingFlags.Static | BindingFlags.NonPublic, [typeof(Keys)])!;
            field.AddEventHandler(glfwEvents, new GlfwCallbacks.KeyCallback( (_, key, _, action, _) =>
            {
                if (action == InputAction.Repeat)
                {
                    var k = (Key)convertKeysMethod.Invoke(null, [key])!;

                    var uiTree = GetFocusedUiTree(window);

                    uiTree.KeyPressed.Add(k);
                }
            }));
        }

        foreach (var mouse in input.Mice)
        {
            mouse.MouseDown += (_, button) =>
            {
                var mouseButton = GetMouseButton(button, window);
                mouseButton.IsMouseButtonDown = true;
                mouseButton.IsMouseButtonPressed = true;
            };
            mouse.MouseUp += (_, button) =>
            {
                var mouseButton = GetMouseButton(button, window);
                mouseButton.IsMouseButtonUp = true;
                mouseButton.IsMouseButtonDown = false;
                mouseButton.IsMouseButtonReleased = true;
            };
            mouse.Scroll += (_, wheel) =>
            {
                var uiTree = GetHoveredUiTree(window);
                uiTree.ScrollDelta = new Vector2(wheel.X, wheel.Y);
            };
        }
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