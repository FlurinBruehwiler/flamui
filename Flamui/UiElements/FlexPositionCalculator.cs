using Flamui.Layouting;
using EnumMAlign = Flamui.MAlign;

namespace Flamui.UiElements;

public static class FlexPositionCalculator
{
    public static BoxSize ComputePosition(List<IUiElement> children, MAlign mAlign, XAlign xAlign, Dir dir, BoxSize size)
    {
        switch (mAlign)
        {
            case EnumMAlign.FlexStart:
                return CalculateFlexStart(children, dir, size, xAlign);
            // case EnumMAlign.FlexEnd:
            //     return CalculateFlexEnd(children, dir);
            // case EnumMAlign.SpaceBetween:
            //     return RenderSpaceBetween(children, dir);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static BoxSize CalculateFlexStart(List<IUiElement> children, Dir dir, BoxSize size, XAlign xAlign)
    {
        var mainOffset = 0f;
        var crossSize = 0f;

        foreach (var child in children)
        {
            SetPosition(mainOffset, child, dir, size, xAlign);
            mainOffset += child.Size.GetMainAxis(dir);

            var childCrossSize = child.Size.GetCrossAxis(dir);
            if (childCrossSize > crossSize)
                crossSize += childCrossSize;
        }

        var mainSize = mainOffset;

        return BoxSize.FromDirection(dir, mainSize, crossSize);
    }

    // private static BoxSize CalculateFlexEnd(List<ILayoutable> children, Dir dir)
    // {
    //     var mainOffset = RemainingMainAxisSize();
    //
    //     foreach (var child in Children)
    //     {
    //         if (child is UiContainer { PAbsolute: true } divChild)
    //         {
    //             PositionAbsoluteItem(divChild);
    //             continue;
    //         }
    //
    //         SetPosition(mainOffset, child);
    //         mainOffset += GetItemMainAxisLength(child) + PGap;
    //     }
    //
    //     return new Size();
    // }
    //
    // private Size RenderSpaceBetween()
    // {
    //     var totalRemaining = RemainingMainAxisSize();
    //     var space = totalRemaining / (Children.Count - 1);
    //
    //     var mainOffset = 0f;
    //
    //     foreach (var child in Children)
    //     {
    //         if (child is UiContainer { PAbsolute: true } divChild)
    //         {
    //             PositionAbsoluteItem(divChild);
    //             continue;
    //         }
    //
    //         SetPosition(mainOffset, child);
    //         mainOffset += GetItemMainAxisLength(child) + space + PGap;
    //     }
    //
    //     return new Size();
    // }

    private static void SetPosition(float mainOffset, IUiElement item, Dir dir, BoxSize size, XAlign xAlign)
    {
        item.ParentData = item.ParentData with
        {
            Position = Point.FromDirection(dir, mainOffset, GetCrossAxisOffset(item, xAlign, size, dir))
        };
    }

    private static float GetCrossAxisOffset(IUiElement item, XAlign xAlign, BoxSize size, Dir dir)
    {
        return xAlign switch
        {
            XAlign.FlexStart => 0,
            XAlign.FlexEnd => size.GetCrossAxis(dir) - item.Size.GetCrossAxis(dir),
            XAlign.Center => size.GetCrossAxis(dir) / 2 - item.Size.GetCrossAxis(dir) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
