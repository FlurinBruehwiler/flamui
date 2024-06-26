using Flamui.Layouting;

namespace Flamui.UiElements;

public static class AbsoluteLayouter
{
    private static void Position(UiElement child, BoxSize boxSize, AbsoluteInfo info)
    {
        var horizontalOffset = 0f;

        if (info.Position.Left is {} left)
        {
            horizontalOffset = left;
        }

        if (info.Position.Right is { } right)
        {
            horizontalOffset = boxSize.Width + right;
        }

        child.ParentData = child.ParentData with
        {
            Position = new Point(horizontalOffset, info.Position.Top ?? 0)
        };
    }

    public static void LayoutAbsoluteChildren(List<UiElement> children, BoxSize boxSize)
    {
        foreach (var child in children)
        {
            if(child.UiElementInfo.AbsoluteInfo is not {} info)
                continue;

            child.Layout(BoxConstraint.None());
            Position(child, boxSize, info);
        }
    }
}
