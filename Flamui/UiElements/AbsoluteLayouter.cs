using Flamui.Layouting;

namespace Flamui.UiElements;

public static class AbsoluteLayouter
{
    private static void Position(UiElement child, BoxSize boxSize)
    {
        var horizontalOffset = 0f;

        if (child.UiElementInfo.AbsolutePosition.Left is {} left)
        {
            horizontalOffset = left;
        }

        if (child.UiElementInfo.AbsolutePosition.Right is { } right)
        {
            horizontalOffset = boxSize.Width + right;
        }

        child.ParentData = child.ParentData with
        {
            Position = new Point(horizontalOffset, child.UiElementInfo.AbsolutePosition.Top ?? 0)
        };
    }

    public static void LayoutAbsoluteChildren(List<UiElement> children, BoxSize boxSize, FlexContainerInfo info)
    {
        foreach (var child in children)
        {
            if(!child.UiElementInfo.Absolute)
                continue;

            child.Layout(BoxConstraint.None());
            Position(child, boxSize);
        }
    }
}
