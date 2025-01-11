using Flamui.Layouting;

namespace Flamui.UiElements;

public static class FlexSizeCalculator
{
    public static float TotalGapSize(int childCount, FlexContainerInfo info)
    {
        if (childCount == 0)
            return 0;

        return (childCount - 1) * info.Gap;
    }

    public static BoxSize ComputeSize(BoxConstraint constraint, List<UiElement> children, FlexContainerInfo info)
    {
        if (children.Count == 0)
            return new BoxSize();

        float totalFixedSize = 0;
        float totalPercentage = 0;
        float maxCrossSize = 0;

        var relevantChildCount = 0;

        var isMainShrink = info.GetMainSizeKind(info.Direction) == SizeKind.Shrink;

        //Loop through all children
        //- Sum up percentage of flexible children
        //- Layout inflexible children, and sum up the size
        foreach (var child in children)
        {
            child.PrepareLayout(info.Direction);

            if (child.UiElementInfo.AbsoluteInfo.HasValue)
                continue;

            relevantChildCount++;

            totalFixedSize += child.UiElementInfo.Margin.SumInDirection(info.Direction);

            if (child.IsFlexible(out var config))
            {
                if (isMainShrink)
                    throw InvalidLayoutException.Create(InvalidLayoutType.FractionWithinShrink, child);

                totalPercentage += config.Percentage;
                continue;
            }

            var childConstraint = BoxConstraint.FromDirection(info.Direction, 0, float.PositiveInfinity, 0,
                constraint.GetCrossAxis(info.Direction).Max - info.PaddingSizeCross() -
                child.UiElementInfo.Margin.SumInDirection(info.Direction.Other()));
            var size = child.Layout(childConstraint);

            totalFixedSize += size.GetMainAxis(info.Direction);

            maxCrossSize = Math.Max(maxCrossSize, size.GetCrossAxis(info.Direction) + child.UiElementInfo.Margin.SumInDirection(info.Direction.Other()));
        }

        totalFixedSize += info.PaddingSizeMain() + TotalGapSize(relevantChildCount, info);

        var availableSize = constraint.GetMainAxis(info.Direction).Max - totalFixedSize;
        var sizePerPercentage = GetSizePerPercentage(totalPercentage, availableSize);

        //layout all flexible children
        foreach (var child in children)
        {
            if (!child.IsFlexible(out var config))
                continue;

            if(child.UiElementInfo.AbsoluteInfo.HasValue)
                continue;

            var mainSizeConstraint = config.Percentage * sizePerPercentage;

            var size = child.Layout(BoxConstraint.FromDirection(info.Direction, mainSizeConstraint, mainSizeConstraint, 0,
                constraint.GetCrossAxis(info.Direction).Max - info.PaddingSizeCross() - child.UiElementInfo.Margin.SumInDirection(info.Direction.Other())));

            totalFixedSize += size.GetMainAxis(info.Direction);
            maxCrossSize = Math.Max(maxCrossSize, size.GetCrossAxis(info.Direction));
        }

        return BoxSize.FromDirection(info.Direction, totalFixedSize, maxCrossSize + info.PaddingSizeCross());
    }

    private static float GetSizePerPercentage(float totalPercentage, float availableSize)
    {
        float sizePerPercent;

        if (totalPercentage > 100)
        {
            sizePerPercent = availableSize / totalPercentage;
        }
        else
        {
            sizePerPercent = availableSize / 100;
        }

        return sizePerPercent;
    }
}
