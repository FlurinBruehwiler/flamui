using Flamui.Layouting;
using EnumMAlign = Flamui.MAlign;

namespace Flamui.UiElements;

public static class FlexPositionCalculator
{
    public static BoxSize ComputePosition(List<UiElement> children, BoxSize size, FlexContainerInfo info)
    {
        if(children.Count == 0)
            return size;

        switch (info.MainAlignment)
        {
            case EnumMAlign.FlexStart:
                return CalculateFlexStart(children, size, info);
            case EnumMAlign.FlexEnd:
                return CalculateFlexEnd(children, size, info);
            case EnumMAlign.Center:
                return CalculateFlexCenter(children, size, info);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static BoxSize CalculateFlexStart(List<UiElement> children, BoxSize size, FlexContainerInfo info)
    {
        var mainOffset = info.Padding.StartOfDirection(info.Direction);
        // var crossSize = 0f;

        foreach (var child in children)
        {
            SetPosition(mainOffset, child, size, info);
            mainOffset += child.BoxSize.GetMainAxis(info.Direction) + info.Gap;

            // var childCrossSize = child.BoxSize.GetCrossAxis(info.Direction);
            // if (childCrossSize > crossSize)
            //     crossSize += childCrossSize;
        }

        var mainSize = mainOffset - info.Gap;

        return new BoxSize();
        // return BoxSize.FromDirection(info.Direction, mainSize + info.PaddingSizeMain(), crossSize + info.PaddingSizeCross());
    }

    private static BoxSize CalculateFlexEnd(List<UiElement> children, BoxSize size, FlexContainerInfo info)
    {
        var startOffset = size.GetMainAxis(info.Direction) - info.Padding.EndOfDirection(info.Direction);

        for (var i = children.Count - 1; i >= 0; i--)
        {
            var child = children[i];

            startOffset -= child.BoxSize.GetMainAxis(info.Direction);

            SetPosition(startOffset, child, size, info);
            startOffset -= info.Gap;
        }

        return new BoxSize();
    }

    private static BoxSize CalculateFlexCenter(List<UiElement> children, BoxSize size, FlexContainerInfo info)
    {
        var totalSize = 0f;

        //try to remove this loop, we could precalculate it in the FlexSizeCalculation
        foreach (var child in children)
        {
            totalSize += child.BoxSize.GetMainAxis(info.Direction);
        }

        totalSize += FlexSizeCalculator.TotalGapSize(children.Count, info);

        var center = size.GetMainAxis(info.Direction) / 2;
        var offset = center - totalSize / 2;

        foreach (var child in children)
        {
            SetPosition(offset, child, size, info);
            offset += child.BoxSize.GetMainAxis(info.Direction) + info.Gap;
        }

        return new BoxSize();
    }

    private static void SetPosition(float mainOffset, UiElement item, BoxSize size, FlexContainerInfo info)
    {
        var point = Point.FromDirection(info.Direction, mainOffset,
            GetCrossAxisOffset(item, size, info));

        item.ParentData = item.ParentData with
        {
            Position = new Point(point.X, point.Y)
        };
    }

    private static float GetCrossAxisOffset(UiElement item, BoxSize size, FlexContainerInfo info)
    {
        return info.CrossAlignment switch
        {
            XAlign.FlexStart => info.Padding.StartOfDirection(info.Direction.Other()),
            XAlign.FlexEnd => size.GetCrossAxis(info.Direction) - info.Padding.EndOfDirection(info.Direction.Other()) - item.BoxSize.GetCrossAxis(info.Direction),
            XAlign.Center => size.GetCrossAxis(info.Direction) / 2 - item.BoxSize.GetCrossAxis(info.Direction) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
