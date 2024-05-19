using Flamui.Layouting;

namespace Flamui.UiElements;

public static class FlexSizeCalculator
{
    private static float TotalGapSize(List<UiElement> children, FlexContainerInfo info)
    {
        if (children.Count == 0)
            return 0;

        return (children.Count - 1) * info.Gap;
    }

    public static BoxSize ComputeSize(BoxConstraint constraint, List<UiElement> children, FlexContainerInfo info)
    {
        if (children.Count == 0)
            return new BoxSize().ApplyConstraint(constraint);

        float totalFixedSize = 0;
        float totalPercentage = 0;
        float maxCrossSize = 0;

        //Loop through all children
        //- Sum up percentage of flexible children
        //- Layout inflexible children, and sum up the size
        foreach (var child in children)
        {
            child.PrepareLayout(info.Direction);

            if (child.IsFlexible(out var config))
            {
                totalPercentage += config.Percentage;
                continue;
            }

            var size = child.Layout(BoxConstraint.FromDirection(info.Direction, 0, float.PositiveInfinity, 0,
                constraint.GetCrossAxis(info.Direction).Max - info.PaddingSizeCross()));

            totalFixedSize += size.GetMainAxis(info.Direction);
            maxCrossSize = Math.Max(maxCrossSize, size.GetCrossAxis(info.Direction));
        }

        var availableSize = constraint.GetMainAxis(info.Direction).Max - totalFixedSize - info.PaddingSizeMain() - TotalGapSize(children, info);
        var sizePerPercentage = GetSizePerPercentage(totalPercentage, availableSize);

        //layout all flexible children
        foreach (var child in children)
        {
            if (!child.IsFlexible(out var config))
                continue;

            var mainSizeConstraint = config.Percentage * sizePerPercentage;

            var size = child.Layout(BoxConstraint.FromDirection(info.Direction, mainSizeConstraint, mainSizeConstraint, 0,
                constraint.GetCrossAxis(info.Direction).Max - info.PaddingSizeCross()));

            totalFixedSize += size.GetMainAxis(info.Direction);
            maxCrossSize = Math.Max(maxCrossSize, size.GetCrossAxis(info.Direction));
        }

        return BoxSize.FromDirection(info.Direction, totalFixedSize, maxCrossSize).ApplyConstraint(constraint);
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
