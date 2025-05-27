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
            case EnumMAlign.Start:
                return CalculateFlexStart(children, size, info);
            case EnumMAlign.End:
                return CalculateFlexEnd(children, size, info);
            case EnumMAlign.Center:
                return CalculateFlexCenter(children, size, info);
            case MAlign.SpaceBetween:
                return CalculateSpaceBetween(children, size, info);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static BoxSize CalculateFlexStart(List<UiElement> children, BoxSize size, FlexContainerInfo info)
    {
        var mainOffset = info.Padding.StartOfDirection(info.Direction);
        var maxCrossSize = 0f;

        foreach (var child in children)
        {
            if (child.UiElementInfo.AbsoluteInfo.HasValue)
                continue;

            mainOffset += child.UiElementInfo.Margin.StartOfDirection(info.Direction);
            SetPosition(mainOffset, child, size, info);
            mainOffset += child.Rect.GetMainAxis(info.Direction) + child.UiElementInfo.Margin.EndOfDirection(info.Direction) + info.Gap;
            maxCrossSize = Math.Max(maxCrossSize,
                child.Rect.GetCrossAxis(info.Direction) +
                child.UiElementInfo.Margin.SumInDirection(info.Direction.Other()));
        }

        mainOffset -= info.Gap;
        mainOffset += info.Padding.EndOfDirection(info.Direction);
        return BoxSize.FromDirection(info.Direction, mainOffset, maxCrossSize);
    }

    private static BoxSize CalculateFlexEnd(List<UiElement> children, BoxSize size, FlexContainerInfo info)
    {
        var startOffset = size.GetMainAxis(info.Direction) - info.Padding.EndOfDirection(info.Direction);

        for (var i = children.Count - 1; i >= 0; i--)
        {
            var child = children[i];

            if (child.UiElementInfo.AbsoluteInfo.HasValue)
                continue;

            startOffset = startOffset - child.Rect.GetMainAxis(info.Direction) - child.UiElementInfo.Margin.EndOfDirection(info.Direction);

            SetPosition(startOffset, child, size, info);
            startOffset = startOffset - info.Gap - child.UiElementInfo.Margin.StartOfDirection(info.Direction);
        }

        return new BoxSize();
    }

    private static BoxSize CalculateSpaceBetween(List<UiElement> children, BoxSize size, FlexContainerInfo info)
    {
        var (totalChildrenSize, relevantChildCount) = SpaceTakenUpByChildren(children, info);

        if (relevantChildCount == 0)
            return new BoxSize();

        var totalSize = size.GetMainAxis(info.Direction) - info.PaddingSizeMain();
        var remainingSize = totalSize - totalChildrenSize;
        var gap = remainingSize / (relevantChildCount - 1);

        var offset = info.Padding.StartOfDirection(info.Direction);

        foreach (var child in children)
        {
            if(child.UiElementInfo.AbsoluteInfo.HasValue)
                continue;

            offset += child.UiElementInfo.Margin.StartOfDirection(info.Direction);
            SetPosition(offset, child, size, info);
            offset += child.Rect.GetMainAxis(info.Direction) + child.UiElementInfo.Margin.EndOfDirection(info.Direction) + gap;
        }

        return new BoxSize();
    }

    private static BoxSize CalculateFlexCenter(List<UiElement> children, BoxSize size, FlexContainerInfo info)
    {
        var (totalSize, relevantChildCount) = SpaceTakenUpByChildren(children, info);

        if (relevantChildCount == 0)
            return new BoxSize();

        //i commented this out, because of a bug, I'm not sure why I added this initially...
        // totalSize -= children.First().UiElementInfo.Margin.StartOfDirection(info.Direction) +
        //              children.Last().UiElementInfo.Margin.EndOfDirection(info.Direction);

        totalSize += FlexSizeCalculator.TotalGapSize(relevantChildCount, info);

        var center = size.GetMainAxis(info.Direction) / 2;
        var offset = center - totalSize / 2;

        foreach (var child in children)
        {
            if (child.UiElementInfo.AbsoluteInfo.HasValue)
                continue;

            offset += child.UiElementInfo.Margin.StartOfDirection(info.Direction);
            SetPosition(offset, child, size, info);
            offset += child.Rect.GetMainAxis(info.Direction) + info.Gap + child.UiElementInfo.Margin.EndOfDirection(info.Direction);
        }

        return new BoxSize();
    }

    private static (float, int) SpaceTakenUpByChildren(List<UiElement> children, FlexContainerInfo info)
    {
        var totalSize = 0f;

        int relevantChildCount = 0;

        //try to remove this loop, we could precalculate it in the FlexSizeCalculation
        foreach (var child in children)
        {
            if (child.UiElementInfo.AbsoluteInfo.HasValue)
                continue;

            relevantChildCount++;

            totalSize += child.Rect.GetMainAxis(info.Direction) + child.UiElementInfo.Margin.SumInDirection(info.Direction);
        }

        return (totalSize, relevantChildCount);
    }

    private static void SetPosition(float mainOffset, UiElement item, BoxSize size, FlexContainerInfo info)
    {
        var point = Point.FromDirection(info.Direction, mainOffset,
            GetCrossAxisOffset(item, size, info));

        item.ParentData = item.ParentData with
        {
            Position = point
        };
    }

    private static float GetCrossAxisOffset(UiElement item, BoxSize size, FlexContainerInfo info)
    {
        return info.CrossAlignment switch
        {
            XAlign.Start => info.Padding.StartOfDirection(info.Direction.Other()) + item.UiElementInfo.Margin.StartOfDirection(info.Direction.Other()),
            XAlign.End => size.GetCrossAxis(info.Direction) - info.Padding.EndOfDirection(info.Direction.Other()) - item.Rect.GetCrossAxis(info.Direction) - item.UiElementInfo.Margin.EndOfDirection(info.Direction.Other()),
            XAlign.Center => size.GetCrossAxis(info.Direction) / 2 - item.Rect.GetCrossAxis(info.Direction) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
