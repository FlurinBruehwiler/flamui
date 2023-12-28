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

        if (GetNextFocusable(Window.ActiveDiv, searchBackwards: shouldSearchBackwards, skipSelfCheck: true) is { } nextFocusable)
        {
            Window.ActiveDiv = nextFocusable;
        }
    }

    private UiContainer? GetNextFocusable(UiElement currentElement, bool searchBackwards = false, bool skipSelfCheck = false)
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

        var sibling = searchBackwards ? currentElement.GetPreviousSibling() : currentElement.GetNextSibling();

        if (sibling is { } nextSibling)
        {
            return GetNextFocusable(nextSibling);
        }

        //we are the last child

        if (currentElement.Parent == null!) // we are already at the root
        {
            return null;
        }

        return SearchAbove(currentElement.Parent, searchBackwards);
    }

    private UiContainer? SearchAbove(UiElementContainer currentElement, bool searchBackwards)
    {
        var sibling = searchBackwards ? currentElement.GetPreviousSibling() : currentElement.GetNextSibling();

        //try go sideways
        if (sibling is { } nextSibling)
        {
            return GetNextFocusable(nextSibling);
        }

        //otherwise go up
        if (currentElement.Parent == null!) // we are already at the root
        {
            return null;
        }

        return SearchAbove(currentElement.Parent, searchBackwards);
    }
}
