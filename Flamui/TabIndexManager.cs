using Flamui.UiElements;

namespace Flamui;

public class TabIndexManager
{
    public void HandleTab()
    {
        if (!Window.IsKeyPressed(SDL_Scancode.SDL_SCANCODE_TAB))
            return;

        if (Window.ActiveDiv is null)
            return;

        var shouldSearchBackwards = Window.IsKeyDown(SDL_Scancode.SDL_SCANCODE_LSHIFT) || Window.IsKeyDown(SDL_Scancode.SDL_SCANCODE_RSHIFT);

        if (shouldSearchBackwards)
        {
            if (GetPreviousFocusable(Window.ActiveDiv, skipSelfCheck: true) is { } previousFocusable)
            {
                Window.ActiveDiv = previousFocusable;
            }
        }
        else
        {
            if (GetNextFocusable(Window.ActiveDiv, skipSelfCheck: true) is { } nextFocusable)
            {
                Window.ActiveDiv = nextFocusable;
            }
        }
    }

    private UiContainer? GetPreviousFocusable(UiElement currentElement, bool skipSelfCheck = false)
    {
        if (!skipSelfCheck && currentElement is UiContainer { PFocusable: true } uiContainer)
        {
            return uiContainer;
        }

        if (currentElement.GetPreviousSibling() is { } previousSibling)
        {
            var lastChild = GetLastChild(previousSibling);

            return GetPreviousFocusable(lastChild);
        }

        if (currentElement.Parent != null!)
        {
            return GetPreviousFocusable(currentElement.Parent);
        }

        return null;
    }

    private UiElement GetLastChild(UiElement uiElement)
    {
        if (uiElement is not UiElementContainer uiElementContainer)
            return uiElement;

        if (uiElementContainer.Children.Count == 0)
        {
            return uiElementContainer;
        }

        return uiElementContainer.Children.Last();
    }

    private UiContainer? GetNextFocusable(UiElement currentElement, bool skipSelfCheck = false)
    {
        //check myself
        if (!skipSelfCheck && currentElement is UiContainer { PFocusable: true } uiContainer)
        {
            return uiContainer; //found it
        }

        //check if has children
        if (currentElement is UiElementContainer uiElementContainer)
        {
            if (uiElementContainer.Children.Count != 0)
            {
                return GetNextFocusable(uiElementContainer.Children.First());
            }
        }

        if (currentElement.GetNextSibling() is { } nextSibling)
        {
            return GetNextFocusable(nextSibling);
        }

        //we are the last/first child

        if (currentElement.Parent == null!) // we are already at the root
        {
            return null;
        }

        return SearchAbove(currentElement.Parent);
    }

    private UiContainer? SearchAbove(UiElementContainer currentElement)
    {
        //try go sideways
        if (currentElement.GetNextSibling() is { } nextSibling)
        {
            return GetNextFocusable(nextSibling);
        }

        //otherwise go up

        if (currentElement.Parent == null!) // we are already at the root
        {
            return null;
        }

        return SearchAbove(currentElement.Parent);
    }
}
