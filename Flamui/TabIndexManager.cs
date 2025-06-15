using Flamui.UiElements;
using Silk.NET.Input;

namespace Flamui;

public sealed class TabIndexManager
{
    public void HandleTab(UiTree tree)
    {
        if (!tree.IsKeyPressed(Key.Tab))
            return;

        if (tree.ActiveDiv is null)
            return;

        var shouldSearchBackwards = tree.IsKeyDown(Key.ShiftLeft) || tree.IsKeyDown(Key.ShiftRight);

        if (shouldSearchBackwards)
        {
            if (GetPreviousFocusable(tree.ActiveDiv, skipSelfCheck: true) is { } previousFocusable)
            {
                tree.ActiveDiv = previousFocusable;
            }
        }
        else
        {
            if (GetNextFocusable(tree.ActiveDiv, skipSelfCheck: true) is { } nextFocusable)
            {
                tree.ActiveDiv = nextFocusable;
            }
        }
    }

    private UiElement? GetPreviousFocusable(UiElement currentElement, bool skipSelfCheck = false)
    {
        if (!skipSelfCheck && currentElement is FlexContainer { Info.Focusable: true } uiContainer)
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

    private UiElement GetLastChild(UiElement flexContainer)
    {
        if (flexContainer is not UiElementContainer uiElementContainer)
            return flexContainer;

        if (uiElementContainer.Children.Count == 0)
        {
            return uiElementContainer;
        }

        return uiElementContainer.Children.Last();
    }

    private FlexContainer? GetNextFocusable(UiElement currentElement, bool skipSelfCheck = false)
    {
        //check myself
        if (!skipSelfCheck && currentElement is FlexContainer { Info.Focusable: true } uiContainer)
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

    private FlexContainer? SearchAbove(UiElement currentElement)
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
