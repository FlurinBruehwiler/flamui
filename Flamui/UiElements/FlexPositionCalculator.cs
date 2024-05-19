﻿using Flamui.Layouting;
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

        foreach (var child in children)
        {
            mainOffset += child.UiElementInfo.Margin.StartOfDirection(info.Direction);
            SetPosition(mainOffset, child, size, info);
            mainOffset += child.BoxSize.GetMainAxis(info.Direction) + child.UiElementInfo.Margin.EndOfDirection(info.Direction) + info.Gap;
        }

        return new BoxSize();
    }

    private static BoxSize CalculateFlexEnd(List<UiElement> children, BoxSize size, FlexContainerInfo info)
    {
        var startOffset = size.GetMainAxis(info.Direction) - info.Padding.EndOfDirection(info.Direction);

        for (var i = children.Count - 1; i >= 0; i--)
        {
            var child = children[i];

            startOffset = startOffset - child.BoxSize.GetMainAxis(info.Direction) - child.UiElementInfo.Margin.EndOfDirection(info.Direction);

            SetPosition(startOffset, child, size, info);
            startOffset = startOffset - info.Gap - child.UiElementInfo.Margin.StartOfDirection(info.Direction);
        }

        return new BoxSize();
    }

    //todo respect margin
    private static BoxSize CalculateFlexCenter(List<UiElement> children, BoxSize size, FlexContainerInfo info)
    {
        var totalSize = 0f;

        //try to remove this loop, we could precalculate it in the FlexSizeCalculation
        foreach (var child in children)
        {
            totalSize += child.BoxSize.GetMainAxis(info.Direction) + child.UiElementInfo.Margin.SumInDirection(info.Direction);
        }

        //ignore the margin at the start and end
        totalSize -= children.First().UiElementInfo.Margin.StartOfDirection(info.Direction) +
                     children.Last().UiElementInfo.Margin.EndOfDirection(info.Direction);

        totalSize += FlexSizeCalculator.TotalGapSize(children.Count, info);

        var center = size.GetMainAxis(info.Direction) / 2;
        var offset = center - totalSize / 2;

        foreach (var child in children)
        {
            offset += child.UiElementInfo.Margin.StartOfDirection(info.Direction);
            SetPosition(offset, child, size, info);
            offset += child.BoxSize.GetMainAxis(info.Direction) + info.Gap + child.UiElementInfo.Margin.EndOfDirection(info.Direction);
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
            XAlign.FlexStart => info.Padding.StartOfDirection(info.Direction.Other()) + item.UiElementInfo.Margin.StartOfDirection(info.Direction.Other()),
            XAlign.FlexEnd => size.GetCrossAxis(info.Direction) - info.Padding.EndOfDirection(info.Direction.Other()) - item.BoxSize.GetCrossAxis(info.Direction) - item.UiElementInfo.Margin.EndOfDirection(info.Direction.Other()),
            XAlign.Center => size.GetCrossAxis(info.Direction) / 2 - item.BoxSize.GetCrossAxis(info.Direction) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
